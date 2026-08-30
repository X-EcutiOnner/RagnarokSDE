using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using TokeiLibrary.WPF.Styles.ListView;
using System.Windows;
using SDE.Databases.ItemCombos.Parser;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using SDE.ApplicationConfiguration;
using System.Linq;
using SDE.Databases.Generic.Features;
using SDE.Editor.Navigation;
using SDE.Editor.Database;

namespace SDE.Databases.ItemCombos.Features {
	/// <summary>
	/// Interaction logic for QuestView.xaml
	/// </summary>
	public partial class ItemComboViewYaml : UserControl, IDatabaseView {
		private ItemComboViewModel _viewModel;
		private DbTab _tab;
		private EditableListController<ItemComboViewModel, ItemCombo, ItemComboReaderYaml> _linkedComboList;

		public ItemComboViewYaml() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new ItemComboViewModel(tab);
			_viewModel.SetModel(null, null);
			DataContext = _viewModel;

			_tab = tab;

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvLinkedCombos, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Combo ID", DisplayExpression = nameof(ItemComboViewModel.DisplayComboId), FixedWidth = 80, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ItemComboViewModel.DisplayComboId) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Items", DisplayExpression = nameof(ItemComboViewModel.DisplayComboName), IsFill = true, ToolTipBinding = nameof(ItemComboViewModel.DisplayComboName), MinWidth = 40 },
			}, new DefaultListViewComparer<ItemComboViewModel>(), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			_linkedComboList = new EditableListController<ItemComboViewModel, ItemCombo, ItemComboReaderYaml>(_lvLinkedCombos,
				copy: items => _viewModel.CopyLinkedCombos(items),
				modes: new EditableListMode[] { EditableListMode.Copy },
				sortEntriesOnSourceChanged: false
			);

			TkMenuItem mi = new TkMenuItem() { Header = "Select", RequiresItem = true, ShortcutCmd = "Application.Select" };
			mi.SetValue(WpfProperties.ImagePathProperty, "arrowdown.png");
			mi.Click += (s, e) => Select();
			_lvLinkedCombos.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.Select, mi, _lvLinkedCombos);

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		public void Select() {
			if (_lvLinkedCombos.SelectedItems.Count > 0) {
				TabNavigation.SelectList(DataSources.ItemCombo, _lvLinkedCombos.SelectedItems.OfType<ItemComboViewModel>().Select(p => p.Tuple.Key));
			}
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<ItemCombo>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}
	}
}
