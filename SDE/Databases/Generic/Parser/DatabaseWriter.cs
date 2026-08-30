using SDE.Editor.Database;
using SDE.View;
using System.Text;

namespace SDE.Databases.Generic.Parser {
	public abstract class DatabaseWriter {
		public virtual bool SplitDatabaseFiles { get; }

		public MergedTable MobDb;
		public MergedTable ItemDb;

		public DatabaseWriter() {
			MobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
			ItemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);
		}

		public virtual void Writer(DbSaveContext context, BaseDatabase db) {
			context.MetaMobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
			context.MetaItemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);
		}

		public abstract void WriteEntry(StringBuilder builder, ReadableTuple tuple);
	}
}
