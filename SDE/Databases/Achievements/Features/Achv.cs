using SDE.Core;
using SDE.Databases.Achievements.Common;
using System;
using System.Collections.Generic;

namespace SDE.Databases.Achievements.Features {
	public class Achv : ICloneable {
		public AchvGroupType Group;
		public string Name;
		public List<AchvTarget> Targets = new List<AchvTarget>();
		public string Condition;
		public string Map;
		public List<AchvDependent> Dependents = new List<AchvDependent>();
		public string RewardItem;
		public string RewardAmount = "1";
		public string RewardScript;
		public string RewardTitleId;
		public string Score;

		public object Clone() {
			var clone = (Achv)this.MemberwiseClone();

			clone.Targets = new List<AchvTarget>();
			foreach (var target in this.Targets) {
				if (target != null) {
					clone.Targets.Add((AchvTarget)target.Clone());
				}
			}

			clone.Dependents = new List<AchvDependent>();
			foreach (var dependent in this.Dependents) {
				if (dependent != null) {
					clone.Dependents.Add((AchvDependent)dependent.Clone());
				}
			}

			return clone;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<Achv>.Equals(this, (Achv)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<Achv>.GetHashCode(this);
		}
	}
}
