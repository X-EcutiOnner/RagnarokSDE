using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary.WPF.Styles.ListView;
using TokeiLibrary;
using ErrorManager;
using SDE.Databases.Pets.Parser;
using System.Globalization;
using SDE.View;
using SDE.View.Editors;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Pets.Features {
	/// <summary>
	/// Interaction logic for PetView.xaml
	/// </summary>
	public partial class PetViewYaml : UserControl, IDatabaseView {
		private PetViewModel _viewModel;
		private DbTab _tab;
		private EditableListController<EvolutionViewModel, Evolution, PetReaderYaml> _evolutionList;
		private EditableListController<ItemRequirementViewModel, ItemRequirement, PetReaderYaml> _itemRequirementList;

		public PetViewYaml() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new PetViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvEvolutions, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Mob ID", DisplayExpression = nameof(EvolutionViewModel.DisplayTargetId), FixedWidth = 60, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(EvolutionViewModel.DisplayTargetId) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Name", DisplayExpression = nameof(EvolutionViewModel.DisplayTargetName), IsFill = true, ToolTipBinding = nameof(EvolutionViewModel.DisplayTargetName), MinWidth = 40, TextWrapping = TextWrapping.Wrap },
			}, new DefaultListViewComparer<EvolutionViewModel>(true, nameof(EvolutionViewModel.DisplayTargetId)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvRequirements, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Item ID", DisplayExpression = nameof(ItemRequirementViewModel.DisplayItemId), FixedWidth = 60, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ItemRequirementViewModel.DisplayItemId) },
				new ListViewDataTemplateHelper.ImageColumnInfo { Header = "", DisplayExpression = nameof(ItemRequirementViewModel.DataImage), FixedWidth = 26, MaxHeight = 24 },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Name", DisplayExpression = nameof(ItemRequirementViewModel.DisplayItemName), IsFill = true, ToolTipBinding = nameof(ItemRequirementViewModel.DisplayItemName), TextWrapping = TextWrapping.Wrap, MinWidth = 40 },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Amount", DisplayExpression = nameof(ItemRequirementViewModel.Amount), ToolTipBinding = nameof(ItemRequirementViewModel.Amount), FixedWidth = 60, TextAlignment = TextAlignment.Right },
			}, new DefaultListViewComparer<ItemRequirementViewModel>(true, nameof(ItemRequirementViewModel.DisplayItemId)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			_evolutionList = new EditableListController<EvolutionViewModel, Evolution, PetReaderYaml>(_lvEvolutions,
				copy: items => _viewModel.CopyEvolutions(items),
				pasteLoadYaml: (reader, p) => reader.LoadEvolution(p),
				changeListFunc: _viewModel.ChangeEvolutions,
				newModel: () => new Evolution() { Target = "1002" },
				select: model => (DataSources.Mob, model.Target),
				editFunc: (vm, m) => _editEvolution(vm, m),
				modes: new EditableListMode[] { EditableListMode.DefaultWithEdit }
			);

			_itemRequirementList = new EditableListController<ItemRequirementViewModel, ItemRequirement, PetReaderYaml>(_lvRequirements,
				copy: items => _viewModel.CopyItemRequirements(items),
				pasteLoadYaml: (reader, p) => reader.LoadItemRequirement(p),
				changeListFunc: _viewModel.ChangeItemRequirements,
				newModel: () => new ItemRequirement() { Item = "501", Amount = "1" },
				select: model => (DataSources.Item, model.Item),
				isEnabled: () => _viewModel.IsEvolutionSelected,
				editFunc: (vm, m) => _editRequirement(vm, m),
				modes: new EditableListMode[] { EditableListMode.DefaultWithEdit }
			);

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<Pet>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			_evolutionList.SelectPrevious();

			Core.Extensions.ClearUndos(_gridContainer);
		}

		#region Evolutions ListView
		private void _lvEvolutions_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_viewModel.SelectedEvolution = _lvEvolutions.SelectedItem as EvolutionViewModel;

			_evolutionList.SaveSelection();
			_evolutionList.SelectPrevious();
			_itemRequirementList.SelectPrevious();
		}

		private void _editEvolution(EvolutionViewModel vm, Evolution m) {
			var evolution = vm;

			if (evolution == null)
				return;

			SelectTupleDialog select = new SelectTupleDialog(SdeEditor.Project.GetMergedTable(DataSources.Pet), DataSources.Pet, evolution.Target);
			select.Owner = WpfUtilities.TopWindow;

			if (select.ShowDialog() == true)
				evolution.Target = select.Id;
		}
		#endregion

		#region ItemRequirements ListView
		private void _lvRequirements_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_viewModel.SelectedItemRequirement = _lvRequirements.SelectedItem as ItemRequirementViewModel;

			_itemRequirementList.SaveSelection();
			_itemRequirementList.SelectPrevious();
		}

		private void _editRequirement(ItemRequirementViewModel vm, ItemRequirement m) {
			var entry = vm;

			if (entry == null)
				return;

			DropEditDialog dialog = new DropEditDialog(entry.Item, entry.Amount.ToString(CultureInfo.InvariantCulture), DataSources.Item);
			dialog._tbDrop.Text = "Amount";
			dialog.Owner = WpfUtilities.TopWindow;

			if (dialog.ShowDialog() == true) {
				string sid = dialog.Id;
				string svalue = dialog.DropChance;
				int value;
				int id;

				Int32.TryParse(sid, out id);

				if (!Core.Extensions.GetIntFromFloatValue(svalue, out value)) {
					ErrorHandler.HandleException("Invalid format (integer or float value only)");
					return;
				}

				if (id <= 0) {
					return;
				}

				entry.Item = id.ToString();
				entry.Amount = value.ToString();
			}
		}
		#endregion
	}
}
