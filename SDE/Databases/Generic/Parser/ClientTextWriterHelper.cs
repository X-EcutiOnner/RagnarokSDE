using ErrorManager;
using SDE.ApplicationConfiguration;
using SDE.Editor.Backups;
using SDE.Editor.Database;
using SDE.Editor.Files;
using SDE.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Services;

namespace SDE.Databases.Generic.Parser {
	public sealed class ClientTextWriterHelper<TModel> where TModel : class {
		public static void SaveFileToSystem(ClientTextFileParser<TModel> parser) {
			SaveFileToSystem(null, parser);
		}

		public static void SaveFileToSystem(string output, ClientTextFileParser<TModel> parser) {
			var sdb = SdeEditor.Project;
			string filename = parser.GetFilename();
			string tableName = Path.GetFileNameWithoutExtension(filename);
			var db = sdb.GetDb(parser.Source);

			if (output == null && sdb.MetaGrf.GetData(filename) == null) {
				Debug.Ignore(() => DbDebugHelper.OnWriteStatusUpdate(parser.Source, filename, null, "Table not saved (" + tableName + ")."));
				return;
			}

			if (output == null)
				BackupManager.Instance.BackupClient(filename);

			var context = new DbSaveContext(db);
			if (!context.IsTableModified()) return;

			string tmpFilename = Path.Combine(SdeAppConfiguration.TempPath, Path.GetFileName(filename));
			Encoding encoding = EncodingService.DisplayEncoding;

			List<ReadableTuple> tuples = db.Table.GetSortedItems().ToList();
			int previousItemId = -1;

			StringBuilder b = new StringBuilder();

			for (int i = 0; i < tuples.Count; i++) {
				ReadableTuple tuple = tuples[i];
				bool lastItem = i == tuples.Count - 1;
				var model = tuple.GetModel<TModel>();
				int itemId = tuple.Key;

				if (!parser.Write(model, b, previousItemId, itemId))
					continue;

				previousItemId = itemId;
			}

			File.WriteAllText(tmpFilename, b.ToString(), encoding);

			if (output == null) {
				var data = sdb.MetaGrf.GetData(filename);
				var toWrite = File.ReadAllBytes(tmpFilename);

				if (data != null && Methods.ByteArrayCompare(data, toWrite)) return;

				IOHelper.SetData(filename, toWrite);
			}
			else {
				string copyPath = Path.Combine(output, Path.GetFileName(filename));

				try {
					File.Delete(copyPath);
					File.Copy(tmpFilename, copyPath);
					File.Delete(tmpFilename);
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			}

			Debug.Ignore(() => DbDebugHelper.OnWriteStatusUpdate(DataSources.ClientQuest, sdb.MetaGrf.FindTkPath(filename), null, "Saving client table (" + tableName + ")."));
		}
	}
}
