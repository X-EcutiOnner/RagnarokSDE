using Database;
using SDE.Editor.Database;
using System;

namespace SDE.Databases.Pets.Properties {
	public class PetBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return DbUtilities.MobId2Name(Tuple.GetKey<int>().ToString());
		}
	}
}
