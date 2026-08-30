using Database;
using SDE.Databases.ItemCombos.Features;
using System;
using System.Text;

namespace SDE.Databases.ItemCombos.Properties {
	public class ItemComboIdBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			StringBuilder b = new StringBuilder();
			var model = Tuple.GetModel<ItemCombo>();

			if (model == null)
				return "";

			char separator = '\n';

			foreach (var item in model.NameIds) {
				if (String.IsNullOrEmpty(item.Item))
					continue;

				b.Append(item + ":" + separator);
			}

			return b.ToString().TrimEnd(separator);
		}
	}
}
