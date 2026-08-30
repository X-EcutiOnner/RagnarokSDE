using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.Common {
	[Register(typeof(ElementLevelTypeInfo))]
	public enum ElementLevelType {
		ELELV_1 = 1,
		ELELV_2 = 2,
		ELELV_3 = 3,
		ELELV_4 = 4,
	}

	public static class ElementLevelTypeInfo {
		public const string Marker = "ELELV_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ElementLevelTypeInfo() {
			All.Add(new EnumInfoBase(ElementLevelType.ELELV_1, "1", Marker));
			All.Add(new EnumInfoBase(ElementLevelType.ELELV_2, "2", Marker));
			All.Add(new EnumInfoBase(ElementLevelType.ELELV_3, "3", Marker));
			All.Add(new EnumInfoBase(ElementLevelType.ELELV_4, "4", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ElementLevelType>(All, TypeToInfo, Marker);
		}
	}
}
