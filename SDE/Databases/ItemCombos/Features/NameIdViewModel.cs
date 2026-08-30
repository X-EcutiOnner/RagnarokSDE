using SDE.Databases.Generic.Features;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SDE.Databases.ItemCombos.Features {
	public class NameIdViewModel : BaseModelView<NameId>, INotifyPropertyChanged {
		private readonly ItemComboViewModel _vm;

		public NameIdViewModel(ItemComboViewModel viewModel, NameId model) {
			Model = model;
			_vm = viewModel;
		}

		public string Item {
			get => Model?.Item;
			set => ExecuteCommand(value);
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(value, fieldName,
				tuples => tuples.Select(p => p.GetModel<ItemCombo>()).ToList(),
				_vm.NameIds.OfType<BaseModelView<NameId>>().ToList(),
				q => q.NameIds,
				_vm.Tab,
				v => _vm.IsLocked = v);
		}
	}
}
