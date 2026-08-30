using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;

namespace SDE.Databases.ClientQuests.Parser {
	public class ClientQuestReaderCsv : DatabaseReaderCsv<int> {
		public override void Loader(DbLoadContext context, BaseDatabase db) {
			ClientQuestReaderHelper.LoadDataFromSystem(db, ClientQuestTextFileParsers.Quest);
		}
	}
}
