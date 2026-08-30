using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using TokeiLibrary.WPF.Styles.ListView;
using System.Windows;
using SDE.Databases.Achievements.Parser;
using Lua.Structure;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.ClientAchievements.Features {
	/// <summary>
	/// Interaction logic for ClientAchvView.xaml
	/// </summary>
	public partial class ClientAchvViewLua : UserControl, IDatabaseView {
		private ClientAchvViewModel _viewModel;
		private DbTab _tab;
		private EditableListController<ClientAchvResourceViewModel, ClientAchvResource, ClientAchvReaderLua> _resourcesList;

		public ClientAchvViewLua() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new ClientAchvViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvResources, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "ID", DisplayExpression = nameof(ClientAchvResourceViewModel.PreviewId), FixedWidth = 50, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ClientAchvResourceViewModel.PreviewId) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Text", DisplayExpression = nameof(ClientAchvResourceViewModel.Text), IsFill = true, ToolTipBinding = nameof(ClientAchvResourceViewModel.Text), MinWidth = 100, TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Count", DisplayExpression = nameof(ClientAchvResourceViewModel.Count), FixedWidth = 50, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ClientAchvResourceViewModel.Count) },
				//new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Shortcut", DisplayExpression = nameof(ClientAchvResourceViewModel.Shortcut), FixedWidth = 50, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ClientAchvResourceViewModel.Shortcut) },
			}, new DefaultListViewComparer<ClientAchvResourceViewModel>(true, nameof(ClientAchvResourceViewModel.Id)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			_resourcesList = new EditableListController<ClientAchvResourceViewModel, ClientAchvResource, ClientAchvReaderLua>(_lvResources,
				copy: items => _viewModel.CopyResources(items),
				pasteLoadLua: (reader, p) => {
					var r = reader.LoadResource((LKeyValue)p);
					r.Id = "";
					return r;
				},
				changeListFunc: _viewModel.ChangeResources,
				newModel: () => _viewModel.Model.UiType == Common.ClientAchvUiType.UITYPE_TEXT ? new ClientAchvResource() { Text = "Objective to complete." } : new ClientAchvResource() { Text = "Hunt 10 Porings.", Count = "10" },
				sortEntriesOnSourceChanged: false,
				modes: new EditableListMode[] { EditableListMode.Default, EditableListMode.MoveUp, EditableListMode.MoveDown }
			);

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			ClientAchv model = tuple == null ? null : tuple.GetModel<ClientAchv>();

			if (tuple != null) {
				_viewModel.SetModel(tuple, model);
			}
			else {
				_viewModel.SetModel(null, null);
			}

			_resourcesList.SelectPrevious();

			Core.Extensions.ClearUndos(_gridContainer);
		}

		private void _lvResources_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_viewModel.SelectedResource = _lvResources.SelectedItem as ClientAchvResourceViewModel;

			_resourcesList.SaveSelection();
			_resourcesList.SelectPrevious();
			if (_tab.Table.Commands.IsLocked)
				return;
			Core.Extensions.ClearUndos(_gridResources);
		}
	}
}
