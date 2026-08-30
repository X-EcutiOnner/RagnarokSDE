using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System.Collections.Generic;

namespace SDE.Databases.Generic.TabCommands {
	public class Delete : TabCommand {
		public Delete() {
			AllowMultipleSelection = true;
			DisplayName = "Delete";
			ImagePath = "delete.png";
			Shortcut = SdeCommands.DbDelete;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			var tab = SdeEditor.Instance.FindTopmostTab();
			tab.Commands.DeleteItems();
		}
	}
}
