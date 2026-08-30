using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class FirstValidationErrorConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is IEnumerable errors) {
				var firstError = errors.Cast<ValidationError>().FirstOrDefault();
				return firstError?.ErrorContent;
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
