using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class WidthToGridLengthConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is double actualWidth) {
				return new GridLength(actualWidth);
			}

			return GridLength.Auto;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
