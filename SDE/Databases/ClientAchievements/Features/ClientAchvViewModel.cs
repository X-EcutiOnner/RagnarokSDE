using Database.Commands;
using SDE.Databases.Achievements.Parser;
using SDE.Databases.ClientAchievements.Common;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TokeiLibrary.WPF;

namespace SDE.Databases.ClientAchievements.Features {
	public class ClientAchvViewModel : BaseModelView<ClientAchv> {
		public RangeObservableCollection<ClientAchvResourceViewModel> Resources { get; } = new RangeObservableCollection<ClientAchvResourceViewModel>();

		private ClientAchvResourceViewModel _selectedResource;

		public bool IsLocked { get; set; }

		public ClientAchvViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, ClientAchv model) {
			if (IsLocked)
				return;

			Model = model;
			Tuple = tuple;

			_selectedResource = null;

			OnResourcesListUpdated();
			OnPropertyChanged("");
		}

		public string Id {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple.Key.ToString() == value)
					return;

				if (int.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Id));
			}
		}

		public string Title { get => Model?.Title; set => ExecuteCommand(value); }
		public string Group { get => Model?.Group; set => ExecuteCommand(value); }
		public string Major { get => Model?.Major; set => ExecuteCommand(value); }
		public string Minor { get => Model?.Minor; set => ExecuteCommand(value); }
		public string Summary { get => Model?.Summary; set => ExecuteCommand(value); }
		public string Details { get => Model?.Details; set => ExecuteCommand(value); }
		public string RewardTitle { get => Model?.RewardTitle; set => ExecuteCommand(value); }
		public string RewardBuff { get => Model?.RewardBuff; set => ExecuteCommand(value); }
		public string RewardItem { get => Model?.RewardItem; set => ExecuteCommand(value); }
		public string Score { get => Model?.Score; set => ExecuteCommand(value); }
		public EnumInfoBase UiType { get => EnumInfos.GetEnumBase(Model?.UiType); set => ExecuteCommand((ClientAchvUiType)value.Value); }

		private bool _groupValid = true;

		public bool GroupValid {
			get => _groupValid;
			set {
				if (_groupValid != value) {
					_groupValid = value;
					OnPropertyChanged();
				}
			}
		}

		#region Resources
		public ClientAchvResourceViewModel SelectedResource {
			get => _selectedResource;
			set {
				if (_selectedResource == value)
					return;

				_selectedResource = value;
				OnPropertyChanged(nameof(SelectedResource));
				OnPropertyChanged(nameof(IsResourceSelected));
			}
		}

		public bool IsResourceSelected => _selectedResource != null;

		public void ChangeResources(List<ClientAchvResource> resources, ListCommandMode mode) {
			if (resources.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.Resources, resources, mode);
				OnResourcesListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void OnResourcesListUpdated() {
			Resources.ClearAndAddRange(Model == null ? new List<ClientAchvResourceViewModel>() : Model.Resources.Select(p => new ClientAchvResourceViewModel(this, p)));
		}

		public void CopyResources(List<ClientAchvResource> entries) => Copy<ClientAchvResource, ClientAchvWriterLua>(entries, (v, writer, b) => writer.WriteResource(Model, b, v));
		#endregion

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}
	}
}
