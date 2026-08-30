using Database.Commands;
using ErrorManager;
using SDE.ApplicationConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using System.Windows;
using System.Windows.Controls;
using Database;
using SDE.View;
using TokeiLibrary.WPF;
using Utilities;
using Utilities.Extension;
using SDE.View.Dialogs;
using System.IO;
using GRF.GrfSystem;
using System.Globalization;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Navigation;
using SDE.Editor.Files;

namespace SDE.Editor.Generic.DbTabs {
	public class DbTabCommandManager {
		private DbTab _tab;

		public DbTabCommandManager(DbTab tab) {
			_tab = tab;
		}

		public void Cut() => SafeExecute(_cut);
		public void DeleteItems() => SafeExecute(_deleteItems);
		public void CopyItemTo() => SafeExecute(() => _copyTo(_tab.SelectedItem));
		public void CopyItemTo(BaseDatabase db) => SafeExecute(() => _copyTo(_tab.ListView.SelectedItems.OfType<ReadableTuple>().ToList(), db));
		public void ChangeId() => SafeExecute(() => _changeId(_tab.SelectedItem));
		public void ChangeKey(int key) => ChangeKey(_tab.ListView.SelectedItem as ReadableTuple, key);
		public void ShowSelectedOnly() => SafeExecute(_showSelectedOnly);
		public void AddNewItem() => SafeExecute(_addNewItem);
		public void AddNewItemRaw() => SafeExecute(_addNewItemRaw);
		public void SelectInNotepad() => SafeExecute(_selectInNotepad);

