using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Achievements.Common {
	[Register(typeof(AchvGroupTypeInfo))]
	public enum AchvGroupType {
		AG_NONE = 0,
		AG_ADD_FRIEND,
		AG_ADVENTURE,
		AG_BABY,
		AG_BATTLE,
		AG_CHATTING,
		AG_CHATTING_COUNT,
		AG_CHATTING_CREATE,
		AG_CHATTING_DYING,
		AG_EAT,
		AG_GET_ITEM,
		AG_GET_ZENY,
		AG_GOAL_ACHIEVE,
		AG_GOAL_LEVEL,
		AG_GOAL_STATUS,
		AG_JOB_CHANGE,
		AG_MARRY,
		AG_PARTY,
		AG_ENCHANT_FAIL,
		AG_ENCHANT_SUCCESS,
		AG_SPEND_ZENY,
		AG_TAMING,
		AG_MAX
	}

	public static class AchvGroupTypeInfo {
		public const string Marker = "AG_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static AchvGroupTypeInfo() {
			All.Add(new EnumInfoBase(AchvGroupType.AG_NONE, "None", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_ADD_FRIEND, "Add friend", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_ADVENTURE, "Adventure", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_BABY, "Baby", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_BATTLE, "Battle", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_CHATTING, "Chatting", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_CHATTING_COUNT, "Chatting count", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_CHATTING_CREATE, "Chatting create", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_CHATTING_DYING, "Chatting dying", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_EAT, "Eat", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_GET_ITEM, "Get item", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_GET_ZENY, "Get zeny", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_GOAL_ACHIEVE, "Goal achieve", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_GOAL_LEVEL, "Goal level", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_GOAL_STATUS, "Goal status", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_JOB_CHANGE, "Job change", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_MARRY, "Marry", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_PARTY, "Party", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_ENCHANT_FAIL, "Enchant fail", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_ENCHANT_SUCCESS, "Enchant success", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_SPEND_ZENY, "Spend zeny", Marker));
			All.Add(new EnumInfoBase(AchvGroupType.AG_TAMING, "Taming", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<AchvGroupType>(All, TypeToInfo, Marker);
		}
	}
}
