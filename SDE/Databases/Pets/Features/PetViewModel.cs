using Database.Commands;
using SDE.Databases.Generic.Features;
using SDE.Databases.Pets.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TokeiLibrary.WPF;

namespace SDE.Databases.Pets.Features {
	public class PetViewModel : BaseModelView<Pet> {
		public RangeObservableCollection<EvolutionViewModel> Evolutions { get; } = new RangeObservableCollection<EvolutionViewModel>();
		public RangeObservableCollection<ItemRequirementViewModel> ItemRequirements { get; } = new RangeObservableCollection<ItemRequirementViewModel>();

		private EvolutionViewModel _selectedEvolution;
		private ItemRequirementViewModel _selectedItemRequirement;

		public bool IsLocked { get; set; }

		public PetViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, Pet model) {
			if (IsLocked)
				return;

			Model = model;
			Tuple = tuple;

			_selectedEvolution = null;
			_selectedItemRequirement = null;

			OnEvolutionsListUpdated();
			OnItemRequirementsListUpdated();
			OnPropertyChanged("");
		}

		public string Mob {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple?.Key.ToString() == value)
					return;

				if (int.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Mob));
			}
		}

		public string TameItem { get => Model?.TameItem; set => ExecuteCommand(value); }
		public string EggItem { get => Model?.EggItem; set => ExecuteCommand(value); }
		public string EquipItem { get => Model?.EquipItem; set => ExecuteCommand(value); }
		public string FoodItem { get => Model?.FoodItem; set => ExecuteCommand(value); }
		public string Fullness { get => Model?.Fullness; set => ExecuteCommand(value); }
		public string HungryDelay { get => Model?.HungryDelay; set => ExecuteCommand(value); }
		public string HungerIncrease { get => Model?.HungerIncrease; set => ExecuteCommand(value); }
		public string IntimacyStart { get => Model?.IntimacyStart; set => ExecuteCommand(value); }
		public string IntimacyFed { get => Model?.IntimacyFed; set => ExecuteCommand(value); }
		public string IntimacyOverfed { get => Model?.IntimacyOverfed; set => ExecuteCommand(value); }
		public string IntimacyHungry { get => Model?.IntimacyHungry; set => ExecuteCommand(value); }
		public string IntimacyOwnerDie { get => Model?.IntimacyOwnerDie; set => ExecuteCommand(value); }
		public string CaptureRate { get => Model?.CaptureRate; set => ExecuteCommand(value); }
		public bool SpecialPerformance { get => Model == null ? false : Model.SpecialPerformance; set => ExecuteCommand(value); }
		public bool DisablePetTalk { get => Model == null ? false : Model.DisablePetTalk; set => ExecuteCommand(value); }
		public string AttackRate { get => Model?.AttackRate; set => ExecuteCommand(value); }
		public string RetaliateRate { get => Model?.RetaliateRate; set => ExecuteCommand(value); }
		public string ChangeTargetRate { get => Model?.ChangeTargetRate; set => ExecuteCommand(value); }
		public bool AllowAutoFeed { get => Model == null ? false : Model.AllowAutoFeed; set => ExecuteCommand(value); }
		public string Script { get => Model?.Script; set => ExecuteCommand(value); }
		public string SupportScript { get => Model?.SupportScript; set => ExecuteCommand(value); }
		public string Speed { get => Model?.Speed; set => ExecuteCommand(value); }
		public string AegisName { get => Model?.AegisName; set => ExecuteCommand(value); }
		public string DisplayName { get => Model?.DisplayName; set => ExecuteCommand(value); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}

		#region Evolutions
		public EvolutionViewModel SelectedEvolution {
			get => _selectedEvolution;
			set {
				if (_selectedEvolution == value)
					return;

				_selectedEvolution = value;
				OnPropertyChanged(nameof(SelectedEvolution));
				OnPropertyChanged(nameof(IsEvolutionSelected));
				OnItemRequirementsListUpdated();
			}
		}

		public bool IsEvolutionSelected => _selectedEvolution != null;
		
		public void OnEvolutionsListUpdated() {
			Evolutions.ClearAndAddRange(Model == null ? new List<EvolutionViewModel>() : Model.Evolutions.Select(p => new EvolutionViewModel(this, p)));
		}
		public void CopyEvolutions(List<Evolution> entries) => Copy<Evolution, PetWriterYaml>(entries, (v, writer, b) => writer.WriteEvolution(b, v));

		public void ChangeEvolutions(List<Evolution> targets, ListCommandMode mode) {
			if (targets.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.Evolutions, targets, mode);
				OnEvolutionsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}
		#endregion

		#region ItemRequirements
		public ItemRequirementViewModel SelectedItemRequirement {
			get => _selectedItemRequirement;
			set {
				if (_selectedItemRequirement == value)
					return;

				_selectedItemRequirement = value;
				OnPropertyChanged(nameof(SelectedItemRequirement));
				OnPropertyChanged(nameof(IsItemRequirementSelected));
			}
		}

		public bool IsItemRequirementSelected => _selectedItemRequirement != null;
		
		public void OnItemRequirementsListUpdated() {
			ItemRequirements.ClearAndAddRange(SelectedEvolution == null ? new List<ItemRequirementViewModel>() : SelectedEvolution.Model.ItemRequirements.Select(p => new ItemRequirementViewModel(this, p)));
		}
		public void CopyItemRequirements(List<ItemRequirement> entries) => Copy<ItemRequirement, PetWriterYaml>(entries, (v, writer, b) => writer.WriteItemRequirement(b, v));

		public void ChangeItemRequirements(List<ItemRequirement> entries, ListCommandMode mode) {
			if (entries.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => SelectedEvolution.Model.ItemRequirements, entries, mode);
				OnItemRequirementsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}
		#endregion
	}
}
