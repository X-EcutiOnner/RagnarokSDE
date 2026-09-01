using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extension;

namespace SDE.Databases.Generic.Parser {
	public static class DbWriter {
		public enum EnumOutputFormat {
			PascalStyle,
			Lowercase,
			Yaml,
		}

		public static Dictionary<string, string> EnumValueToStringCache = new Dictionary<string, string>();

		public static string EnumValueToString<T>(T value, EnumOutputFormat format = EnumOutputFormat.PascalStyle) where T : struct, Enum {
			string prefix = EnumInfos.GetMarker<T>();
			return EnumValueToString(value, prefix, format);
		}

		public static string EnumValueToString(Enum value, string prefix, EnumOutputFormat format = EnumOutputFormat.PascalStyle) {
			var result = value.ToString();

			if (EnumValueToStringCache.TryGetValue(result, out string ret))
				return ret;

			ret = result.ReplaceFirst(prefix, "");

			if (format == EnumOutputFormat.Lowercase) {
				ret = ret.ToLowerInvariant();
			}
			else {
				StringBuilder r = new StringBuilder();

				bool capital = true;

				for (int i = 0; i < ret.Length; i++) {
					if (char.IsDigit(ret[i])) {
						r.Append(ret[i] - '0');
						capital = true;
					}
					else if (char.IsLetter(ret[i])) {
						if (capital) {
							r.Append(char.ToUpper(ret[i]));
							capital = false;
						}
						else {
							r.Append(char.ToLower(ret[i]));
						}
					}
					else {
						r.Append(ret[i]);
						capital = true;
					}
				}

				ret = r.ToString();
			}

			EnumValueToStringCache[result] = ret;
			return ret;
		}

		public static string ToBool(bool value) {
			return value ? "true" : "false";
		}

		public static string SetZeroDefault(string value) {
			if (String.IsNullOrEmpty(value))
				return "0";

			return value;
		}

		public static string SetEmptyDefault(string value) {
			if (String.IsNullOrEmpty(value))
				return "";

			return value;
		}

		public static string SetTextScript(string script) {
			if (String.IsNullOrEmpty(script))
				return "{}";

			return "{ " + Methods.Aggregate(script.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim(' ', '\t') + " ").ToList(), "").Trim(' ') + " }";
		}

		public static void ExpandFlagToBool<TEnum>(StringBuilder builder, long flagValue, string variableName, string indent) where TEnum : struct, Enum {
			string subIndent = indent + "  ";

			if (variableName != "")
				builder.AppendLine(indent + variableName + ":");

			List<EnumInfoBase> flagInfoValues = EnumInfos.GetEnumInfoList<TEnum>();

			foreach (var flagValueInfo in flagInfoValues) {
				if ((flagValueInfo.ValueLong & flagValue) != 0) {
					builder.Append(subIndent);
					builder.Append(flagValueInfo.YamlName);
					builder.AppendLine(": true");
				}
			}
		}

		public static void ExpandArrayToBool(StringBuilder builder, string value, string variableName, string indent) {
			string[] values = value.Split(':');

			if (values.Length == 0)
				return;

			builder.AppendLine(indent + variableName + ":");
			indent += "  ";

			foreach (var sValue in values) {
				if (sValue == "")
					continue;

				builder.AppendLine(indent + sValue + ": true");
			}
		}

		public static void ExpandLevelList(StringBuilder builder, string value, string variableName, string level, string count, string indent) {
			string[] data = value.Split(':');
			int k = 1;

			if (data.Length == 1) {
				builder.Append(indent);
				builder.Append(variableName);
				builder.Append(": ");
				builder.AppendLine(data[0]);
				return;
			}

			builder.Append(indent);
			builder.Append(variableName);
			builder.AppendLine(":");

			string levelIndent = indent + "  - ";
			string countIndent = indent + "    ";

			foreach (var field in data) {
				if (field == "") {
					k++;
					continue;
				}

				builder.Append(levelIndent);
				builder.Append(level);
				builder.Append(": ");
				builder.AppendLine(k.ToString(CultureInfo.InvariantCulture));
				k++;

				builder.Append(countIndent);
				builder.Append(count);
				builder.Append(": ");
				builder.AppendLine(field);
			}
		}

