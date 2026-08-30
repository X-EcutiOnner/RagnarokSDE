using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SDE.Databases.Mobs.Features {
	public class ItemDropViewModel : BaseModelView<ItemDrop> {
		private readonly MobViewModel _vm;

		public ItemDropViewModel(MobViewModel viewModel, ItemDrop model, bool isMvp = false) {
			Model = model;
			_vm = viewModel;
			IsMvp = isMvp;
		}

		public Brush ForegroundBrush {
			get {
				if (IsRandomGroup)
					return ReadableTupleBrush.CellBrushMvp;
				if (StealProtected)
					return ReadableTupleBrush.CellBrushModified;
				if (IsMvp)
					return ReadableTupleBrush.CellBrushMvp;

				return ReadableTupleBrush.TextForeground;
			}
		}

		public string Item { get => Model?.Item; set => ExecuteCommand(value); }
		public string Rate { get => Model?.Rate; set => ExecuteCommand(value); }
		public bool StealProtected { get => Model == null ? false : Model.StealProtected; set => ExecuteCommand(value); }
		public string RandomOptionGroup { get => Model?.RandomOptionGroup; set => ExecuteCommand(value); }
		public string Index { get => Model?.Index; set => ExecuteCommand(value); }
		public bool IsMvp { get; set; }

		public string ItemPreview {
			get => DbUtilities.ItemId2Name(Model?.Item);
		}
		public string RatePreview {
			get {
				Int32.TryParse(Rate, out int rate);
				return string.Format(CultureInfo.InvariantCulture, "{0:0.00} %", rate / 100f);
			}
		}
		public bool IsRandomGroup { get => Model == null ? false : !String.IsNullOrEmpty(Model.RandomOptionGroup); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => _vm.IsLocked = v);
		}
	}
}
