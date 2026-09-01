using SDE.Core;
using SDE.Editor.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDE.Databases.ItemCombos.Features {
	public class ItemCombo : ICloneable {
		public const int MaxNameIdCount = 10;

		public List<NameId> NameIds = new List<NameId>();
		public string Script;
		public bool Clear;

		public string DisplayNameIds {
			get {
				StringBuilder b = new StringBuilder();
				char separator = '\n';

				foreach (var item in NameIds) {
					if (String.IsNullOrEmpty(item.Item))
						continue;

					b.Append(item + ":" + separator);
				}

				return b.ToString().TrimEnd(separator);
			}
		}

		public string DisplayNames {
			get {
				StringBuilder b = new StringBuilder();

				foreach (var item in NameIds) {
					if (String.IsNullOrEmpty(item.Item))
						continue;

					b.Append(DbUtilities.ItemId2Name(item) + "\n");
				}

				return b.ToString().TrimEnd('\n');
			}
		}

		public object Clone() {
			var itemCombo = (ItemCombo)MemberwiseClone();

			itemCombo.NameIds = NameIds.Select(p => new NameId() { Item = p.Item }).ToList();

			return itemCombo;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<ItemCombo>.Equals(this, (ItemCombo)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<ItemCombo>.GetHashCode(this);
		}

		public static long ToUniqueId(List<NameId> nameIds) {
			// FNV-1a 64-bit offsets
			ulong hash = 14695981039346656037;
			const ulong prime = 1099511628211;

			// The unique key generated must be order independent
			foreach (var item in nameIds.OrderBy(p => p.Item)) {
				string str = item.Item; // Access the inner string
				if (string.IsNullOrEmpty(str))
					continue;

				// Hash the string characters directly without allocating memory
				for (int i = 0; i < str.Length; i++) {
					hash ^= str[i];
					hash *= prime;
				}

				hash *= prime;
			}

			return (long)hash;
		}

		public long ToUniqueId() {
			return ToUniqueId(NameIds);
		}
	}
}
