using Database;
using SDE.Databases.Mobs.Features;
using System;

namespace SDE.Databases.Mobs.Properties {
	public class MobNameBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<Mob>().Name ?? "";
		}
	}
}
