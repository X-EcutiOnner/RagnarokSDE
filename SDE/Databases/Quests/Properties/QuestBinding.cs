using Database;
using SDE.Databases.Quests.Features;
using System;

namespace SDE.Databases.Quests.Properties {
	public class QuestBinding : IBinding {
		#region IBinding Members
		public Database.Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetModel<Quest>().Title ?? "";
		}
	}
}
