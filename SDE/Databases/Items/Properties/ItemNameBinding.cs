using Database;
using SDE.Databases.Items.Features;
using System;

namespace SDE.Databases.Items.Properties {
	public class ItemNameBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<Item>().Name ?? "";
		}
	}
}
