using SDE.Databases.Generic.Parser;
using SDE.Databases.Skills.Parser;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.Titles.Parser {
	public class TitleParserProvider : DatabaseParserProvider {
		public override DatabaseReader GetReader(FileType fileType) {
			return new TitleReaderLua();
		}

		public override DatabaseWriter GetWriter(FileType fileType) {
			throw new Exception("No writer available for this table: " + nameof(TitleParserProvider) + ".");
		}
	}
}
