using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System.Collections.Generic;

namespace SDE.Databases.Generic.TabCommands {
	public class ShowSelectedOnly : TabCommand {
		public ShowSelectedOnly() {
			AllowMultipleSelection = true;
			DisplayName = "Show selected items only";
			ImagePath = "find.png";
			Shortcut = SdeCommands.Restrict;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			var tab = SdeEditor.Instance.FindTopmostTab();
			tab.Commands.ShowSelectedOnly();
		}
	}
}
