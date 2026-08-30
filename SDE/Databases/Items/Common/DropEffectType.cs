using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Items.Common {
	[Register(typeof(DropEffectTypeInfo))]
	public enum DropEffectType {
		DROPEFFECT_NONE,
		DROPEFFECT_CLIENT,
		DROPEFFECT_BLUE_PILLAR,
		DROPEFFECT_YELLOW_PILLAR,
		DROPEFFECT_PURPLE_PILLAR,
		DROPEFFECT_GREEN_PILLAR,
		DROPEFFECT_RED_PILLAR,
		DROPEFFECT_WHITE_PILLAR,
		DROPEFFECT_ORANGE_PILLAR,
	}

	public static class DropEffectTypeInfo {
		public const string Marker = "DROPEFFECT_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static DropEffectTypeInfo() {
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_NONE, "None", Marker));
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_CLIENT, "Client", Marker, "CLIENT"));
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_BLUE_PILLAR, "Blue Pillar", Marker, "BLUE_PILLAR"));
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_YELLOW_PILLAR, "Yellow Pillar", Marker, "YELLOW_PILLAR"));
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_PURPLE_PILLAR, "Purple Pillar", Marker, "PURPLE_PILLAR"));
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_GREEN_PILLAR, "Green Pillar", Marker, "GREEN_PILLAR"));
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_RED_PILLAR, "Red Pillar", Marker, "RED_PILLAR"));
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_WHITE_PILLAR, "White Pillar", Marker, "WHITE_PILLAR"));
			All.Add(new EnumInfoBase(DropEffectType.DROPEFFECT_ORANGE_PILLAR, "Orange Pillar", Marker, "ORANGE_PILLAR"));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<DropEffectType>(All, TypeToInfo, Marker);
		}
	}
}
