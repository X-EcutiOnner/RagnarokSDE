using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Database.Commands;
using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Files;
using SDE.Editor.Parsers.Libconfig;
using Utilities;

namespace SDE.Editor.Parsers.Yaml {
	public unsafe class YamlParser {
		public ParserObject Output { get; set; }
		private readonly List<string> _allLines;
		private readonly List<ParserKeyValue> _writeKeyValues;
		private List<ParserArray> _writeArrays;
		private readonly bool _list;
		private readonly string _idKey;
		private Dictionary<string, ParserKeyValue> _indexedWriteKeyValues = null;
		private Dictionary<string, ParserArray> _indexedWriteArrays = null;
		
		public const string Indent4 = "    ";
		public const string Indent6 = "      ";
		public const string Indent8 = "        ";
		public const string Indent10 = "          ";
		public const string Indent12 = "            ";
		public const string Indent14 = "              ";

		public List<string> AllLines => _allLines;

		public YamlParser(string file, ParserMode mode = ParserMode.Read, string idKey = "") {
			if (mode == ParserMode.Write) {
				_allLines = File.ReadAllLines(file, SdeAppConfiguration.EncodingServer).ToList();
			}

			TextFileHelper.LatestFile = file;

			byte[] data = File.ReadAllBytes(file);
			
			fixed (byte* p = data) {
				_p = p;
				_lastLineStartOffset = _p;
				_end = p + data.Length;
				_lineNumber = 1;
				_fileName = file;
				_encoding = SdeAppConfiguration.EncodingServer;

				_parser_main();
			}

			if (mode == ParserMode.Write) {
				var list = Output as ParserArrayBase;
				
				if (list != null) {
					if (Output["Body"] == null) {
						_allLines.Add("");
						_allLines.Add("Body:");

						var body = new ParserKeyValue("Body", _allLines.Count - 1);
						list.AddElement(body);
						body.Parent = list;
						body.Value = new ParserList(_allLines.Count - 1);
					}

					var entries = list.OfType<ParserKeyValue>().ToList();

					if (entries.Count == 1) {
						_writeKeyValues = entries[0].Value.OfType<ParserKeyValue>().ToList();
						_writeArrays = entries[0].Value.OfType<ParserArray>().ToList();
					}
					else if (entries.Count == 2) {	
						_writeKeyValues = entries[1].Value.OfType<ParserKeyValue>().ToList();
						_writeArrays = entries[1].Value.OfType<ParserArray>().ToList();
					}

					if (_writeArrays.Count > 0) {
						_idKey = ((ParserKeyValue)_writeArrays[0].Objects[0]).Key;
					}
					else {
						_idKey = idKey;
					}
					
					_list = true;
				}
			}
		}

		public class ByteBuilder {
			private byte[] _data;
			private int _length = 0;

			public ByteBuilder() {
				_data = new byte[16];
			}

			public void Append(byte b) {
				if (_length >= _data.Length) {
					byte[] dataExpand = new byte[_length * 2];

					Buffer.BlockCopy(_data, 0, dataExpand, 0, _data.Length);

					_data = dataExpand;
				}

				_data[_length] = b;
				_length++;
			}

			public override string ToString() {
				return SdeAppConfiguration.EncodingServer.GetString(_data, 0, _length);
			}
		}

		public enum YamlListType {
			NotDefined,
			Array,
			KeyValue,
		}

		byte* _p;
		byte* _end;
		private int _lineNumber = 1;
		private int _lineIndent = 0;
		private byte* _lastLineStartOffset;
		private string _fileName;
		private Encoding _encoding;

		private unsafe void _parser_main() {
			Output = new ParserArray(_lineNumber);
			Output.Indent = -1;
			Output.ChildrenIndent = -1;

			_readNode(Output, 0, YamlListType.NotDefined);

			if (Output.ParserType == ParserTypes.Array) {
				var parserArray = Output.To<ParserArray>();

				// Copy pate handling
				if (parserArray.Objects.Count > 0 && parserArray.Objects[0].ParserType == ParserTypes.Array) {
					var tmp_list = new ParserList(0);
					var tmp_keyValue = new ParserKeyValue("copy_paste", 0);

					tmp_list.Objects.AddRange(parserArray.Objects);
					tmp_keyValue.Value = tmp_list;
					parserArray.Objects.Clear();
					parserArray.Objects.Add(tmp_keyValue);
				}
			}

			// Calculate lengths
			if (Output != null) {
				_calculateLength(Output);
				Output.Length = _getLength(Output);
			}
		}

