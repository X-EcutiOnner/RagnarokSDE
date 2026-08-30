using SDE.Core;
using System;

namespace SDE.Databases.ClientItems.Features {
	public class ClientItem : ICloneable {
		public string IdentifiedDisplayName;
		public string IdentifiedDescription;
		public string IdentifiedResourceName;
		public string UnidentifiedDisplayName;
		public string UnidentifiedDescription;
		public string UnidentifiedResourceName;
		public string Affix;
		public bool IsCostume;
		public string NumberOfSlots;
		public string Illustration;
		public bool IsCard;
		public bool IsPostfix;
		public string ClassNumber;

		public object Clone() {
			return MemberwiseClone();
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<ClientItem>.Equals(this, (ClientItem)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<ClientItem>.GetHashCode(this);
		}
	}
}
