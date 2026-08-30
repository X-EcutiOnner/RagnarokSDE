using SDE.Databases.Achievements.Parser;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.ClientQuests.Parser {
	public class ClientQuestParserProvider : DatabaseParserProvider {
		public override DatabaseReader GetReader(FileType fileType) {
			switch (fileType) {
				case FileType.Lua:
					return new ClientQuestReaderLua();
				case FileType.Txt:
					return new ClientQuestReaderCsv();
			}

			throw new Exception("No reader found for the specified format: '" + fileType + "'.");
		}

		public override DatabaseWriter GetWriter(FileType fileType) {
			switch (fileType) {
				case FileType.Lua:
					return new ClientQuestWriterLua();
				case FileType.Txt:
					return new ClientQuestWriterCsv();
			}

			throw new Exception("No writer found for the specified format: '" + fileType + "'.");
		}
	}
}
