using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using TokeiLibrary.WPF.Styles.ListView;
using System.Windows;
using SDE.Databases.Items.Parser;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary;
using TokeiLibrary.WPF;
using ErrorManager;
using System;
using SDE.Databases.Mobs.Features;
using TokeiLibrary.Shortcuts;
using SDE.ApplicationConfiguration;
using System.Collections.Generic;
using Database.Commands;
using Utilities.Commands;
using SDE.View;
using SDE.View.Editors;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Items.Features {
	/// <summary>
	/// Interaction logic for ItemView.xaml
	/// </summary>
	public partial class ItemViewCsv : UserControl, IDatabaseView {
		private ItemViewModel _viewModel;
		private DbTab _tab;
		private EditableListController<MobDropViewModel, MobDrop, ItemReaderYaml> _mobDropList;

		public ItemViewCsv() {
			InitializeComponent();
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new ItemViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_lvMobDrops, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Mob ID", DisplayExpression = nameof(MobDropViewModel.Mob), FixedWidth = 60, TextAlignment = TextAlignment.Right, ToolTipBinding = nameof(MobDropViewModel.Mob) },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Name", DisplayExpression = nameof(MobDropViewModel.NamePreview), IsFill = true, ToolTipBinding = nameof(MobDropViewModel.NamePreview), MinWidth = 40, TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Drop %", DisplayExpression = nameof(MobDropViewModel.RatePreview), SearchGetAccessor = nameof(MobDropViewModel.Rate), ToolTipBinding = nameof(MobDropViewModel.RatePreview), FixedWidth = 60, TextAlignment = TextAlignment.Right },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Type", DisplayExpression = nameof(MobDropViewModel.Mvp), SearchGetAccessor = nameof(MobDropViewModel.Mvp), FixedWidth = 45, TextAlignment = TextAlignment.Center },
			}, new DefaultListViewComparer<MobDropViewModel>(true, nameof(MobDropViewModel.Mob)), null, defaultBrushOverride: "{Binding ForegroundBrush}");

			_mobDropList = new EditableListController<MobDropViewModel, MobDrop, ItemReaderYaml>(_lvMobDrops,
				editFunc: (vm, m) => _editMobDrop(vm, m),
				select: m => (DataSources.Mob, m.Mob.ToString()),
				sortEntriesOnSourceChanged: false
			);

			var sources = new DataSource[] { DataSources.Mob, DataSources.MobImport };

			foreach (var source in sources) {
				var db = SdeEditor.Project.GetDb(source);

				WeakEventManager<BaseDatabase, EventArgs>.AddHandler(db, nameof(BaseDatabase.TableModified), OnTableModified);
			}

			CreateMobDropContextMenu();

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
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

		private void CreateMobDropContextMenu() {
			_lvMobDrops.ContextMenu = new ContextMenu();

			_mobDropList.AddSelectMenu();

			{
				var mi = _mobDropList.AddEditMenu();
				mi.Header = "Edit drop chance";
				mi.RequiresItem = true;
			}

			{
				TkMenuItem mi = new TkMenuItem() { Header = "Remove", RequiresItem = true, ShortcutCmd = "Application.Delete" };
				mi.SetValue(WpfProperties.ImagePathProperty, "delete.png");
				mi.Click += (s, e) => _mobDropList.Execute(_delete);
				_lvMobDrops.ContextMenu.Items.Add(mi);
				ApplicationShortcut.Link(SdeCommands.Delete, mi, _lvMobDrops);
			}

			_mobDropList.AddSeparator();

			{
				TkMenuItem mi = new TkMenuItem() { Header = "Add as normal drop" };
				mi.SetValue(WpfProperties.ImagePathProperty, "add.png");
				mi.Click += (s, e) => _mobDropList.Execute(() => _addNew(false));
				_lvMobDrops.ContextMenu.Items.Add(mi);
			}
			
			{
				TkMenuItem mi = new TkMenuItem() { Header = "Add as MVP drop" };
				mi.SetValue(WpfProperties.ImagePathProperty, "add.png");
				mi.Click += (s, e) => _mobDropList.Execute(() => _addNew(true));
				_lvMobDrops.ContextMenu.Items.Add(mi);
			}
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			if (tuple != null) {
				_viewModel.SetModel(tuple, tuple.GetModel<Item>());
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}

		#region MobDrops ListView
		private bool _editMobDrop(MobDropViewModel vm, MobDrop model) {
			try {
				InputDialog dialog = new InputDialog("Enter the new drop rate (integer or float)", "Drop rate", model.ItemDrop.Rate);
				dialog.TextBoxInput.VerticalContentAlignment = VerticalAlignment.Center;
				dialog.Owner = WpfUtilities.TopWindow;
				dialog.TextBoxInput.Loaded += delegate {
					dialog.TextBoxInput.SelectAll();
					dialog.TextBoxInput.Focus();
				};

				if (dialog.ShowDialog() == true) {
					if (!Core.Extensions.GetIntFromFloatValue(dialog.Input, out int rate)) {
						throw new Exception("Invalid format (integer or float value only).");
					}

					// Retrieve source
					var mobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
					var tuple = mobDb.TryGetTuple(model.Mob);

					if (tuple == null)
						throw new Exception("Couldn't find mob ID '" + model.Mob + "' in any mob database.");

					mobDb.Commands.SetModelValue(tuple, model.ItemDrop, nameof(ItemDrop.Rate), rate.ToString(), false);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}

			return false;
		}

		private void _addNew(bool isMvp) {
			var btable = SdeEditor.Project.GetMergedTable(DataSources.Mob);

			DropEditDialog dialog = new DropEditDialog("", "10.0 %", DataSources.Mob, true);
			dialog.Owner = WpfUtilities.TopWindow;

			if (dialog.ShowDialog() == true) {
				if (!Int32.TryParse(dialog.Id, out int id) || id <= 0)
					return;

				if (!Core.Extensions.GetIntFromFloatValue(dialog.DropChance, out int rate)) {
					ErrorHandler.HandleException("Invalid format (integer or float value only)");
					return;
				}

				var tuple = btable.TryGetTuple(id);

				if (tuple == null) {
					ErrorHandler.HandleException("Mob id not found.");
					return;
				}

				var model = tuple.GetModel<Mob>();
				var drops = isMvp ? model.MvpDrops : model.Drops;
				btable.Commands.SetModelListValue(tuple, () => drops, new List<ItemDrop> { new ItemDrop() { Item = _viewModel.Tuple.Key.ToString(), Rate = rate.ToString() } }, ListCommandMode.Add);
			}

			btable.Commands.EndEdit();
		}

		private void _delete() {
			var lv = _lvMobDrops;

			if (lv.SelectedItems.Count <= 0)
				return;

			var btable = SdeEditor.Project.GetMergedTable(DataSources.Mob);
			btable.Commands.Begin();

			Dictionary<int, List<ItemDrop>> drops = new Dictionary<int, List<ItemDrop>>();
			Dictionary<int, List<ItemDrop>> mvpDrops = new Dictionary<int, List<ItemDrop>>();

			for (int i = 0; i < lv.SelectedItems.Count; i++) {
				var viewModel = (MobDropViewModel)lv.SelectedItems[i];

				var dico = viewModel.IsMvp ? mvpDrops : drops;
				var mobId = viewModel.Model.Mob;

				if (!dico.TryGetValue(mobId, out var listDrops)) {
					listDrops = new List<ItemDrop>();
					dico[mobId] = listDrops;
				}

				listDrops.Add(viewModel.Model.ItemDrop);
			}

			foreach (var entry in drops) {
				var tuple = btable.TryGetTuple(entry.Key);

				if (tuple == null)
					continue;

				var model = tuple.GetModel<Mob>();
				btable.Commands.SetModelListValue(tuple, () => model.Drops, entry.Value, ListCommandMode.Remove);
			}

			foreach (var entry in mvpDrops) {
				var tuple = btable.TryGetTuple(entry.Key);

				if (tuple == null)
					continue;

				var model = tuple.GetModel<Mob>();
				btable.Commands.SetModelListValue(tuple, () => model.MvpDrops, entry.Value, ListCommandMode.Remove);
			}

			btable.Commands.End();
		}

		private void _lvMobDrops_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_mobDropList.SaveSelection();
			_mobDropList.SelectPrevious();
		}
		#endregion
	}
}
