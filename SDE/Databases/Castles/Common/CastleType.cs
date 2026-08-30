using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Castles.Common {
	[Register(typeof(CastleTypeInfo))]
	public enum CastleType {
		WOE_FIRST_EDITION = 1,
		WOE_SECOND_EDITION,
		WOE_THIRD_EDITION,
		WOE_MAX
	}

	public static class CastleTypeInfo {
		public const string Marker = "WOE_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static CastleTypeInfo() {
			All.Add(new EnumInfoBase(CastleType.WOE_FIRST_EDITION, "First Edition", Marker));
			All.Add(new EnumInfoBase(CastleType.WOE_SECOND_EDITION, "Second Edition", Marker));
			All.Add(new EnumInfoBase(CastleType.WOE_THIRD_EDITION, "Third Edition", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<CastleType>(All, TypeToInfo, Marker);
		}
	}
}
