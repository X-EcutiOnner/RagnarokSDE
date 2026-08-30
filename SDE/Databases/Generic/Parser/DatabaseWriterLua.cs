using GRF.IO;
using Lua.Structure;
using SDE.ApplicationConfiguration;
using SDE.Core;
using SDE.Editor.Backups;
using SDE.Editor.Database;
using SDE.Editor.Files;
using SDE.View;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Services;

namespace SDE.Databases.Generic.Parser {
	public abstract class DatabaseWriterLua : DatabaseWriter {
		public abstract string TableName { get; }

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			base.Writer(context, db);
			var project = SdeEditor.Project;

			if (context.Source.IsClientSide)
				context.FilePath = context.Source.ClientSidePath();

			if (db.Table.Commands.CommandIndex == -1) {
				if (SdeAppConfiguration.AlwaysOverwriteFiles) {
					var storeFile = DbPathLocator.GetStoredFile(context.FilePath);

					if (!IOHelper.SameFile(storeFile, context.FilePath)) {
						// Test their modified date
						GrfPath.Delete(context.FilePath);
						File.Copy(storeFile, context.FilePath);
					}
				}

				DbDebugHelper.OnWriteStatusUpdate(db.Source, context.FilePath, db, "Table not modified (will not be saved).");
				return;
			}

			if (project.MetaGrf.GetData(context.FilePath) == null) {
				Debug.Ignore(() => DbDebugHelper.OnWriteStatusUpdate(db.Source, context.FilePath, null, db.Source.UidName + " table not saved."));
				return;
			}

			BackupManager.Instance.BackupClient(context.FilePath);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine(TableName + " = {");

			List<ReadableTuple> tuples = db.Table.GetSortedItems().ToList();
			ReadableTuple tuple;

			for (int index = 0, count = tuples.Count; index < count; index++) {
				tuple = tuples[index];
				WriteEntry(builder, tuple);
			}

			//if (builder.Length > 2 && builder[builder.Length - 3] == ',' && builder[builder.Length - 2] == '\r' && builder[builder.Length - 1] == '\n') {
			//	builder.Remove(builder.Length - 1, 1);
			//}

			builder.AppendLine("}");
			builder.AppendLine();

			WriteFunctions(db, builder);

			IOHelper.SetData(context.FilePath, EncodingService.DisplayEncoding.GetBytes(builder.ToString()));
		}

		public virtual void WriteFunctions(BaseDatabase db, StringBuilder builder) {
			var functionData = db.Attached[SdeStrings.LuaReaderFunctions] as List<LBaseType>;

			foreach (var function in functionData) {
				if (function is LKeyValue keyValue) {
					builder.AppendLine(keyValue.ToString());
				}
				else {
					builder.AppendLine(function.ToString());
				}
			}
		}
	}
}
