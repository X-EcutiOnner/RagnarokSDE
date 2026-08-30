using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database;
using Database.Commands;
using SDE.Editor.Database;
using SDE.Editor.Files;

namespace SDE.Editor.Writers {
	/// <summary>
	/// This is a stream writer and reader. It's used to preserve the format of a file.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CsvWriter {
		protected List<string> _sourceFileLines;
		private DbAttribute _fileKeyRef;
		protected List<int> _intIds = new List<int>();
		protected HashSet<int> _indIdsIndex = new HashSet<int>();
		protected List<string> _stringIds = new List<string>();
		protected string _newLine;
		protected char _separator;
		protected int _keyIndex = 0;
		private bool _useUniqueId;
		private KeyType _keyType;
		private int _rndOffset;

		public Func<string, int> Key2Id;
		public Func<int, string> Id2Key;

		private enum KeyType {
			Int,
			String,
		}

		public CsvWriter(string path, bool useUniqueId = false, DbAttribute fileKeyRef = null) {
			_useUniqueId = useUniqueId;
			_keyType = _useUniqueId ? KeyType.String : KeyType.Int;
			_separator = ',';
			_sourceFileLines = LineStreamReader.ReadAllLines(path, out _newLine).ToList();
			_fileKeyRef = fileKeyRef;
			_init();
		}

		public void Write(int key, string line) {
			int? index = _intIds.IndexOf(key);

			if (index > -1) {
				_sourceFileLines[index.Value] = line;

				// Remove other references, sometimes csv files write multiple ones of the same entry
				for (int i = 0; i < _intIds.Count; i++) {
					if (i != index.Value && _intIds[i] == key) {
						_sourceFileLines[i] = null;
					}
				}
			}
			else {
				index = _intIds.FirstOrDefault(p => p > -1 && p > key);

				if (index > -1) {
					index = _intIds.IndexOf(index.Value);
				}

				if (_intIds.All(p => key > p)) {
					index = _intIds.Count;
				}

				if (index < 0)
					index = 0;

				_sourceFileLines.Insert(index.Value, line);
				_intIds.Insert(index.Value, key);
			}
		}

		public void Write(string key, string line) {
			int index = _stringIds.IndexOf(key);

			if (index > -1) {
				_sourceFileLines[index] = line;
				return;
			}
			else {
				var intKey = Core.Extensions.SafeAtoi(key);
				index = 0;

				if (intKey != 0) {
					for (; index < _intIds.Count; index++) {
						int existing = _intIds[index];

						if (existing != -1 && existing >= intKey) {
							break;
						}
					}
				}

				for (; index < _stringIds.Count; index++) {
					string existing = _stringIds[index];

					if (existing != null && StringComparer.OrdinalIgnoreCase.Compare(existing, key) > 0) {
						break;
					}
				}

				_sourceFileLines.Insert(index, line);
				_stringIds.Insert(index, key);
			}
		}

		protected void _init() {
			string current;
			HashSet<string> keys = new HashSet<string>();

			for (int i = 0; i < _sourceFileLines.Count; i++) {
				current = _sourceFileLines[i];

				if ((current.Length >= 2 && current[0] == '/' && current[1] == '/') || String.IsNullOrEmpty(current)) {
					_intIds.Add(-1);
					
					if (_keyType == KeyType.String)
						_stringIds.Add(null);
				}
				else {
					// Using StringSplitOptions.RemoveEmptyEntries would remove useful info
					string[] elements = current.Split(new char[] { _separator });

					if (elements.Length > _keyIndex) {
						if (elements[_keyIndex].IndexOf("//", StringComparison.Ordinal) > -1) {
							elements[_keyIndex] = elements[_keyIndex].Substring(0, elements[_keyIndex].IndexOf("//", StringComparison.Ordinal));
						}

						if (_keyType == KeyType.Int) {
							if (Int32.TryParse(elements[_keyIndex], out int val)) {
								_intIds.Add(val);
								_indIdsIndex.Add(val);
							}
						}
						else {
							var lastKey = current;

							while (_stringIds.Contains(lastKey)) {
								lastKey = lastKey + "_" + _rndOffset++;
							}

							if (Int32.TryParse(elements[0], out int val)) {
								_intIds.Add(val);
							}
							else {
								_intIds.Add(-1);
							}

							_stringIds.Add(lastKey);
						}
					}
					else {
						_intIds.Add(-1);
						
						if (_keyType == KeyType.String)
							_stringIds.Add(null);
					}
				}
			}
		}

