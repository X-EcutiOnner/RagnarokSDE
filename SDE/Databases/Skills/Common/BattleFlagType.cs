using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Register(typeof(BattleFlagTypeInfo))]
	public enum BattleFlagType {
		BF_NONE = 0x0000, /// None
		BF_WEAPON = 0x0001, /// Weapon attack
		BF_MAGIC = 0x0002, /// Magic attack
		BF_MISC = 0x0004, /// Misc attack
	}

	public static class BattleFlagTypeInfo {
		public const string Marker = "BF_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static BattleFlagTypeInfo() {
			All.Add(new EnumInfoBase(BattleFlagType.BF_NONE, "None", Marker));
			All.Add(new EnumInfoBase(BattleFlagType.BF_WEAPON, "Weapon", Marker));
			All.Add(new EnumInfoBase(BattleFlagType.BF_MAGIC, "Magic", Marker));
			All.Add(new EnumInfoBase(BattleFlagType.BF_MISC, "Misc", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<BattleFlagType>(All, TypeToInfo, Marker);
		}
	}
}
