using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SDE.Databases.Pets.Features {
	public class ItemRequirementViewModel : BaseModelView<ItemRequirement> {
		private PetViewModel _vm;

		public ItemRequirementViewModel(PetViewModel viewModel, ItemRequirement model) {
			_vm = viewModel;
			Model = model;
		}

		public Brush ForegroundBrush => ReadableTupleBrush.TextForeground;

		public string Item {
			get => CachedDbs.AegisNameItem.ToStringId(Model.Item);
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(DisplayItemName));
				OnPropertyChanged(nameof(DisplayItemId));
			}
		}

		public string DisplayItemId => !string.IsNullOrEmpty(Model?.Item) ? CachedDbs.AegisNameItem.ToStringId(Model?.Item) : "";
		public string DisplayItemName => DbUtilities.ItemId2Name(Item);

		public string Amount {
			get => Model.Amount;
			set => ExecuteCommand(value);
		}

		public BitmapSource DataImage {
			get => Core.Extensions.GetIconDataImage(DisplayItemId);
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => _vm.IsLocked = v);
		}
	}
}
