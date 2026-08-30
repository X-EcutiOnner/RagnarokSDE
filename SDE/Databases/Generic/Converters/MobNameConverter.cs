using SDE.Editor.Database;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class MobNameConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null)
				return "";

			var sValue = value.ToString();

			if (Int32.TryParse(sValue, out int intValue)) {
				return DbUtilities.MobId2Name(intValue);
			}

			return sValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
