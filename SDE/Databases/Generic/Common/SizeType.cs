using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.Common {
	[Register(typeof(SizeTypeInfo))]
	public enum SizeType {
		SZ_SMALL = 0,
		SZ_MEDIUM,
		SZ_BIG,
		SZ_ALL,
		SZ_MAX,

		Size_Small = SZ_SMALL,
		Size_Medium = SZ_MEDIUM,
		Size_Large = SZ_BIG,
		Size_All = SZ_ALL,
	}

	public static class SizeTypeInfo {
		public const string Marker = "Size_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static SizeTypeInfo() {
			All.Add(new EnumInfoBase(SizeType.Size_Small, "Small", Marker));
			All.Add(new EnumInfoBase(SizeType.Size_Medium, "Medium", Marker));
			All.Add(new EnumInfoBase(SizeType.Size_Large, "Large", Marker));
			All.Add(new EnumInfoBase(SizeType.Size_All, "All", Marker) { Visible = false });

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<SizeType>(All, TypeToInfo, Marker);
		}
	}
}
