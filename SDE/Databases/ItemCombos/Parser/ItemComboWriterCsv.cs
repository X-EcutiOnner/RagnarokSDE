using Database;
using SDE.Databases.Generic.Parser;
using SDE.Databases.ItemCombos.Features;
using SDE.Editor.Database;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace SDE.Databases.ItemCombos.Parser {
	public class ItemComboWriterCsv : DatabaseWriterCsv {
		public override string KeyField => "Id";
		public override DbAttribute FileKeyRef => ItemComboAttributes.FileKeyRef;

		public override string WriteEntry(ReadableTuple tuple) {
			if (tuple == null)
				return "";

			var model = tuple.GetModel<ItemCombo>();

			StringBuilder b = new StringBuilder();

			foreach (var nameId in model.NameIds) {
				if (!String.IsNullOrEmpty(nameId.Item))
					b.Append(nameId.Item + ":");
			}

			var output = new List<string> {
				b.ToString().TrimEnd(':'),
				DbWriter.SetTextScript(model.Script)
			};

			return Methods.Aggregate(output, ",");
		}
	}
}
