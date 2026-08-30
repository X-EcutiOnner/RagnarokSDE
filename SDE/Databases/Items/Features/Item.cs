using SDE.Core;
using SDE.Databases.Generic.Common;
using SDE.Databases.Items.Common;
using System;

namespace SDE.Databases.Items.Features {
	public class Item : ICloneable {
		public string AegisName;
		public string Name;
		public ItemType Type = ItemType.IT_ETC;
		public AmmoType AmmoType = 0;
		public CardType CardType = 0;
		public WeaponType WeaponType = 0;
		public string SubType {
			get {
				switch (Type) {
					case ItemType.IT_AMMO:
						return ((int)AmmoType).ToString();
					case ItemType.IT_CARD:
						return ((int)CardType).ToString();
					case ItemType.IT_WEAPON:
						return ((int)WeaponType).ToString();
					default:
						return null;
				}
			}
		}
		public string Buy;
		public string Sell;
		public string Weight;
		public string Attack;
		public string MagicAttack;
		public string Defense;
		public string Range;
		public string Slots;
		public string Jobs = "0xFFFFFFFFFFFFFFFF";
		public string Classes = ((long)ItemJobFlag.ITEMJ_ALL).ToString();
		public GenderType Gender = GenderType.SEX_BOTH;
		public string Locations;
		public string WeaponLevel;
		public string ArmorLevel;
		public string EquipLevelMin;
		public string EquipLevelMax;
		public bool Refineable;
		public bool Gradable;
		public string View;
		public string AliasName;
		public string Flags;
		public DropEffectType DropEffect = DropEffectType.DROPEFFECT_NONE;
		public string Delay;
		public string DelayStatus;
		public string StackAmount;
		public string StackFlags;
		public string NoUseOverride = "100";
		public string NoUseFlags;
		public string TradeOverride = "100";
		public string TradeFlags;
		public string Script;
		public string EquipScript;
		public string UnEquipScript;

		public object Clone() {
			return MemberwiseClone();
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<Item>.Equals(this, (Item)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<Item>.GetHashCode(this);
		}
	}
}
