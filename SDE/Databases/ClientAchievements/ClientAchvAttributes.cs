using Database;
using SDE.Databases.ClientAchievements.Features;
using SDE.Databases.ClientAchievements.Properties;

namespace SDE.Databases.ClientAchievements {
	public sealed class ClientAchvAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new ClientAchvAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new ClientAchvAttributes(new ModelAttribute(typeof(ClientAchv)));
		public static readonly DbAttribute Display = new ClientAchvAttributes(new DbAttribute("Name", typeof(ClientAchvTitleBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private ClientAchvAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
