using SDE.Core;
using System;

namespace SDE.Databases.Achievements.Features {
	public class AchvDependent : ICloneable {
		public string Id;
		public bool Active = true;

		public object Clone() {
			return this.MemberwiseClone();
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<AchvDependent>.Equals(this, (AchvDependent)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<AchvDependent>.GetHashCode(this);
		}
	}
}
