using Database;
using SDE.Databases.ClientAchievements.Features;
using System;

namespace SDE.Databases.ClientAchievements.Properties {
	public class ClientAchvTitleBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<ClientAchv>().Title ?? "";
		}
	}
}
