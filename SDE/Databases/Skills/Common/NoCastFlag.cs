using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(NoCastFlagInfo))]
	public enum NoCastFlag : Int64 {
		NOCAST_NORMAL = 0x1,
		NOCAST_PVP_MAPS = 0x2,
		NOCAST_GVG_MAPS = 0x4,
		NOCAST_BG_MAPS = 0x8,
		NOCAST_WOE_TE_MAPS = 0x10,
		NOCAST_ZONE_1 = 0x20,
		NOCAST_ZONE_2 = 0x40,
		NOCAST_ZONE_3 = 0x80,
		NOCAST_ZONE_4 = 0x100,
		NOCAST_ZONE_5 = 0x200,
		NOCAST_ZONE_6 = 0x400,
		NOCAST_ZONE_7 = 0x800,
		NOCAST_ZONE_8 = 0x1000,
	}

	public static class NoCastFlagInfo {
		public const string Marker = "NOCAST_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static NoCastFlagInfo() {
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_NORMAL, "Normal maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_PVP_MAPS, "PvP maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_GVG_MAPS, "GvG maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_BG_MAPS, "Battleground maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_WOE_TE_MAPS, "WoE:TE maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_ZONE_1, "Zone 1 maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_ZONE_2, "Zone 2 maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_ZONE_3, "Zone 3 maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_ZONE_4, "Zone 4 maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_ZONE_5, "Zone 5 maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_ZONE_6, "Zone 6 maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_ZONE_7, "Zone 7 maps", Marker));
			All.Add(new EnumInfoBase(NoCastFlag.NOCAST_ZONE_8, "Zone 8 maps", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<NoCastFlag>(All, TypeToInfo, Marker);
		}
	}
}
