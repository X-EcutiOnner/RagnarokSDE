using SDE.Core;
using System;
using System.Collections.Generic;

namespace SDE.Databases.Pets.Features {
	public class Evolution : ICloneable {
		public string Target = "";
		public List<ItemRequirement> ItemRequirements = new List<ItemRequirement>();

		public object Clone() {
			Evolution evolution = new Evolution();

			evolution.Target = Target;

			foreach (var itemRequirement in ItemRequirements)
				ItemRequirements.Add((ItemRequirement)itemRequirement.Clone());

			return evolution;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<Evolution>.Equals(this, (Evolution)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<Evolution>.GetHashCode(this);
		}
	}
}
