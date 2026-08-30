using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(SkillCopyJobAllowedFlagInfo))]
	public enum SkillCopyJobAllowedFlag : Int64 {
		SKJA_ROGUE = 0x1,
		SKJA_STALKER = 0x2,
		SKJA_SHADOW_CHASER = 0x4,
		SKJA_TRANS_SHADOW_CHASER = 0x8,
		SKJA_BABY_ROGUE = 0x10,
		SKJA_BABY_SHADOW_CHASER = 0x20,

		SKJA_ALL = SKJA_ROGUE | SKJA_STALKER | SKJA_SHADOW_CHASER | SKJA_TRANS_SHADOW_CHASER | SKJA_BABY_ROGUE | SKJA_BABY_SHADOW_CHASER,
	}

	public static class SkillCopyJobAllowedFlagInfo {
		public const string Marker = "SKJA_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static SkillCopyJobAllowedFlagInfo() {
			All.Add(new EnumInfoBase(SkillCopyJobAllowedFlag.SKJA_ROGUE, "Rogue", Marker));
			All.Add(new EnumInfoBase(SkillCopyJobAllowedFlag.SKJA_STALKER, "Stalker", Marker));
			All.Add(new EnumInfoBase(SkillCopyJobAllowedFlag.SKJA_SHADOW_CHASER, "Shadow Chaser", Marker));
			All.Add(new EnumInfoBase(SkillCopyJobAllowedFlag.SKJA_TRANS_SHADOW_CHASER, "Trans Shadow Chaser", Marker));
			All.Add(new EnumInfoBase(SkillCopyJobAllowedFlag.SKJA_BABY_ROGUE, "Baby Rogue", Marker));
			All.Add(new EnumInfoBase(SkillCopyJobAllowedFlag.SKJA_BABY_SHADOW_CHASER, "Baby Shadow Chaser", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<SkillCopyJobAllowedFlag>(All, TypeToInfo, Marker);
		}
	}
}
