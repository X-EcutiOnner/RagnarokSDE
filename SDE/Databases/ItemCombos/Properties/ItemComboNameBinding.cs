using Database;
using SDE.Databases.ItemCombos.Features;
using SDE.Editor.Database;
using System;
using System.Text;

namespace SDE.Databases.ItemCombos.Properties {
	public class ItemComboNameBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			StringBuilder b = new StringBuilder();
			var model = Tuple.GetModel<ItemCombo>();

			if (model == null)
				return "";

			foreach (var item in model.NameIds) {
				if (String.IsNullOrEmpty(item.Item))
					continue;

				b.Append(DbUtilities.ItemId2Name(item) + "\n");
			}

			return b.ToString().TrimEnd('\n');
		}
	}
}
