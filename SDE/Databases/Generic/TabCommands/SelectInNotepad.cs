using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System.Collections.Generic;

namespace SDE.Databases.Generic.TabCommands {
	public class SelectInNotepad : TabCommand {
		public SelectInNotepad() {
			AllowMultipleSelection = true;
			DisplayName = "Select in Notepad++";
			ImagePath = "notepad.png";
			Shortcut = SdeCommands.DbOpenInNotepad;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			var tab = SdeEditor.Instance.FindTopmostTab();
			tab.Commands.SelectInNotepad();
		}
	}
}
