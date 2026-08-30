using Database;
using Database.Commands;
using SDE.Editor.Database;
using SDE.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace SDE.Editor.Generic.DbTabs {
	public class DbTabDatabaseEventManager {
		private DbTab _tab;

		private readonly HashSet<ReadableTuple> _pendingDeletedItems = new HashSet<ReadableTuple>();
		private DispatcherTimer _pendingDeletedUpdateTimer;

		private readonly HashSet<ReadableTuple> _pendingAddedItems = new HashSet<ReadableTuple>();
		private DispatcherTimer _pendingAddedUpdateTimer;

		public DbTabDatabaseEventManager(DbTab tab) {
			_tab = tab;

			_pendingDeletedUpdateTimer = new DispatcherTimer();
			_pendingDeletedUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
			_pendingDeletedUpdateTimer.Tick += _pendingDeletedUpdateTimer_Tick;

			_pendingAddedUpdateTimer = new DispatcherTimer();
			_pendingAddedUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
			_pendingAddedUpdateTimer.Tick += _pendingAddedUpdateTimer_Tick;
		}

		public void Subscribe(Table<int, ReadableTuple> table) {
			table.TupleRemoved += _table_TupleRemoved;
			table.TupleAdded += _table_TupleAdded;
			table.TupleModified += _table_TupleModified;
			table.TableUpdated += _table_TableUpdated;

			table.Commands.PreviewCommandRedo += _commands_PreviewCommand;
			table.Commands.PreviewCommandUndo += _commands_PreviewCommand;
			table.Commands.CommandRedo += _commands_Command;
			table.Commands.CommandUndo += _commands_Command;

			SdeEditor.Project.Reloaded += _database_Reloaded;
		}

		private void _table_TableUpdated(object sender) {
			if (_tab.SearchEngine.IsLoaded)
				_tab.SearchEngine.Filter(this);
		}

		private void _table_TupleRemoved(object sender, int key, ReadableTuple value) {
			_pendingDeletedItems.Add(value);
			_pendingDeletedUpdateTimer.Stop();
			_pendingDeletedUpdateTimer.Start();
		}

		private void _pendingDeletedUpdateTimer_Tick(object sender, EventArgs e) {
			_pendingDeletedUpdateTimer.Stop();

			var items = _pendingDeletedItems.ToList();
			_pendingDeletedItems.Clear();

			if (items.Count == 0)
				return;

			if (items.Count > 50) {
				_tab.SearchEngine.Filter(this);
				return;
			}

			if (_tab.ListView.ItemsSource != null) {
				var l = _tab.ListView.ItemsSource as RangeObservableCollection<ReadableTuple>;
				l.RemoveRange(items);
			}
		}

		private void _table_TupleModified(object sender, int key, ReadableTuple value) {
			_tab.SearchEngine.SetOrder(value);
		}

		private void _table_TupleAdded(object sender, int key, ReadableTuple tuple) {
			if (_tab.Settings.SearchEngine.SetupImageDataGetter != null && tuple.GetImageData == null) {
				tuple.GetImageData = _tab.Settings.SearchEngine.SetupImageDataGetter;
			}

			_pendingAddedItems.Add(tuple);
			_pendingAddedUpdateTimer.Stop();
			_pendingAddedUpdateTimer.Start();
		}

		private void _pendingAddedUpdateTimer_Tick(object sender, EventArgs e) {
			_pendingAddedUpdateTimer.Stop();

			var items = _pendingAddedItems.ToList();
			_pendingAddedItems.Clear();

			if (items.Count == 0)
				return;

			if (items.Count > 50) {
				_tab.SearchEngine.Filter(this, () => _tab.SelectItems(items.OfType<ReadableTuple>().ToList(), focus: true));
				return;
			}

			_tab.SearchEngine.AddTuples(items);
			_tab.SelectItems(items.OfType<ReadableTuple>().ToList(), focus: true);
		}

		private void _database_Reloaded(object sender) {
			_tab.Dispatch(delegate {
				if (((Grid)_tab.Content).IsVisible) {
					_tab.SearchEngine.Filter(this);
				}
				else {
					_tab.DelayedReload = true;
				}
			});
		}

		private void _commands_PreviewCommand(object sender, ITableCommand<int, ReadableTuple> command) {
			_tab.SearchEngine.Collection?.Disable();
			_tab.ListView.Disable();
		}

		private void _commands_Command(object sender, ITableCommand<int, ReadableTuple> command) {
			_tab.ListView.UpdateAndEnable();
			_tab.SearchEngine.Collection?.UpdateAndEnable();
		}
	}
}
