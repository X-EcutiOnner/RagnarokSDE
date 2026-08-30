using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.ItemCombos.Parser {
	public class ItemComboParserProvider : DatabaseParserProvider {
		public override DatabaseReader GetReader(FileType fileType) {
			switch (fileType) {
				case FileType.Yaml:
					return new ItemComboReaderYaml();
				case FileType.Txt:
					return new ItemComboReaderCsv();
			}

			throw new Exception("No reader found for the specified format: '" + fileType + "'.");
		}

		public override DatabaseWriter GetWriter(FileType fileType) {
			switch (fileType) {
				case FileType.Yaml:
					return new ItemComboWriterYaml();
				case FileType.Txt:
					return new ItemComboWriterCsv();
			}

			throw new Exception("No writer found for the specified format: '" + fileType + "'.");
		}
	}
}
