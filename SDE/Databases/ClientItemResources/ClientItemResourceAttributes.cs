using Database;

namespace SDE.Databases.ClientItemResources {
	public sealed class ClientItemResourceAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new ClientItemResourceAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute ResourceName = new ClientItemResourceAttributes(new DbAttribute("ResourceName", typeof(string), "", "Item name"));

		private ClientItemResourceAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
