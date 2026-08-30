using SDE.Databases.ClientItems.Features;
using SDE.Databases.Items.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using SDE.View;
using System;
using System.IO;
using Utilities;

namespace SDE.Databases.ClientItems.Parser {
	public sealed class ClientItemReaderHelper {
		public static void LoadDataFromSystem(BaseDatabase db, ClientItemTextFileParser parser, string filePath = null) {
			string file = parser.GetFilename();
			var table = db.Table;
			TextFileHelper.LatestFile = file;
			DbSaveContext context = new DbSaveContext(null);

			try {
				var data = filePath != null ? File.ReadAllBytes(filePath) : SdeEditor.MetaGrf.GetData(file);
				
				foreach (string[] elements in TextFileHelper.GetElements(data, parser.AllowMultiLine)) {
					int itemId = Int32.Parse(elements[0]);
					var tuple = table.EnsureExists(itemId);
					var model = tuple.GetModel<ClientItem>();
					parser.Read(model, elements);
				}

				Debug.Ignore(() => DbDebugHelper.OnLoaded(db.Source, SdeEditor.MetaGrf.FindTkPath(file), db));
			}
			catch (Exception err) {
				if (!context.ReportException(err)) return;
				//Debug.Ignore(() => DbDebugHelper.OnExceptionThrown(db.DbSource, file, db));
				//throw new Exception(TextFileHelper.GetLastError(), err);
			}
		}

		public static void LoadViewId(BaseDatabase db) {
			var sItems = SdeEditor.Project.GetMergedTable(DataSources.Item);

			foreach (var tuple in db.Table.FastItems) {
				var sTuple = sItems.TryGetTuple(tuple.GetKey<int>());

				if (sTuple != null) {
					tuple.GetModel<ClientItem>().ClassNumber = sTuple.GetModel<Item>().View;
				}
			}
		}
	}
}
