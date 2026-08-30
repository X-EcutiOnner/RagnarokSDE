using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(DamageFlagInfo))]
	public enum DamageFlag : Int64 {
		NK_NODAMAGE = 1L << 0,
		NK_SPLASH = 1L << 1,
		NK_SPLASHSPLIT = 1L << 2,
		NK_IGNOREATKCARD = 1L << 3,
		NK_IGNOREELEMENT = 1L << 4,
		NK_IGNOREDEFENSE = 1L << 5,
		NK_IGNOREFLEE = 1L << 6,
		NK_IGNOREDEFCARD = 1L << 7,
		NK_CRITICAL = 1L << 8,
		NK_IGNORELONGCARD = 1L << 9,
		NK_SIMPLEDEFENSE = 1L << 10,
		NK_MAX = 1L << 11,
	}

	public static class DamageFlagInfo {
		public const string Marker = "NK_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static DamageFlagInfo() {
			All.Add(new EnumInfoBase(DamageFlag.NK_NODAMAGE, "NoDamage", Marker) { ToolTip = "No damage skill." });
			All.Add(new EnumInfoBase(DamageFlag.NK_SPLASH, "Splash", Marker) { ToolTip = "Has splash area" });
			All.Add(new EnumInfoBase(DamageFlag.NK_SPLASHSPLIT, "SplashSplit", Marker) { ToolTip = "Damage should be split among targets" });
			All.Add(new EnumInfoBase(DamageFlag.NK_IGNOREATKCARD, "IgnoreAtkCard", Marker) { ToolTip = "Skill ignores caster's % damage cards (misc type always ignores)" });
			All.Add(new EnumInfoBase(DamageFlag.NK_IGNOREELEMENT, "IgnoreElement", Marker) { ToolTip = "Skill ignores elemental adjustments." });
			All.Add(new EnumInfoBase(DamageFlag.NK_IGNOREDEFENSE, "IgnoreDefense", Marker) { ToolTip = "Skill ignores target's defense (misc type always ignores)." });
			All.Add(new EnumInfoBase(DamageFlag.NK_IGNOREFLEE, "IgnoreFlee", Marker) { ToolTip = "Skill ignores target's flee (magic type always ignores)." });
			All.Add(new EnumInfoBase(DamageFlag.NK_IGNOREDEFCARD, "IgnoreDefCard", Marker) { ToolTip = "Skill ignores target's def cards." });
			All.Add(new EnumInfoBase(DamageFlag.NK_CRITICAL, "Critical", Marker) { ToolTip = "Skill can crit." });
			All.Add(new EnumInfoBase(DamageFlag.NK_IGNORELONGCARD, "IgnoreLongCard", Marker) { ToolTip = "Ignore long range card effects." });
			All.Add(new EnumInfoBase(DamageFlag.NK_SIMPLEDEFENSE, "SimpleDefense", Marker) { ToolTip = "(Renewal-only) Physical damage is flatly reduced by DEF+DEF2. RES is ignored." });

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<DamageFlag>(All, TypeToInfo, Marker);
		}
	}
}
