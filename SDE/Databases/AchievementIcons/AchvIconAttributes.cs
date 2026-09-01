using Database;

namespace SDE.Databases.AchievementIcons {
	public sealed class AchvIconAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new AchvIconAttributes(new PrimaryAttribute("Id", typeof(int), "", "Constant ID"));
		public static readonly DbAttribute Value = new AchvIconAttributes(new DbAttribute("Value", typeof(string), ""));
		public static readonly DbAttribute StringId = new AchvIconAttributes(new DbAttribute("StringId", typeof(string), "", "Constant ID"));

		private AchvIconAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
