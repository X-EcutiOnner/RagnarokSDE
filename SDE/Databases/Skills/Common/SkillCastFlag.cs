using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(SkillCastFlagInfo))]
	public enum SkillCastFlag : Int64 {
		SKILL_CAST_IGNOREDEX = 0x1,
		SKILL_CAST_IGNORESTATUS = 0x2,
		SKILL_CAST_IGNOREITEMBONUS = 0x4,
	}

	public static class SkillCastFlagInfo {
		public const string Marker = "SKILL_CAST_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static SkillCastFlagInfo() {
			All.Add(new EnumInfoBase(SkillCastFlag.SKILL_CAST_IGNOREDEX, "IgnoreDex", Marker) { ToolTip = "Not affected by dex." });
			All.Add(new EnumInfoBase(SkillCastFlag.SKILL_CAST_IGNORESTATUS, "IgnoreStatus", Marker) { ToolTip = "Not affected by statuses (Suffragium, etc)." });
			All.Add(new EnumInfoBase(SkillCastFlag.SKILL_CAST_IGNOREITEMBONUS, "IgnoreItemBonus", Marker) { ToolTip = "Not affected by item bonuses (equips, cards)." });

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<SkillCastFlag>(All, TypeToInfo, Marker);
		}
	}
}
