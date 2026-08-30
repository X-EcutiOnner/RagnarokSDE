using SDE.Databases.ClientAchievements.Common;
using SDE.Databases.ClientAchievements.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;
using System.Text;
using Utilities.Extension;

namespace SDE.Databases.Achievements.Parser {
	public class ClientAchvWriterLua : DatabaseWriterLua {
		public override string TableName => "achievement_tbl";

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			var model = tuple.GetModel<ClientAchv>();

			builder.AppendLine($"\t[{tuple.Key}] = {{");
			builder.AppendLine("\t\tUI_Type = " + (model.UiType == ClientAchvUiType.UITYPE_TEXT ? "0" : "1") + ",");
			builder.AppendLine($"\t\tgroup = \"{model.Group.Escape(EscapeMode.KeepAsciiCode)}\",");
			builder.AppendLine($"\t\tmajor = {model.Major},");
			builder.AppendLine($"\t\tminor = {model.Minor},");
			builder.AppendLine($"\t\ttitle = \"{model.Title.Escape(EscapeMode.KeepAsciiCode)}\",");
			builder.AppendLine($"\t\tcontent = {{");
			builder.AppendLine($"\t\t\tsummary = \"{model.Summary.Escape(EscapeMode.KeepAsciiCode)}\",");
			builder.AppendLine($"\t\t\tdetails = \"{model.Details.Escape(EscapeMode.KeepAsciiCode)}\",");
			builder.AppendLine($"\t\t}},");
			builder.AppendLine($"\t\tresource = {{");

			int index = 1;

			foreach (var resource in model.Resources) {
				string oldId = resource.Id;

				if (String.IsNullOrEmpty(resource.Id)) {
					resource.Id = index.ToString();
				}

				WriteResource(model, builder, resource);

				resource.Id = oldId;
				index++;
			}
			builder.AppendLine($"\t\t}},");

			if (_hasReward(model)) {
				builder.AppendLine($"\t\treward = {{");

				if (!String.IsNullOrEmpty(model.RewardTitle)) {
					builder.AppendLine($"\t\t\ttitle = {DbReader.ToInt(model.RewardTitle)},");
				}

				if (!String.IsNullOrEmpty(model.RewardBuff)) {
					builder.AppendLine($"\t\t\tbuff = {DbReader.ToInt(model.RewardBuff)},");
				}

				if (!String.IsNullOrEmpty(model.RewardItem)) {
					builder.AppendLine($"\t\t\titem = {DbReader.ToInt(model.RewardItem)},");
				}

				builder.AppendLine($"\t\t}},");
			}
			else {
				builder.AppendLine($"\t\treward = {{}},");
			}

			builder.AppendLine($"\t\tscore = {DbReader.ToInt(model.Score)},");
			builder.AppendLine($"\t}},");
		}

		private bool _hasReward(ClientAchv model) {
			return !String.IsNullOrEmpty(model.RewardTitle) || !String.IsNullOrEmpty(model.RewardBuff) || !String.IsNullOrEmpty(model.RewardItem);
		}

		public void WriteResource(ClientAchv model, StringBuilder builder, ClientAchvResource resource) {
			var id = resource.Id;

			if (String.IsNullOrEmpty(resource.Id)) {
				resource.Id = model.Resources.IndexOf(resource).ToString();
			}

			builder.AppendLine($"\t\t\t[{resource.Id}] = {{");
			builder.AppendLine($"\t\t\t\ttext = \"{resource.Text.Escape(EscapeMode.KeepAsciiCode)}\",");

			if (model.UiType == ClientAchvUiType.UITYPE_TEXT_AND_COUNTER) {
				builder.AppendLine($"\t\t\t\tcount = {DbReader.ToInt(resource.Count)},");
			}

			if (DbReader.ToInt(resource.Shortcut, out int intValue) && intValue > 0) {
				builder.AppendLine($"\t\t\t\tshortcut = {intValue},");
			}

			builder.AppendLine($"\t\t\t}},");
			resource.Id = id;
		}
	}
}