		private void _readNode(ParserObject parent, int indent, YamlListType listType) {
			string word_s = null;
			
			while (_p < _end) {
				char c = (char)*_p;

				switch (*_p) {
					case (byte)'#':
						SkipLine();
						continue;
					case (byte)'\r':  // Ignore character
						_p++;
						continue;
					case (byte)'\n':
						_p++;
						NewLine();

						if (listType == YamlListType.KeyValue && word_s != null) {
							ParserString value = new ParserString(word_s, _lineNumber - 1);
							value.Indent = parent.Indent;
							value.Parent = parent;

							switch (parent.ParserType) {
								case ParserTypes.List:
									((ParserList)parent).AddElement(value);
									break;
								case ParserTypes.Array:
									((ParserArray)parent).AddElement(value);
									break;
							}
						}
						continue;
					case (byte)'-':
						SetupParent(parent, indent);

						if (listType == YamlListType.NotDefined) {
							listType = YamlListType.Array;
						}

						// Validate parent indent
						switch(parent.ParserType) {
							case ParserTypes.List:
							case ParserTypes.Array:
								if (_lineIndent < parent.ChildrenIndent)
									return;

								if (_lineIndent > parent.ChildrenIndent)
									throw GetException("Unexpected indent (parent indent: " + parent.Indent + ", parent child indent: " + parent.ChildrenIndent + ", current indent: " + _lineIndent + ").");

								break;
						}

						if (!(_p + 2 < _end && _p[1] == ' ' && IsLetter(_p[2]))) {
							throw GetException("Expected a space after the hyphen for the list declaration.");
						}

						// Array declaration
						ParserArray array = new ParserArray(_lineNumber);
						array.Indent = _lineIndent;
						_lineIndent += 2;
						array.ChildrenIndent = _lineIndent;
						array.Parent = parent;
						_p += 2;

						_readNode(array, _lineIndent, YamlListType.NotDefined);

						switch (parent.ParserType) {
							case ParserTypes.List:
								((ParserList)parent).AddElement(array);
								break;
							case ParserTypes.Array:	// Used for copy pasting only
								((ParserArray)parent).AddElement(array);
								break;
							default:
								throw GetException("Unexpected parent node type. It can either be a list or an array, found a '" + parent.ParserType + "'.");
						}

						continue;
					case (byte)' ':
						_lineIndent++;
						_p++;
						continue;
					case (byte)':':
						if (string.IsNullOrEmpty(word_s)) {
							throw GetException("Missing declaration key before ':'.");
						}

						_p++;
						Trim();

						ParserKeyValue keyValue = new ParserKeyValue(word_s, _lineNumber);
						keyValue.Indent = parent.Indent;
						keyValue.Parent = parent;
						word_s = null;

						// List declaration
						if (_p < _end && (*_p == '\n' || (_p + 1 < _end && *_p == '\r' && _p[1] == '\n'))) {
							ParserList list = new ParserList(_lineNumber);
							list.Indent = _lineIndent;
							list.Parent = parent;
							list.ChildrenIndent = -1;
							keyValue.Value = list;

							_readNode(list, _lineIndent, YamlListType.NotDefined);
						}
						else if (*_p == '[') { // Aggregate parsing, does not support multi-line
							_p++;
							ParserAggregate aggregate = new ParserAggregate(_lineNumber);
							aggregate.Parent = parent;

							while (_p < _end) {
								c = (char)*_p;

								if (c == '\r' || c == '\n')
									throw GetException("Unexpected syntax; multi-line aggregate arrays are not supported.");

								if (c == ']')
									break;

								word_s = c == '\"' ? ReadValue() : ReadWord();
								aggregate.AddElement(new ParserString(word_s.Trim(' '), _lineNumber));
								c = (char)*_p;

								while (c != '\n' && (c == ',' || c == ' ' || c == '\r')) {
									_p++;
									c = (char)*_p;
								}
							}

							word_s = null;
							keyValue.Value = aggregate;
						}
						else {	// KeyValue, get the line number first!
							var parserString = new ParserString(null, _lineNumber);
							parserString.Parent = parent;
							parserString.Value = ReadValue();
							parserString.Length = _valueLength;
							keyValue.Value = parserString;
						}

						switch (parent.ParserType) {
							case ParserTypes.List:
								((ParserList)parent).AddElement(keyValue);
								break;
							case ParserTypes.Array:
								((ParserArray)parent).AddElement(keyValue);
								break;
						}

						continue;
					default:
						SetupParent(parent, indent);

						if (listType == YamlListType.NotDefined) {
							listType = YamlListType.KeyValue;
						}

						// Validate parent indent
						switch(parent.ParserType) {
							case ParserTypes.List:
								if (_lineIndent < parent.ChildrenIndent || _lineIndent <= parent.Indent)
									return;

								if (_lineIndent == parent.ChildrenIndent && listType != YamlListType.KeyValue)
									return;

								if (_lineIndent > parent.ChildrenIndent)
									throw GetException("Unexpected indent while reading key (parent indent: " + parent.Indent + ", parent child indent: " + parent.ChildrenIndent + ", current indent: " + _lineIndent + ").");

								break;
							case ParserTypes.Array:
								if (_lineIndent < parent.ChildrenIndent)
									return;

								if (_lineIndent > parent.ChildrenIndent)
									throw GetException("Unexpected indent while reading key (parent indent: " + parent.Indent + ", parent child indent: " + parent.ChildrenIndent + ", current indent: " + _lineIndent + ").");

								break;
						}

						word_s = ReadKey().Trim(' ', '\t');

						if (word_s.Length == 0) {
							throw GetException("Null-length word. This is most likely caused by an unexpected character in a string.");
						}

						Trim();
						continue;
				}
			}
		}

