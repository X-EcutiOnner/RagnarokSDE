using Database.Commands;
using SDE.Databases;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using SDE.View;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TokeiLibrary.WPF;
using Utilities;
using Utilities.Extension;

namespace SDE.Editor.Shops {
	public class ShopViewModel : BaseModelView<Shop> {
		public bool IsLocked { get; set; }

		public RangeObservableCollection<ShopItemViewModel> Items { get; } = new RangeObservableCollection<ShopItemViewModel>();
		private ShopItemViewModel _selectedItem;

		public delegate void ShopCodeUpdatedEventHandler(string newShopCode);
		public event ShopCodeUpdatedEventHandler ShopCodeUpdated;

		public ShopViewModel() {
		}

		public void SetModel(Shop model) {
			Model = model;
			_selectedItem = null;

			OnItemsListUpdated();
			OnPropertyChanged("");
		}

		public EnumInfoBase Type { get => EnumInfos.GetEnumBase(Model?.Type); set => ExecuteCommand((ShopType)value.Value); }
		public string NpcPosition { get => Model?.NpcPosition; set => ExecuteCommand(value); }
		public string NpcDisplayName { get => Model?.NpcDisplayName; set => ExecuteCommand(value); }
		public string NpcViewId { get => Model?.NpcViewId; set => ExecuteCommand(value); }
		public string Currency { get => Model?.Currency; set => ExecuteCommand(value); }

		public ShopItemViewModel SelectedItem {
			get => _selectedItem;
			set {
				if (_selectedItem == value)
					return;

				_selectedItem = value;
				OnPropertyChanged(nameof(SelectedItem));
				OnPropertyChanged(nameof(IsItemSelected));
			}
		}

		public bool IsItemSelected => _selectedItem != null;

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			var currentValue = TypeTreeHelper.GetValue(Model, fieldName);

			if (currentValue.ToString() == value.ToString())
				return;

			TypeTreeHelper.SetValue(Model, fieldName, value);
			UpdateShopScript();
			OnPropertyChanged(fieldName);
		}

		public void UpdateShopScript() {
			StringBuilder builder = new StringBuilder();

			builder.AppendFormat("{0}\t{3}\t{1}\t{2},", Model.NpcPosition, Model.NpcDisplayName, Model.NpcViewId, Model.Type == ShopType.Trader ? "trader" : "shop");

			if (Currency.ToInt() != 0) {
				builder.Append(Currency);
				builder.Append(",");
			}

			if (Model.Type == ShopType.Trader) {
				builder.AppendLine("{");
				builder.AppendLine("OnInit:");

				var itemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);

				for (int index = 0; index < Model.Items.Count; index++) {
					var item = Model.Items[index];
					builder.Append("\tsellitem ");
					builder.Append(DbUtilities.ItemId2AegisName(item.Item, itemDb));

					if (item.Price.ToInt() == -1) {
						builder.AppendLine(";");
					}
					else {
						builder.Append(", ");
						builder.Append(item.Price);
						builder.AppendLine(";");
					}
				}

				builder.AppendLine("}");
			}
			else {
				for (int index = 0; index < Model.Items.Count; index++) {
					var item = Model.Items[index];
					builder.Append(item.Item);
					builder.Append(":");
					builder.Append(item.Price);

					if (index != Model.Items.Count - 1) {
						builder.Append(",");
					}
				}
			}

			ShopCodeUpdated?.Invoke(builder.ToString());
		}

		#region Items list
		public void ChangeItems(List<ShopItem> models, ListCommandMode mode) {
			if (models.Count == 0)
				return;

			try {
				IsLocked = true;

				switch (mode) {
					case ListCommandMode.ChangeList:
						Model.Items = models;
						break;
					case ListCommandMode.Remove:
						foreach (var model in models) {
							Model.Items.Remove(model);
						}
						break;
					case ListCommandMode.Add:
						Model.Items.AddRange(models);
						break;
				}

				UpdateShopScript();
				OnItemsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void OnItemsListUpdated() {
			Items.ClearAndAddRange(Model == null ? new List<ShopItemViewModel>() : Model.Items.Select(p => new ShopItemViewModel(this, p)));
		}
		#endregion

		public void LoadFromShopCode(string code) {
			var model = new Shop(code);
			SetModel(model);
		}

		public void ConfigChanged() {
			OnPropertyChanged("");
			Items.ToList().ForEach(p => p.OnPropertyChanged(""));
		}
	}
}