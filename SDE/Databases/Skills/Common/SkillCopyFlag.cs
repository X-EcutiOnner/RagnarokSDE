using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(SkillCopyFlagInfo))]
	public enum SkillCopyFlag : Int64 {
		SKILL_COPY_PLAGIARISM = 0x1,
		SKILL_COPY_REPRODUCE = 0x2,
	}

	public static class SkillCopyFlagInfo {
		public const string Marker = "SKILL_COPY_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static SkillCopyFlagInfo() {
			All.Add(new EnumInfoBase(SkillCopyFlag.SKILL_COPY_PLAGIARISM, "Plagiarism", Marker));
			All.Add(new EnumInfoBase(SkillCopyFlag.SKILL_COPY_REPRODUCE, "Reproduce", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<SkillCopyFlag>(All, TypeToInfo, Marker);
		}
	}
}
