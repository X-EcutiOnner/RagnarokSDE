using ErrorManager;
using GRF.IO;
using SDE.Databases.Generic.Common;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using SDE.Editor.Parsers.Yaml;
using SDE.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Utilities;
using Utilities.Extension;

namespace SDE.Databases.Generic.Parser {
	public class DbReader {
		public static bool LoadEnum<T>(ref T value, string text, bool loadMissingValues = true) where T : struct, Enum {
			string prefix = EnumInfos.GetMarker<T>();

			if (EnumInfos.TryStringToEnum(text, prefix, out T r)) {
				value = r;
				return true;
			}
			else {
				if (!loadMissingValues)
					return false;

				// Auto-add missing entries
				value = EnumInfos.AddRaw<T>(text, prefix);
				return true;
			}
		}

		public static bool LoadEnum<T>(ref T value, ParserObject parser, string prefix = "") where T : struct, Enum {
			if (parser == null)
				return false;

			return LoadEnum(ref value, (string)parser);
		}

		public static T LoadEnum<T>(ParserObject parser, T defaultValue) where T : struct, Enum {
			if (parser == null)
				return defaultValue;

			T v = defaultValue;
			LoadEnum(ref v, (string)parser);
			return v;
		}

		public static T LoadEnum<T>(string parser, T defaultValue) where T : struct, Enum {
			if (parser == null)
				return defaultValue;

			T v = defaultValue;
			LoadEnum(ref v, parser);
			return v;
		}

		public static string LoadFlag<T>(ParserObject parser) where T : struct, Enum {
			return LoadFlag<T>(parser, default, default);
		}

		public static string LoadFlag<T>(ParserObject parser, T defaultValue, T allValue) where T : struct, Enum {
			if (parser == null)
				return ((long)(object)defaultValue).ToString();

			// There are no nodes, it's a direct value.
			// Just load it as an enum in that case.
			if (parser is ParserString parserString) {
				return ((long)(object)LoadEnum(parser, defaultValue)).ToString();
			}

			// "All" nodes are treated first
			var values = parser.OfType<ParserKeyValue>().ToList();
			long result = 0;

			var allIndex = values.FindIndex(p => p.Key == "All");
			
			if (allIndex > -1) {
				if (values[allIndex].ObjectValue == "true")
					result |= (long)(object)allValue;
				else
					result &= ~(long)(object)allValue;
			}

			foreach (var value in values) {
				if (value.Key == "All" ||
					value.Key == "Override" ||
					value.Key == "DropEffect" ||
					value.Key == "Amount")
					continue;

				T v = default;

				if (!LoadEnum(ref v, value.Key)) {
					continue;
				}

				if (value.ObjectValue == "true")
					result |= (long)(object)v;
				else
					result &= ~(long)(object)v;
			}

			return result.ToString();
		}

		public static void LevelListToString(ref string field, ParserObject list, string id, string value) {
			field = LevelListToString(list, id, value, field);
		}

