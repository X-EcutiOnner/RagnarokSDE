using System;
using System.Globalization;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class RatePreviewConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var sValue = (value ?? "").ToString();

			Int32.TryParse(sValue, out int val);
			return String.Format(CultureInfo.InvariantCulture, "{0:0.00} %", val / 100f);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
