using SDE.Databases.Castles.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.Castles.Parser {
	public class CastleReaderCsv : DatabaseReaderCsv<int> {
		public override void ReadEntry(DbLoadContext context, string[] elements) {
			int id = Int32.Parse(elements[0]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Castle>();

			if (elements.Length < 4)
				return;

			int eleIdx = 1;
			model.Map = elements[eleIdx++];
			model.Name = elements[eleIdx++];
			model.Npc = elements[eleIdx++];
		}
	}
}
