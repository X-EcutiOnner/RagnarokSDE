using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SDE.Databases.Pets.Features {
	public class EvolutionViewModel : BaseModelView<Evolution> {
		private PetViewModel _vm;

		public EvolutionViewModel(PetViewModel viewModel, Evolution model) {
			_vm = viewModel;
			Model = model;
		}

		public Brush ForegroundBrush => ReadableTupleBrush.TextForeground;

		public string Target {
			get => CachedDbs.AegisNameMob.ToStringId(Model.Target);
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(DisplayTargetName));
				OnPropertyChanged(nameof(DisplayTargetId));
			}
		}

		public string DisplayTargetId => !string.IsNullOrEmpty(Model?.Target) ? CachedDbs.AegisNameMob.ToStringId(Model?.Target) : "";
		public string DisplayTargetName => DbUtilities.MobId2Name(Target);

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => _vm.IsLocked = v);
		}
	}
}
