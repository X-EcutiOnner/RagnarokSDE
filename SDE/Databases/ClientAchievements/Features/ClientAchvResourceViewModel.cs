using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SDE.Databases.ClientAchievements.Features {
	public class ClientAchvResourceViewModel : BaseModelView<ClientAchvResource> {
		private readonly ClientAchvViewModel _vm;

		public ClientAchvResourceViewModel(ClientAchvViewModel viewModel, ClientAchvResource model) {
			Model = model;
			_vm = viewModel;
		}

		public Brush ForegroundBrush => ReadableTupleBrush.TextForeground;

		public string Id {
			get => Model?.Id;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(PreviewId));
			}
		}

		public string PreviewId {
			get {
				if (String.IsNullOrEmpty(Model?.Id)) {
					return (_vm.Resources.IndexOf(this) + 1).ToString();
				}

				return Model?.Id;
			}
		}

		public string Text { get => Model?.Text; set => ExecuteCommand(value); }
		public string Count { get => Model?.Count; set => ExecuteCommand(value); }
		public string Shortcut { get => Model?.Shortcut; set => ExecuteCommand(value); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(value, fieldName,
				tuples => tuples.Select(p => p.GetModel<ClientAchv>()).ToList(),
				_vm.Resources.OfType<BaseModelView<ClientAchvResource>>().ToList(),
				q => q.Resources,
				_vm.Tab,
				v => _vm.IsLocked = v);
		}
	}
}
