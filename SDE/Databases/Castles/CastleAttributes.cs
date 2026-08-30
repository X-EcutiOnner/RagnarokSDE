using Database;
using SDE.Databases.Castles.Features;
using SDE.Databases.Castles.Properties;

namespace SDE.Databases.Castles {
	public sealed class CastleAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new CastleAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new CastleAttributes(new ModelAttribute(typeof(Castle)));
		public static readonly DbAttribute Display = new CastleAttributes(new DbAttribute("Name", typeof(CastleBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private CastleAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
