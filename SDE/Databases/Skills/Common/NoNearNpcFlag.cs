using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(NoNearNpcFlags))]
	public enum NoNearNpcFlag : Int64 {
		SKILL_NONEAR_WARPPORTAL = 0x1,
		SKILL_NONEAR_SHOP = 0x2,
		SKILL_NONEAR_NPC = 0x4,
		SKILL_NONEAR_TOMB = 0x8,
	}

	public static class NoNearNpcFlags {
		public const string Marker = "SKILL_NONEAR_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static NoNearNpcFlags() {
			All.Add(new EnumInfoBase(NoNearNpcFlag.SKILL_NONEAR_WARPPORTAL, "WarpPortal", Marker));
			All.Add(new EnumInfoBase(NoNearNpcFlag.SKILL_NONEAR_SHOP, "Shop", Marker));
			All.Add(new EnumInfoBase(NoNearNpcFlag.SKILL_NONEAR_NPC, "Npc", Marker));
			All.Add(new EnumInfoBase(NoNearNpcFlag.SKILL_NONEAR_TOMB, "Tomb", Marker));

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<NoNearNpcFlag>(All, TypeToInfo, Marker);
		}
	}
}
