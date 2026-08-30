using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Quests.Features {
	/// <summary>
	/// Interaction logic for QuestView.xaml
	/// </summary>
	public partial class QuestViewCsv : UserControl, IDatabaseView {
		private QuestViewModel _viewModel;
		private DbTab _tab;

		public QuestViewCsv() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new QuestViewModel(tab) { MaximumTargets = 3, MaximumDrops = 3 };
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
				_viewModel.SetModel(tuple, tuple.GetModel<Quest>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}
	}
}
