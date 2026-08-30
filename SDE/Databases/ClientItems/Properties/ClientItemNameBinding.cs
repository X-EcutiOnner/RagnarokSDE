using Database;
using SDE.Databases.ClientItems.Features;
using System;

namespace SDE.Databases.ClientItems.Properties {
	public class ClientItemNameBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<ClientItem>().IdentifiedDisplayName ?? "";
		}
	}
}
