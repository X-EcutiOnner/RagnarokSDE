using SDE.Databases.Generic.Parser;
using SDE.Databases.Pets.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using System;
using System.Linq;

namespace SDE.Databases.Pets.Parser {
	public class PetReaderYaml : DatabaseReaderYaml<int> {
		public override string KeyField => "Mob";

		public override void ReadEntry(DbLoadContext context, ParserObject pet) {
			if (!Int32.TryParse(CachedDbs.AegisNameMob.ToStringId(pet[KeyField].ObjectValue), out int id)) {
				throw new Exception("Failed to find the MobID for AegisName '" + pet[KeyField].ObjectValue + "'.");
			}

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Pet>();
			Pet previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (Pet)model.Clone();
				model.Evolutions.Clear();
			}

			foreach (var entry in pet.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "TameItem":
						model.TameItem = CachedDbs.AegisNameItem.ToStringId(entry.ObjectValue);
						break;
					case "EggItem":
						model.EggItem = CachedDbs.AegisNameItem.ToStringId(entry.ObjectValue);
						break;
					case "EquipItem":
						model.EquipItem = CachedDbs.AegisNameItem.ToStringId(entry.ObjectValue);
						break;
					case "FoodItem":
						model.FoodItem = CachedDbs.AegisNameItem.ToStringId(entry.ObjectValue);
						break;
					case "Fullness":
						model.Fullness = entry.ObjectValue;
						break;
					case "HungryDelay":
						model.HungryDelay = entry.ObjectValue;
						break;
					case "HungerIncrease":
						model.HungerIncrease = entry.ObjectValue;
						break;
					case "IntimacyStart":
						model.IntimacyStart = entry.ObjectValue;
						break;
					case "IntimacyFed":
						model.IntimacyFed = entry.ObjectValue;
						break;
					case "IntimacyOverfed":
						model.IntimacyOverfed = entry.ObjectValue;
						break;
					case "IntimacyHungry":
						model.IntimacyHungry = entry.ObjectValue;
						break;
					case "IntimacyOwnerDie":
						model.IntimacyOwnerDie = entry.ObjectValue;
						break;
					case "CaptureRate":
						model.CaptureRate = entry.ObjectValue;
						break;
					case "SpecialPerformance":
						model.SpecialPerformance = Boolean.Parse(entry.ObjectValue);
						break;
					case "AttackRate":
						model.AttackRate = entry.ObjectValue;
						break;
					case "RetaliateRate":
						model.RetaliateRate = entry.ObjectValue;
						break;
					case "ChangeTargetRate":
						model.Fullness = entry.ObjectValue;
						break;
					case "AllowAutoFeed":
						model.AllowAutoFeed = Boolean.Parse(entry.ObjectValue);
						break;
					case "Script":
						model.Script = entry.ObjectValue;
						break;
					case "SupportScript":
						model.SupportScript = entry.ObjectValue;
						break;
					case "Evolution":
						foreach (var evolutionList in entry.Value) {
							model.Evolutions.Add(LoadEvolution(evolutionList));
						}
						break;
				}
			}

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, PetAttributes.Model, model, false);
			}
		}

		public Evolution LoadEvolution(ParserObject parser) {
			Evolution evolution = new Evolution();

			evolution.Target = CachedDbs.AegisNameMob.ToStringId(parser["Target"]);
			var itemRequirements = parser["ItemRequirements"];

			if (itemRequirements != null) {
				foreach (var itemRequirement in itemRequirements) {
					evolution.ItemRequirements.Add(LoadItemRequirement(itemRequirement));
				}
			}

			return evolution;
		}

		public ItemRequirement LoadItemRequirement(ParserObject parser) {
			ItemRequirement itemRequirement = new ItemRequirement();

			itemRequirement.Item = CachedDbs.AegisNameItem.ToStringId(parser["Item"]);
			itemRequirement.Amount = parser["Amount"] ?? "0";

			return itemRequirement;
		}
	}
}
