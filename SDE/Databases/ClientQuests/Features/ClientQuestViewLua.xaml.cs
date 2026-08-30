using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary.WPF.Styles.ListView;
using TokeiLibrary;
using ErrorManager;
using SDE.View.Dialogs;
using System.Globalization;
using Lua.Structure;
using SDE.Databases.ClientQuests.Parser;
using SDE.Core.Avalon;
using ICSharpCode.AvalonEdit;
using SDE.View;
using SDE.View.Editors;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.ClientQuests.Features {
	/// <summary>
	/// Interaction logic for PetView.xaml
	/// </summary>
	public partial class ClientQuestViewLua : UserControl, IDatabaseView {
		private ClientQuestViewModel _viewModel;
		private DbTab _tab;
		private EditableListController<ClientQuestRewardViewModel, ClientQuestReward, ClientQuestReaderLua> _rewardsList;

		public ClientQuestViewLua() {
			InitializeComponent();

			AvalonLoader.Load((_propDescription.Editor as Border).Child as TextEditor);
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new ClientQuestViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvRewards, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Name ID", DisplayExpression = nameof(ClientQuestRewardViewModel.Item), FixedWidth = 60, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ClientQuestRewardViewModel.Item) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Name", DisplayExpression = nameof(ClientQuestRewardViewModel.DisplayItemName), IsFill = true, ToolTipBinding = nameof(ClientQuestRewardViewModel.DisplayItemName), MinWidth = 40, TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Count", DisplayExpression = nameof(ClientQuestRewardViewModel.Count), FixedWidth = 50, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ClientQuestRewardViewModel.Count) },
			}, new DefaultListViewComparer<ClientQuestRewardViewModel>(true, nameof(ClientQuestRewardViewModel.Item)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			_rewardsList = new EditableListController<ClientQuestRewardViewModel, ClientQuestReward, ClientQuestReaderLua>(_lvRewards,
				copy: items => _viewModel.CopyRewards(items),
				pasteLoadLua: (reader, p) => reader.LoadReward((LList)p),
				changeListFunc: _viewModel.ChangeRewards,
				newModel: () => new ClientQuestReward() { Item = "501", Count = "1" },
				select: model => (DataSources.Item, model.Item),
				editFunc: (vm, m) => _editEvolution(vm, m),
				modes: new EditableListMode[] { EditableListMode.DefaultWithEdit }
			);

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

			_rewardsList.SelectPrevious();

			Core.Extensions.ClearUndos(_gridContainer);
		}

		#region Evolutions ListView
		private void _lvRewards_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_viewModel.SelectedReward = _lvRewards.SelectedItem as ClientQuestRewardViewModel;

			_rewardsList.SaveSelection();
			_rewardsList.SelectPrevious();
		}

		private void _editEvolution(ClientQuestRewardViewModel vm, ClientQuestReward m) {
			var entry = vm;

			if (entry == null)
				return;

			DropEditDialog dialog = new DropEditDialog(entry.Item, entry.Count.ToString(CultureInfo.InvariantCulture), DataSources.Item);
			dialog._tbDrop.Text = "Count";
			dialog.Owner = WpfUtilities.TopWindow;

			if (dialog.ShowDialog() == true) {
				string sid = dialog.Id;
				string svalue = dialog.DropChance;

				Int32.TryParse(sid, out int id);

				if (!Int32.TryParse(svalue, out int value)) {
					ErrorHandler.HandleException("Invalid format, expected an integer.");
					return;
				}

				if (id <= 0) {
					return;
				}

				entry.Item = id.ToString();
				entry.Count = value.ToString();
				entry.OnPropertyChanged("");
			}
		}
		#endregion
	}
}
