using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;

namespace SDE.Databases.Pets.Features {
	/// <summary>
	/// Interaction logic for PetView.xaml
	/// </summary>
	public partial class PetViewCsv : UserControl, IDatabaseView {
		private PetViewModel _viewModel;
		private DbTab _tab;

		public PetViewCsv() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new PetViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<Pet>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}
	}
}
