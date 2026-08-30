using SDE.Databases.Emotes;
using SDE.View;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class EmotePreviewConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null)
				return "";

			var sValue = value.ToString();

			if (Int32.TryParse(sValue, out int intValue)) {
				var db = SdeEditor.Project.GetMergedTable(DataSources.Emote);
				var tuple = db.TryGetTuple(intValue);

				if (tuple != null) {
					return tuple.GetValue<string>(EmoteAttributes.Emote) + " (" + intValue + ")";
				}
			}

			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
