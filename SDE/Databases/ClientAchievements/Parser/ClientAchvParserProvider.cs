using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;

namespace SDE.Databases.Achievements.Parser {
	public class ClientAchvParserProvider : DatabaseParserProvider {
		public override DatabaseReader GetReader(FileType fileType) {
			return new ClientAchvReaderLua();
		}

		public override DatabaseWriter GetWriter(FileType fileType) {
			return new ClientAchvWriterLua();
		}
	}
}
