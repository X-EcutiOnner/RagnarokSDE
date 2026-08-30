using SDE.Editor.Database;
using SDE.View;

namespace SDE.Databases.Generic.Parser {
	public abstract class DatabaseReader {
		public virtual bool SplitDatabaseFiles { get; }
		public MergedTable MobDb;
		public MergedTable ItemDb;

		public DatabaseReader() {
			MobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
			ItemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);
		}

		public virtual void Loader(DbLoadContext context, BaseDatabase db) {
			if (context != null) {
				context.MetaMobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
				context.MetaItemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);
			}
		}
	}
}
