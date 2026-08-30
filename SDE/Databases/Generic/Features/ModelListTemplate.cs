using Database.Commands;
using ErrorManager;
using GRF.GrfSystem;
using Lua.Structure;
using SDE.ApplicationConfiguration;
using SDE.Editor.Navigation;
using SDE.Editor.Parsers;
using SDE.Editor.Parsers.Yaml;
using SDE.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;
using Utilities;
using Utilities.Services;

namespace SDE.Databases.Generic.Features {
	public enum EditableListMode {
		Copy,
		PasteLua,
		PasteYaml,
		PasteCsv,
		Select,
		Delete,
		New,
		Edit,
		Default,
		DefaultWithEdit,
		MoveUp,
		MoveDown,
	}

	public class EditableListController<TViewModel, TModel, TDatabaseReader> where TViewModel : BaseModelView<TModel> where TDatabaseReader : new() where TModel : class, new() {
		private TDatabaseReader _databaseReader;
		private ListBox _listView;
		private Action<List<TModel>> _copy;
		private Func<TDatabaseReader, ParserObject, TModel> _pasteLoadYaml;
		private Func<TDatabaseReader, LBaseType, TModel> _pasteLoadLua;
		private Func<TDatabaseReader, string[], TModel> _pasteLoadCsv;
		private Action<List<TModel>, ListCommandMode> _changeListFunc;
		private Func<TModel> _newModel;
		private Func<TModel, (DataSource, string)> _select;
		private Func<bool> _isEnabled;
		private Action<TViewModel, TModel> _editFunc;
		private int _maxEntryCount;
		public int LastSelectionIndex = 0;
		public TModel LastSelectedModel = null;
		public List<TModel> LastSelectedModels = new List<TModel>();

		public EditableListController(ListBox list,
			Action<List<TModel>> copy = null,
			Func<TDatabaseReader, ParserObject, TModel> pasteLoadYaml = null,
			Func<TDatabaseReader, LBaseType, TModel> pasteLoadLua = null,
			Func<TDatabaseReader, string[], TModel> pasteLoadCsv = null,
			Action<List<TModel>, ListCommandMode> changeListFunc = null,
			Func<TModel> newModel = null,
			Func<TModel, (DataSource, string)> select = null,
			Func<bool> isEnabled = null,
			Action<TViewModel, TModel> editFunc = null,
			EditableListMode[] modes = null,
			bool sortEntriesOnSourceChanged = true,
			int maxEntryCount = int.MaxValue
		) {
			_databaseReader = new TDatabaseReader();
			_listView = list;
			_copy = copy;
			_pasteLoadYaml = pasteLoadYaml;
			_pasteLoadLua = pasteLoadLua;
			_pasteLoadCsv = pasteLoadCsv;
			_changeListFunc = changeListFunc;
			_newModel = newModel;
			_select = select;
			_isEnabled = isEnabled ?? (() => true);
			_editFunc = editFunc;
			_maxEntryCount = maxEntryCount;

			if (sortEntriesOnSourceChanged && list is ListView listView)
				ListViewExtensions.SetCustomSortOnItemsSourceChanged(listView);

			if (modes != null) {
				_addMenus(modes);
			}
		}

		private void _addMenus(EditableListMode[] modes) {
			if (_listView.ContextMenu == null)
				_listView.ContextMenu = new ContextMenu();

			foreach (var mode in modes) {
				switch (mode) {
					case EditableListMode.Default:
						AddCopyMenu();
						AddPasteMenu();
						AddSelectMenu();
						AddDeleteMenu();
						AddNewMenu();
						break;
					case EditableListMode.DefaultWithEdit:
						AddSelectMenu();
						AddEditMenu();
						AddDeleteMenu();
						AddSeparator();
						AddCopyMenu();
						AddPasteMenu();
						AddNewMenu();
						break;
					case EditableListMode.Copy:
						AddCopyMenu();
						break;
					case EditableListMode.PasteLua:
						AddPasteLuaMenu();
						break;
					case EditableListMode.PasteYaml:
						AddPasteYamlMenu();
						break;
					case EditableListMode.Select:
						AddSelectMenu();
						break;
					case EditableListMode.Delete:
						AddDeleteMenu();
						break;
					case EditableListMode.New:
						AddNewMenu();
						break;
					case EditableListMode.Edit:
						AddEditMenu();
						break;
					case EditableListMode.MoveUp:
						AddMoveUpMenu();
						break;
					case EditableListMode.MoveDown:
						AddMoveDownMenu();
						break;
				}
			}
		}

