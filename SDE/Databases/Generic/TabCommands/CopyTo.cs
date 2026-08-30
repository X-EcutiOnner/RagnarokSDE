using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System.Collections.Generic;

namespace SDE.Databases.Generic.TabCommands {
	public class CopyTo : TabCommand {
		public CopyTo() {
			AllowMultipleSelection = true;
			DisplayName = "Copy to...";
			ImagePath = "convert.png";
			Shortcut = SdeCommands.DbCopyItemTo;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			var tab = SdeEditor.Instance.FindTopmostTab();
			tab.Commands.CopyItemTo();
		}
	}
}
