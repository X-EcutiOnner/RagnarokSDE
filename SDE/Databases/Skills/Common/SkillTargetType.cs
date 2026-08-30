using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Register(typeof(SkillTargetTypeInfo))]
	public enum SkillTargetType {
		INF_PASSIVE_SKILL = 0x00, // Used just for skill_db parsing
		INF_ATTACK_SKILL = 0x01,
		INF_GROUND_SKILL = 0x02,
		INF_SELF_SKILL = 0x04, // Skills casted on self where target is automatically chosen
							   // 0x08 not assigned
		INF_SUPPORT_SKILL = 0x10,
		INF_TRAP_SKILL = 0x20,
	}

	public static class SkillTargetTypeInfo {
		public const string Marker = "INF_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static SkillTargetTypeInfo() {
			All.Add(new EnumInfoBase(SkillTargetType.INF_PASSIVE_SKILL, "Passive", Marker) { ToolTip = "Passive skill. (Default)" });
			All.Add(new EnumInfoBase(SkillTargetType.INF_ATTACK_SKILL, "Attack", Marker) { ToolTip = "Damage enemies." });
			All.Add(new EnumInfoBase(SkillTargetType.INF_GROUND_SKILL, "Ground", Marker) { ToolTip = "Ground placement skill." });
			All.Add(new EnumInfoBase(SkillTargetType.INF_SELF_SKILL, "Self", Marker) { ToolTip = "Self cast skill." });
			All.Add(new EnumInfoBase(SkillTargetType.INF_SUPPORT_SKILL, "Support", Marker) { ToolTip = "Friendly cast skill." });
			All.Add(new EnumInfoBase(SkillTargetType.INF_TRAP_SKILL, "Trap", Marker) { ToolTip = "Trap cast skill." });

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<SkillTargetType>(All, TypeToInfo, Marker);
		}
	}
}
