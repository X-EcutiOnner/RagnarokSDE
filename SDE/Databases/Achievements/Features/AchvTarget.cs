using SDE.Core;
using System;

namespace SDE.Databases.Achievements.Features {
	public class AchvTarget : ICloneable {
		public string Id;
		public string Mob;
		public string Count = "1";

		public object Clone() {
			return this.MemberwiseClone();
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<AchvTarget>.Equals(this, (AchvTarget)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<AchvTarget>.GetHashCode(this);
		}
	}
}
