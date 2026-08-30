using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(SkillUnitFlags))]
	public enum SkillUnitFlag : Int64 {
		UF_NONE = 0,
		UF_NOENEMY = 1L << 1, // If 'defunit_not_enemy' is set, the target is changed to 'friend'
		UF_NOREITERATION = 1L << 2,   // Spell cannot be stacked
		UF_NOFOOTSET = 1L << 3,   // Spell cannot be cast near/on targets
		UF_NOOVERLAP = 1L << 4,   // Spell effects do not overlap
		UF_PATHCHECK = 1L << 5,   // Only cells with a shootable path will be placed
		UF_NOPC = 1L << 6,    // May not target players
		UF_NOMOB = 1L << 7,   // May not target mobs
		UF_SKILL = 1L << 8,   // May target skills
		UF_DANCE = 1L << 9,   // Dance
		UF_ENSEMBLE = 1L << 10,    // Duet
		UF_SONG = 1L << 11,    // Song
		UF_DUALMODE = 1L << 12,    // Spells should trigger both ontimer and onplace/onout/onleft effects.
		UF_NOKNOCKBACK = 1L << 13, // Skill unit cannot be knocked back
		UF_RANGEDSINGLEUNIT = 1L << 14,    // hack for ranged layout, only display center
		UF_CRAZYWEEDIMMUNE = 1L << 15, // Immune to Crazy Weed removal
		UF_REMOVEDBYFIRERAIN = 1L << 16,   // removed by Fire Rain
		UF_KNOCKBACKGROUP = 1L << 17,  // knockback skill unit with its group instead of single unit
		UF_HIDDENTRAP = 1L << 18,  // Hidden trap [Cydh]
		UF_MAX = 1L << 19,
	}

	public static class SkillUnitFlags {
		public const string Marker = "UF_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static SkillUnitFlags() {
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_NONE, "None", Marker) { Visible = false, ToolTip = "No flags." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_NOENEMY, "NoEnemy", Marker) { ToolTip = "If battle_config::defunit_not_enemy is enabled, the Target is changed to Friend." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_NOREITERATION, "NoReiteration", Marker) { ToolTip = "Spell cannot be stacked." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_NOFOOTSET, "NoFootSet", Marker) { ToolTip = "Spell cannot be cast near/on targets." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_NOOVERLAP, "NoOverlap", Marker) { ToolTip = "Spell effects do not overlap." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_PATHCHECK, "PathCheck", Marker) { ToolTip = "Only cells in a shootable path will be placed. If not set, effects apply through walls for ranged units." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_NOPC, "NoPc", Marker) { ToolTip = "Spell cannot affect players." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_NOMOB, "NoMob", Marker) { ToolTip = "Spell cannot affect mobs." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_SKILL, "Skill", Marker) { ToolTip = "Spell can affect skills." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_DANCE, "Dance", Marker) { ToolTip = "Dance unit." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_ENSEMBLE, "Ensemble", Marker) { ToolTip = "Duet unit." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_SONG, "Song", Marker) { ToolTip = "Song unit." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_DUALMODE, "DualMode", Marker) { ToolTip = "Spell has effects both at an interval and when you step in/out." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_NOKNOCKBACK, "NoKnockback", Marker) { ToolTip = "Cannot be knocked back (only unit that can be damaged)." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_RANGEDSINGLEUNIT, "RangedSingleUnit", Marker) { ToolTip = "Layout hack, use layout range property but only display center." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_CRAZYWEEDIMMUNE, "CrazyWeedImmune", Marker) { ToolTip = "Immune to GN_CRAZYWEED." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_REMOVEDBYFIRERAIN, "RemovedByFireRain", Marker) { ToolTip = "Removed by RL_FIRE_RAIN." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_KNOCKBACKGROUP, "KnockbackGroup", Marker) { ToolTip = "Knock back a whole skill group (by default, skill unit is knocked back by each unit)." });
			All.Add(new EnumInfoBase(SkillUnitFlag.UF_HIDDENTRAP, "HiddenTrap", Marker) { ToolTip = "Hidden trap. See battle_config::traps_setting to enable this flag." });

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<SkillUnitFlag>(All, TypeToInfo, Marker);
		}
	}
}
