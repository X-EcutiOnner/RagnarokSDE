using Database.Commands;
using SDE.Databases.Achievements.Common;
using SDE.Databases.Achievements.Parser;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TokeiLibrary.WPF;

namespace SDE.Databases.Achievements.Features {
	public class AchvViewModel : BaseModelView<Achv> {
		public int MaximumDrops = -1;
		public int MaximumTargets = -1;

		public RangeObservableCollection<AchvDependentViewModel> Dependents { get; } = new RangeObservableCollection<AchvDependentViewModel>();
		public RangeObservableCollection<AchvTargetViewModel> Targets { get; } = new RangeObservableCollection<AchvTargetViewModel>();

		private AchvDependentViewModel _selectedDependent;
		private AchvTargetViewModel _selectedTarget;

		public bool IsLocked { get; set; }

		public AchvViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, Achv quest) {
			if (IsLocked)
				return;

			Model = quest;
			Tuple = tuple;

			_selectedDependent = null;
			_selectedTarget = null;

			OnDependentsListUpdated();
			OnTargetsListUpdated();
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

		public EnumInfoBase Group { get => EnumInfos.GetEnumBase(Model?.Group); set => ExecuteCommand((AchvGroupType)value.Value); }

		public string Name { get => Model?.Name; set => ExecuteCommand(value); }
		public string Condition { get => Model?.Condition; set => ExecuteCommand(value); }
		public string Map { get => Model?.Map; set => ExecuteCommand(value); }
		public string RewardItem { get => Model?.RewardItem; set => ExecuteCommand(value); }
		public string RewardAmount { get => Model?.RewardAmount; set => ExecuteCommand(value); }
		public string RewardScript { get => Model?.RewardScript; set => ExecuteCommand(value); }
		public string RewardTitleId { get => Model?.RewardTitleId; set => ExecuteCommand(value); }
		public string Score { get => Model?.Score; set => ExecuteCommand(value); }

		public AchvDependentViewModel SelectedDependent {
			get => _selectedDependent;
			set {
				if (_selectedDependent == value)
					return;

				_selectedDependent = value;
				OnPropertyChanged(nameof(SelectedDependent));
				OnPropertyChanged(nameof(IsDependentSelected));
			}
		}

		public AchvTargetViewModel SelectedTarget {
			get => _selectedTarget;
			set {
				if (_selectedTarget == value)
					return;

				_selectedTarget = value;
				OnPropertyChanged(nameof(SelectedTarget));
				OnPropertyChanged(nameof(IsTargetSelected));
			}
		}

		public bool IsDependentSelected => _selectedDependent != null;
		public bool IsTargetSelected => _selectedTarget != null;

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}

		#region Dependents list
		public void ChangeDependents(List<AchvDependent> entries, ListCommandMode mode) {
			if (entries.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.Dependents, entries, mode);
				OnDependentsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void OnDependentsListUpdated() {
			Dependents.ClearAndAddRange(Model == null ? new List<AchvDependentViewModel>() : Model.Dependents.Select(p => new AchvDependentViewModel(this, p)));
		}

		public void CopyDependents(List<AchvDependent> entries) => Copy<AchvDependent, AchvWriterYaml>(entries, (v, writer, b) => writer.WriteDependent(b, v));
		#endregion

		#region Targets list
		public void ChangeTargets(List<AchvTarget> models, ListCommandMode mode) {
			if (models.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.Targets, models, mode);
				OnTargetsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void OnTargetsListUpdated() {
			Targets.ClearAndAddRange(Model == null ? new List<AchvTargetViewModel>() : Model.Targets.Select(p => new AchvTargetViewModel(this, p)));
		}

		public void CopyTargets(List<AchvTarget> entries) => Copy<AchvTarget, AchvWriterYaml>(entries, (v, writer, b) => writer.WriteTarget(b, v));
		#endregion
	}
}
