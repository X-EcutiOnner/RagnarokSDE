using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.MobSkills.Common {
	[Register(typeof(MobSkillStateTypeInfo))]
	public enum MobSkillStateType {
		MSS_ANY = -1,
		MSS_IDLE,
		MSS_WALK,
		MSS_LOOT,
		MSS_DEAD,
		MSS_BERSERK, //Aggressive mob attacking
		MSS_ANGRY,   //Mob retaliating from being attacked.
		MSS_RUSH,    //Mob following a player after being attacked.
		MSS_FOLLOW,  //Mob following a player without being attacked.
		MSS_ANYTARGET,

		MSS_CHASE = MSS_RUSH,
		MSS_ATTACK = MSS_BERSERK,
	}

	public static class MobSkillStateTypeInfo {
		public const string Marker = "MSS_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static MobSkillStateTypeInfo() {
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_ANY, "Any", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_IDLE, "Idle", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_WALK, "Walk", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_LOOT, "Loot", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_DEAD, "Dead", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_ATTACK, "Attack", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_ANGRY, "Angry", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_CHASE, "Chase", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_FOLLOW, "Follow", Marker));
			All.Add(new EnumInfoBase(MobSkillStateType.MSS_ANYTARGET, "Any target", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<MobSkillStateType>(All, TypeToInfo, Marker);
		}
	}
}
