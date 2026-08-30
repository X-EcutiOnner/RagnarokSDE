using Database.Commands;
using SDE.Databases.Generic.Features;
using SDE.Databases.Quests.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TokeiLibrary.WPF;

namespace SDE.Databases.Quests.Features {
	public class QuestViewModel : BaseModelView<Quest>, INotifyPropertyChanged {
		public int MaximumDrops = -1;
		public int MaximumTargets = -1;

		public RangeObservableCollection<QuestTargetViewModel> Targets { get; } = new RangeObservableCollection<QuestTargetViewModel>();
		public RangeObservableCollection<QuestDropViewModel> Drops { get; } = new RangeObservableCollection<QuestDropViewModel>();

		private QuestTargetViewModel _selectedTarget;
		private QuestDropViewModel _selectedDrop;

		public bool IsLocked { get; set; }

		public QuestViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, Quest model) {
			if (IsLocked)
				return;

			Model = model;
			Tuple = tuple;

			_selectedTarget = null;
			_selectedDrop = null;

			OnTargetsListUpdated();
			OnDropsListUpdated();
			OnPropertyChanged("");
		}

		public string Id {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple?.Key.ToString() == value)
					return;

				if (Int32.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Id));
			}
		}

		public string Title { get => Model?.Title; set => ExecuteCommand(value); }
		public string TimeLimit { get => Model?.TimeLimit; set => ExecuteCommand(value); }

		public QuestTargetViewModel SelectedTarget {
			get => _selectedTarget;
			set {
				if (_selectedTarget == value)
					return;

				_selectedTarget = value;
				OnPropertyChanged(nameof(SelectedTarget));
				OnPropertyChanged(nameof(IsTargetSelected));
			}
		}

		public QuestDropViewModel SelectedDrop {
			get => _selectedDrop;
			set {
				if (_selectedDrop == value)
					return;

				_selectedDrop = value;
				OnPropertyChanged(nameof(SelectedDrop));
				OnPropertyChanged(nameof(IsDropSelected));
			}
		}

		public bool IsTargetSelected => _selectedTarget != null;
		public bool IsDropSelected => _selectedDrop != null;

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}

		public void ChangeTargets(List<QuestTarget> targets, ListCommandMode mode) {
			if (targets.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.Targets, targets, mode);
				OnTargetsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void ChangeDrops(List<QuestDrop> drops, ListCommandMode mode) {
			if (drops.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.Drops, drops, mode);
				OnDropsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void ChangeMobTarget(List<MapMobTarget> targets, ListCommandMode mode) {
			if (targets.Count == 0 || SelectedTarget == null)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => SelectedTarget.Model.MapMobTargets, targets, mode);
				SelectedTarget.OnMapMobTargetsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void OnTargetsListUpdated() {
			// Support for CSV format
			if (Model == null && MaximumTargets > -1) {
				List<QuestTargetViewModel> list = new List<QuestTargetViewModel>();
				for (int i = 0; i < MaximumTargets; i++)
					list.Add(new QuestTargetViewModel(this, new QuestTarget()));

				Targets.ClearAndAddRange(list);
				return;
			}

			Targets.ClearAndAddRange(Model == null ? new List<QuestTargetViewModel>() : Model.Targets.Select(p => new QuestTargetViewModel(this, p)));
		}

		public void OnDropsListUpdated() {
			// Support for CSV format
			if (Model == null && MaximumDrops > -1) {
				List<QuestDropViewModel> list = new List<QuestDropViewModel>();
				for (int i = 0; i < MaximumDrops; i++)
					list.Add(new QuestDropViewModel(this, new QuestDrop()));

				Drops.ClearAndAddRange(list);
				return;
			}

			Drops.ClearAndAddRange(Model == null ? new List<QuestDropViewModel>() : Model.Drops.Select((p, i) => new QuestDropViewModel(this, p)));
		}

		public void CopyTargets(List<QuestTarget> entries) => Copy<QuestTarget, QuestWriterYaml>(entries, (v, writer, b) => writer.WriteTarget(b, v));
		public void CopyDrops(List<QuestDrop> entries) => Copy<QuestDrop, QuestWriterYaml>(entries, (v, writer, b) => writer.WriteDrop(b, v));
		public void CopyMapMobTargets(List<MapMobTarget> entries) => Copy<MapMobTarget, QuestWriterYaml>(entries, (v, writer, b) => writer.WriteMapMobTarget(b, v));
	}
}
