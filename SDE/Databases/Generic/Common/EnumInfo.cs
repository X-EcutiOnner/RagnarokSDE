using SDE.Databases.Generic.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDE.Databases.Generic.Common {
	public sealed class EnumInfoBase {
		public Enum Value;
		public long ValueLong;
		public string EnumName;
		public string DisplayName;
		public string PascalName;
		public string YamlName;
		public string ToolTip;
		public string FlagDisplay;
		public bool Visible = true;

		public EnumInfoBase(Enum type, string displayName, string prefix, string yamlName = null, bool visible = true) {
			Value = type;
			ValueLong = Convert.ToInt64(type);
			DisplayName = displayName;

			if (type != null)
				EnumName = type.ToString();

			if (yamlName == null)
				YamlName = DbWriter.EnumValueToString(type, prefix);
			else
				YamlName = yamlName;

			if (DisplayName.Contains(" ")) {
				StringBuilder b = new StringBuilder();
				bool capital = true;

				for (int i = 0; i < DisplayName.Length; i++) {
					var c = DisplayName[i];

					switch (c) {
						case ' ':
						case '_':
							capital = true;
							continue;
						default:
							if (char.IsDigit(c))
								capital = true;
							break;
					}

					if (capital) {
						b.Append(char.ToUpperInvariant(c));
						capital = false;
					}
					else {
						b.Append(char.ToLowerInvariant(c));
					}
				}

				PascalName = b.ToString();
			}
			else {
				PascalName = DisplayName;
			}

			Visible = visible;
		}

		public override string ToString() {
			return DisplayName;
		}
	}

	public class EnumInfo {
		public Type Type;
		public List<EnumInfoBase> ListData;
		public Dictionary<Enum, EnumInfoBase> TypeToInfo;
		private Dictionary<string, EnumInfoBase> _yamlToEnumDict;
		public string Marker;
		public long LastValue;
		public bool IsFlag;
		public bool Dirty = true;

		public List<EnumInfoBase> GetList() {
			return ListData;
		}

		public Dictionary<Enum, EnumInfoBase> GetTypeToInfo() {
			return TypeToInfo;
		}

		public Dictionary<string, EnumInfoBase> GetYamlToEnumDictionary() {
			if (Dirty) {
				_yamlToEnumDict = new Dictionary<string, EnumInfoBase>(StringComparer.OrdinalIgnoreCase);

				foreach (var enumBase in ListData) {
					_yamlToEnumDict[enumBase.YamlName] = enumBase;

					string standardName = enumBase.Value.ToString();
					_yamlToEnumDict[standardName] = enumBase;
				}

				Dirty = false;
			}
			
			return _yamlToEnumDict;
		}
	}

	public static class EnumInfos {
		public static Dictionary<Type, EnumInfo> Enums = new Dictionary<Type, EnumInfo>();
		private static HashSet<Type> _enumTypes = new HashSet<Type>();

		public static void AddUInt64<T>(List<EnumInfoBase> data, Dictionary<Enum, EnumInfoBase> typeToInfo, string marker) {
			var enumType = typeof(T);
			_ensureInstantiated(enumType);

			bool isFlag = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false);

			Enums[enumType] = new EnumInfo() { ListData = data, Marker = marker, TypeToInfo = typeToInfo, Type = enumType, IsFlag = isFlag, LastValue = (long)data.Max(p => (UInt64)(object)p.Value) };
		}

		public static void Add<T>(List<EnumInfoBase> data, Dictionary<Enum, EnumInfoBase> typeToInfo, string marker) {
			var enumType = typeof(T);
			_ensureInstantiated(enumType);

			bool isFlag = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false);

			Enums[enumType] = new EnumInfo() { ListData = data, Marker = marker, TypeToInfo = typeToInfo, Type = enumType, IsFlag = isFlag, LastValue = data.Max(p => isFlag ? (long)(object)p.Value : (int)(object)p.Value) };
		}

		public static bool TryStringToEnum(Type enumType, string yaml, string prefix, out Enum r) {
			if (yaml == null) {
				r = default;
				return false;
			}

			_ensureInstantiated(enumType);

			var enumInfo = Enums[enumType];
			var dico = enumInfo.GetYamlToEnumDictionary();

			EnumInfoBase result;

			if (dico.TryGetValue(prefix + yaml, out result)) {
				r = result.Value;
				return true;
			}

			if (dico.TryGetValue(yaml, out result)) {
				r = result.Value;
				return true;
			}

			r = default;
			return false;
		}

		public static bool TryStringToEnum<T>(string yaml, string prefix, out T r) where T : struct, Enum {
			if (yaml == null) {
				r = default;
				return false;
			}

			if (Enum.TryParse(prefix + yaml, true, out T r1)) {
				r = r1;
				return true;
			}

			var enumType = typeof(T);
			_ensureInstantiated(enumType);

			var enumInfo = Enums[enumType];
			var dico = enumInfo.GetYamlToEnumDictionary();

			EnumInfoBase result;

			if (dico.TryGetValue(prefix + yaml, out result)) {
				r = (T)(object)result.Value;
				return true;
			}

			if (dico.TryGetValue(yaml, out result)) {
				r = (T)(object)result.Value;
				return true;
			}

			r = default;
			return false;
		}

		private static void _ensureInstantiated(Type enumType) {
			if (!_enumTypes.Contains(enumType)) {
				var attr = (RegisterAttribute)Attribute.GetCustomAttribute(enumType, typeof(RegisterAttribute));

				if (attr != null) {
					System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(attr.TargetClass.TypeHandle);
				}

				_enumTypes.Add(enumType);
			}
		}

		public static List<EnumInfoBase> GetEnumInfoList<T>() where T : struct, Enum {
			return GetEnumInfoList(typeof(T));
		}

		public static List<EnumInfoBase> GetEnumInfoList(Type enumType) {
			_ensureInstantiated(enumType);

			return Enums[enumType].GetList();
		}

		public static Dictionary<Enum, EnumInfoBase> GetEnumTypeToInfo<T>() where T : struct, Enum {
			return GetEnumTypeToInfo(typeof(T));
		}

		public static EnumInfoBase GetEnumBase<T>(T? value, T defValue) where T : struct, Enum {
			if (value == null)
				return GetEnumTypeToInfo(typeof(T))[defValue];

			return GetEnumTypeToInfo(typeof(T))[value.Value];
		}

		public static EnumInfoBase GetEnumBase<T>(T? value) where T : struct, Enum {
			if (value == null)
				return null;

			return GetEnumTypeToInfo(typeof(T))[value.Value];
		}

		public static Dictionary<Enum, EnumInfoBase> GetEnumTypeToInfo(Type enumType) {
			_ensureInstantiated(enumType);

			return Enums[enumType].GetTypeToInfo();
		}

		public static string GetMarker<T>() where T : struct, Enum {
			return GetMarker(typeof(T));
		}

		public static string GetMarker(Type enumType) {
			_ensureInstantiated(enumType);

			return Enums[enumType].Marker;
		}

		public static bool Exists(Type enumType) {
			_ensureInstantiated(enumType);

			return Enums.ContainsKey(enumType);
		}

		public static bool IsFlag(Type enumType) {
			_ensureInstantiated(enumType);

			return Enums[enumType].IsFlag;
		}

		public static T AddRaw<T>(string name, string prefix) where T : struct, Enum {
			var enumType = typeof(T);
			_ensureInstantiated(enumType);

			var data = Enums[enumType];
			var value = data.LastValue;
			EnumInfoBase enumInfo;

			if (data.IsFlag) {
				data.LastValue *= 2;
				enumInfo = new EnumInfoBase((T)(object)data.LastValue, name, prefix) { EnumName = name, YamlName = name };
			}
			else {
				data.LastValue++;
				enumInfo = new EnumInfoBase((T)(object)(int)data.LastValue, name, prefix) { EnumName = name, YamlName = name };
			}

			data.ListData.Add(enumInfo);
			data.TypeToInfo[enumInfo.Value] = enumInfo;
			data.Dirty = true;

			if (data.IsFlag)
				return (T)(object)data.LastValue;
			else
				return (T)(object)(int)data.LastValue;
		}

		public static string ToYamlString<T>(T key) where T : struct, Enum {
			var values = GetEnumTypeToInfo<T>();
			return values[key].YamlName;
		}
	}
}
