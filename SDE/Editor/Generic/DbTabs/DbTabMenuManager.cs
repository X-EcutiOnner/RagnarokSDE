using Database.Commands;
using ErrorManager;
using SDE.Databases.Generic.TabCommands;
using SDE.Editor.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles;

namespace SDE.Editor.Generic.DbTabs {
	public class DbTabMenuManager {
		private DbTab _tab;

		public DbTabMenuManager(DbTab tab) {
			_tab = tab;
		}

		public void CreateContextMenu() {
			var lv = _tab.ListView;

			foreach (TabCommand commandCopy in _tab.Settings.AddedCommands) {
				if (commandCopy.Visibility != Visibility.Visible) continue;

				if (commandCopy is TabCommandSeparator) {
					lv.ContextMenu.Items.Add(new Separator());
					continue;
				}

				TabCommand command = commandCopy;
				TkMenuItem item = new TkMenuItem();

				if (command.GetDisplayName != null)
					item.Header = command.GetDisplayName();
				else
					item.Header = command.DisplayName;

				item.Click += (e, a) => _menuItem_Click(command);

				Image image = new Image();
				image.Source = ApplicationManager.PreloadResourceImage(command.ImagePath);
				item.Icon = image;

				if (command.Shortcut != null) {
					ApplicationShortcut.Link(command.Shortcut, item, _tab);
				}

				lv.ContextMenu.Items.Add(item);

				if (command.GetDisplayName != null) {
					lv.ContextMenu.IsVisibleChanged += delegate {
						item.Header = command.GetDisplayName();
					};
				}
			}
		}

		protected void _menuItem_Click(TabCommand command) {
			if (command.AddToCommandsStack) {
				if (command.AllowMultipleSelection) {
					List<ITableCommand<int, ReadableTuple>> commands = new List<ITableCommand<int, ReadableTuple>>();
					_tab.SearchEngine.Collection.Disable();

					for (int index = 0; index < _tab.ListView.SelectedItems.Count; index++) {
						ReadableTuple rItem = (ReadableTuple)_tab.ListView.SelectedItems[index];
						var cmd = command.Command(rItem);

						if (cmd != null)
							commands.Add(cmd);
					}

					if (commands.Count > 0)
						_tab.Table.Commands.AddGroupedCommands(commands);

					_tab.SearchEngine.Collection.UpdateAndEnable();
					_tab.Update();
				}
				else {
					try {
						if (_tab.ListView.SelectedItem != null) {
							ReadableTuple rItem = _tab.SelectedItem;
							_tab.Table.Commands.StoreAndExecute(command.Command(rItem));
						}
					}
					catch (Exception err) {
						ErrorHandler.HandleException(err);
					}
				}
			}
			else {
				if (command.GenericCommand == null) {
					ErrorHandler.HandleException("The added command in the generic database tab hasn't been setup correctly.");
					return;
				}

				command.GenericCommand(_tab.ListView.SelectedItems.OfType<ReadableTuple>().ToList());
			}
		}
	}
}