		public static string ScriptToSingleLineScript(string val) {
			return Methods.Aggregate(val.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim(' ', '\t') + " ").ToList(), "").Trim(' ');
		}

		public static string AutoFormatScript(string val) {
			val = val ?? "";
			StringBuilder builder = new StringBuilder();

			int index = 0;
			int level = 0;
			int parenthesis = 0;
			bool quotation = false;
			bool trim = false;
			int lines = 1;
			bool isNewLine = true;
			bool isCondition = false;

			while (index < val.Length) {
				char c = val[index];

				switch (c) {
					case ';':
						if (!quotation && parenthesis == 0) {
							lines++;
							builder.Append(";\r\n");
							builder.AppendIndent(level);
							trim = true;
							isNewLine = true;
							isCondition = false;
						}
						else {
							builder.Append(c);
						}
						break;
					case '{':
						if (!quotation && parenthesis == 0) {
							lines++;

							if (trim)
								builder.Append(" {\r\n");
							else
								builder.Append("{\r\n");

							level++;
							builder.AppendIndent(level);
							trim = true;
							isNewLine = true;
							isCondition = false;
						}
						else {
							builder.Append(c);
						}
						break;
					case '}':
						if (!quotation && parenthesis == 0) {
							level--;
							lines++;

							if (builder.Length > 0 && builder[builder.Length - 1] == '\t') {
								builder[builder.Length - 1] = '}';
							}
							else {
								builder.Append(c);
							}

							builder.Append('\n');
							builder.AppendIndent(level);
							trim = true;
							isNewLine = true;
							isCondition = false;
						}
						else {
							builder.Append(c);
						}
						break;
					case ' ':
					case '\t':
						if (trim) {
							index++;
							continue;
						}
						builder.Append(c);
						break;
					case '"':
						trim = false;
						quotation = !quotation;
						builder.Append(c);
						break;
					case '(':
						if (!quotation) {
							parenthesis++;
						}

						trim = false;
						builder.Append(c);
						break;
					case ')':
						if (!quotation && parenthesis > 0) {
							parenthesis--;
						}

						if (isCondition && parenthesis == 0)
							trim = true;
						else
							trim = false;

						builder.Append(c);
						break;
					default:
						trim = false;

						if (isNewLine) {
							isNewLine = false;

							if (val.IndexOf("if", index, StringComparison.OrdinalIgnoreCase) == index && index + "if".Length < val.Length) {
								var cc = val[index + "if".Length];

								if (cc == ' ' || cc == '\t' || cc == '(')
									isCondition = true;

								builder.Append("if");
								index += "if".Length;
								continue;
							}
							else if (val.IndexOf("else if", index, StringComparison.OrdinalIgnoreCase) == index) {
								isCondition = true;

								builder.Append("else if");
								index += "else if".Length;
								continue;
							}
							else if (val.IndexOf("else", index, StringComparison.OrdinalIgnoreCase) == index && index + "else".Length < val.Length) {
								var cc = val[index + "else".Length];

								if (cc == ' ' || cc == '\t' || cc == '(')
									isCondition = true;

								builder.Append("else");
								index += "else".Length;
								trim = true;
								continue;
							}
						}

						isNewLine = false;

						if (parenthesis == 0) {
							if (isCondition) {
								lines++;
								builder.Append("\r\n");
								builder.AppendIndent(level + 1);
							}

							isCondition = false;
						}

						builder.Append(c);
						break;
				}

				index++;
			}

			return builder.ToString();
		}

		public static string ToYamlScript(string val, string indent) {
			StringBuilder builder = new StringBuilder();

			val = AutoFormatScript(val);

			var lines = val.Split('\n').Select(p => p.Trim('\r')).Where(p => p != "").ToList();

			if (lines.Count > 0 && lines[0] == ".@i = getpetinfo(PETINFO_INTIMATE);") {
				lines.Insert(1, "");
			}

			for (int i = 0; i < lines.Count; i++) {
				int j;

				for (j = 0; j < lines[i].Length; j++) {
					if (lines[i][j] != '\t')
						break;
				}

				if (j > 0) {
					lines[i] = lines[i].Substring(j);

					for (int k = 0; k < j; k++) {
						//builder.Append("  ");
						builder.Append("   ");
					}
				}

				builder.Append(indent);
				builder.AppendLine(lines[i]);
			}

			return builder.ToString().Trim('\r', '\n');
		}
	}
}
