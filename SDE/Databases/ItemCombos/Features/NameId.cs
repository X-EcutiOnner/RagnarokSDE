using SDE.Editor.Database;

namespace SDE.Databases.ItemCombos.Features {
	public class NameId {
		public string Item = "";

		public NameId() {

		}

		public NameId(string item) {
			Item = item;
		}

		public override string ToString() {
			return Item;
		}

		public static implicit operator NameId(string value) => new NameId() { Item = value };
		public static implicit operator string(NameId wrapper) => wrapper?.Item ?? string.Empty;
		public static implicit operator IntOrString(NameId wrapper) => wrapper?.Item ?? string.Empty;
	}
}
