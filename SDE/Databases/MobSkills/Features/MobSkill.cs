using SDE.Core;
using SDE.Databases.Generic.Parser;
using SDE.Databases.MobSkills.Common;
using System;

namespace SDE.Databases.MobSkills.Features {
	public class MobSkill : ICloneable {
		public string MobId;
		public string FriendlyDisplay;
		public MobSkillStateType State = MobSkillStateType.MSS_ANY;
		public string SkillId;
		public string SkillLv;
		public string Rate;
		public string CastTime;
		public string Delay;
		public bool Cancelable = false;
		public MobSkillTargetType Target = MobSkillTargetType.MST_TARGET;
		public MobSkillCond1Type Cond1 = MobSkillCond1Type.MSC_ALWAYS;
		public object CValue;
		public string Val1;
		public string Val2;
		public string Val3;
		public string Val4;
		public string Val5;
		public string Emotion;
		public string Chat;

		// Quick getters for easier conversions
		public int IntMobId => DbReader.ToInt(MobId);
		public int IntSkillId => DbReader.ToInt(SkillId);

		public object Clone() {
			return MemberwiseClone();
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<MobSkill>.Equals(this, (MobSkill)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<MobSkill>.GetHashCode(this);
		}
	}
}
