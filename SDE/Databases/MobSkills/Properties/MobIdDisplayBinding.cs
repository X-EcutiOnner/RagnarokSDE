using Database;
using SDE.Databases.MobSkills.Features;
using System;

namespace SDE.Databases.MobSkills.Properties {
	public class MobIdDisplayBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<MobSkill>().MobId ?? "";
		}
	}
}
