using SDE.Core;
using SDE.Databases.Generic.Parser;
using System;

namespace SDE.Databases.Mobs.Features {
	public class ItemDrop : ICloneable {
		public string Item;
		public string Rate;
		public bool StealProtected;
		public string RandomOptionGroup;
		public string Index;
		public int ItemInt => DbReader.ToInt(Item);

		public object Clone() {
			return MemberwiseClone();
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<ItemDrop>.Equals(this, (ItemDrop)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<ItemDrop>.GetHashCode(this);
		}
	}
}
