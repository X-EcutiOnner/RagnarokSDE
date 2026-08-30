using Database;
using SDE.Databases.Skills.Features;
using System;

namespace SDE.Databases.Skills.Properties {
	public class SkillNameBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<Skill>().Description ?? "";
		}
	}
}
