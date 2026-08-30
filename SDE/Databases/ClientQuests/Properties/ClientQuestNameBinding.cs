using Database;
using SDE.Databases.ClientQuests.Features;
using System;

namespace SDE.Databases.ClientQuests.Properties {
	public class ClientQuestNameBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<ClientQuest>().Title ?? "";
		}
	}
}
