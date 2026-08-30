using SDE.Databases.Generic.Parser;
using SDE.Databases.Pets.Features;
using SDE.Editor.Database;
using System.Text;

namespace SDE.Databases.Pets.Parser {
	public class PetWriterYaml : DatabaseWriterYaml {
		public override string KeyField => "Id";

		public override string KeyToYamlKey(int key) {
			return DbUtilities.MobId2AegisName(key, MobDb);
		}

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<Pet>();
			int intValue = 0;

			builder.AppendLine($"  - Mob: " + DbUtilities.MobId2AegisName(tuple.Key, MobDb));

			if (!DbReader.IsZero(model.TameItem))
				builder.AppendLine($"    TameItem: " + DbUtilities.ItemId2AegisName(model.TameItem, ItemDb));

			if (!DbReader.IsZero(model.EggItem))
				builder.AppendLine($"    EggItem: " + DbUtilities.ItemId2AegisName(model.EggItem, ItemDb));

			if (!DbReader.IsZero(model.EquipItem))
				builder.AppendLine($"    EquipItem: " + DbUtilities.ItemId2AegisName(model.EquipItem, ItemDb));

			if (!DbReader.IsZero(model.FoodItem))
				builder.AppendLine($"    FoodItem: " + DbUtilities.ItemId2AegisName(model.FoodItem, ItemDb));

			//if (!DbReader.IsZero(model.Fullness))
				builder.AppendLine($"    Fullness: {model.Fullness}");

			if (DbReader.ToInt(model.HungryDelay, out intValue) && intValue != 60)
				builder.AppendLine($"    HungryDelay: {intValue}");

			if (DbReader.ToInt(model.HungerIncrease, out intValue) && intValue != 20)
				builder.AppendLine($"    HungerIncrease: {intValue}");

			if (DbReader.ToInt(model.IntimacyStart, out intValue) && intValue != 250)
				builder.AppendLine($"    IntimacyStart: {intValue}");

			if (DbReader.ToInt(model.IntimacyFed, out intValue) /*&& intValue != 50*/)
				builder.AppendLine($"    IntimacyFed: {intValue}");

			if (DbReader.ToInt(model.IntimacyOverfed, out intValue) && intValue != -100)
				builder.AppendLine($"    IntimacyOverfed: {intValue}");

			if (DbReader.ToInt(model.IntimacyHungry, out intValue) && intValue != -5)
				builder.AppendLine($"    IntimacyHungry: {intValue}");

			if (DbReader.ToInt(model.IntimacyOwnerDie, out intValue) && intValue != -20)
				builder.AppendLine($"    IntimacyOwnerDie: {intValue}");

			if (DbReader.ToInt(model.CaptureRate, out intValue))
				builder.AppendLine($"    CaptureRate: {intValue}");

			if (model.SpecialPerformance == false)
				builder.AppendLine($"    SpecialPerformance: false");

			if (DbReader.ToInt(model.AttackRate, out intValue) && intValue != 0)
				builder.AppendLine($"    AttackRate: {intValue}");

			if (DbReader.ToInt(model.RetaliateRate, out intValue) && intValue != 0)
				builder.AppendLine($"    RetaliateRate: {intValue}");

			if (DbReader.ToInt(model.ChangeTargetRate, out intValue) && intValue != 0)
				builder.AppendLine($"    ChangeTargetRate: {intValue}");

			if (model.AllowAutoFeed == true)
				builder.AppendLine($"    AllowAutoFeed: true");

			if (!DbReader.IsNullOrEmpty(model.Script)) {
				builder.AppendLine($"    Script: >");
				builder.AppendLine(DbWriter.ToYamlScript(model.Script, "      "));
			}

			if (!DbReader.IsNullOrEmpty(model.SupportScript)) {
				builder.AppendLine($"    SupportScript: >");
				builder.AppendLine(DbWriter.ToYamlScript(model.SupportScript, "      "));
			}

			if (model.Evolutions.Count > 0) {
				builder.AppendLine("    Evolution:");

				foreach (var evolution in model.Evolutions) {
					WriteEvolution(builder, evolution);
				}
			}
		}

		public void WriteEvolution(StringBuilder builder, Evolution evolution) {
			builder.AppendLine("      - Target: " + DbUtilities.MobId2AegisName(evolution.Target, MobDb));

			if (evolution.ItemRequirements.Count > 0) {
				builder.AppendLine("        ItemRequirements:");

				foreach (var req in evolution.ItemRequirements) {
					WriteItemRequirement(builder, req);
				}
			}
		}

		public void WriteItemRequirement(StringBuilder builder, ItemRequirement req) {
			builder.AppendLine("          - Item: " + DbUtilities.ItemId2AegisName(req.Item, ItemDb));
			builder.AppendLine("            Amount: " + req.Amount);
		}
	}
}
