using SDE.Databases.Generic.Common;
using SDE.Databases.Items.Common;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SDE.Databases.Generic.Converters {
	public sealed class FlagPreviewConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (values == null || values.Length != 2)
				return "";

			string input = values[0] as string;
			Type enumType = values[1] as Type;

			long value = 0;

			input = input ?? "";

			if (input != "" && !Int64.TryParse(input, out value)) {
				if (input.StartsWith("0x") || input.StartsWith("0X"))
					value = System.Convert.ToInt64(input.Substring(2), 16);
				else
					return "";
			}

			if (value < 0)
				return "";

			StringBuilder builder = new StringBuilder();

			var enumValues = EnumInfos.GetEnumInfoList(enumType).Where(p => p.Visible).ToList();

			long vAll = 0;

			foreach (var valueL in enumValues)
				vAll |= (long)(object)valueL.Value;

			// Don't bother showing "All" for only 1 or 2 flags...
			if (value == vAll && enumValues.Count > 2) {
				return "All";
			}

			if (enumType == typeof(TradeFlag) && TradeFlagInfo.ProcessFlagToName((TradeFlag)value, builder)) {
				return builder.ToString().TrimEnd(',', ' ');
			}
			else if (enumType == typeof(EquipLocationFlag) && EquipLocationFlagInfo.ProcessFlagToName((EquipLocationFlag)value, builder)) {
				return builder.ToString().TrimEnd(',', ' ');
			}
			else if (enumType == typeof(ItemJobFlag) && ItemJobFlagInfo.ProcessFlagToName((ItemJobFlag)value, builder)) {
				return builder.ToString().TrimEnd(',', ' ');
			}

			for (int i = 0; i < enumValues.Count; i++) {
				var valueEnum = enumValues[i];
				var t = valueEnum.ValueLong;

				if ((value & t) != 0)
					builder.Append(valueEnum.PascalName + ", ");
			}

			var output = builder.ToString();
			output = output.Trim(',', ' ');

			if (output == "")
				output = "None";

			return output;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			// One-way bindings can throw a NotSupportedException
			throw new NotSupportedException();
		}
	}
}
