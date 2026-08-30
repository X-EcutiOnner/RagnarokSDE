using SDE.Databases.Generic.Common;
using SDE.Databases.Items.Common;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SDE.Databases.Items.Converters {
	public sealed class ItemTypeToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null)
				return Visibility.Collapsed;
			
			var type = (ItemType)(object)((EnumInfoBase)value).Value;

			switch (type) {
				case ItemType.IT_WEAPON:
					if (parameter.ToString() == "Weapon" || parameter.ToString() == "WeaponOrArmor")
						return Visibility.Visible;
					break;
				case ItemType.IT_AMMO:
					if (parameter.ToString() == "Ammo")
						return Visibility.Visible;
					break;
				case ItemType.IT_CARD:
					if (parameter.ToString() == "Card")
						return Visibility.Visible;
					break;
				case ItemType.IT_ARMOR:
					if (parameter.ToString() == "Armor" || parameter.ToString() == "WeaponOrArmor")
						return Visibility.Visible;
					break;
			}

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
