using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System.Collections.Generic;
using System.Windows;

namespace SDE.Databases.Generic.TabCommands {
	public class CopyToImportTable : TabCommand {
		private BaseDatabase _db;

		public CopyToImportTable(BaseDatabase db) {
			_db = db;
			var dbSource = db.Source;
			
			if (dbSource.IsImport) {
				Visibility = Visibility.Collapsed;
				return;
			}

			AllowMultipleSelection = false;
			GetDisplayName = () => "Copy to [" + dbSource.ImportTable.DisplayName + "]...";
			ImagePath = "convert.png";
			Shortcut = SdeCommands.CopyTo2;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			SdeEditor.Instance.FindTopmostTab().Commands.CopyItemTo(SdeEditor.Project.GetDb(_db.Source.ImportTable));
		}
	}
}
