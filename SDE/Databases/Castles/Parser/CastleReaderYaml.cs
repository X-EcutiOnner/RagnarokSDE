using SDE.Databases.Generic.Parser;
using System;
using System.Linq;
using SDE.Databases.Castles.Common;
using SDE.Databases.Castles.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;

namespace SDE.Databases.Castles.Parser {
	public class CastleReaderYaml : DatabaseReaderYaml<int> {
		public override string KeyField => "Id";

		public override void ReadEntry(DbLoadContext context, ParserObject castle) {
			int id = Int32.Parse(castle[KeyField]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Castle>();
			Castle previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (Castle)model.Clone();
			}

			foreach (var entry in castle.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "Map":
						model.Map = entry.ObjectValue;
						break;
					case "Name":
						model.Name = entry.ObjectValue;
						break;
					case "Npc":
						model.Npc = entry.ObjectValue;
						break;
					case "Type":
						model.Type = DbReader.LoadEnum(entry.Value, CastleType.WOE_FIRST_EDITION);
						break;
					case "ClientId":
						model.ClientId = entry.ObjectValue;
						break;
					case "WarpEnabled":
						model.WarpEnabled = Boolean.Parse(entry.ObjectValue);
						break;
					case "WarpX":
						model.WarpX = entry.ObjectValue;
						break;
					case "WarpY":
						model.WarpY = entry.ObjectValue;
						break;
					case "WarpCost":
						model.WarpCost = entry.ObjectValue;
						break;
					case "WarpCostSiege":
						model.WarpCostSiege = entry.ObjectValue;
						break;
				}
			}

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, CastleAttributes.Model, model, false);
			}
		}
	}
}