		public string ReadKey() {
			byte* start = _p;
			byte previous = 0;

			while (_p < _end) {
				if (*_p == '\r' || *_p == '\n' || *_p == ':' || (*_p == '#' && (previous == ' ' || previous == '\t')))
					break;
				previous = *_p;
				_p++;
			}

			return _encoding.GetString(start, (int)(_p - start));
		}

		public string ReadWord() {
			byte* start = _p;

			while (_p < _end) {
				if (*_p == '\r' || *_p == '\n' || *_p == ':' || *_p == ']')
					break;
				_p++;
			}

			return _encoding.GetString(start, (int)(_p - start));
		}

		private int _valueLength = 0;

		public string ReadValue() {
			byte* start = _p;
			_valueLength = 0;

			if (*_p == '\"') {
				start = ++_p;

				while (_p < _end && *_p != '\r' && *_p != '\n') {
					if (*_p == '\"' && *(_p - 1) != '\\') {
						_p++;
						break;
					}

					_p++;
				}

				_valueLength = 1;
				return _encoding.GetString(start, (int)(_p - start) - 1);
			}
			else if (*_p == '|' || *_p == '>') {
				int indent = _lineIndent;

				SkipLine();
				MoveToIndentEnd();

				if (_lineIndent < indent) // Technically valid
					return "";

				// Remove all comment blocks, makes it easier to handle afterwards
				indent = _lineIndent;
				int readLines = 0;
				bool trim = true;
				int read = 0;
				StringBuilder b = new StringBuilder();

				while (_p < _end) {
					if (trim) {
						while (_p < _end && (*_p == ' ' || *_p == '\t')) _p++;
					}

					if (*_p == '\n' || (*_p == '\r' && _p + 1 < _end && _p[1] == '\n')) {
						if (*_p == '\n')
							_p++;
						else
							_p += 2;

						NewLine();
						MoveToIndentEnd(indent);

						if (read > 0) {
							b.Append(_encoding.GetString(start, read));
							b.Append(' ');
						}

						readLines++;
						trim = true;
						read = 0;
						continue;
					}

					if (_lineIndent < indent) { // Needs to be checked after to make an exception for empty lines
						break;
					}

					if (*_p == '/' && _p + 1 < _end) {
						if (_p[1] == '/') {
							SkipLine();
							NewLine();
							MoveToIndentEnd(indent);
							readLines++;
							trim = true;
							read = 0;
							continue;
						}
						else if (_p[1] == '*') {
							// Read start comment block '/*'
							_p += 2;

							while (_p < _end) {
								if (*_p == '\n')
									NewLine();
								else if (*_p == '*' && _p + 1 < _end && _p[1] == '/')
									break;
								_p++;
							}

							// Read end comment block '*/'
							_p += 2;
							continue;
						}
					}

					if (trim) {
						if (read > 0)
							b.Append(_encoding.GetString(start, read));
						start = _p;
					}

					trim = false;
					read++;
					_p++;
				}

				if (read > 0)
					b.Append(_encoding.GetString(start, read));

				int trimIdx = b.Length - 1;
				int trimLength = 0;
				while (b.Length > 0 && b[trimIdx - trimLength] == ' ') trimLength++;

				if (trimLength > 0)
					b.Remove(b.Length - trimLength, trimLength);

				_valueLength = readLines;
				return b.ToString();
			}
			else {
				byte previousChar = (byte)' ';

				while (_p < _end && *_p != '\r' && *_p != '\n') {
					if (*_p == '#' && (previousChar == ' ' || previousChar == '\t'))
						break;

					previousChar = *_p;
					_p++;
				}

				byte* end = _p;

				while (end > start && (*(end - 1) == ' ' || *(end - 1) == '\t')) end--;
				_valueLength = 1;
				return _encoding.GetString(start, (int)(end - start));
			}
		}

