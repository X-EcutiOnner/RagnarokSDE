using SDE.Core;
using SDE.Databases.Skills.Common;
using System;

namespace SDE.Databases.Skills.Features {
	public class Skill : ICloneable {
		public string Name;
		public string Description;
		public string MaxLevel;
		public BattleFlagType BF_Type = BattleFlagType.BF_NONE;
		public SkillTargetType INF_TargetType = SkillTargetType.INF_PASSIVE_SKILL;
		public string NK_DamageFlags;
		public string INF2_Flags;
		public string Range;
		public DamageType DMG_Hit = DamageType.DMG_NORMAL;
		public string HitCount;
		public string Element = "Neutral";
		public string SplashArea;
		public string ActiveInstance;
		public string Knockback;
		public string GiveAp;
		public string CopyFlagsSkill;
		public string CopyJobAllowed = ((long)SkillCopyJobAllowedFlag.SKJA_ALL).ToString();
		public string CopyFlagsRemoveRequirement;
		public string NoNearNPCRange;
		public string NoNearNPCType;
		public bool CastCancel = true;
		public string CastDefenseReduction;
		public string CastTime;
		public string AfterCastActDelay;
		public string AfterCastWalkDelay;
		public string Duration1;
		public string Duration2;
		public string Cooldown;
		public string FixedCastTime;
		public string CastTimeFlags;
		public string CastDelayFlags;
		public SkillRequire Require = new SkillRequire();
		public SkillUnit Unit = new SkillUnit();
		public string Status;

		// CSV values
		public string NoCastFlags;

		public object Clone() {
			var obj = (Skill)MemberwiseClone();

			obj.Require = (SkillRequire)Require.Clone();
			obj.Unit = (SkillUnit)Unit.Clone();

			return obj;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<Skill>.Equals(this, (Skill)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<Skill>.GetHashCode(this);
		}
	}
}
