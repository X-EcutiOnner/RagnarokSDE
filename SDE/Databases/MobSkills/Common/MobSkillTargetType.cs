using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.MobSkills.Common {
	[Register(typeof(MobSkillTargetTypeInfo))]
	public enum MobSkillTargetType {
		MST_TARGET = 0,
		MST_RANDOM, //Random Target!
		MST_SELF,
		MST_FRIEND,
		MST_MASTER,
		MST_AROUND5,
		MST_AROUND6,
		MST_AROUND7,
		MST_AROUND8,
		MST_AROUND1,
		MST_AROUND2,
		MST_AROUND3,
		MST_AROUND4,
		MST_AROUND = MST_AROUND4,

		MST_RANDOMTARGET = MST_RANDOM,
	}

	public static class MobSkillTargetTypeInfo {
		public const string Marker = "MST_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static MobSkillTargetTypeInfo() {
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_TARGET, "Target", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_SELF, "Self", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_FRIEND, "Friend", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_MASTER, "Master", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_RANDOMTARGET, "Random target", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND1, "3x3 area around self", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND2, "5x5 area around self", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND3, "7x7 area around self", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND4, "9x9 area around self", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND5, "3x3 area around target", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND6, "5x5 area around target", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND7, "7x7 area around target", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND8, "9x9 area around target", Marker));
			All.Add(new EnumInfoBase(MobSkillTargetType.MST_AROUND, "", Marker) { Visible = false });

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<MobSkillTargetType>(All, TypeToInfo, Marker);
		}
	}
}
