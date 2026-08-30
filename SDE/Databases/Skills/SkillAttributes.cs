using Database;
using SDE.Databases.Skills.Features;
using SDE.Databases.Skills.Properties;

namespace SDE.Databases.Skills {

	public sealed class SkillAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new SkillAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new SkillAttributes(new ModelAttribute(typeof(Skill)));
		public static readonly DbAttribute Display = new SkillAttributes(new DbAttribute("Name", typeof(SkillNameBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };
		public static readonly DbAttribute CopyableFileKeyRef = new SkillAttributes(new DbAttribute("FileKeyRef", typeof(string), null, "FileKeyRef")) { Visibility = VisibleState.Hidden };

		private SkillAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
