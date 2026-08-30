using Database;
using SDE.Databases.ClientQuests.Features;
using SDE.Databases.ClientQuests.Properties;

namespace SDE.Databases.ClientQuests {
	public sealed class ClientQuestAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new ClientQuestAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new ClientQuestAttributes(new ModelAttribute(typeof(ClientQuest)));
		public static readonly DbAttribute Display = new ClientQuestAttributes(new DbAttribute("Name", typeof(ClientQuestNameBinding), null, "Display name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private ClientQuestAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
