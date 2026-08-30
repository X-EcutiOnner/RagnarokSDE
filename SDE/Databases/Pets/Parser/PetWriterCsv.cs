using SDE.Databases.Generic.Parser;
using SDE.Databases.Pets.Features;
using SDE.Editor.Database;
using System.Collections.Generic;
using Utilities;

namespace SDE.Databases.Pets.Parser {
	public class PetWriterCsv : DatabaseWriterCsv {
		public override string KeyField => "Id";

		public override string WriteEntry(ReadableTuple tuple) {
			if (tuple == null)
				return "";

			var model = tuple.GetModel<Pet>();

			var output = new List<string> {
				tuple.Key.ToString(),
				DbWriter.SetEmptyDefault(model.AegisName),
				DbWriter.SetEmptyDefault(model.DisplayName),
				DbWriter.SetZeroDefault(model.TameItem),
				DbWriter.SetZeroDefault(model.EggItem),
				DbWriter.SetZeroDefault(model.EquipItem),
				DbWriter.SetZeroDefault(model.FoodItem),
				DbWriter.SetZeroDefault(model.Fullness),
				DbWriter.SetZeroDefault(model.HungryDelay),
				DbWriter.SetZeroDefault(model.IntimacyHungry),
				DbWriter.SetZeroDefault(model.IntimacyOverfed),
				DbWriter.SetZeroDefault(model.IntimacyStart),
				DbWriter.SetZeroDefault(model.IntimacyOwnerDie),
				DbWriter.SetZeroDefault(model.CaptureRate),
				DbWriter.SetZeroDefault(model.Speed),
				model.SpecialPerformance ? "1" : "0",
				model.DisablePetTalk ? "1" : "0",
				DbWriter.SetZeroDefault(model.AttackRate),
				DbWriter.SetZeroDefault(model.RetaliateRate),
				DbWriter.SetZeroDefault(model.ChangeTargetRate),
				DbWriter.SetTextScript(model.Script),
				DbWriter.SetTextScript(model.SupportScript)
			};

			return Methods.Aggregate(output, ",");
		}
	}
}
