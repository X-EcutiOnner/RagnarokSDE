using ErrorManager;
using SDE.ApplicationConfiguration;
using SDE.Databases.ClientItems.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SDE.Databases.ClientItems.TabCommands {
	public class CopyToClipboardLua : TabCommand {
		public CopyToClipboardLua() {
			AllowMultipleSelection = false;
			DisplayName = "Copy entries to clipboard (lua)";
			ImagePath = "export.png";
			Shortcut = SdeCommands.Copy;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			try {
				var writer = new ClientItemWriterLua();
				StringBuilder builder = new StringBuilder();
				foreach (var tuple in tuples) {
					writer.WriteEntry(builder, tuple);
				}
				Clipboard.SetDataObject(builder.ToString());
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}
