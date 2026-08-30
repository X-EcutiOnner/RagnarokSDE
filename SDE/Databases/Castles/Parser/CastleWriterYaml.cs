using SDE.Databases.Castles.Features;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;
using System.Text;

namespace SDE.Databases.Castles.Parser {
	public class CastleWriterYaml : DatabaseWriterYaml {
		public override string KeyField => "Id";

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<Castle>();
			int intValue = 0;

			builder.AppendLine($"  - Id: {tuple.Key}");
			builder.AppendLine($"    Map: {model.Map}");
			builder.AppendLine($"    Name: {model.Name}");
			builder.AppendLine($"    Npc: {model.Npc}");
			builder.AppendLine($"    Type: " + EnumInfos.ToYamlString(model.Type));

			if (!DbReader.IsZero(model.ClientId))
				builder.AppendLine($"    ClientId: {model.ClientId}");

			if (model.WarpEnabled)
				builder.AppendLine($"    WarpEnabled: true");

			if (!DbReader.IsZero(model.WarpX))
				builder.AppendLine($"    WarpX: {model.WarpX}");

			if (!DbReader.IsZero(model.WarpY))
				builder.AppendLine($"    WarpY: {model.WarpY}");

			if (Int32.TryParse(model.WarpCost, out intValue) && intValue != 100)
				builder.AppendLine($"    WarpCost: {model.WarpCost}");

			if (Int32.TryParse(model.WarpCostSiege, out intValue) && intValue != 100000)
				builder.AppendLine($"    WarpCostSiege: {model.WarpCostSiege}");
		}
	}
}
