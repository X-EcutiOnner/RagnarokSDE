using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.Items.Parser {
	public class ItemParserProvider : DatabaseParserProvider {
		public override DatabaseReader GetReader(FileType fileType) {
			switch (fileType) {
				case FileType.Yaml:
					return new ItemReaderYaml();
				case FileType.Txt:
					return new ItemReaderCsv();
			}

			throw new Exception("No reader found for the specified format: '" + fileType + "'.");
		}

		public override DatabaseWriter GetWriter(FileType fileType) {
			switch (fileType) {
				case FileType.Yaml:
					return new ItemWriterYaml();
				case FileType.Txt:
					return new ItemWriterCsv();
			}

			throw new Exception("No writer found for the specified format: '" + fileType + "'.");
		}
	}
}
