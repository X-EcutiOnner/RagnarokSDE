using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDE.View.Editors.ItemCostEdit {
	public class ItemCostContainer {
		public List<ItemCostViewModel> ItemCosts { get; } = new List<ItemCostViewModel>();
		public int Count => ItemCosts.Count;

		public ItemCostContainer(string text, int count) {
			var data = text.Split(':');

			for (int i = 0; i < data.Length; i += 3) {
				if (i + 3 > data.Length)
					break;

				var itemCost = new ItemCost();
				itemCost.ItemId = data[i];
				itemCost.Amount = data[i + 1];
				itemCost.Level = data[i + 2];

				if (itemCost.Level == "0")
					itemCost.Level = "";

				ItemCosts.Add(new ItemCostViewModel(itemCost));
			}

			for (int i = ItemCosts.Count; i < count; i++) {
				ItemCosts.Add(new ItemCostViewModel(new ItemCost()));
			}
		}

		public string GetPreview() {
			StringBuilder b = new StringBuilder();

			foreach (var itemCost in ItemCosts) {
				if (String.IsNullOrEmpty(itemCost.ItemId))
					continue;

				b.Append(DbUtilities.ItemId2Name(itemCost.ItemId) + " (" + DbReader.ToInt(itemCost.Amount) + (DbReader.ToInt(itemCost.Level, out int intValue) && intValue > 0 ? ":" + intValue : "") + "), ");
			}

			return b.ToString().Trim(' ', ',');
		}

		public string GetCompactText() {
			string output = "";

			foreach (var itemCost in ItemCosts) {
				if (String.IsNullOrEmpty(itemCost.ItemId))
					continue;

				output += $"{DbReader.ToInt(itemCost.ItemId)}:{DbReader.ToInt(itemCost.Amount)}:{DbReader.ToInt(itemCost.Level)}:";
			}

			return output.TrimEnd(':');
		}
	}
}
