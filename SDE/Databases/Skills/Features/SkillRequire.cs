using SDE.Core;
using SDE.Databases.Skills.Common;
using System;

namespace SDE.Databases.Skills.Features {
	public class SkillRequire : ICloneable {
		public string HpCost;
		public string SpCost;
		public string ApCost;
		public string HpRateCost;
		public string SpRateCost;
		public string ApRateCost;
		public string MaxHpTrigger;
		public string ZenyCost;
		public string Weapon;
		public string Ammo;
		public string AmmoAmount;
		public RequiredStateType State = RequiredStateType.ST_NONE;
		public string Status;
		public string SpiritSphereCost;
		public string ItemCost;
		public string Equipment;

		public object Clone() {
			return MemberwiseClone();
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<SkillRequire>.Equals(this, (SkillRequire)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<SkillRequire>.GetHashCode(this);
		}
	}
}
