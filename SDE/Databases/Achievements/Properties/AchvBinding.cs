using Database;
using SDE.Databases.Achievements.Features;

namespace SDE.Databases.Achievements.Properties {
	public class AchvBinding : IBinding {
		#region IBinding Members
		public Tuple Tuple { get; set; }
		public DbAttribute AttachedAttribute { get; set; }
		#endregion

		public override string ToString() {
			return Tuple.GetValue<Achv>(AchvAttributes.Model).Name ?? "";
		}
	}
}
