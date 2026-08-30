using Database;
using SDE.Databases.Pets.Features;
using SDE.Databases.Pets.Properties;

namespace SDE.Databases.Pets {
	public sealed class PetAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new PetAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new PetAttributes(new ModelAttribute(typeof(Pet)));
		public static readonly DbAttribute Display = new PetAttributes(new DbAttribute("Name", typeof(PetBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private PetAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
