using SDE.ApplicationConfiguration;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Shops;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Utilities.Extension;

namespace SDE.Databases.Generic.Converters {
	public sealed class ShopViewIdConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null)
				return "";

			var viewId = value.ToString();
			int ival;

			if (SdeAppConfiguration.AlwaysUseViewId) {
				if (!Int32.TryParse(viewId, out ival)) {
					JobType jobType = 0;

					if (DbReader.LoadEnum(ref jobType, viewId, false)) {
						return ((int)jobType).ToString(CultureInfo.InvariantCulture);
					}
				}
			}
			else {
				if (Int32.TryParse(viewId, out ival)) {
					viewId = ShopSimulatorDialog.ViewIdToString(ival);

					if (!String.IsNullOrEmpty(viewId)) {
						if (viewId.IsExtension(".act", ".spr")) {
							return Path.GetFileNameWithoutExtension(viewId.ToUpper());
						}
						else {
							return Path.GetFileName(viewId).ToUpper();
						}
					}
				}
			}

			return viewId.ToUpper();
		}
	}
}
