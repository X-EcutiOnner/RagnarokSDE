using SDE.Core;
using SDE.Databases.Skills.Common;
using System;

namespace SDE.Databases.Skills.Features {
	public class SkillUnit : ICloneable {
		public string Id;
		public string AlternateId;
		public string Layout;
		public string Range;
		public string Interval;
		public BattleCheckTargetType Target = BattleCheckTargetType.BCT_ALL;
		public string Flag;

		public object Clone() {
			return MemberwiseClone();
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<SkillUnit>.Equals(this, (SkillUnit)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<SkillUnit>.GetHashCode(this);
		}
	}
}
