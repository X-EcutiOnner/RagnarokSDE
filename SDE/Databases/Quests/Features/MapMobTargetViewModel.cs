using SDE.Databases.Generic.Features;
using System.Runtime.CompilerServices;

namespace SDE.Databases.Quests.Features {
	public class MapMobTargetViewModel : BaseModelView<MapMobTarget> {
		private readonly QuestViewModel _vm;

		public MapMobTargetViewModel(QuestViewModel viewModel, MapMobTarget model) {
			Model = model;
			_vm = viewModel;
		}

		public string MobName { get => Model.MobName; set => ExecuteCommand(value); }
		public bool Active { get => Model == null ? false : Model.Active; set => ExecuteCommand(value); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => _vm.IsLocked = v);
		}
	}
}
