using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Utilities;

namespace SDE.Databases.ClientItems.Features {
	public class ClientItemViewModel : BaseModelView<ClientItem> {
		public bool IsLocked { get; set; }

		public ICommand CopyIdDescToUnDescCommand { get; set; }

		public ClientItemViewModel(DbTab tab) {
			Tab = tab;

			CopyIdDescToUnDescCommand = new RelayCommand(OnCopyIdDescToUnDescCommand);
		}

		public void SetModel(ReadableTuple tuple, ClientItem model) {
			if (IsLocked)
				return;

			Model = model;
			Tuple = tuple;

			OnPropertyChanged("");
		}

		public string Id {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple?.Key.ToString() == value)
					return;

				if (int.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Id));
			}
		}

		public bool IsCard { get => Model == null ? false : Model.IsCard; set => ExecuteCommand(value); }
		public string ClassNumber { get => Model?.ClassNumber; set => ExecuteCommand(value); }
		public string Illustration { get => Model?.Illustration; set => ExecuteCommand(value); }
		public string NumberOfSlots { get => Model?.NumberOfSlots; set => ExecuteCommand(value); }
		public string Affix { get => Model?.Affix; set => ExecuteCommand(value); }
		public bool IsCostume { get => Model == null ? false : Model.IsCostume; set => ExecuteCommand(value); }
		public bool IsPostfix { get => Model == null ? false : Model.IsPostfix; set => ExecuteCommand(value); }
		public string IdentifiedResourceName { get => Model?.IdentifiedResourceName; set => ExecuteCommand(value); }
		public string UnidentifiedResourceName { get => Model?.UnidentifiedResourceName; set => ExecuteCommand(value); }
		public string IdentifiedDisplayName { get => Model?.IdentifiedDisplayName; set => ExecuteCommand(value); }
		public string UnidentifiedDisplayName { get => Model?.UnidentifiedDisplayName; set => ExecuteCommand(value); }
		public string IdentifiedDescription { get => Model?.IdentifiedDescription; set => ExecuteCommand(value); }
		public string UnidentifiedDescription { get => Model?.UnidentifiedDescription; set => ExecuteCommand(value); }

		private bool _identifiedResourceNameValid = true;

		public bool IdentifiedResourceNameValid {
			get => _identifiedResourceNameValid;
			set {
				if (_identifiedResourceNameValid != value) {
					_identifiedResourceNameValid = value;
					OnPropertyChanged();
				}
			}
		}

		private bool _unidentifiedResourceNameValid = true;

		public bool UnidentifiedResourceNameValid {
			get => _unidentifiedResourceNameValid;
			set {
				if (_unidentifiedResourceNameValid != value) {
					_unidentifiedResourceNameValid = value;
					OnPropertyChanged();
				}
			}
		}

		public void OnCopyIdDescToUnDescCommand() {
			Execute(Model, p => _onCopyIdDescToUnDescCommand(p), nameof(ClientItem.UnidentifiedDescription), v => IsLocked = v);
		}

		private string _onCopyIdDescToUnDescCommand(ClientItem model) {
			return model.IdentifiedDescription;
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}
	}
}
