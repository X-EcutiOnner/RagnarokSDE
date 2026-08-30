using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System.Collections.Generic;

namespace SDE.Databases.Generic.TabCommands {
	public class Cut : TabCommand {
		public Cut() {
			AllowMultipleSelection = true;
			DisplayName = "Cut";
			ImagePath = "cut.png";
			Shortcut = SdeCommands.Cut;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			var tab = SdeEditor.Instance.FindTopmostTab();
			tab.Commands.Cut();
		}
	}
}
