using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using ErrorManager;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using TokeiLibrary;
using Utilities.Commands;
using Tuple = Database.Tuple;

namespace SDE.Editor.Navigation {
	/// <summary>
	/// This class keeps track of the selected tabs.
	/// It also selects items in the various tables.
	/// </summary>
	public class TabNavigation : AbstractCommand<INagivationCommand> {
		private readonly object _lock = new object();
		private readonly TabControl _tab;
		private SelectionChangedCommand _firstSelection;
		private DateTime _now;
		private INagivationCommand _pendingCommand;

		public TabNavigation(TabControl tab) {
			_tab = tab;
			Instance = this;
			_now = DateTime.Now;
		}

		public static bool IsSelecting { get; set; }

		public static TabNavigation Instance { get; set; }

		public bool Disabled { get; set; }

		protected override void _execute(INagivationCommand command) {
			command.Execute(this);
		}

		protected override void _undo(INagivationCommand command) {
			if (_pendingCommand != null) {
				_storeAndExecute(_pendingCommand, true);
			}

			command.Undo(this);
		}

		protected override void _redo(INagivationCommand command) {
			command.Execute(this);
		}

		public override void StoreAndExecute(INagivationCommand command) {
			_storeAndExecute(command, false);
		}

		public static void SelectQuiet(DataSource source, int id) {
			Instance.SelectInternalQuiet(source, new List<int> { id });
		}

		public static void Select(DataSource source, int id) {
			Instance.SelectInternal(source, new List<int> { id });
		}

		public static void SelectList(DataSource source, IEnumerable<int> id) {
			try {
				List<int> result = id.ToList();

				if (result.Count == 0) {
					ErrorHandler.HandleException("No items match the query in [" + source.DisplayName + "].", ErrorLevel.NotSpecified);
					return;
				}

				Instance.SelectInternal(source, result);
			}
			catch (Exception err) {
				ErrorHandler.HandleException("Failed to parse the search query.\r\n\r\n" + err.Message);
			}
		}

		public void SelectInternal(DataSource source, List<int> tuplesGen) {
			Task.Run(() => _selectInternal(source, tuplesGen, true));
		}

		public void SelectInternalQuiet(DataSource source, List<int> tuplesGen) {
			Task.Run(() => _selectInternal(source, tuplesGen, false));
		}

		private void _selectInternal(DataSource source, List<int> tuplesGen, bool changeTab) {
			try {
				lock (_lock) {
					IsSelecting = true;

					if (source.ImportTable == null) {
						TabItem item = _tab.Dispatch(() => _tab.Items.Cast<TabItem>().FirstOrDefault(p => p.Header.ToString() == source));

						if (item is DbTab) {
							DbTab tab = (DbTab)item;

							var table = tab.Table;
							List<ReadableTuple> tuples = tuplesGen.Select(table.TryGetTuple).Where(p => p != null).ToList();

							if (tuples.Count == 0) {
								if (changeTab)
									ErrorHandler.HandleException((tuplesGen.Count > 1 ? "Items do" : "Item does") + " not exist in [" + source.DisplayName + "].", ErrorLevel.NotSpecified);
								return;
							}

							if (!_containsAny(tab, tuples)) {
								tab.IgnoreFilterOnce();
								tab.Filter();
								_waitForFilter(tab);

								if (!_containsAny(tab, tuples)) {
									if (changeTab)
										ErrorHandler.HandleException((tuplesGen.Count > 1 ? "Items" : "Item") + " not found in [" + source.DisplayName + "]. Try clearing the search filter on the specified table.", ErrorLevel.NotSpecified);
									return;
								}
							}

							tab.Dispatch(p => p.IsSelected = true);
							_waitForFilter(tab);
							tab.Dispatch(p => p.SelectItems(tuples, focus: true));
						}
						else {
							if (item == null) return;

							if (changeTab)
								item.Dispatch(p => p.IsSelected = true);
						}
					}
					else {
						TabItem item = _tab.Dispatch(() => _tab.Items.Cast<TabItem>().FirstOrDefault(p => p.Header.ToString() == source));
						TabItem item2 = _tab.Dispatch(() => _tab.Items.Cast<TabItem>().FirstOrDefault(p => p.Header.ToString() == source.ImportTable));

						if (item is DbTab && item2 is DbTab) {
							DbTab tab = (DbTab)item;
							DbTab tab2 = (DbTab)item2;

							var table = tab.Table;
							var table2 = tab2.Table;
							List<ReadableTuple> tuples = tuplesGen.Select(table.TryGetTuple).Where(p => p != null).ToList();
							List<ReadableTuple> tuples2 = tuplesGen.Select(table2.TryGetTuple).Where(p => p != null).ToList();

							if (tuples.Count == 0 && tuples2.Count == 0) {
								if (changeTab)
									ErrorHandler.HandleException((tuplesGen.Count > 1 ? "Items do" : "Item does") + " not exist in either [" + source.DisplayName + "] or [" + source.ImportTable.DisplayName + "].", ErrorLevel.NotSpecified);
								return;
							}

							if (!_containsAny(tab, tuples)) {
								tab.IgnoreFilterOnce();
								tab.Filter();
								_waitForFilter(tab);
							}

							if (!_containsAny(tab2, tuples)) {
								tab2.IgnoreFilterOnce();
								tab2.Filter();
								_waitForFilter(tab2);
							}

							if (!_containsAny(tab, tuples) && !_containsAny(tab2, tuples2)) {
								if (!_containsAny(tab, tuples) && !_containsAny(tab2, tuples2)) {
									if (changeTab)
										ErrorHandler.HandleException((tuplesGen.Count > 1 ? "Items" : "Item") + " not found in either [" + source.DisplayName + "] or [" + source.ImportTable.DisplayName + "], but . Try clearing the search filter on the specified table.", ErrorLevel.NotSpecified);
									return;
								}
							}

							DbTab tabToSelect = _containsAny(tab2, tuples2) ? tab2 : tab;

							if (changeTab)
								tabToSelect.Dispatch(p => p.IsSelected = true);

							_waitForFilter(tabToSelect);
							tabToSelect.Dispatch(p => p.SelectItems(tabToSelect == tab ? tuples : tuples2, focus: true));
						}
						else {
							if (item == null) return;
							if (changeTab)
								item.Dispatch(p => p.IsSelected = true);
						}
					}
				}
			}
			catch {
			}
			finally {
				IsSelecting = false;
			}
		}

