using SDE.Editor.Database;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class ItemPreviewConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return DbUtilities.ItemPreview(value as string);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