		public void ChangeKey(ReadableTuple item, int key) {
			if (_tab.Table.ContainsKey(key)) {
				if (WindowProvider.ShowDialog("An item with this ID already exists. Would you like to replace it?", "ID already exists", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes) {
					try {
						_tab.Table.Commands.Begin();
						_tab.Table.Commands.Delete(key);
						_tab.Table.Commands.ChangeKey(item.GetValue<int>(_tab.Settings.AttributeList.PrimaryAttribute), key, ChangeKeyCallback);
					}
					finally {
						_tab.Table.Commands.End();
						_tab.ListView.ScrollToCenterOfView(_tab.ListView.SelectedItem);
					}
				}

				return;
			}

			_tab.Table.Commands.ChangeKey(item.GetValue<int>(_tab.Settings.AttributeList.PrimaryAttribute), key, ChangeKeyCallback);
			_tab.ListView.ScrollToCenterOfView(_tab.ListView.SelectedItem);
		}

		private void _cut() {
			ApplicationShortcut.ExecuteCommand(SdeCommands.Copy, _tab);
			_deleteItems();
		}

		private void _deleteItems() {
			if (_tab.ListView.SelectedItems.Count > 0) {
				List<ITableCommand<int, ReadableTuple>> commands = new List<ITableCommand<int, ReadableTuple>>();
				_tab.SearchEngine.Collection.Disable();

				for (int index = 0; index < _tab.ListView.SelectedItems.Count; index++) {
					ReadableTuple item = (ReadableTuple)_tab.ListView.SelectedItems[index];
					commands.Add(new DeleteTuple<int, ReadableTuple>(item.Key, ItemDeletedCallback));
				}

				_tab.Table.Commands.AddGroupedCommands(commands, SelectLastSelectedCallback);
				_tab.SearchEngine.Collection.UpdateAndEnable();
			}
		}

		private void _copyTo(ReadableTuple item) {
			try {
				if (item == null)
					return;

				int id = _tab.GetNewItemId(item.Key, true);
				int oldId = item.GetValue<int>(_tab.Settings.AttributeList.PrimaryAttribute);

				if (_tab.Table.ContainsKey(id)) {
					if (WindowProvider.ShowDialog("An item with this ID already exists (\"" + _tab.Table.TryGetTuple(id)[_tab.Settings.AttDisplay.Index].ToString().RemoveBreakLines() + "\"). Do you want to replace it?",
						"Item already exists", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
						return;
				}

				_tab.Table.Commands.CopyTupleTo(oldId, id, CopyToCallback);
			}
			catch (KeyInvalidException) {
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _copyTo(List<ReadableTuple> items, BaseDatabase db) {
			try {
				if (items == null || items.Count == 0)
					return;

				if ((_tab.Database.Source & DataSources.MobSkillsItems) != 0) {
					try {
						db.Table.Commands.Begin();

						for (int i = 0; i < items.Count; i++) {
							var item = items[i];

							int oldKey = item.GetValue<int>(_tab.Settings.AttributeList.PrimaryAttribute);
							int newKey = db.Table.GenerateUniqueId();

							if (i == items.Count - 1)
								db.Table.Commands.CopyTupleTo(_tab.Table, oldKey, newKey, (a, b, c, d, e) => CopyToCallback2(db, c, d, e));
							else
								db.Table.Commands.CopyTupleTo(_tab.Table, oldKey, newKey, (a, b, c, d, e) => CopyToCallback3(c, d, e));
						}
					}
					catch (Exception err) {
						db.Table.Commands.CancelEdit();
						ErrorHandler.HandleException(err);
					}
					finally {
						db.Table.Commands.End();
					}
					return;
				}

				if (db.Source == _tab.Database.Source && items.Count == 1) {
					_copyTo(items[0]);
					return;
				}

				CopyToDialog dialog = new CopyToDialog(_tab, items, _tab.Database, db);
				dialog.ShowDialog();
			}
			catch (KeyInvalidException) {
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _changeId(ReadableTuple selectedItem) {
			if (_tab.GetNewKey(selectedItem, out int id)) {
				ChangeKey(selectedItem, id);
			}
		}

		private void _showSelectedOnly() {
			_tab.SearchEngine.SetRange(_tab.ListView.SelectedItems.Cast<ReadableTuple>().Select(p => p.Key).ToList());
		}


		public void _addNewItem() {
			try {
				if (_tab.Settings.CustomAddItemMethod != null) {
					_tab.Settings.CustomAddItemMethod();
					return;
				}

				int id = _tab.GetNewItemId(default);

				ReadableTuple item = (ReadableTuple)Activator.CreateInstance(typeof(ReadableTuple), id, _tab.Settings.AttributeList);
				item.Added = true;

				_tab.Settings.NewItemAddedFunction?.Invoke(item);

				_tab.Table.Commands.AddTuple(id, item, false);
				_tab.ListView.ScrollToCenterOfView(item);
			}
			catch (KeyInvalidException) {
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void _addNewItemRaw() {
			string defaultValue = Clipboard.ContainsText() ? Clipboard.GetText() : "";

			InputDialog dialog = new InputDialog("Paste the database lines here.", "Add new raw items", defaultValue, false, false);
			dialog.Owner = WpfUtilities.TopWindow;
			dialog.TextBoxInput.Loaded += delegate {
				dialog.TextBoxInput.SelectAll();
				dialog.TextBoxInput.Focus();
			};
			dialog.TextBoxInput.AcceptsReturn = true;
			dialog.TextBoxInput.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			dialog.TextBoxInput.TextWrapping = TextWrapping.NoWrap;
			dialog.TextBoxInput.VerticalContentAlignment = VerticalAlignment.Top;
			dialog.TextBoxInput.Height = 200;
			dialog.TextBoxInput.MinHeight = 200;
			dialog.TextBoxInput.MaxHeight = 200;
			dialog.TextBoxInput.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

			if (dialog.ShowDialog() == true) {
				try {
					_tab.Table.Commands.Begin();

					string text = dialog.Input;
					string tempPath = TemporaryFilesManager.GetTemporaryFilePath("db_tmp_{0:0000}.txt");
					File.WriteAllText(tempPath, text);
					SdeEditor.Project.GetDb(_tab.Database.Source).LoadFromClipboard(tempPath);
				}
				catch {
					_tab.Table.Commands.CancelEdit();
				}
				finally {
					_tab.Table.Commands.EndEdit();
				}

				_tab.Update();
			}
		}

		public void _selectInNotepad() {
			ReadableTuple item = _tab.ListView.SelectedItem as ReadableTuple;

			if (item != null) {
				string displayId = item.GetValue<string>(_tab.Settings.AttId);
				TkPath path;

				if ((path = DbPathLocator.DetectPath(_tab.Database.Source)) != null) {
					if (path.IsContainer) {
						ErrorHandler.HandleException("The file cannot be opened because it is not stored locally.");
						return;
					}

					string[] lines = File.ReadAllLines(path.FilePath);

					string line = lines.FirstOrDefault(p => p.StartsWith(displayId + ","));

					if (line == null)
						line = lines.FirstOrDefault(p => p.StartsWith(displayId + "\t"));

					if (line == null)
						line = lines.FirstOrDefault(p => p.Contains("Id: " + displayId));

					if (line == null)
						line = lines.FirstOrDefault(p => p.Contains("id: " + displayId));

					if (line == null)
						line = lines.FirstOrDefault(p => p.Contains("[" + displayId + "] ="));

					if (line == null)
						line = lines.FirstOrDefault(p => p.StartsWith(displayId));

					if (line == null) {
						int ival;
						if (!Int32.TryParse(displayId, out ival))
							line = lines.FirstOrDefault(p => p.Contains(displayId + ":"));
					}

					if (line != null)
						TabsMaker.SelectInNotepadpp(path, (lines.ToList().IndexOf(line) + 1).ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		#region Callbacks
		private void CopyToCallback(int oldkey, int newkey, bool executed) {
			if (executed) {
				_tab.Table.GetTuple(newkey).Added = true;

				if (_tab.IsCurrentTabSelected())
					_tab.ListView.SelectedItem = _tab.Table.GetTuple(newkey);

				_tab.ListView.ScrollToCenterOfView(_tab.ListView.SelectedItem);
			}
		}

		private void CopyToCallback2(BaseDatabase dbDest, Table<int, ReadableTuple> tableDest, int newkey, bool executed) {
			if (executed) {
				tableDest.GetTuple(newkey).Added = true;
				TabNavigation.Select(dbDest.Source, newkey);
			}
		}

		private void CopyToCallback3(Table<int, ReadableTuple> tableDest, int newkey, bool executed) {
			if (executed) {
				tableDest.GetTuple(newkey).Added = true;
			}
		}

		private void ChangeKeyCallback(int oldkey, int newkey, bool executed) {
			int key = executed ? newkey : oldkey;

			ReadableTuple tuple = _tab.Table.GetTuple(key);
			_tab.SearchEngine.SetOrder(tuple);

			if (tuple != _tab.ListView.SelectedItem) {
				_tab.ListView.SelectedItem = tuple;
			}
		}

		private void SelectLastSelectedCallback(bool executed) {
			if (executed) {
			}
			else {
				if (_tab.ListView.SelectedItem != null)
					Task.Run(() => _tab.ListView.Dispatch(p => p.ScrollToCenterOfView(_tab.ListView.SelectedItem)));
			}
		}

		private void ItemDeletedCallback(int key, ReadableTuple value, bool executed) {
			if (executed) {
				_tab.ListView.Items.Delete(value);
			}
		}
		#endregion

		private void SafeExecute(Action action) {
			try {
				action();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}
