using SDE.Editor.Database;
using SDE.Editor.Parsers;
using SDE.View;

namespace SDE.Databases.ClientItemResources {
	public class ClientItemResourceDatabase : BaseDatabase {
		public ClientItemResourceDatabase() {
			Source = DataSources.ClientResourceDb;
			AttributeList = ClientItemResourceAttributes.AttributeList;
			TabGenerator = null;
			IsGenerateTab = false;
		}

		protected override void _loadDb() {
			TextFileHelper.LatestFile = Source;

			foreach (string[] elements in TextFileHelper.GetElements(SdeEditor.MetaGrf.GetData(@"data\idnum2itemresnametable.txt"))) {
				try {
					int itemId = int.Parse(elements[0]);
					Table.SetRaw(itemId, ClientItemResourceAttributes.ResourceName, elements[1]);
				}
				catch {
				}
			}
		}
	}
}
