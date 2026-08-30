using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Castles.Features {
	/// <summary>
	/// Interaction logic for QuestView.xaml
	/// </summary>
	public partial class CastleViewYaml : UserControl, IDatabaseView {
		private CastleViewModel _viewModel;
		private DbTab _tab;

		public CastleViewYaml() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new CastleViewModel(tab);
			_viewModel.SetModel(null, null);
			DataContext = _viewModel;

			_tab = tab;

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<Castle>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}
	}
}
