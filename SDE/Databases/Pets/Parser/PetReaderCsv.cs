using SDE.Databases.Generic.Parser;
using SDE.Databases.Pets.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Pets.Parser {
	public class PetReaderCsv : DatabaseReaderCsv<int> {
		public override void ReadEntry(DbLoadContext context, string[] elements) {
			int id = int.Parse(elements[0]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Pet>();
			Pet previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (Pet)model.Clone();
				model.Evolutions.Clear();
			}

			if (elements.Length < 22)
				return;

			int eleIdx = 1;
			model.AegisName = elements[eleIdx++];
			model.DisplayName = elements[eleIdx++];
			model.TameItem = elements[eleIdx++];
			model.EggItem = elements[eleIdx++];
			model.EquipItem = elements[eleIdx++];
			model.FoodItem = elements[eleIdx++];
			model.Fullness = elements[eleIdx++];
			model.HungryDelay = elements[eleIdx++];
			model.IntimacyHungry = elements[eleIdx++];
			model.IntimacyOverfed = elements[eleIdx++];
			model.IntimacyStart = elements[eleIdx++];
			model.IntimacyOwnerDie = elements[eleIdx++];
			model.CaptureRate = elements[eleIdx++];
			model.Speed = elements[eleIdx++];
			model.SpecialPerformance = elements[eleIdx++] == "1" ? true : false;
			model.DisablePetTalk = elements[eleIdx++] == "1" ? true : false;
			model.AttackRate = elements[eleIdx++];
			model.RetaliateRate = elements[eleIdx++];
			model.ChangeTargetRate = elements[eleIdx++];
			model.Script = DbReader.FromScript(elements[eleIdx++]);
			model.SupportScript = DbReader.FromScript(elements[eleIdx++]);

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, PetAttributes.Model, model, false);
			}
		}
	}
}
