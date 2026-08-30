using System;
using System.Globalization;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class ResPreviewConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var sValue = (value ?? "").ToString();

			if (!long.TryParse(sValue, out long val) || val <= 0) {
				return "";
			}

			return String.Format("{0} - {1:0.00}%", value, val / (val + 400.0) * 80.0).Replace(",", ".");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
