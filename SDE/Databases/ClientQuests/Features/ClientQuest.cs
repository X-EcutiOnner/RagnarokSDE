using SDE.Core;
using System;
using System.Collections.Generic;

namespace SDE.Databases.ClientQuests.Features {
	public class ClientQuest : ICloneable {
		public string Title;
		public string IconName;
		public string Summary;
		public string BgName;
		public string NpcSpr;
		public string NpcNavi;
		public string NpcPosX;
		public string NpcPosY;
		public string QuestInfo1;
		public string QuestInfo2;
		public string QuestInfo3;
		public string Description;
		public bool CoolTimeQuest;
		public List<ClientQuestReward> Rewards = new List<ClientQuestReward>();
		public string RewardEXP;
		public string RewardJEXP;

		// CSV properties
		public string SG;
		public string QUE;

		public object Clone() {
			var model = (ClientQuest)MemberwiseClone();

			model.Rewards = new List<ClientQuestReward>();

			foreach (var reward in Rewards)
				model.Rewards.Add((ClientQuestReward)reward.Clone());

			return model;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<ClientQuest>.Equals(this, (ClientQuest)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<ClientQuest>.GetHashCode(this);
		}
	}
}