		public void SkipLine() {
			while (_p < _end && *_p != '\n') _p++;
		}

		public void MoveToIndentEnd(int max = int.MaxValue) {
			while (_p < _end && *_p == ' ' && _lineIndent < max) {
				_lineIndent++;
				_p++;
			}
		}

		public void Trim() {
			while (true) {
				while (_p < _end && (*_p == ' ' || *_p == '\t')) _p++;

				if (*_p == '#') {
					SkipLine();
					continue;
				}

				break;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsLetter(byte b) {
			return (b >= 'a' && b <= 'z' ||
					b >= 'A' && b <= 'Z' ||
					b >= '0' && b <= '9' ||
					b == '_' || b == '-' || b == '\'' || b == '.' || b == '+' || b == '?' || b == '<' || b == '>' || b == '/');
		}

		private void NewLine() {
			_lineNumber++;
			_lineIndent = 0;
			_lastLineStartOffset = _p;
		}

		public void SetupParent(ParserObject parent, int indent) {
			if (parent.Indent == -1) {
				if (_lineIndent < indent)
					throw GetException("Expected list or array declaration.");

				parent.Indent = _lineIndent;
			}

			if (parent.ChildrenIndent == -1) {
				parent.ChildrenIndent = _lineIndent;
			}
		}

		public Exception GetException(string reason) {
			// Attempt to read last line
			string lastLine = "";
			byte* start = _p;

			try {
				_p = _lastLineStartOffset;

				while (_p < _end && *_p != '\n') _p++;

				int length = (int)(_p - start);

				if (length > 0) {
					lastLine = _encoding.GetString(start, length);
				}
			}
			finally {
				_p = start;
			}

			return new Exception("Failed to parse " + _fileName + " at line " + _lineNumber + ", position " + (int)(_p - _lastLineStartOffset) + "\r\n" + (lastLine == "" ? "" : "* " + lastLine + "\r\n" + "Error: " + reason));
		}

		private int _getLength(ParserObject obj) {
			switch (obj.ParserType) {
				case ParserTypes.String:
					return Math.Max(1, obj.Length);
				case ParserTypes.Aggregate:
				case ParserTypes.Null:
				case ParserTypes.Number:
					return 1;
				case ParserTypes.List:
					var list = (ParserList)obj;

					if (list.Objects.Count == 0)
						return 0;

					return list.Last().Line - obj.Line + list.Last().Length;
				case ParserTypes.Array:
					var array = (ParserArray)obj;

					if (array.Objects.Count == 0)
						return 0;

					return array.Last().Line - obj.Line + array.Last().Length;
				case ParserTypes.KeyValue:
					var keyValue = (ParserKeyValue)obj;

					if (keyValue.Value == null)
						return 0;

					return _getLength(keyValue.Value) + keyValue.Line - obj.Line;
			}

			return 0;
		}

		private void _calculateLength(ParserObject obj) {
			switch (obj.ParserType) {
				case ParserTypes.String:
					obj.Length = Math.Max(obj.Length, 1);
					break;
				case ParserTypes.Aggregate:
				case ParserTypes.Null:
				case ParserTypes.Number:
					obj.Length = 1;
					break;
				case ParserTypes.Array:
				case ParserTypes.List:
					foreach (var ele in obj) {
						_calculateLength(ele);
					}

					obj.Length = _getLength(obj);
					break;
				case ParserTypes.KeyValue:
					var value = ((ParserKeyValue)obj).Value;
					_calculateLength(value);
					obj.Length = _getLength(obj);
					break;
			}
		}

		public void Write(string key, string line) {
			if (!_list) {
				ParserKeyValue conf = _writeKeyValuesFind(key);

				if (conf == null) {
					// Add a new one!
					var entry = new ParserKeyValue(key, int.MaxValue) {
						Value = new ParserString(line, int.MaxValue),
						Added = true,
						Length = 1
					};

					_writeKeyValues.Add(entry);
					_indexedWriteKeyValues[key] = entry;
					return;
				}

				conf.Modified = true;
				conf.Value = new ParserString(line, conf.Value.Line);
			}
			else {
				ParserArray conf = _writeArraysFind(key);
				//_writeArrays.FirstOrDefault(p => p[_idKey] == key);

				if (conf == null) {
					var entry = new ParserArray(int.MaxValue) {
						Objects = new List<ParserObject> {
							new ParserKeyValue(_idKey, int.MaxValue) {
								Value = new ParserString(key, int.MaxValue),
							},
							new ParserKeyValue("Content__", int.MaxValue) {
								Value = new ParserString(line, int.MaxValue),
							}
						},
						Added = true,
						Length = 1
					};

					_writeArrays.Add(entry);
					_indexedWriteArrays[key] = entry;
					return;
				}

				conf.Modified = true;
				conf.Objects = new List<ParserObject> {
					new ParserKeyValue(_idKey, int.MaxValue) {
						Value = new ParserString(key, int.MaxValue),
					},
					new ParserKeyValue("Content__", int.MaxValue) {
						Value = new ParserString(line, int.MaxValue),
					}
				};
			}
		}

		private ParserKeyValue _writeKeyValuesFind(string key) {
			if (_indexedWriteKeyValues == null) {
				_indexedWriteKeyValues = new Dictionary<string, ParserKeyValue>();

				foreach (var entry in _writeKeyValues) {
					if (_indexedWriteKeyValues.ContainsKey(entry.Key)) {
						continue;
					}

					_indexedWriteKeyValues[entry.Key] = entry;
				}
			}

			if (key == null)
				return null;

			ParserKeyValue ret;

			if (_indexedWriteKeyValues.TryGetValue(key, out ret))
				return ret;

			return null;
		}

		private ParserArray _writeArraysFind(string key) {
			if (_indexedWriteArrays == null) {
				_indexedWriteArrays = new Dictionary<string, ParserArray>();

				foreach (var entry in _writeArrays) {
					if (_indexedWriteArrays.ContainsKey(entry[_idKey])) {
						continue;
					}

					_indexedWriteArrays[entry[_idKey]] = entry;
				}
			}

			if (key == null)
				return null;

			ParserArray ret;

			if (_indexedWriteArrays.TryGetValue(key, out ret))
				return ret;

			return null;
		}

		public void Remove(BaseDatabase db, Func<int, string> key2string = null) {
			if (db.Table.Commands.GetUndoCommands() == null)
				return;

			if (key2string == null) {
				key2string = v => v.ToString();
			}

			foreach (GroupCommand<int, ReadableTuple> command in db.Table.Commands.GetUndoCommands().OfType<GroupCommand<int, ReadableTuple>>()) {
				foreach (DeleteTuple<int, ReadableTuple> deleteCommand in command.Commands.OfType<DeleteTuple<int, ReadableTuple>>()) {
					Delete(key2string(deleteCommand.Key));
				}

				foreach (ChangeTupleKey<int, ReadableTuple> changeTupleKeyCommand in command.Commands.OfType<ChangeTupleKey<int, ReadableTuple>>()) {
					// If the key was changed, the old key must be removed
					Delete(key2string(changeTupleKeyCommand.Key));
				}
			}

			foreach (ChangeTupleKey<int, ReadableTuple> command in db.Table.Commands.GetUndoCommands().OfType<ChangeTupleKey<int, ReadableTuple>>()) {
				// If the key was changed, the old key must be removed
				Delete(key2string(command.Key));
			}
		}

		public void Delete(string sKey) {
			if (!_list) {
				ParserKeyValue conf = _writeKeyValuesFind(sKey);
				
				if (conf != null) {
					for (int i = 0; i < conf.Length; i++) {
						_allLines[i + conf.Line - 1] = null;
					}
				}
			}
			else {
				ParserArray conf = _writeArraysFind(sKey);

				if (conf != null) {
					for (int i = 0; i < conf.Length; i++) {
						_allLines[i + conf.Line - 1] = null;
					}
				}
			}
		}

		public void WriteFile(string path) {
			// Where to add lines
			int addIndex = _allLines.Count;
			StringBuilder builder = new StringBuilder();

			if (!_list) {
				var last = _writeKeyValues.Where(p => !p.Added).OrderByDescending(p => p.Line).FirstOrDefault();

				if (last != null) {
					addIndex = last.Line + (last.Length <= 0 ? 1 : last.Length) - 1;
				}

				foreach (var confElement in _writeKeyValues.OrderByDescending(p => p.Line)) {
					if (confElement.Added) {
						_allLines.Insert(addIndex, string.Concat("\t", confElement.Key, ": ", confElement.Value));
						continue;
					}

					if (confElement.Modified) {
						for (int i = 0; i < confElement.Length; i++) {
							_allLines[i + confElement.Line - 1] = null;
						}

						_allLines[confElement.Line - 1] = string.Concat("\t", confElement.Key, ": ", confElement.Value);
					}
				}
			}
			else {
				// Set modified lines first
				foreach (var confElement in _writeArrays.Where(p => p.Modified)) {
					for (int i = 0; i < confElement.Length; i++) {
						_allLines[i + confElement.Line - 1] = null;
					}

					_allLines[confElement.Line - 1] = confElement.Objects[1].ObjectValue;
				}

				AlphanumComparer alphaComparer = new AlphanumComparer(StringComparison.OrdinalIgnoreCase);
				_writeArrays = _writeArrays.OrderBy(p => p[_idKey], alphaComparer).ToList();

				// Set added lines in order of their ID
				foreach (var confElement in _writeArrays.Where(p => p.Added).OrderByDescending(p => int.Parse(p[_idKey]))) {
					int lineIndex = _getInsertIndex(confElement);

					// Mark the element as normal so that it can appear in the insert list
					confElement.Added = false;
					confElement.Line = lineIndex;
					confElement.Length = 0;
					_allLines.Insert(lineIndex - 1, confElement.Objects[1].ObjectValue);
				}
			}

			foreach (var line in _allLines) {
				if (line != null)
					builder.AppendLine(line);
			}

			IOHelper.WriteAllText(path, builder.ToString());
		}

		private int _getInsertIndex(ParserArray confElement) {
			if (_writeArrays.Count == 0) {
				return _allLines.Count + 1;
			}

			var last = _writeArrays.LastOrDefault(p => !p.Added);

			if (last == null) {
				return _allLines.Count + 1;
			}

			int lineIndex = last.Line + last.Length;

			int arrayIndex = _writeArrays.IndexOf(confElement);

			arrayIndex++;

			while (arrayIndex < _writeArrays.Count && _writeArrays[arrayIndex].Added) {
				arrayIndex++;
			}

			if (arrayIndex < _writeArrays.Count - 1) {
				return _writeArrays[arrayIndex].Line;
			}

			return lineIndex;
		}
	}
}