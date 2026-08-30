using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.Quests.Parser {
	public class QuestParserProvider : DatabaseParserProvider {
		public override DatabaseReader GetReader(FileType fileType) {
			switch (fileType) {
				case FileType.Yaml:
					return new QuestReaderYaml();
				case FileType.Txt:
					return new QuestReaderCsv();
			}

			throw new Exception("No reader found for the specified format: '" + fileType + "'.");
		}

		public override DatabaseWriter GetWriter(FileType fileType) {
			switch (fileType) {
				case FileType.Yaml:
					return new QuestWriterYaml();
				case FileType.Txt:
					return new QuestWriterCsv();
			}

			throw new Exception("No writer found for the specified format: '" + fileType + "'.");
		}
	}
}
