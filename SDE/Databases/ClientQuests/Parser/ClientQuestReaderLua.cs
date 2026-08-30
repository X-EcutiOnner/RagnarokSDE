using Database;
using Lua.Structure;
using SDE.Databases.ClientQuests.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System.Linq;

namespace SDE.Databases.ClientQuests.Parser {
	public class ClientQuestReaderLua : DatabaseReaderLua {
		public override string TableName => "QuestInfoList";

		public override void LoadEntry(Table<int, ReadableTuple> table, LKeyValue item, BaseDatabase db) {
			int id = int.Parse(item.Key);
			LList entries = item.Value as LList;

			var tuple = table.EnsureExists(id);
			var model = tuple.GetModel<ClientQuest>();
			ClientQuest previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (ClientQuest)model.Clone();
				model.Rewards.Clear();
			}

			if (entries != null) {
				foreach (LKeyValue entry in entries.OfType<LKeyValue>()) {
					switch (entry.Key) {
						case "Title":
							model.Title = ((LStringValue)entry.Value).Value;
							break;
						case "IconName":
							model.IconName = ((LStringValue)entry.Value).Value;
							break;
						case "Summary":
							model.Summary = ((LStringValue)entry.Value).Value;
							break;
						case "BgName":
							model.BgName = ((LStringValue)entry.Value).Value;
							break;
						case "NpcSpr":
							model.NpcSpr = ((LStringValue)entry.Value).Value;
							break;
						case "NpcNavi":
							model.NpcNavi = ((LStringValue)entry.Value).Value;
							break;
						case "NpcPosX":
							model.NpcPosX = ((LStringValue)entry.Value).Value;
							break;
						case "NpcPosY":
							model.NpcPosY = ((LStringValue)entry.Value).Value;
							break;
						case "QuestInfo1":
							model.QuestInfo1 = LoadMultiLineText((LList)entry.Value);
							break;
						case "QuestInfo2":
							model.QuestInfo2 = LoadMultiLineText((LList)entry.Value);
							break;
						case "QuestInfo3":
							model.QuestInfo3 = LoadMultiLineText((LList)entry.Value);
							break;
						case "Description":
							model.Description = LoadMultiLineText((LList)entry.Value);
							break;
						case "RewardEXP":
							model.RewardEXP = ((LStringValue)entry.Value).Value;
							break;
						case "RewardJEXP":
							model.RewardJEXP = ((LStringValue)entry.Value).Value;
							break;
						case "RewardItemList":
							foreach (LList rewardEntry in (LList)entry.Value) {
								model.Rewards.Add(LoadReward(rewardEntry));
							}
							break;
						case "CoolTimeQuest":
							model.CoolTimeQuest = ((LStringValue)entry.Value).Value != "0";
							break;
					}
				}

				if (table.EnableEvents && previousModel != null) {
					if (previousModel.Equals(model))
						return;

					table.Commands.Set(tuple, ClientQuestAttributes.Model, model, false);
				}
			}
		}

		public string LoadMultiLineText(LList list) {
			string text = "";

			for (int i = 0; i < list.Variables.Count; i++) {
				LBaseType descEntry = list.Variables[i];
				text += ((LStringValue)descEntry).Value;
				
				if (i != list.Variables.Count - 1)
					text += "\r\n";
			}

			return text;
		}

		public ClientQuestReward LoadReward(LList list) {
			ClientQuestReward reward = new ClientQuestReward();

			foreach (LKeyValue rewardEntrySub in list) {
				switch (rewardEntrySub.Key) {
					case "ItemID":
						reward.Item = ((LStringValue)rewardEntrySub.Value).Value;
						break;
					case "ItemNum":
						reward.Count = ((LStringValue)rewardEntrySub.Value).Value;
						break;
				}
			}

			return reward;
		}
	}
}
