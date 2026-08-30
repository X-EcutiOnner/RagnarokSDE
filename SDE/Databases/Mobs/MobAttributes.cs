using Database;
using SDE.Databases.Mobs.Features;
using SDE.Databases.Mobs.Properties;

namespace SDE.Databases.Mobs {
	public sealed class MobAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new MobAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new MobAttributes(new ModelAttribute(typeof(Mob)));
		public static readonly DbAttribute Display = new MobAttributes(new DbAttribute("Name", typeof(MobNameBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private MobAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
