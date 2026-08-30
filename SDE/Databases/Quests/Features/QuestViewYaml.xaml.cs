using SDE.Editor.Generic.DbTabs;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary.WPF.Styles.ListView;
using SDE.Databases.Quests.Parser;
using SDE.Databases.Generic.Features;
using SDE.Editor.Parsers;
using SDE.Editor.Database;

namespace SDE.Databases.Quests.Features {
	/// <summary>
	/// Interaction logic for QuestView.xaml
	/// </summary>
	public partial class QuestViewYaml : UserControl, IDatabaseView {
		private QuestViewModel _viewModel;
		private DbTab _tab;
		private EditableListController<QuestTargetViewModel, QuestTarget, QuestReaderYaml> _targetList;
		private EditableListController<QuestDropViewModel, QuestDrop, QuestReaderYaml> _dropList;
		private EditableListController<MapMobTargetViewModel, MapMobTarget, QuestReaderYaml> _mapMobTargetList;

		public QuestViewYaml() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new QuestViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvTargets, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Mob ID", DisplayExpression = nameof(QuestTargetViewModel.DisplayMobId), FixedWidth = 50, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(QuestTargetViewModel.DisplayMobId) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Description", DisplayExpression = nameof(QuestTargetViewModel.DisplayMobName), IsFill = true, ToolTipBinding = nameof(QuestTargetViewModel.DisplayMobName), MinWidth = 100, TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Count", DisplayExpression = nameof(QuestDropViewModel.Count), FixedWidth = 50, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(QuestDropViewModel.Count) },
			}, new DefaultListViewComparer<QuestTargetViewModel>(true, nameof(QuestTargetViewModel.DisplayMobId)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvDrops, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Item ID", DisplayExpression = nameof(QuestDropViewModel.DisplayNameId), FixedWidth = 70, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(QuestDropViewModel.DisplayNameId) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Name", DisplayExpression = nameof(QuestDropViewModel.DisplayItemName), IsFill = true, ToolTipBinding = nameof(QuestDropViewModel.DisplayItemName), MinWidth = 100, TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Rate", DisplayExpression = nameof(QuestDropViewModel.DropRatePreview), FixedWidth = 60, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(QuestDropViewModel.DropRatePreview) },
			}, new DefaultListViewComparer<QuestDropViewModel>(true, nameof(QuestDropViewModel.DisplayNameId)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			_targetList = new EditableListController<QuestTargetViewModel, QuestTarget, QuestReaderYaml>(_lvTargets,
				copy: items => _viewModel.CopyTargets(items),
				pasteLoadYaml: (reader, p) => reader.LoadQuestTarget(p),
				changeListFunc: _viewModel.ChangeTargets,
				newModel: () => new QuestTarget() { Mob = "1002", Count = "10" },
				select: model => (DataSources.Mob, model.Mob),
				modes: new EditableListMode[] { EditableListMode.Default },
				sortEntriesOnSourceChanged: false
			);

			_dropList = new EditableListController<QuestDropViewModel, QuestDrop, QuestReaderYaml>(_lvDrops,
				copy: items => _viewModel.CopyDrops(items),
				pasteLoadYaml: (reader, p) => reader.LoadQuestDrop(p),
				changeListFunc: _viewModel.ChangeDrops,
				newModel: () => new QuestDrop() { Item = "501", Mob = "1002", Count = "1" },
				select: model => (DataSources.Item, model.Item),
				modes: new EditableListMode[] { EditableListMode.Default },
				sortEntriesOnSourceChanged: false
			);

			_mapMobTargetList = new EditableListController<MapMobTargetViewModel, MapMobTarget, QuestReaderYaml>(_lvMapMobTargets,
				copy: items => _viewModel.CopyMapMobTargets(items),
				pasteLoadYaml: (reader, p) => reader.LoadMapMobTarget(p as ParserKeyValue),
				changeListFunc: _viewModel.ChangeMobTarget,
				newModel: () => new MapMobTarget() { MobName = "1002", Active = true },
				select: model => (DataSources.Mob, model.MobName),
				modes: new EditableListMode[] { EditableListMode.Default },
				sortEntriesOnSourceChanged: false
			);

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

			_targetList.SelectPrevious();
			_dropList.SelectPrevious();
			if (_tab.Table.Commands.IsLocked)
				return;
			Core.Extensions.ClearUndos(_gridProperties);
		}

		#region Targets ListView
		private void _lvTargets_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_viewModel.SelectedTarget = _lvTargets.SelectedItem as QuestTargetViewModel;

			_targetList.SaveSelection();
			_targetList.SelectPrevious();
			if (_tab.Table.Commands.IsLocked)
				return;
			Core.Extensions.ClearUndos(_gridTargets);
		}
		#endregion

		#region Drops ListView
		private void _lvDrops_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_viewModel.SelectedDrop = _lvDrops.SelectedItem as QuestDropViewModel;

			_dropList.SaveSelection();
			_dropList.SelectPrevious();
			if (_tab.Table.Commands.IsLocked)
				return;
			Core.Extensions.ClearUndos(_gridDrops);
		}
		#endregion

		#region MapMobTargets ListView
		private void _lvMapMobTargets_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_mapMobTargetList.SaveSelection();
			_mapMobTargetList.SelectPrevious();
		}
		#endregion
	}
}
