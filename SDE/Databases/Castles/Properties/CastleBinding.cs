using Database;
using SDE.Databases.Castles.Features;
using System;

namespace SDE.Databases.Castles.Properties {
	public class CastleBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<Castle>().Name ?? "";
		}
	}
}
