using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary.WPF.Styles.ListView;
using TokeiLibrary;
using ErrorManager;
using SDE.Databases.Mobs.Parser;
using SDE.View.Dialogs;
using SDE.Core;
using SDE.Databases.MobSkills.Features;
using SDE.Databases.MobSkills.Parser;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.Shortcuts;
using SDE.ApplicationConfiguration;
using Utilities.Commands;
using SDE.View;
using SDE.View.Editors;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Mobs.Features {
	/// <summary>
	/// Interaction logic for MobView.xaml
	/// </summary>
	public partial class MobViewCsv : UserControl, IDatabaseView {
		private MobViewModel _viewModel;
		private DbTab _tab;
		private EditableListController<ItemDropViewModel, ItemDrop, MobReaderYaml> _dropList;
		private EditableListController<ItemDropViewModel, ItemDrop, MobReaderYaml> _mvpDropList;
		private EditableListController<MobSkillViewModel, MobSkill, MobSkillReaderCsv> _mobSkillList;

		public MobViewCsv() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new MobViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvDrops, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Item ID", DisplayExpression = nameof(ItemDropViewModel.Item), FixedWidth = 45, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ItemDropViewModel.Item) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Name", DisplayExpression = nameof(ItemDropViewModel.ItemPreview), IsFill = true, ToolTipBinding = nameof(ItemDropViewModel.ItemPreview), MinWidth = 40, TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Drop %", DisplayExpression = nameof(ItemDropViewModel.RatePreview), SearchGetAccessor = nameof(ItemDropViewModel.Rate), ToolTipBinding = nameof(ItemDropViewModel.RatePreview), FixedWidth = 60, TextAlignment = TextAlignment.Right },
			}, new DefaultListViewComparer<ItemDropViewModel>(true, nameof(ItemDropViewModel.Item)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvMvpDrops, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Item ID", DisplayExpression = nameof(ItemDropViewModel.Item), FixedWidth = 45, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(ItemDropViewModel.Item) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Name", DisplayExpression = nameof(ItemDropViewModel.ItemPreview), IsFill = true, ToolTipBinding = nameof(ItemDropViewModel.ItemPreview), MinWidth = 40, TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Drop %", DisplayExpression = nameof(ItemDropViewModel.RatePreview), SearchGetAccessor = nameof(ItemDropViewModel.Rate), ToolTipBinding = nameof(ItemDropViewModel.RatePreview), FixedWidth = 60, TextAlignment = TextAlignment.Right },
			}, new DefaultListViewComparer<ItemDropViewModel>(true, nameof(ItemDropViewModel.Item)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvMobSkills, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Skill", DisplayExpression = nameof(MobSkillViewModel.SkillNamePreview), ToolTipBinding = nameof(MobSkillViewModel.SkillId), FixedWidth = 60, TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Condition", DisplayExpression = nameof(MobSkillViewModel.ConditionPreview), ToolTipBinding = nameof(MobSkillViewModel.ConditionPreview), IsFill = true, TextAlignment = TextAlignment.Left, TextWrapping = TextWrapping.Wrap }
			}, new DefaultListViewComparer<MobSkillViewModel>(), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			_dropList = new EditableListController<ItemDropViewModel, ItemDrop, MobReaderYaml>(_lvDrops,
				copy: items => _viewModel.CopyDrops(items),
				pasteLoadYaml: (reader, p) => reader.LoadItemDrop(p),
				changeListFunc: _viewModel.ChangeDrops,
				newModel: () => _addDrop(isMvp: false, isCard: false),
				select: model => (DataSources.Item, model.Item),
				editFunc: (vm, m) => _editDrop(vm, m, isMvp: false),
				maxEntryCount: 10
			);

			CreateDropListContextMenu();

			_mvpDropList = new EditableListController<ItemDropViewModel, ItemDrop, MobReaderYaml>(_lvMvpDrops,
				copy: items => _viewModel.CopyDrops(items),
				pasteLoadYaml: (reader, p) => reader.LoadItemDrop(p),
				changeListFunc: _viewModel.ChangeMvpDrops,
				newModel: () => _addDrop(isMvp: true, isCard: false),
				select: model => (DataSources.Item, model.Item),
				editFunc: (vm, m) => _editDrop(vm, m, isMvp: true),
				modes: new EditableListMode[] { EditableListMode.DefaultWithEdit },
				maxEntryCount: 3
			);

			_mobSkillList = new EditableListController<MobSkillViewModel, MobSkill, MobSkillReaderCsv>(_lvMobSkills,
				copy: items => _viewModel.CopyMobSkills(items),
				pasteLoadCsv: (reader, p) => {
					MobSkill model = new MobSkill();
					reader.ReadEntry(model, p);
					return model;
				},
				changeListFunc: _viewModel.ChangeMobSkills,
				sortEntriesOnSourceChanged: false
			);

			var sources = new DataSource[] { DataSources.Skill, DataSources.SkillImport, DataSources.MobSkill, DataSources.MobSkillImport };

			foreach (var source in sources) {
				var db = SdeEditor.Project.GetDb(source);

				WeakEventManager<BaseDatabase, EventArgs>.AddHandler(db, nameof(BaseDatabase.TableModified), OnTableModified);
			}

			CreateMobSkillsContextMenu();

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Extensions.SetupZIndex(_gridContainer);
		}

		private void CreateDropListContextMenu() {
			_lvDrops.ContextMenu = new ContextMenu();

			_dropList.AddSelectMenu();
			_dropList.AddEditMenu();
			_dropList.AddDeleteMenu();
			_dropList.AddSeparator();
			_dropList.AddCopyMenu();
			_dropList.AddPasteCsvMenu();

			{
				TkMenuItem mi = new TkMenuItem() { Header = "Add normal drop", ShortcutCmd = "Application.New" };
				mi.SetValue(WpfProperties.ImagePathProperty, "add.png");
				mi.Click += (s, e) => _addDrop(isMvp: false, isCard: false);
				_lvDrops.ContextMenu.Items.Add(mi);
				ApplicationShortcut.Link(SdeCommands.New, mi, _lvDrops);
			}

			{
				TkMenuItem mi = new TkMenuItem() { Header = "Add card drop", ShortcutCmd = "Database.AddNewMvpDrop" };
				mi.SetValue(WpfProperties.ImagePathProperty, "add.png");
				mi.Click += (s, e) => _addDrop(isMvp: false, isCard: true);
				_lvDrops.ContextMenu.Items.Add(mi);
				ApplicationShortcut.Link(SdeCommands.DbAddNewMvpDrop, mi, _lvDrops);
			}
		}

		private void OnTableModified(object sender, EventArgs e) {
			if (sender is BaseDatabase db) {
				switch (db.Table.Commands.StackStatus) {
					case StackStatus.Undo:
					case StackStatus.Redo:
					case StackStatus.Restore:
					case StackStatus.Execute:
						if (_tab.List.SelectedItem != null)
							_updateAction(_tab.List.SelectedItem as ReadableTuple);
						break;
				}
			}
		}

		private void CreateMobSkillsContextMenu() {
			_lvMobSkills.ContextMenu = new ContextMenu();

			{
				TkMenuItem mi = new TkMenuItem() { Header = "Select skill", RequiresItem = true };
				mi.SetValue(WpfProperties.ImagePathProperty, "arrowdown.png");
				mi.Click += (s, e) => _mobSkillList.Select(model => (DataSources.Skill, model.SkillId));
				_lvMobSkills.ContextMenu.Items.Add(mi);
			}

			{
				TkMenuItem mi = new TkMenuItem() { Header = "Select mob skill", RequiresItem = true, ShortcutCmd = "Application.Select" };
				mi.SetValue(WpfProperties.ImagePathProperty, "arrowdown.png");
				mi.Click += (s, e) => _mobSkillList.Select(model => (DataSources.MobSkill, _viewModel.MobSkills2Tuple[model].Key.ToString()));
				_lvMobSkills.ContextMenu.Items.Add(mi);
				ApplicationShortcut.Link(SdeCommands.Select, mi, _lvMobSkills);
			}

			{
				var mi = _mobSkillList.AddDeleteMenu();
				mi.Header = "Remove mob skill";
			}

			_mobSkillList.AddSeparator();
			_mobSkillList.AddCopyMenu();
			_mobSkillList.AddPasteMenu();
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<Mob>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			//_dropList.SelectPrevious();

			Core.Extensions.ClearUndos(_gridContainer);
		}

		#region Drops ListView
		private void _lvDrops_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_dropList.SaveSelection();
			_dropList.SelectPrevious();
		}
		
		private bool _editDrop(ItemDropViewModel vm, ItemDrop model, bool isMvp, bool isNew = false) {
			try {
				DropEditDialog dialog = new DropEditDialog(model.Item ?? "", model.Rate ?? "0", DataSources.Item, !isMvp, 0);

				dialog._tbRandGroup.Text = model.RandomOptionGroup ?? "";
				dialog._tbStealProtected.IsChecked = model.StealProtected;

				dialog.Owner = WpfUtilities.TopWindow;

				if (dialog.ShowDialog() == true) {
					if (!Int32.TryParse(dialog.Id, out int id) || id <= 0)
						return false;

					if (!Extensions.GetIntFromFloatValue(dialog.DropChance, out int rate)) {
						ErrorHandler.HandleException("Invalid format (integer or float value only)");
						return false;
					}

					try {
						if (model.Item == id.ToString() &&
							model.Rate == rate.ToString() &&
							(model.RandomOptionGroup ?? "") == dialog.RandGroup &&
							model.StealProtected == dialog.StealProtected)
							return false;

						if (isNew) {
							model.Item = id.ToString();
							model.Rate = rate.ToString();
						}
						else {
							_tab.Table.Commands.Begin();
							_tab.Table.Commands.SetModelValue(_viewModel.Tuple, model, nameof(ItemDrop.Item), id.ToString());
							_tab.Table.Commands.SetModelValue(_viewModel.Tuple, model, nameof(ItemDrop.Rate), rate.ToString());
							_tab.Table.Commands.End();
							vm.OnPropertyChanged("");
						}

						return true;
					}
					catch {
						_tab.Table.Commands.CancelEdit();
						throw;
					}
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}

			return false;
		}

		private ItemDrop _addDrop(bool isMvp, bool isCard) {
			var model = _viewModel.Model;
			var itemDropIndex = isCard ? model.Drops.FindIndex(p => p.StealProtected) : -1;
			ItemDropViewModel vm = null;
			ItemDrop itemDrop = null;
			bool isNew = true;

			if (itemDropIndex == -1) {
				if (!isMvp && model.Drops.Count >= 10)
					throw new ArgumentException($"You cannot add more than 10 items. Delete an item and then add a new one.");

				itemDrop = new ItemDrop();
				vm = new ItemDropViewModel(_viewModel, itemDrop);
			}
			else {
				itemDrop = model.Drops[itemDropIndex];
				vm = _viewModel.Drops[itemDropIndex];
				isNew = false;
			}

			itemDrop.StealProtected = isCard;
			
			if (!isNew) {
				_editDrop(vm, itemDrop, isMvp, isNew: false);
				return null;
			}

			if (!_editDrop(vm, itemDrop, isMvp, isNew: true))
				return null;

			var list = isMvp ? _mvpDropList : _dropList;
			list.AddAndSelect(itemDrop);
			return null;
		}
		#endregion

		#region MvpDrops ListView
		private void _lvMvpDrops_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_mvpDropList.SaveSelection();
			_mvpDropList.SelectPrevious();
		}
		#endregion

		#region MobSkills ListView
		private void _lvMobSkills_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_mobSkillList.SaveSelection();
			_mobSkillList.SelectPrevious();
		}
		#endregion
	}
}
