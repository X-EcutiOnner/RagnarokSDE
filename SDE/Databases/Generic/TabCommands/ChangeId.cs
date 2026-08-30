using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System.Collections.Generic;

namespace SDE.Databases.Generic.TabCommands {
	public class ChangeId : TabCommand {
		public ChangeId() {
			AllowMultipleSelection = true;
			DisplayName = "Change ID...";
			ImagePath = "properties.png";
			Shortcut = SdeCommands.DbChangeId;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			var tab = SdeEditor.Instance.FindTopmostTab();
			tab.Commands.ChangeId();
		}
	}
}
