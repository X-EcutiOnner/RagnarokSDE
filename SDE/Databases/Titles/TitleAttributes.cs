using Database;

namespace SDE.Databases.Titles {
	public sealed class TitleAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new TitleAttributes(new PrimaryAttribute("Id", typeof(int), "", "Constant ID"));
		public static readonly DbAttribute Title = new TitleAttributes(new DbAttribute("Title", typeof(string), ""));

		private TitleAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
