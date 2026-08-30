using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SDE.Databases.Achievements.Features {
	public class AchvTargetViewModel : BaseModelView<AchvTarget> {
		private readonly AchvViewModel _vm;

		public AchvTargetViewModel(AchvViewModel viewModel, AchvTarget model) {
			Model = model;
			_vm = viewModel;
		}

		public string Id {
			get => Model?.Id;
			set => ExecuteCommand(value);
		}

		public string Mob {
			get => Model?.Mob;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(PreviewMob));
			}
		}

		public string PreviewMob {
			get => DbUtilities.MobPreview(Model?.Mob);
		}

		public string Count {
			get => Model?.Count;
			set => ExecuteCommand(value);
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(value, fieldName,
				tuples => tuples.Select(p => p.GetModel<Achv>()).ToList(),
				_vm.Targets.OfType<BaseModelView<AchvTarget>>().ToList(),
				q => q.Targets,
				_vm.Tab,
				v => _vm.IsLocked = v);
		}
	}
}
