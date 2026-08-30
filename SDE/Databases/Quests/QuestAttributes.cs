using Database;
using SDE.Databases.Quests.Features;
using SDE.Databases.Quests.Properties;

namespace SDE.Databases.Quests {
	public sealed class QuestAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new QuestAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new QuestAttributes(new ModelAttribute(typeof(Quest)));
		public static readonly DbAttribute Display = new QuestAttributes(new DbAttribute("Name", typeof(QuestBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };

		private QuestAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
