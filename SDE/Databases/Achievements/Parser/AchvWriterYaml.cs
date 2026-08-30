using SDE.Databases.Achievements.Features;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;
using System.Text;

namespace SDE.Databases.Achievements.Parser {
	public class AchvWriterYaml : DatabaseWriterYaml {
		public override string KeyField => "Id";

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<Achv>();
			int intValue = 0;

			builder.AppendLine($"  - Id: {tuple.Key}");

			if (model.Group != Common.AchvGroupType.AG_NONE)
				builder.AppendLine($"    Group: " + EnumInfos.ToYamlString(model.Group));

			if (!DbReader.IsNullOrEmpty(model.Name))
				builder.AppendLine($"    Name: " + DbReader.YamlString(model.Name));

			if (model.Targets.Count > 0) {
				builder.AppendLine($"    Targets:");
				
				int id = 0;

				foreach (var target in model.Targets) {
					bool manualIdSet = false;

					if (String.IsNullOrEmpty(target.Id)) {
						target.Id = id.ToString();
						manualIdSet = true;
					}

					WriteTarget(builder, target);
					id++;

					if (manualIdSet)
						target.Id = "";
				}
			}

			if (!DbReader.IsNullOrEmpty(model.Condition))
				builder.AppendLine($"    Condition: \" {model.Condition} \"");
			
			if (!DbReader.IsNullOrEmpty(model.Map))
				builder.AppendLine($"    Map: {model.Map}");

			if (model.Dependents.Count > 0) {
				builder.AppendLine($"    Dependents:");
				
				foreach (var dependent in model.Dependents) {
					WriteDependent(builder, dependent);
				}
			}

			var reward = WriteReward(model);
			
			if (!String.IsNullOrEmpty(reward)) {
				builder.AppendLine("    Rewards:");
				builder.Append(reward);
			}

			if (DbReader.ToInt(model.Score, out intValue) && intValue != 0)
				builder.AppendLine($"    Score: {intValue}");
		}

		private string WriteReward(Achv model) {
			StringBuilder builder = new StringBuilder();
			int intValue;

			if (DbReader.ToInt(model.RewardItem, out intValue) && intValue != 0)
				builder.AppendLine("      Item: " + DbUtilities.ItemId2AegisName(model.RewardItem, ItemDb));

			if (DbReader.ToInt(model.RewardAmount, out intValue) && intValue != 1)
				builder.AppendLine($"      Amount: {intValue}");

			if (!DbReader.IsNullOrEmpty(model.RewardScript))
				builder.AppendLine($"      Script: \" " + model.RewardScript.Trim(' ', '\t') + " \"");

			if (DbReader.ToInt(model.RewardTitleId, out intValue) && intValue != 0)
				builder.AppendLine($"      TitleId: {intValue}");

			return builder.ToString();
		}

		public void WriteDependent(StringBuilder builder, AchvDependent model) {
			builder.AppendLine($"      {model.Id}: " + (model.Active ? "true" : "false"));
		}

		public void WriteTarget(StringBuilder builder, AchvTarget model) {
			builder.AppendLine("      - Id: " + DbReader.ToInt(model.Id));
			bool hasMobData = false;

			if (!DbReader.IsZero(model.Mob)) {
				builder.AppendLine("        Mob: " + DbUtilities.MobId2AegisName(model.Mob, MobDb));
				hasMobData = true;
			}

			if (DbReader.ToInt(model.Count, out int intValue) && (!hasMobData || intValue != 1))
				builder.AppendLine($"        Count: {intValue}");
		}
	}
}
