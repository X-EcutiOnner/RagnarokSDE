using SDE.ApplicationConfiguration;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SDE.Databases.Generic.TabCommands {
	public class CopyToClipboardOther : TabCommand {
		private BaseDatabase _db;
		private FileType[] _formats = new FileType[2];

		public CopyToClipboardOther(BaseDatabase db, FileType activeFormat = FileType.Yaml, FileType otherFormat = FileType.Txt) {
			_formats[0] = activeFormat;
			_formats[1] = otherFormat;
			_db = db;
			AllowMultipleSelection = true;
			GetDisplayName = delegate {
				FileType targetFormat = otherFormat;

				if (db.ActiveFormat == otherFormat) {
					targetFormat = activeFormat;
				}

				string outputFormat = "";

				switch (targetFormat) {
					case FileType.Lua:
						outputFormat = "lua";
						break;
					case FileType.Yaml:
						outputFormat = "yml";
						break;
					case FileType.Txt:
						outputFormat = "txt";
						break;
				}

				return "Copy (" + outputFormat + ")";
			};
			ImagePath = "export.png";
			Shortcut = SdeCommands.Copy2;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			StringBuilder builder = new StringBuilder();

			var targetFormat = _db.ActiveFormat;

			targetFormat = _formats[0] == targetFormat ? _formats[1] : _formats[0];

			switch (targetFormat) {
				case FileType.Lua:
					var writerLua = (DatabaseWriterLua)_db.Parser.GetWriter(FileType.Lua);

					foreach (var tuple in tuples) {
						writerLua.WriteEntry(builder, tuple);
					}

					break;
				case FileType.Txt:
					var writerCsv = (DatabaseWriterCsv)_db.Parser.GetWriter(FileType.Txt);

					foreach (var tuple in tuples) {
						writerCsv.WriteEntry(builder, tuple);
					}

					break;
				case FileType.Yaml:
					var writerYaml = (DatabaseWriterYaml)_db.Parser.GetWriter(FileType.Yaml);

					foreach (var tuple in tuples) {
						writerYaml.WriteEntry(builder, tuple);
					}

					break;
			}

			Clipboard.SetDataObject(builder.ToString());
		}
	}
}
