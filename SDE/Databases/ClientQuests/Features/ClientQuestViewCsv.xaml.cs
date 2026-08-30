using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.ClientQuests.Features {
	/// <summary>
	/// Interaction logic for ClientQuestView.xaml
	/// </summary>
	public partial class ClientQuestViewCsv : UserControl, IDatabaseView {
		private ClientQuestViewModel _viewModel;
		private DbTab _tab;

		public ClientQuestViewCsv() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new ClientQuestViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<ClientQuest>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}
	}
}
