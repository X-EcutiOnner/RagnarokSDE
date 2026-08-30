using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.MobSkills.Parser {
	public class MobSkillParserProvider : DatabaseParserProvider {
		public override DatabaseReader GetReader(FileType fileType) {
			switch (fileType) {
				case FileType.Txt:
					return new MobSkillReaderCsv();
			}

			throw new Exception("No reader found for the specified format: '" + fileType + "'.");
		}

		public override DatabaseWriter GetWriter(FileType fileType) {
			switch (fileType) {
				case FileType.Txt:
					return new MobSkillWriterCsv();
			}

			throw new Exception("No writer found for the specified format: '" + fileType + "'.");
		}
	}
}
