using Database;
using SDE.Databases.Items.Features;
using SDE.Databases.Items.Properties;

namespace SDE.Databases.Items {
	public sealed class ItemAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new ItemAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new ItemAttributes(new ModelAttribute(typeof(Item)));
		public static readonly DbAttribute Display = new ItemAttributes(new DbAttribute("Name", typeof(ItemNameBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private ItemAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
