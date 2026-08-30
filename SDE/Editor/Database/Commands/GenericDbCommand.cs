using System;
using Database;
using Database.Commands;

namespace SDE.Editor.Database.Commands {
	/// <summary>
	/// Holds a table command
	/// </summary>
	public class GenericDbCommand : IGenericDbCommand {
		private readonly ITableCommand<int, ReadableTuple> _command;
		private readonly CommandsHolder<int, ReadableTuple> _commandsList;
		private readonly string _displayName;

		public GenericDbCommand(BaseDatabase db) {
			_displayName = db.Source.DisplayName;
			Table = db.Table;

			_command = Table.Commands.Current;
			_commandsList = Table.Commands;
		}

		public Table<int, ReadableTuple> Table { get; private set; }

		#region IGenericDbCommand Members
		public void Execute() {
			_commandsList.Redo();
		}

		public void Undo() {
			_commandsList.Undo();
		}

		public string CommandDescription {
			get { return string.Format("[{0}], {1}", _displayName, _command.CommandDescription); }
		}
		#endregion
	}
}