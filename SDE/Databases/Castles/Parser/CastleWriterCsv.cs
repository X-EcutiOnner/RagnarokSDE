using SDE.Databases.Castles.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using Utilities;

namespace SDE.Databases.Castles.Parser {
	public class CastleWriterCsv : DatabaseWriterCsv {
		public override string KeyField => "Id";

		public override string WriteEntry(ReadableTuple tuple) {
			if (tuple == null)
				return "";

			var model = tuple.GetModel<Castle>();

			string[] output = new string[4];

			int eleIdx = 0;
			output[eleIdx++] = tuple.Key.ToString();
			output[eleIdx++] = model.Map;
			output[eleIdx++] = model.Name;
			output[eleIdx++] = model.Npc;

			return Methods.Aggregate(output, ",");
		}
	}
}
