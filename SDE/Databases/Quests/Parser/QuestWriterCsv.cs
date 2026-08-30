using SDE.Databases.Generic.Parser;
using SDE.Databases.Quests.Features;
using SDE.Editor.Database;
using Utilities;

namespace SDE.Databases.Quests.Parser {
	public class QuestWriterCsv : DatabaseWriterCsv {
		public override string KeyField => "Id";

		public override string WriteEntry(ReadableTuple tuple) {
			if (tuple == null)
				return "";

			var model = tuple.GetModel<Quest>();

			string[] output = new string[18];

			int eleIdx = 0;
			output[eleIdx++] = tuple.Key.ToString();
			output[eleIdx++] = DbWriter.SetZeroDefault(model.TimeLimit);

			for (int i = 0; i < 3; i++) {
				if (i < model.Targets.Count) {
					output[eleIdx++] = DbWriter.SetZeroDefault(model.Targets[i].Mob);
					output[eleIdx++] = DbWriter.SetZeroDefault(model.Targets[i].Count);
				}
				else {
					output[eleIdx++] = "0";
					output[eleIdx++] = "0";
				}
			}

			for (int i = 0; i < 3; i++) {
				if (i < model.Drops.Count) {
					output[eleIdx++] = DbWriter.SetZeroDefault(model.Drops[i].Mob);
					output[eleIdx++] = DbWriter.SetZeroDefault(model.Drops[i].Item);
					output[eleIdx++] = DbWriter.SetZeroDefault(model.Drops[i].Rate);
				}
				else {
					output[eleIdx++] = "0";
					output[eleIdx++] = "0";
					output[eleIdx++] = "0";
				}
			}

			output[eleIdx++] = model.Title;

			return Methods.Aggregate(output, ",");
		}
	}
}
