using SDE.Core;
using System;

namespace SDE.Databases.Quests.Features {
	public class MapMobTarget : ICloneable {
		public string MobName;
		public bool Active;

		public object Clone() {
			return new MapMobTarget {
				MobName = MobName,
				Active = Active
			};
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<MapMobTarget>.Equals(this, (MapMobTarget)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<MapMobTarget>.GetHashCode(this);
		}
	}
}
