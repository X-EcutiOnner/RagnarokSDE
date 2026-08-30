using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Skills.Features {
	/// <summary>
	/// Interaction logic for SkillView.xaml
	/// </summary>
	public partial class SkillViewYaml : UserControl, IDatabaseView {
		private SkillViewModel _viewModel;
		private DbTab _tab;

		public SkillViewYaml() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new SkillViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<Skill>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}
	}
}
