using SDE.Databases.Generic.Parser;
using SDE.Editor;
using SDE.Editor.Database;

namespace SDE.Databases.ClientItems.Parser {
	public class ClientItemParserProvider : DatabaseParserProvider {
		public override DatabaseReader GetReader(FileType fileType) {
			if (ProjectConfiguration.UseLuaFiles)
				return new ClientItemReaderLua();

			return new ClientItemReaderCsv();
		}

		public override DatabaseWriter GetWriter(FileType fileType) {
			if (ProjectConfiguration.UseLuaFiles)
				return new ClientItemWriterLua();

			return new ClientItemWriterCsv();
		}
	}
}
