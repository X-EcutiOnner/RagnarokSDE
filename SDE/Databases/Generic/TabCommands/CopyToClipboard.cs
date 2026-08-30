using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SDE.Databases.Generic.TabCommands {
	public class CopyToClipboard : TabCommand {
		private BaseDatabase _db;
		private FileType _format;

		public CopyToClipboard(BaseDatabase db, FileType format = FileType.Detect) {
			_db = db;
			_format = format;
			AllowMultipleSelection = true;
			GetDisplayName = delegate {
				var targetFormat = format;

				if (targetFormat == FileType.Detect)
					targetFormat = db.ActiveFormat;

				switch (targetFormat) {
					case FileType.Yaml:
						return "Copy (yml)";
					case FileType.Lua:
						return "Copy (lua)";
					default:
						return "Copy (txt)";
				}
			};
			ImagePath = "export.png";
			Shortcut = SdeCommands.Copy;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			StringBuilder builder = new StringBuilder();
			var writer = _db.Parser.GetWriter(_format == FileType.Detect ? _db.ActiveFormat : _format);

			foreach (var tuple in tuples) {
				writer.WriteEntry(builder, tuple);
			}

			Clipboard.SetDataObject(builder.ToString());
		}
	}
}
