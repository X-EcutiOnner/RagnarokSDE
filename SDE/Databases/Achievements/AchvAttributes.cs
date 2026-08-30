using Database;
using SDE.Databases.Achievements.Features;
using SDE.Databases.Achievements.Properties;

namespace SDE.Databases.Achievements {
	public sealed class AchvAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new AchvAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new AchvAttributes(new ModelAttribute(typeof(Achv)));
		public static readonly DbAttribute Display = new AchvAttributes(new DbAttribute("Name", typeof(AchvBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private AchvAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
