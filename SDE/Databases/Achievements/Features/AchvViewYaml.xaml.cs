using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using SDE.Databases.Achievements.Parser;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Achievements.Features {
	/// <summary>
	/// Interaction logic for PetView.xaml
	/// </summary>
	public partial class AchvViewYaml : UserControl, IDatabaseView {
		private AchvViewModel _viewModel;
		private DbTab _tab;
		private EditableListController<AchvTargetViewModel, AchvTarget, AchvReaderYaml> _targetsList;
		private EditableListController<AchvDependentViewModel, AchvDependent, AchvReaderYaml> _dependentList;

		public AchvViewYaml() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new AchvViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			_targetsList = new EditableListController<AchvTargetViewModel, AchvTarget, AchvReaderYaml>(_lvTargets,
				copy: items => _viewModel.CopyTargets(items),
				pasteLoadYaml: (reader, p) => {
					var r = reader.LoadTarget(p);
					r.Id = "";
					return r;
				},
				changeListFunc: _viewModel.ChangeTargets,
				newModel: () => new AchvTarget() { Mob = "1002", Count = "10" },
				select: model => (DataSources.Mob, model.Mob),
				modes: new EditableListMode[] { EditableListMode.Default }
			);
			
			_dependentList = new EditableListController<AchvDependentViewModel, AchvDependent, AchvReaderYaml>(_lvDependents,
				copy: items => _viewModel.CopyDependents(items),
				pasteLoadYaml: (reader, p) => reader.LoadDependent(p),
				changeListFunc: _viewModel.ChangeDependents,
				newModel: () => new AchvDependent() { Id = "0", Active = true },
				select: model => (DataSources.Achievement, model.Id),
				modes: new EditableListMode[] { EditableListMode.Default }
			);
			
			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<Achv>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			_targetsList.SelectPrevious();
			_dependentList.SelectPrevious();
		
			Core.Extensions.ClearUndos(_gridContainer);
		}

		#region ItemRequirements ListView
		private void _lvDependents_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_dependentList.SaveSelection();
			_dependentList.SelectPrevious();
		}
		#endregion

		#region Targets ListView
		private void _lvTargets_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_targetsList.SaveSelection();
			_targetsList.SelectPrevious();
		}
		#endregion
	}
}
