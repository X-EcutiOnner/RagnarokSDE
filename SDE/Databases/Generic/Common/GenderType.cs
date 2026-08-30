using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.Common {
	[Register(typeof(GenderTypeInfo))]
	public enum GenderType {
		SEX_FEMALE = 0,
		SEX_MALE,
		SEX_BOTH,
	}

	public static class GenderTypeInfo {
		public const string Marker = "SEX_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static GenderTypeInfo() {
			All.Add(new EnumInfoBase(GenderType.SEX_FEMALE, "Female", Marker));
			All.Add(new EnumInfoBase(GenderType.SEX_MALE, "Male", Marker));
			All.Add(new EnumInfoBase(GenderType.SEX_BOTH, "Both", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<GenderType>(All, TypeToInfo, Marker);
		}
	}
}