		private bool _containsAny(DbTab tab, List<ReadableTuple> tuples) {
			return tab.Dispatch(new Func<bool>(delegate {
				for (int i = 0; i < tuples.Count; i++) {
					if (tab.ListView.Items.Contains(tuples[i]))
						return true;
				}

				return false;
			}));
		}

		private static void _waitForFilter(DbTab tab) {
			int max = 20;
			while (tab.IsFiltering && max > 0) {
				Thread.Sleep(100);
				max--;
			}
		}

		public void Select(string tabName, object tuple, ListView view) {
			Disabled = true;
			try {
				foreach (DbTab item in _tab.Items.OfType<DbTab>()) {
					if (item.Header.ToString() == tabName) {
						item.IsSelected = true;
						view.SelectedItem = tuple;
						view.ScrollIntoView(tuple);
						break;
					}
				}
			}
			finally {
				Disabled = false;
			}
		}

		public override List<INagivationCommand> GetUndoCommands() {
			if (_pendingCommand != null) {
				_storeAndExecute(_pendingCommand, true);
			}

			List<INagivationCommand> commands = _commands.Take(_commandIndexCurrent).ToList();
			commands.Insert(0, _firstSelection);
			return commands;
		}

		private void _storeAndExecute(INagivationCommand command, bool forceSet = false) {
			if (Disabled)
				return;

			SelectionChangedCommand sc = command as SelectionChangedCommand;

			if (sc == null || sc.View == null || sc.Tuple == null)
				return;

			if ((DateTime.Now - _now).TotalMilliseconds < 200 && !forceSet) {
				_pendingCommand = command;
				_now = DateTime.Now;
				return;
			}

			if (_pendingCommand != null && _pendingCommand != command) {
				_storeAndExecute(_pendingCommand, true);
			}

			_pendingCommand = null;

			if (_firstSelection == null) {
				_firstSelection = (SelectionChangedCommand)command;
				return;
			}

			sc.PreviousPosition = GetLastCommand();

			if (sc.PreviousPosition != null && sc.PreviousPosition.Tuple == sc.Tuple && ReferenceEquals(sc.PreviousPosition.View, sc.View))
				return;

			base.Store(command);

			lock (_thisLock) {
				while (_commands.Count > 30) {
					_firstSelection = (SelectionChangedCommand)_commands[0];
					_commands.RemoveAt(0);
					_commandIndexCurrent--;
				}
			}

			_now = new DateTime(DateTime.Now.Ticks);
		}

		public SelectionChangedCommand GetLastCommand() {
			if (_commandIndexCurrent == -1)
				return _firstSelection;

			return _commands[_commandIndexCurrent] as SelectionChangedCommand;
		}
	}
}