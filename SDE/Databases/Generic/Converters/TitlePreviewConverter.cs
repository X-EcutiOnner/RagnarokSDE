using SDE.Databases.Titles;
using SDE.View;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class TitlePreviewConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null)
				return "";

			var sValue = value.ToString();

			if (Int32.TryParse(sValue, out int intValue)) {
				var db = SdeEditor.Project.GetMergedTable(DataSources.Title);

				var tuple = db.TryGetTuple(intValue);

				if (tuple != null) {
					return tuple.GetValue<string>(TitleAttributes.Title) + " (" + intValue + ")";
				}
			}

			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
