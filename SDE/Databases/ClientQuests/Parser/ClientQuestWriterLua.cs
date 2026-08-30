using SDE.Databases.ClientQuests.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;
using System.Linq;
using System.Text;
using Utilities.Extension;

namespace SDE.Databases.Achievements.Parser {
	public class ClientQuestWriterLua : DatabaseWriterLua {
		public override string TableName => "QuestInfoList = {}\r\nQuestInfoList";

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			var model = tuple.GetModel<ClientQuest>();

			builder.AppendLine($"\t[{tuple.Key}] = {{");
			builder.AppendLine($"\t\tTitle = \"{model.Title.Escape(EscapeMode.KeepAsciiCode)}\",");

			if (!DbReader.IsNullOrEmpty(model.IconName))
				builder.AppendLine($"\t\tIconName = \"{model.IconName}\",");

			AppendMultiLineText(builder, nameof(model.Description), model.Description ?? " ");

			builder.AppendLine($"\t\tSummary = \"{model.Summary.Escape(EscapeMode.KeepAsciiCode)}\",");

			if (model.CoolTimeQuest)
				builder.AppendLine($"\t\tCoolTimeQuest = 1,");

			if (!DbReader.IsNullOrEmpty(model.NpcSpr))
				builder.AppendLine($"\t\tNpcSpr = \"{model.NpcSpr.Escape(EscapeMode.KeepAsciiCode)}\",");

			if (!DbReader.IsNullOrEmpty(model.NpcNavi))
				builder.AppendLine($"\t\tNpcNavi = \"{model.NpcNavi.Escape(EscapeMode.KeepAsciiCode)}\",");

			if (!DbReader.IsNullOrEmpty(model.NpcPosX))
				builder.AppendLine($"\t\tNpcPosX = {DbReader.ToInt(model.NpcPosX)},");

			if (!DbReader.IsNullOrEmpty(model.NpcPosY))
				builder.AppendLine($"\t\tNpcPosY = {DbReader.ToInt(model.NpcPosY)},");

			if (!DbReader.IsNullOrEmpty(model.RewardEXP))
				builder.AppendLine($"\t\tRewardEXP = \"{model.RewardEXP.Escape(EscapeMode.KeepAsciiCode)}\",");

			if (!DbReader.IsNullOrEmpty(model.RewardJEXP))
				builder.AppendLine($"\t\tRewardJEXP = \"{model.RewardJEXP.Escape(EscapeMode.KeepAsciiCode)}\",");

			if (model.Rewards.Count > 0) {
				builder.AppendLine("\t\tRewardItemList = {");

				foreach (var reward in model.Rewards)
					WriteReward(builder, reward);

				builder.AppendLine("\t\t},");
			}

			if (!DbReader.IsNullOrEmpty(model.BgName))
				builder.AppendLine($"\t\tBgName = \"{model.BgName}\",");

			AppendMultiLineText(builder, nameof(model.QuestInfo1), model.QuestInfo1 ?? "");
			AppendMultiLineText(builder, nameof(model.QuestInfo2), model.QuestInfo2 ?? "");
			AppendMultiLineText(builder, nameof(model.QuestInfo3), model.QuestInfo3 ?? "");

			builder.AppendLine($"\t}},");
		}

		private void AppendMultiLineText(StringBuilder builder, string identifier, string text) {
			if (String.IsNullOrEmpty(text))
				return;

			var textData = text.Split('\n').Select(p => p.TrimEnd('\r')).ToList();

			builder.AppendLine($"\t\t{identifier} = {{");
			foreach (var desc in textData) {
				builder.AppendLine($"\t\t\t\"{desc.Escape(EscapeMode.KeepAsciiCode)}\",");
			}
			builder.AppendLine($"\t\t}},");
		}

		public void WriteReward(StringBuilder builder, ClientQuestReward entry) {
			builder.AppendLine($"\t\t\t{{ ItemID = {DbReader.ToInt(entry.Item)}, ItemNum = {DbReader.ToInt(entry.Count)} }},");
		}
	}
}
