using SDE.Databases.Generic.Parser;
using SDE.Databases.Quests.Features;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.Quests.Parser {
	public class QuestReaderCsv : DatabaseReaderCsv<int> {
		public override void ReadEntry(DbLoadContext context, string[] elements) {
			int id = Int32.Parse(elements[0]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Quest>();
			Quest previousModel = model;

			while (model.Targets.Count < 3)
				model.Targets.Add(new QuestTarget());

			while (model.Drops.Count < 3)
				model.Drops.Add(new QuestDrop());
			
			if (table.EnableEvents) {  // From clipboard
				model = (Quest)model.Clone();
				model.Targets.Clear();
				model.Drops.Clear();
			}

			if (elements.Length < 18)
				return;

			int eleIdx = 1;
			model.TimeLimit = elements[eleIdx++];

			for (int i = 0; i < 3; i++) {
				model.Targets[i].Mob = elements[eleIdx++];
				model.Targets[i].Count = elements[eleIdx++];
			}

			for (int i = 0; i < 3; i++) {
				model.Drops[i].Mob = elements[eleIdx++];
				model.Drops[i].Item = elements[eleIdx++];
				model.Drops[i].Rate = elements[eleIdx++];
			}

			model.Title = DbReader.RemoveQuotes(elements[eleIdx++]);

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, QuestAttributes.Model, model, false);
			}
		}
	}
}
