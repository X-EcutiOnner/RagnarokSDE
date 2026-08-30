using Microsoft.Scripting.Utils;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using SDE.View;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SDE.Databases.Quests.Features {
	public class QuestDropViewModel : BaseModelView<QuestDrop> {
		private readonly QuestViewModel _vm;

		public QuestDropViewModel(QuestViewModel viewModel, QuestDrop model) {
			Model = model;
			_vm = viewModel;
		}

		public Brush ForegroundBrush => ReadableTupleBrush.TextForeground;

		public string DisplayNameId {
			get => CachedDbs.AegisNameItem.ToStringId(Model?.Item);
		}

		public string DisplayItemName {
			get {
				if (Model == null)
					return "";

				if (Int32.TryParse(Model.Item, out int value)) {
					var itemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);
					return DbUtilities.ItemId2Name(value, itemDb);
				}

				return Model.Item;
			}
		}

		public string Mob {
			get => Model?.Mob;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(PreviewMob));
			}
		}

		public string PreviewMob {
			get {
				if (String.IsNullOrEmpty(Model?.Mob) || Model?.Mob == "0")
					return "All monsters";

				return DbUtilities.MobPreview(Model?.Mob);
			}
		}

		public string Item {
			get => Model?.Item;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(DisplayNameId));
				OnPropertyChanged(nameof(DisplayItemName));
			}
		}

		public string Count {
			get => Model?.Count;
			set => ExecuteCommand(value);
		}

		public string Rate {
			get => Model?.Rate;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(DropRatePreview));
			}
		}

		public string DropRatePreview {
			get {
				Int32.TryParse(Model?.Rate, out int val);
				return String.Format(CultureInfo.InvariantCulture, "{0:0.00} %", val / 100f);
			}
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(value, fieldName,
				tuples => tuples.Select(p => p.GetModel<Quest>()).ToList(),
				_vm.Drops.OfType<BaseModelView<QuestDrop>>().ToList(),
				q => q.Drops,
				_vm.Tab,
				v => _vm.IsLocked = v);
		}
	}
}
