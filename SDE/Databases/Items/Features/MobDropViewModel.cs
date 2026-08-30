using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SDE.Databases.Items.Features {
	public class MobDropViewModel : BaseModelView<MobDrop> {
		private readonly ItemViewModel _vm;

		public MobDropViewModel(ItemViewModel viewModel, MobDrop model) {
			Model = model;
			_vm = viewModel;
		}

		public Brush ForegroundBrush {
			get {
				if (IsMvp)
					return ReadableTupleBrush.CellBrushMvp;
				if (IsRandomGroup)
					return ReadableTupleBrush.CellBrushLzma;
				if (StealProtected)
					return ReadableTupleBrush.CellBrushModified;

				return ReadableTupleBrush.TextForeground;
			}
		}

		public int Mob { get => Model.Mob; }
		public string Item { get => Model.ItemDrop.Item; }
		public string Rate { get => Model.ItemDrop.Rate; }
		public bool StealProtected { get => Model.ItemDrop.StealProtected; }
		public string RandomOptionGroup { get => Model.ItemDrop.RandomOptionGroup; }
		public string Index { get => Model.ItemDrop.Index; }

		public string NamePreview {
			get => DbUtilities.MobId2Name(Model.Mob);
		}
		public string ItemPreview {
			get => DbUtilities.ItemId2Name(Model.ItemDrop.Item);
		}
		public string RatePreview {
			get {
				int.TryParse(Rate, out int rate);
				return string.Format(CultureInfo.InvariantCulture, "{0:0.00} %", rate / 100f);
			}
		}
		public bool IsRandomGroup { get => Model == null ? false : !string.IsNullOrEmpty(Model.ItemDrop.RandomOptionGroup); }
		public bool IsMvp { get => Model == null ? false : Model.IsMvp; }
		public string Mvp { get => IsMvp ? "MVP" : ""; }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => _vm.IsLocked = v);
		}
	}
}