		public static string LevelListToString(ParserObject list, string id, string value, string def = "") {
			if (list == null)
				return def;

			string ret = "";

			if (list is ParserKeyValue keyValue)
				list = keyValue.Value;

			if (list is ParserString) {
				ret = list;
			}
			else {
				Dictionary<int, string> ranges = new Dictionary<int, string>();

				try {
					foreach (var entry in list) {
						ranges[Int32.Parse(entry[id])] = entry[value];
					}

					int start = 1;
					string previous = "";

					foreach (var entry in ranges.OrderBy(p => p.Key)) {
						if (entry.Key == start) {
							ret += entry.Value + ":";
							previous = entry.Value;
							start = entry.Key + 1;
							continue;
						}

						for (int i = start; i < entry.Key; i++) {
							ret += previous + ":";
						}

						ret += entry.Value + ":";
						previous = entry.Value;
						start = entry.Key + 1;
					}
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			}

			return ret.TrimEnd(':');
		}

		public static string RemoveQuotes(string value) {
			if (value.Length > 2 && value[0] == '\"' && value[value.Length - 1] == '\"') {
				return value.Substring(1, value.Length - 2);
			}

			return value;
		}

		public static void ParseImports(DbLoadContext context, BaseDatabase db, YamlParser parser, Action<DbLoadContext, BaseDatabase> loader) {
			var imports = parser.Output["Footer.Imports"];

			if (imports == null)
				return;

			string compiledMode = SdeEditor.Project.IsRenewal ? "Renewal" : "Prerenewal";

			string dbPath = ProjectConfiguration.DatabasePath;
			dbPath = GrfPath.GetDirectoryName(dbPath);
			dbPath = GrfPath.GetDirectoryName(dbPath);

			List<string> files = new List<string>();

			foreach (var entry in imports) {
				var path = entry["Path"];
				var mode = entry["Mode"];

				if (mode != null && mode.ObjectValue != compiledMode)
					continue;

				var dbFile = GrfPath.CombineUrl(dbPath, ((string)path).ReplaceAll("/", "\\"));

				if (!File.Exists(dbFile))
					continue;

				files.Add(dbFile);
			}

			db?.Progress?.SplitTier(files.Count);

			for (int i = 0; i < files.Count; i++) {
				string dbFile = files[i];

				try {
					context.FileType = FileType.Yaml;
					context.FilePath = dbFile;
					DbPathLocator.StoreFile(context.FilePath);
					DbDebugHelper.OnLoaded(context.Source, context.FilePath, db);

					db.Attached["Import:" + Path.GetFileNameWithoutExtension(context.FilePath)] = true;

					var storeCompareList = db.Attached["StoreCompare"] as List<string>;

					if (storeCompareList == null) {
						storeCompareList = new List<string>();
						db.Attached["StoreCompare"] = storeCompareList;
					}

					storeCompareList.Add(context.FilePath);
					loader(context, db);
				}
				finally {
					// The last tier will be marked as completed from the main loader in SdeDatabase.Reload
					if (i != files.Count - 1)
						db?.Progress?.CompleteTier();
				}
			}
		}

		public static string YamlReserved = ":[]{}#*|\"%@`";

		public static string YamlString(string value) {
			if (String.IsNullOrEmpty(value) || IsPlainYamlString(value)) {
				return value ?? "";
			}

			if (!value.Contains("\""))
				return "\"" + value + "\"";

			if (!value.Contains("'"))
				return "'" + value + "'";

			return "\"" + value.Replace("\"", "\\\"") + "\"";
		}

		public static bool IsPlainYamlString(string value) {
			// Plain scalars cannot have leading/trailing whitespace.
			if (Char.IsWhiteSpace(value[0]) ||
				Char.IsWhiteSpace(value[value.Length - 1]))
				return false;

			// A plain scalar cannot contain line breaks.
			foreach (char c in value) {
				if (c == '\r' || c == '\n')
					return false;
			}

			// Most YAML indicators are only special at the beginning.
			switch (value[0]) {
				case '-':
				case '?':
				case ':':
				case ',':
				case '[':
				case ']':
				case '{':
				case '}':
				case '#':
				case '&':
				case '*':
				case '!':
				case '|':
				case '>':
				case '\'':
				case '"':
				case '%':
				case '@':
				case '`':
					// - ? : are allowed when followed by a non-space character,
					// but the other indicators aren't.
					if ((value[0] == '-' ||
						 value[0] == '?' ||
						 value[0] == ':') &&
						value.Length > 1 &&
						!Char.IsWhiteSpace(value[1])) {
						break;
					}

					return false;
			}

			// ": " and " #" are never allowed in plain scalars.
			for (int i = 0; i < value.Length - 1; i++) {
				if (value[i] == ':' && Char.IsWhiteSpace(value[i + 1]))
					return false;

				if (value[i] == ' ' && value[i + 1] == '#')
					return false;
			}

			return true;
		}

		public static bool IsNullOrEmpty(string value) {
			return value == null || value == "";
		}

		public static bool IsZero(string value) {
			return value == null || value == "" || value == "0";
		}

		public static bool IsZero(string value, out int intValue) {
			intValue = 0;

			if (value == null || value == "" || value == "0") {
				return true;
			}

			if (Int32.TryParse(value, out intValue)) {
				return intValue == 0;
			}

			return true;
		}

		public static int ToInt(string value) {
			if (value == null || value == "")
				return 0;

			Int32.TryParse(value, out int res);
			return res;
		}

		public static long ToLong(string value) {
			if (value == null || value == "")
				return 0;

			long.TryParse(value, out long res);
			return res;
		}

		public static bool IsExpandString(string value) {
			if (value == null || value == "") {
				return false;
			}

			// If all values are 0, then there's nothing to write
			var data = value.Split(':');
			int intValue;

			foreach (var val in data) {
				if (Int32.TryParse(val, out intValue) && intValue != 0)
					return true;
			}

			return false;
		}

		public static bool ToInt(string value, out int intValue) {
			if (value == null || value == "") {
				intValue = 0;
				return true;
			}

			var d = value.Split(':')[0];
			intValue = FormatConverters.IntOrHexConverter(d);
			return true;
		}

		public static bool ToLong(string value, out long longValue) {
			if (value == null || value == "") {
				longValue = 0;
				return true;
			}

			long.TryParse(value, out longValue);
			return true;
		}

		public static string FromScript(string value) {
			value = value.Trim(' ', '\t');

			if (value.Length >= 2 && value[0] == '{' && value[value.Length - 1] == '}')
				value = value.Substring(1, value.Length - 2);

			return value.Trim(' ', '\t');
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string SetEmptyIfZero(string value) {
			return value == "0" ? "" : value;
		}
	}
}
