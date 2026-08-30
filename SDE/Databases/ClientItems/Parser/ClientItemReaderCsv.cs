using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;

namespace SDE.Databases.ClientItems.Parser {
	public class ClientItemReaderCsv : DatabaseReaderCsv<int> {
		public override void Loader(DbLoadContext context, BaseDatabase db) {
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.IdentifiedDescription);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.UnidentifiedDescription);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.IdentifiedDisplayName);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.UnidentifiedDisplayName);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.IdentifiedResourceName);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.UnidentifiedResourceName);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.CardIllustration);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.CardAffix);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.CardPostfix);
			ClientItemReaderHelper.LoadDataFromSystem(db, ClientItemTextFileParsers.NumberOfSlots);
			ClientItemReaderHelper.LoadViewId(db);
		}
	}
}
