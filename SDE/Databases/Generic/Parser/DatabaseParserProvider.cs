using SDE.Editor.Database;

namespace SDE.Databases.Generic.Parser {
	public abstract class DatabaseParserProvider {
		/// <summary>
		/// Indicates whether or not this parser reads/writes multiple files.
		/// </summary>
		public bool SplitDatabaseFiles { get; set; }

		public abstract DatabaseReader GetReader(FileType fileType);
		public abstract DatabaseWriter GetWriter(FileType fileType);

		public FileType ReadFileType = FileType.Detect;

		public void Read(DbLoadContext context, BaseDatabase db) {
			ReadFileType = context.FileType;
			GetReader(context.FileType).Loader(context, db);
		}

		public void Write(DbSaveContext context, BaseDatabase db) {
			GetWriter(context.FileType).Writer(context, db);
		}
	}
}
