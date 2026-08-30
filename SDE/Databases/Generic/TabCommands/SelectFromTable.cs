using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.Navigation;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.TabCommands {
	public class SelectFromTable : TabCommand {
		private DataSource _targetSource;

		public SelectFromTable(DataSource targetSource) {
			_targetSource = targetSource;

			AllowMultipleSelection = false;
			DisplayName = $"Select in [{targetSource.DisplayName}]";
			ImagePath = "arrowdown.png";
			Shortcut = SdeCommands.Select;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			TabNavigation.SelectList(_targetSource, tuples.Select(p => p.Key));
		}
	}
}
