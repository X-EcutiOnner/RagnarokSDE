using Database;
using SDE.Databases.ItemCombos.Features;
using SDE.Databases.ItemCombos.Properties;

namespace SDE.Databases.ItemCombos {
	public sealed class ItemComboAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new ItemComboAttributes(new PrimaryAttribute("Id", typeof(int), 0, "Id"));
		public static readonly DbAttribute Model = new ItemComboAttributes(new ModelAttribute(typeof(ItemCombo)));
		public static readonly DbAttribute DisplayId = new ItemComboAttributes(new DbAttribute("ComboId", typeof(ItemComboIdBinding), null, "Combo ID")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };
		public static readonly DbAttribute DisplayName2 = new ItemComboAttributes(new DbAttribute("Name", typeof(ItemComboNameBinding), null, "Name")) { IsDisplayAttribute = true, Visibility = VisibleState.Hidden };
		public static readonly DbAttribute FileKeyRef = new ItemComboAttributes(new DbAttribute("FileKeyRef", typeof(string), null, "FileKeyRef")) { Visibility = VisibleState.Hidden };

		private ItemComboAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
