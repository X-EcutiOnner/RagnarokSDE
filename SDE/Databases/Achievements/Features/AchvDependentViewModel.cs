using SDE.Databases.Generic.Features;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SDE.Databases.Achievements.Features {
	public class AchvDependentViewModel : BaseModelView<AchvDependent> {
		private readonly AchvViewModel _vm;

		public AchvDependentViewModel(AchvViewModel viewModel, AchvDependent model) {
			Model = model;
			_vm = viewModel;
		}

		public string Id { get => Model.Id; set => ExecuteCommand(value); }
		public bool Active { get => Model == null ? false : Model.Active; set => ExecuteCommand(value); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(value, fieldName,
				tuples => tuples.Select(p => p.GetModel<Achv>()).ToList(),
				_vm.Dependents.OfType<BaseModelView<AchvDependent>>().ToList(),
				q => q.Dependents,
				_vm.Tab,
				v => _vm.IsLocked = v);
		}
	}
}
