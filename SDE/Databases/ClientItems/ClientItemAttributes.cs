using Database;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.ClientItems.Properties;

namespace SDE.Databases.ClientItems {
	public sealed class ClientItemAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new ClientItemAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new ClientItemAttributes(new ModelAttribute(typeof(ClientItem)));
		public static readonly DbAttribute Display = new ClientItemAttributes(new DbAttribute("Name", typeof(ClientItemNameBinding), null, "Display name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private ClientItemAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
