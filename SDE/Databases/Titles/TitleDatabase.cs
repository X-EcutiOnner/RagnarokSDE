using SDE.Databases.Titles.Parser;
using SDE.Editor.Database;

namespace SDE.Databases.Titles {
	public class TitleDatabase : BaseDatabase {
		public TitleDatabase() {
			Source = DataSources.Title;
			AttributeList = TitleAttributes.AttributeList;
			Parser = new TitleParserProvider();
			ThrowFileNotFoundException = false;
			TabGenerator = null;
			IsGenerateTab = false;
		}
	}
}
