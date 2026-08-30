using SDE.Core;
using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Mobs.Features {
	public class Mob : ICloneable {
		public string AegisName;
		public string Name;
		public string JapaneseName;
		public string Level = "1";
		public string Hp = "1";
		public string Sp = "1";
		public string BaseExp;
		public string JobExp;
		public string MvpExp;
		public string Attack;
		public string Attack2;
		public string Defense;
		public string MagicDefense;
		public string Resistance;
		public string MagicResistance;
		public string Str = "1";
		public string Agi = "1";
		public string Vit = "1";
		public string Int = "1";
		public string Dex = "1";
		public string Luk = "1";
		public string AttackRange;
		public string SkillRange;
		public string ChaseRange;
		public SizeType Size = SizeType.Size_Small;
		public RaceType Race = RaceType.RC_FORMLESS;
		public string RaceGroups;
		public ElementType Element = ElementType.ELE_NEUTRAL;
		public ElementLevelType ElementLevel = ElementLevelType.ELELV_1;
		public string WalkSpeed = "150";
		public string AttackDelay = "100";
		public string AttackMotion = "100";
		public string ClientAttackMotion;
		public string DamageMotion;
		public string DamageTaken = "100";
		public string GroupId;
		public string Title;
		public string Ai;
		public Common.ClassType Class = Common.ClassType.CLASS_NORMAL;
		public string Modes;
		public List<ItemDrop> MvpDrops = new List<ItemDrop>();
		public List<ItemDrop> Drops = new List<ItemDrop>();

		// Sprite redirect
		public string AliasSprite;
		public string ClientSprite;

		public object Clone() {
			var obj = (Mob)MemberwiseClone();

			obj.MvpDrops = MvpDrops.Select(p => (ItemDrop)p.Clone()).ToList();
			obj.Drops = Drops.Select(p => (ItemDrop)p.Clone()).ToList();

			return obj;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<Mob>.Equals(this, (Mob)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<Mob>.GetHashCode(this);
		}

		public bool IsCsv { get; set; }
	}
}
