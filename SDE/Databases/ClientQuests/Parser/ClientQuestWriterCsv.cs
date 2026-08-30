using SDE.Databases.ClientQuests.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System.Text;

namespace SDE.Databases.ClientQuests.Parser {
	public class ClientQuestWriterCsv : DatabaseWriterCsv {
		public override bool SplitDatabaseFiles => true;
		public override string KeyField => "Id";

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			Writer(null);
		}

		public void Writer(string exportPath) {
			ClientTextWriterHelper<ClientQuest>.SaveFileToSystem(exportPath, ClientQuestTextFileParsers.Quest);
		}

		public override string WriteEntry(ReadableTuple tuple) {
			StringBuilder b = new StringBuilder();
			ClientQuestTextFileParsers.Quest.Write(tuple.GetModel<ClientQuest>(), b, -2, tuple.Key);
			return b.ToString();
		}
	}
}
