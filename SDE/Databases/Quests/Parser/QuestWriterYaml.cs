using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Quests.Features;
using SDE.Editor.Database;
using System;
using System.Text;

namespace SDE.Databases.Quests.Parser {
	public class QuestWriterYaml : DatabaseWriterYaml {
		public override string KeyField => "Id";

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<Quest>();

			builder.AppendLine($"  - Id: {tuple.Key}");

			if (!DbReader.IsNullOrEmpty(model.Title))
				builder.AppendLine("    Title: " + DbReader.YamlString(model.Title));

			if (!DbReader.IsNullOrEmpty(model.TimeLimit))
				builder.AppendLine("    TimeLimit: " + DbReader.YamlString(model.TimeLimit));

			if (model.Targets.Count > 0) {
				builder.AppendLine("    Targets:");

				foreach (var target in model.Targets) {
					WriteTarget(builder, target);
				}
			}

			if (model.Drops.Count > 0) {
				builder.AppendLine("    Drops:");

				foreach (var drop in model.Drops) {
					WriteDrop(builder, drop);
				}
			}
		}

		public void WriteTarget(StringBuilder builder, QuestTarget target) {
			int intValue;

			if (!DbReader.IsZero(target.Mob)) {
				builder.AppendLine("      - Mob: " + DbUtilities.MobId2AegisName(target.Mob, MobDb));
			}
			else {
				int id = DbReader.ToInt(target.Id);

				builder.AppendLine($"      - Id: {id}");
			}

			if (!DbReader.IsNullOrEmpty(target.Count))
				builder.AppendLine($"        Count: " + DbReader.ToInt(target.Count));

			if (target.Race != RaceType.RC_ALL)
				builder.AppendLine($"        Race: " + EnumInfos.ToYamlString(target.Race));

			if (target.Size != SizeType.Size_All)
				builder.AppendLine($"        Size: " + EnumInfos.ToYamlString(target.Size));

			if (target.Element != ElementType.ELE_ALL)
				builder.AppendLine($"        Element: " + EnumInfos.ToYamlString(target.Element));

			if (!DbReader.IsZero(target.MinLevel, out intValue))
				builder.AppendLine($"        MinLevel: {intValue}");

			if (!DbReader.IsZero(target.MaxLevel, out intValue))
				builder.AppendLine($"        MaxLevel: {intValue}");

			if (!DbReader.IsNullOrEmpty(target.Location))
				builder.AppendLine($"        Location: {target.Location}");

			if (!DbReader.IsNullOrEmpty(target.MapName))
				builder.AppendLine($"        MapName: {target.MapName}");

			if (target.MapMobTargets.Count > 0) {
				builder.AppendLine($"        MapMobTargets:");

				foreach (var mob in target.MapMobTargets) {
					WriteMapMobTarget(builder, mob);
				}
			}
		}

		public void WriteMapMobTarget(StringBuilder builder, MapMobTarget mob) {
			string key = DbUtilities.MobId2AegisName(mob.MobName, MobDb);
			string value = DbWriter.ToBool(mob.Active);

			builder.AppendLine($"          {key}: {value}");
		}

		public void WriteDrop(StringBuilder builder, QuestDrop drop) {
			int intValue;

			if (!DbReader.IsZero(drop.Mob)) {
				builder.AppendLine("      - Mob: " + DbUtilities.MobId2AegisName(drop.Mob, MobDb));
				builder.AppendLine("        Item: " + DbUtilities.ItemId2AegisName(drop.Item, ItemDb));
			}
			else {
				builder.AppendLine("      - Item: " + DbUtilities.ItemId2AegisName(drop.Item, ItemDb));
			}

			if (Int32.TryParse(drop.Count, out intValue) && intValue != 1)
				builder.AppendLine($"        Count: {intValue}");

			builder.AppendLine($"        Rate: " + DbReader.ToInt(drop.Rate));
		}
	}
}
