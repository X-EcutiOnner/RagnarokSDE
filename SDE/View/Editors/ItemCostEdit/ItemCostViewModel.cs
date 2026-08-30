using SDE.Databases.Generic.Features;
using SDE.View.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SDE.View.Editors.ItemCostEdit {
	public class ItemCostViewModel : BaseModelView<ItemCost> {
		public ItemCostViewModel(ItemCost model) {
			Model = model;
		}

		public string ItemId { get => Model?.ItemId; set => SetValue(ref Model.ItemId, value); }
		public string Amount { get => Model?.Amount; set => SetValue(ref Model.Amount, value); }
		public string Level { get => Model?.Level; set => SetValue(ref Model.Level, value); }

		public void SetValue<T>(ref T field, T value, [CallerMemberName] string fieldName = "") {
			field = value;
			OnPropertyChanged(fieldName);
		}
	}
}
