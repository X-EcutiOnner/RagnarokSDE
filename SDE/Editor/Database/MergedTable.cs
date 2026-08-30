using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Commands;
using Utilities.Commands;

namespace SDE.Editor.Database {
	/// <summary>
	/// A database table which holds multiple tables at the same time.
	/// </summary>
	public class MergedTable : Table<int, ReadableTuple>, IEnumerable<ReadableTuple> {
		private readonly MetaCommandsHolder _commands;
		private readonly List<Table<int, ReadableTuple>> _tables = new List<Table<int, ReadableTuple>>();
		private List<ReadableTuple> _bufferedItems = new List<ReadableTuple>();
		private bool _bufferedTable;

		public int TablesCount {
			get { return _tables.Count; }
		}

		public MergedTable(AttributeList list, bool unsafeContext = false) : base(list, unsafeContext) {
			_commands = new MetaCommandsHolder(null);
		}

		public new int Count {
			get { return FastItems.Count; }
		}

		public override CommandsHolder<int, ReadableTuple> Commands {
			get { return _commands; }
		}

		public override List<ReadableTuple> FastItems {
			get {
				if (_bufferedTable) {
					return _bufferedItems;
				}

				Dictionary<int, ReadableTuple> values = new Dictionary<int, ReadableTuple>(_tables.Last().Tuples);

				for (int i = _tables.Count - 2; i >= 0; i--) {
					foreach (var pair in _tables[i].Tuples) {
						values[pair.Key] = pair.Value;
					}
				}

				return values.Values.ToList();
			}
		}

		#region IEnumerable<ReadableTuple> Members
		IEnumerator<ReadableTuple> IEnumerable<ReadableTuple>.GetEnumerator() {
			return FastItems.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return FastItems.GetEnumerator();
		}
		#endregion

		public void AddTable(Table<int, ReadableTuple> table) {
			_tables.Insert(0, table);
			_commands.AddTable(table);
		}

		public override bool ContainsKey(int key) {
			return _tables.Any(table => table.ContainsKey(key));
		}

		public override T Get<T>(int key, DbAttribute attribute) {
			return _tables.First(p => p.ContainsKey(key)).Get<T>(key, attribute);
		}

		public override object Get(int key, DbAttribute attribute) {
			return _tables.First(p => p.ContainsKey(key)).Get(key, attribute);
		}

		public override ReadableTuple TryGetTuple(int key) {
			ReadableTuple tuple;

			for (int i = 0; i < _tables.Count; i++) {
				tuple = _tables[i].TryGetTuple(key);

				if (tuple != null)
					return tuple;
			}

			return null;
		}

		public override object GetRaw(int key, DbAttribute attribute) {
			return _tables.First(p => p.ContainsKey(key)).GetRaw(key, attribute);
		}

		public override ReadableTuple GetTuple(int key) {
			return _tables.First(p => p.ContainsKey(key)).GetTuple(key);
		}

		public void MergeOnce() {
			_bufferedTable = true;

			Dictionary<int, ReadableTuple> values = new Dictionary<int, ReadableTuple>(_tables.Last().Tuples);

			for (int i = _tables.Count - 2; i >= 0; i--) {
				foreach (var pair in _tables[i].Tuples) {
					values[pair.Key] = pair.Value;
				}
			}

			_bufferedItems = values.Values.ToList();
		}
	}

	public class MetaCommandsHolder : CommandsHolder<int, ReadableTuple> {
		private readonly List<Table<int, ReadableTuple>> _tables = new List<Table<int, ReadableTuple>>();

		public MetaCommandsHolder(Table<int, ReadableTuple> table) : base(table) {
		}

		public void AddTable(Table<int, ReadableTuple> table) {
			_tables.Insert(0, table);
		}

		public override void Store(ITableCommand<int, ReadableTuple> command) {
			throw new NotImplementedException();
		}

		public override void BeginEdit(IGroupCommand<ITableCommand<int, ReadableTuple>> command) {
			_tables.ForEach(p => p.Commands.Begin());
		}

		public override void EndEdit() {
			_tables.ForEach(p => p.Commands.EndEdit());
		}

		public override void StoreAndExecute(ITableCommand<int, ReadableTuple> command) {
			if (command is ChangeTupleProperty<int, ReadableTuple>) {
				for (int i = 0; i < _tables.Count; i++) {
					if (_tables[i].ContainsKey(command.Key)) {
						_tables[i].Commands.StoreAndExecute(command);
						return;
					}
				}

				return;
			}

			if (command is DeleteTuple<int, ReadableTuple>) {
				for (int i = 0; i < _tables.Count; i++) {
					if (_tables[i].ContainsKey(command.Key)) {
						_tables[i].Commands.StoreAndExecute(command);
						//return; Removes in all tables
					}
				}

				return;
			}

			// Execute on the first table that has the entry
			for (int i = 0; i < _tables.Count; i++) {
				if (_tables[i].ContainsKey(command.Key)) {
					_tables[i].Commands.StoreAndExecute(command);
					return;
				}
			}
		}
	}
}