		public void WriteFile(string path) {
			StringBuilder builder = new StringBuilder();
			string[] array = ToArray();

			for (int index = 0; index < array.Length; index++) {
				string line = array[index];
				builder.Append(line);
				builder.Append(_newLine);
			}

			IOHelper.WriteAllText(path, builder.ToString());
		}

		public virtual void Remove(BaseDatabase db) {
			if (db.Table.Commands.GetUndoCommands() == null)
				return;

			foreach (GroupCommand<int, ReadableTuple> commandGroup in db.Table.Commands.GetUndoCommands().OfType<GroupCommand<int, ReadableTuple>>()) {
				foreach (DeleteTuple<int, ReadableTuple> command in commandGroup.Commands.OfType<DeleteTuple<int, ReadableTuple>>()) {
					Delete(command.Key, command.Tuple);
				}

				foreach (ChangeTupleKey<int, ReadableTuple> command in commandGroup.Commands.OfType<ChangeTupleKey<int, ReadableTuple>>()) {
					// If the key was changed, the old key must be removed
					Delete(command.Key, command.Tuple);
				}

				foreach (ChangeTupleProperty<int, ReadableTuple> command in commandGroup.Commands.OfType<ChangeTupleProperty<int, ReadableTuple>>()) {
					if (command.Attribute.Index == 0)
						Delete(command.Key, command.Tuple);
				}
			}

			foreach (ChangeTupleKey<int, ReadableTuple> command in db.Table.Commands.GetUndoCommands().OfType<ChangeTupleKey<int, ReadableTuple>>()) {
				// If the key was changed, the old key must be removed
				Delete(command.Key, command.Tuple);
			}

			foreach (ChangeTupleProperty<int, ReadableTuple> command in db.Table.Commands.GetUndoCommands().OfType<ChangeTupleProperty<int, ReadableTuple>>()) {
				if (command.Attribute.Index == 0)
					Delete(command.Key, command.Tuple);
			}
		}

		public void Delete(int key) {
			if (!_indIdsIndex.Contains(key))
				return;

			int index = _intIds.IndexOf(key);

			while (index > -1) {
				_intIds.RemoveAt(index);
				_sourceFileLines.RemoveAt(index);
				index = _intIds.IndexOf(key);
			}
		}

		public void Delete(int key, ReadableTuple tuple) {
			if (_keyType == KeyType.Int) {
				int index = _intIds.IndexOf(key);

				while (index > -1) {
					_intIds.RemoveAt(index);
					_sourceFileLines.RemoveAt(index);
					index = _intIds.IndexOf(key);
				}
			}
			else {
				string stringKey = tuple.GetValue<string>(_fileKeyRef);

				if (stringKey != null) {
					int index = _stringIds.IndexOf(stringKey);

					while (index > -1) {
						_stringIds.RemoveAt(index);
						_sourceFileLines.RemoveAt(index);
						index = _stringIds.IndexOf(stringKey);
					}
				}
			}
		}

		public void Append(string line) {
			_sourceFileLines.Add(line);
		}

		public string[] ToArray() {
			return _sourceFileLines.ToArray();
		}

		public void ClearAfterComments() {
			int indexStop = 0;
			for (int i = 0; i < _sourceFileLines.Count; i++) {
				if (String.IsNullOrEmpty(_sourceFileLines[i]) || (_sourceFileLines[i].Length >= 2 && _sourceFileLines[i][0] == '/' && _sourceFileLines[i][1] == '/'))
					continue;

				indexStop = i;
				break;
			}

			if (indexStop > -1) {
				_sourceFileLines = _sourceFileLines.Take(indexStop).ToList();
				_intIds = _intIds.Take(indexStop).ToList();
			}
		}
	}
}