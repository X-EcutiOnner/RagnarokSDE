using Database;
using Lua.Structure;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;

namespace SDE.Databases.Titles.Parser {
	public class TitleReaderLua : DatabaseReaderLua {
		public override string TableName => "title_tbl";

		public override void LoadEntry(Table<int, ReadableTuple> table, LKeyValue item, BaseDatabase db) {
			int id = int.Parse(item.Key);

			var tuple = table.EnsureExists(id);
			tuple.SetRawValue(TitleAttributes.Title, ((LStringValue)item.Value).Value);
		}
	}
}