		public void AddCopyMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Copy", RequiresItem = true, ShortcutCmd = "Application.Copy" };
			mi.SetValue(WpfProperties.ImagePathProperty, "copy.png");
			mi.Click += (s, e) => Copy();
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.Copy, mi, _listView);
		}

		public void AddPasteMenu() {
			if (_pasteLoadCsv != null)
				AddPasteCsvMenu();
			else if (_pasteLoadLua != null)
				AddPasteLuaMenu();
			else if (_pasteLoadYaml != null)
				AddPasteYamlMenu();
		}

		public void AddPasteYamlMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Paste", ShortcutCmd = "Application.Paste" };
			mi.SetValue(WpfProperties.ImagePathProperty, "paste.png");
			mi.Click += (s, e) => PasteYaml();
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.Paste, mi, _listView);
		}

		public void AddPasteLuaMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Paste", ShortcutCmd = "Application.Paste" };
			mi.SetValue(WpfProperties.ImagePathProperty, "paste.png");
			mi.Click += (s, e) => PasteLua();
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.Paste, mi, _listView);
		}

		public void AddPasteCsvMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Paste", ShortcutCmd = "Application.Paste" };
			mi.SetValue(WpfProperties.ImagePathProperty, "paste.png");
			mi.Click += (s, e) => PasteCsv();
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.Paste, mi, _listView);
		}

		public TkMenuItem AddSelectMenu(Func<TModel, (DataSource, string)> select = null) {
			select = select ?? _select;

			if (select == null)
				return null;

			TkMenuItem mi = new TkMenuItem() { Header = "Select", RequiresItem = true, ShortcutCmd = "Application.Select" };
			mi.SetValue(WpfProperties.ImagePathProperty, "arrowdown.png");
			mi.Click += (s, e) => Select(select);
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.Select, mi, _listView);
			return mi;
		}

		public TkMenuItem AddDeleteMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Remove", RequiresItem = true, ShortcutCmd = "Application.Delete" };
			mi.SetValue(WpfProperties.ImagePathProperty, "delete.png");
			mi.Click += (s, e) => Remove();
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.Delete, mi, _listView);
			return mi;
		}

		public TkMenuItem AddNewMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Add", ShortcutCmd = "Application.New" };
			mi.SetValue(WpfProperties.ImagePathProperty, "add.png");
			mi.Click += (s, e) => AddNew();
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.New, mi, _listView);
			return mi;
		}

		public TkMenuItem AddEditMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Edit", ShortcutCmd = "Application.Edit" };
			mi.SetValue(WpfProperties.ImagePathProperty, "properties.png");
			mi.Click += (s, e) => Edit();
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.Edit, mi, _listView);
			_listView.MouseDoubleClick += _listView_MouseDoubleClick;
			return mi;
		}

		public void AddMoveUpMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Move up", ShortcutCmd = "Application.MoveUp" };
			mi.SetValue(WpfProperties.ImagePathProperty, "arrowup.png");
			mi.Click += (s, e) => AddMove(-1);
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.MoveUp, mi, _listView);
		}

		public void AddMoveDownMenu() {
			TkMenuItem mi = new TkMenuItem() { Header = "Move down", ShortcutCmd = "Application.MoveDown" };
			mi.SetValue(WpfProperties.ImagePathProperty, "arrowdown.png");
			mi.Click += (s, e) => AddMove(1);
			_listView.ContextMenu.Items.Add(mi);
			ApplicationShortcut.Link(SdeCommands.MoveDown, mi, _listView);
		}

		private void AddMove(int indexShift) {
			if (!_isEnabled())
				return;

			try {
				var models = _listView.Items.OfType<TViewModel>().Select(p => p.Model).ToList();
				List<int> indexes = new List<int>();
				List<TModel> selectedModels = _listView.SelectedItems.OfType<TViewModel>().Select(p => p.Model).ToList();
				var selectedModel = (_listView.SelectedItem as TViewModel).Model;

				foreach (var model in selectedModels) {
					indexes.Add(models.IndexOf(model));
				}

				foreach (var index in indexes) {
					var nIndex = index + indexShift;

					if (nIndex < 0 || nIndex >= models.Count)
						return;
				}

				if (indexShift < 0)
					indexes = indexes.OrderBy(p => p).ToList();
				else
					indexes = indexes.OrderByDescending(p => p).ToList();

				foreach (var index in indexes) {
					if (indexShift < 0) {
						var model = models[index];
						models.RemoveAt(index);
						models.Insert(index + indexShift, model);
					}
					else {
						var model = models[index];
						models.RemoveAt(index);
						models.Insert(index + indexShift, model);
					}
				}

				_changeListFunc(models, ListCommandMode.ChangeList);

				_listView.SelectedItems.Clear();

				Dictionary<TModel, TViewModel> link = new Dictionary<TModel, TViewModel>();

				foreach (var viewModel in _listView.Items.OfType<TViewModel>())
					link[viewModel.Model] = viewModel;

				foreach (TModel model in selectedModels)
					_listView.SelectedItems.Add(link[model]);

				_listView.SelectedItem = link[selectedModel];
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void Execute(Action action) {
			if (!_isEnabled())
				return;

			try {
				action();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void ExecuteOnSelection(Action<TViewModel, TModel> action) {
			if (!_isEnabled())
				return;

			try {
				if (_listView.SelectedItem is TViewModel vm) {
					action(vm, vm.Model);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _listView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			if (_listView.GetObjectAtPoint<ListViewItem>(e.GetPosition(_listView)) != null)
				ApplicationShortcut.ExecuteCommand(SdeCommands.Edit, _listView);
		}

		public void AddSeparator() {
			_listView.ContextMenu.Items.Add(new Separator());
		}

		public void Copy() {
			if (!_isEnabled())
				return;

			try {
				if (_copy == null)
					throw new Exception("Copy method has not been set.");

				_copy(_listView.SelectedItems.OfType<TViewModel>().Select(p => p.Model).ToList());
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void Cut() {
			Copy();
			Remove();
		}

		public void PasteLua() {
			if (!_isEnabled())
				return;

			try {
				var list = EditableListControllerHelper.ReadClipboardLua();

				if (list == null)
					return;

				List<TModel> entries = new List<TModel>();
				bool maxReached = false;

				foreach (LBaseType entry in list) {
					if (_listView.Items.Count + entries.Count >= _maxEntryCount) {
						maxReached = true;
						break;
					}

					entries.Add(_pasteLoadLua(_databaseReader, entry));
				}

				if (entries.Count > 0) {
					_changeListFunc(entries, ListCommandMode.Add);
					LastSelectedModel = entries[0];
					LastSelectedModels.Clear();
					LastSelectedModels.AddRange(entries);
					SelectPrevious(force: true);
				}

				if (maxReached)
					throw new ArgumentException($"You cannot add more than {_maxEntryCount} items. Delete an item and then add a new one.");
			}
			catch (ArgumentException err) {
				ErrorHandler.HandleException(err);
			}
			catch (Exception err) {
				ErrorHandler.HandleException("Failed to parse clipboard data when pasting. Verify that the content matches with the list format.", err);
			}
		}

		public void PasteYaml() {
			if (!_isEnabled())
				return;

			try {
				var parser = EditableListControllerHelper.ReadClipboardYaml();

				if (parser == null)
					return;

				List<TModel> entries = new List<TModel>();
				bool maxReached = false;

				var parserData = parser.Output["copy_paste"] ?? parser.Output["Body"];

				if (parserData == null)
					parserData = parser.Output;

				foreach (var entry in parserData) {
					if (_listView.Items.Count + entries.Count >= _maxEntryCount) {
						maxReached = true;
						break;
					}

					entries.Add(_pasteLoadYaml(_databaseReader, entry));
				}

				if (entries.Count > 0) {
					_changeListFunc(entries, ListCommandMode.Add);
					LastSelectedModel = entries[0];
					LastSelectedModels.Clear();
					LastSelectedModels.AddRange(entries);
					SelectPrevious(force: true);
				}

				if (maxReached)
					throw new ArgumentException($"You cannot add more than {_maxEntryCount} items. Delete an item and then add a new one.");
			}
			catch (ArgumentException err) {
				ErrorHandler.HandleException(err);
			}
			catch (Exception err) {
				ErrorHandler.HandleException("Failed to parse clipboard data when pasting. Verify that the content matches with the list format.", err);
			}
		}

		public void PasteCsv() {
			if (!_isEnabled())
				return;

			try {
				var data = Clipboard.GetText();

				if (string.IsNullOrEmpty(data))
					return;

				var bytes = EncodingService.DisplayEncoding.GetBytes(data);
				var lines = TextFileHelper.GetElementsByCommasAll(bytes).ToList();
				List<TModel> entries = new List<TModel>();
				bool maxReached = false;

				foreach (string[] elements in lines) {
					if (_listView.Items.Count + entries.Count >= _maxEntryCount) {
						maxReached = true;
						break;
					}

					entries.Add(_pasteLoadCsv(_databaseReader, elements));
				}

				if (entries.Count > 0) {
					_changeListFunc(entries, ListCommandMode.Add);
					LastSelectedModel = entries[0];
					LastSelectedModels.Clear();
					LastSelectedModels.AddRange(entries);
					SelectPrevious(force: true);
				}

				if (maxReached)
					throw new ArgumentException($"You cannot add more than {_maxEntryCount} items. Delete an item and then add a new one.");
			}
			catch (ArgumentException err) {
				ErrorHandler.HandleException(err);
			}
			catch (Exception err) {
				ErrorHandler.HandleException("Failed to parse clipboard data when pasting. Verify that the content matches with the list format.", err);
			}
		}

		public void AddNew() {
			if (!_isEnabled())
				return;

			try {
				var model = _newModel();
				AddAndSelect(model);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void AddAndSelect(TModel model) {
			if (model == null)
				return;

			var commands = SdeEditor.Instance.FindTopmostTab().Database.Table.Commands;

			if (_listView.Items.Count >= _maxEntryCount) {
				throw new Exception($"You cannot add more than {_maxEntryCount} items. Delete an item and then add a new one.");
			}

			_changeListFunc(new List<TModel> { model }, ListCommandMode.Add);
			_listView.SelectedItem = _listView.Items.OfType<TViewModel>().FirstOrDefault(p => ReferenceEquals(p.Model, model));
		}

		public void Select(Func<TModel, (DataSource, string)> select = null) {
			if (!_isEnabled())
				return;

			try {
				var item = _listView.SelectedItem as TViewModel;

				if (item == null)
					return;

				var selection = (select ?? _select)(item.Model);

				if (int.TryParse(selection.Item2, out int value) && value > 0)
					TabNavigation.Select(selection.Item1, value);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void Remove() {
			if (!_isEnabled())
				return;

			try {
				_changeListFunc(_listView.SelectedItems.OfType<TViewModel>().Select(p => p.Model).ToList(), ListCommandMode.Remove);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void SelectPrevious(bool force = false) {
			try {
				// There is already something selected, do not select unless a forced reselection is requested.
				if (!force && _listView.SelectedItem != null)
					return;

				if (_listView.Items.Count > 0) {
					if (LastSelectedModels.Count > 0 && LastSelectedModel != null) {
						Dictionary<TViewModel, TModel> links = new Dictionary<TViewModel, TModel>();
						Dictionary<TModel, TViewModel> links2 = new Dictionary<TModel, TViewModel>();

						foreach (var viewModel in _listView.Items.OfType<TViewModel>()) {
							links[viewModel] = viewModel.Model;
							links2[viewModel.Model] = viewModel;
						}

						_listView.SelectedItems.Clear();

						foreach (var model in LastSelectedModels) {
							if (links2.ContainsKey(model))
								_listView.SelectedItems.Add(links2[model]);
						}

						if (links2.ContainsKey(LastSelectedModel))
							_listView.SelectedItem = links2[LastSelectedModel];
						else {
							LastSelectedModels.Clear();
							LastSelectedModel = null;
						}
					}

					if (LastSelectedModel != null)
						_listView.SelectedItem = _listView.Items.OfType<TViewModel>().FirstOrDefault(p => ReferenceEquals(p.Model, LastSelectedModel));

					if (_listView.SelectedItem == null)
						_listView.SelectedIndex = Methods.Clamp(LastSelectionIndex, 0, _listView.Items.Count - 1);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void SaveSelection() {
			try {
				if (_listView.SelectedIndex > -1) {
					LastSelectionIndex = _listView.SelectedIndex;
					LastSelectedModel = (_listView.SelectedItem as TViewModel).Model;
					LastSelectedModels = _listView.SelectedItems.OfType<TViewModel>().Select(p => p.Model).ToList();
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void Edit() {
			if (!_isEnabled())
				return;

			try {
				if (_listView.SelectedItem is TViewModel vm) {
					_editFunc(vm, vm.Model);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void MoveAt() {
			throw new NotImplementedException();
		}

		public void InsertAt() {
			throw new NotImplementedException();
		}
	}

	public static class EditableListControllerHelper {
		public static YamlParser ReadClipboardYaml() {
			var data = Clipboard.GetText();

			if (string.IsNullOrEmpty(data))
				return null;

			var file = TemporaryFilesManager.GetTemporaryFilePath("clipboard_{0:0000}.yml");
			File.WriteAllText(file, data);
			YamlParser parser = new YamlParser(file);

			if (parser.Output == null || ((ParserArray)parser.Output).Objects.Count == 0)
				return null;

			return parser;
		}

		public static LList ReadClipboardLua() {
			var data = Clipboard.GetText();

			if (string.IsNullOrEmpty(data))
				return null;

			var bytes = EncodingService.DisplayEncoding.GetBytes(data);
			var parser = new Lua.Parser("clipboard", bytes);
			return parser.Parse(EncodingService.DisplayEncoding);
		}
	}
}
