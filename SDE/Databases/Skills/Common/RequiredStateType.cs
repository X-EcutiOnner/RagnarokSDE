using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Register(typeof(RequiredStateTypeInfo))]
	public enum RequiredStateType {
		ST_NONE,
		ST_HIDDEN,
		ST_RIDING,
		ST_FALCON,
		ST_CART,
		ST_SHIELD,
		ST_RECOVER_WEIGHT_RATE,
		ST_MOVE_ENABLE,
		ST_WATER,
		ST_RIDINGDRAGON,
		ST_WUG,
		ST_RIDINGWUG,
		ST_MADO,
		ST_ELEMENTALSPIRIT,
		ST_ELEMENTALSPIRIT2,
		ST_PECO,
		ST_SUNSTANCE,
		ST_MOONSTANCE,
		ST_STARSTANCE,
		ST_UNIVERSESTANCE
	}

	public static class RequiredStateTypeInfo {
		public const string Marker = "ST_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static RequiredStateTypeInfo() {
			All.Add(new EnumInfoBase(RequiredStateType.ST_NONE, "None", Marker) { ToolTip = "None (Nothing special)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_HIDDEN, "Hidden", Marker) { ToolTip = "Hidden (Requires to be hidden)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_RIDING, "Riding", Marker) { ToolTip = "Riding (Requires a mount)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_FALCON, "Falcon", Marker) { ToolTip = "Falcon (Requires a Falcon)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_CART, "Cart", Marker) { ToolTip = "Cart (Requires a Pushcart)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_SHIELD, "Shield", Marker) { ToolTip = "Shield (Requires a shield equipped)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_RECOVER_WEIGHT_RATE, "RecoverWeightRate", Marker) { ToolTip = "Recover Weight Rate (Requires to be less than 70% weight)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_MOVE_ENABLE, "MoveEnable", Marker) { ToolTip = "Move enable (Requires to be able to move)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_WATER, "Water", Marker) { ToolTip = "Water (Requires to be standing on a water cell)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_RIDINGDRAGON, "RidingDragon", Marker) { ToolTip = "Riding Dragon (Requires to ride a Warg)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_WUG, "Warg", Marker) { ToolTip = "Warg (Requires a Warg)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_RIDINGWUG, "Ridingwarg", Marker) { ToolTip = "Dragon Warg (Requires to ride a Dragon)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_MADO, "Mado", Marker) { ToolTip = "Mado (Requires to have an active mado)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_ELEMENTALSPIRIT, "Elementalspirit", Marker) { ToolTip = "Elemental Spirit (Requires to have an Elemental Spirit summoned)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_ELEMENTALSPIRIT2, "Elementalspirit2", Marker) { ToolTip = "Elemental Spirit2" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_PECO, "RidingPeco", Marker) { ToolTip = "Dragon Warg (Requires to ride a Peco)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_SUNSTANCE, "SunStance", Marker) { ToolTip = "Sun Stance (Requires Sun Stance active)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_MOONSTANCE, "MoonStance", Marker) { ToolTip = "Moon Stance (Requires Moon Stance active)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_STARSTANCE, "StarsStance", Marker) { ToolTip = "Stars Stance (Requires Stars Stance active)" });
			All.Add(new EnumInfoBase(RequiredStateType.ST_UNIVERSESTANCE, "UniverseStance", Marker) { ToolTip = "Universe Stance (Requires Stars Stance active)" });

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<RequiredStateType>(All, TypeToInfo, Marker);
		}
	}
}
