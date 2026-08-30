using SDE.Databases.Skills.Features;
using SDE.View;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class SkillPreviewConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null)
				return "";

			var sValue = value.ToString();

			if (Int32.TryParse(sValue, out int intValue)) {
				var db = SdeEditor.Project.GetMergedTable(DataSources.Skill);

				var tuple = db.TryGetTuple(intValue);

				if (tuple != null) {
					return tuple.GetModel<Skill>().Description + " (" + intValue + ")";
				}
			}

			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
