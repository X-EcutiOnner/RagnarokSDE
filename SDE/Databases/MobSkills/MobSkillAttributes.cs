using Database;
using SDE.Databases.MobSkills.Features;
using SDE.Databases.MobSkills.Properties;

namespace SDE.Databases.MobSkills {
	public sealed class MobSkillAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new MobSkillAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new MobSkillAttributes(new ModelAttribute(typeof(MobSkill)));
		public static readonly DbAttribute DisplayMobId = new MobSkillAttributes(new DbAttribute("DisplayMobId", typeof(MobIdDisplayBinding), null, "Mob ID")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };
		public static readonly DbAttribute Display = new MobSkillAttributes(new DbAttribute("Name", typeof(MobSkillBinding), null, "Display name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };
		public static readonly DbAttribute FileKeyRef = new MobSkillAttributes(new DbAttribute("FileKeyRef", typeof(string), null, "FileKeyRef")) { Visibility = VisibleState.Hidden };

		private MobSkillAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
