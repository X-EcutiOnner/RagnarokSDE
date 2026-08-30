using SDE.Core;
using System;

namespace SDE.Databases.Pets.Features {
	public class ItemRequirement : ICloneable {
		public string Item;
		public string Amount;

		public object Clone() {
			ItemRequirement req = new ItemRequirement();

			req.Item = Item;
			req.Amount = Amount;

			return req;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<ItemRequirement>.Equals(this, (ItemRequirement)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<ItemRequirement>.GetHashCode(this);
		}
	}
}
