using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using TokeiLibrary.WPF;

namespace SDE.Databases.Quests.Features {
	public class QuestTargetViewModel : BaseModelView<QuestTarget> {
		private readonly QuestViewModel _vm;
		public RangeObservableCollection<MapMobTargetViewModel> MapMobTargets { get; } = new RangeObservableCollection<MapMobTargetViewModel>();

		public QuestTargetViewModel(QuestViewModel viewModel, QuestTarget model) {
			Model = model;
			_vm = viewModel;

			OnMapMobTargetsListUpdated();
		}

		public Brush ForegroundBrush => ReadableTupleBrush.TextForeground;

		public string DisplayMobId => !String.IsNullOrEmpty(Model?.Mob) ? CachedDbs.AegisNameMob.ToStringId(Model?.Mob) : (Model?.Id);

		public string DisplayMobName {
			get {
				if (Model == null)
					return "";

				if (Int32.TryParse(Model.Mob, out int value)) {
					return DbUtilities.MobId2Name(value);
				}

				if (!String.IsNullOrEmpty(Model.Location))
					return Model.Location;

				return Model.Mob;
			}
		}

		public string DisplayCount {
			get => Model?.Count;
		}

		public string Mob {
			get => CachedDbs.AegisNameMob.ToStringId(Model.Mob);
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(DisplayMobName));
				OnPropertyChanged(nameof(DisplayMobId));
			}
		}

		public string Count { get => Model?.Count; set => ExecuteCommand(value); }
		public string Id { get => Model?.Id; set => ExecuteCommand(value); }
		public EnumInfoBase Race { get => EnumInfos.GetEnumBase(Model?.Race); set => ExecuteCommand((RaceType)value.Value); }
		public EnumInfoBase Size { get => EnumInfos.GetEnumBase(Model?.Size); set => ExecuteCommand((SizeType)value.Value); }
		public EnumInfoBase Element { get => EnumInfos.GetEnumTypeToInfo<ElementType>()[Model?.Element]; set => ExecuteCommand((ElementType)value.Value); }
		public string MinLevel { get => Model?.MinLevel; set => ExecuteCommand(value); }
		public string MaxLevel { get => Model?.MaxLevel; set => ExecuteCommand(value); }
		public string Location { get => Model?.Location; set => ExecuteCommand(value); }
		public string MapName { get => Model?.MapName; set => ExecuteCommand(value); }

		public void OnMapMobTargetsListUpdated() => MapMobTargets.ClearAndAddRange(Model.MapMobTargets.Select(p => new MapMobTargetViewModel(_vm, p)));

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(value, fieldName,
				tuples => tuples.Select(p => p.GetModel<Quest>()).ToList(),
				_vm.Targets.OfType<BaseModelView<QuestTarget>>().ToList(),
				q => q.Targets,
				_vm.Tab,
				v => _vm.IsLocked = v);
		}
	}
}
