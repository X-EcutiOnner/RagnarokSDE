using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.MobSkills.Features {
	/// <summary>
	/// Interaction logic for MobSkillView.xaml
	/// </summary>
	public partial class MobSkillViewCsv : UserControl, IDatabaseView {
		private MobSkillViewModel _viewModel;
		private DbTab _tab;

		public MobSkillViewCsv() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new MobSkillViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<MobSkill>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}
	}
}
