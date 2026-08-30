using SDE.Databases.ClientItems.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;

namespace SDE.Databases.ClientItems.Parser {
	public class ClientItemWriterCsv : DatabaseWriterCsv {
		public override bool SplitDatabaseFiles => true;
		public override string KeyField => "Id";

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			Writer(null);
		}

		public void Writer(string exportPath) {
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.CardIllustration);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.CardAffix);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.CardPostfix);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.NumberOfSlots);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.IdentifiedResourceName);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.UnidentifiedResourceName);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.IdentifiedDescription);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.UnidentifiedDescription);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.IdentifiedDisplayName);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.UnidentifiedDisplayName);
		}
	}
}
