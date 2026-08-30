using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SDE.Databases.Generic.Converters {
	public sealed class ListViewIndexConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var item = value as DependencyObject;
			if (item == null) return -1;

			// Walk up the visual tree to find the ListViewItem container
			DependencyObject container = item;
			while (container != null && !(container is ListViewItem)) {
				container = VisualTreeHelper.GetParent(container);
			}

			if (container is ListViewItem listViewItem) {
				// Walk up further to find the parent ListView
				DependencyObject parent = VisualTreeHelper.GetParent(listViewItem);
				while (parent != null && !(parent is ListView)) {
					parent = VisualTreeHelper.GetParent(parent);
				}

				if (parent is ListView listView) {
					// Get the index of the container
					int index = listView.ItemContainerGenerator.IndexFromContainer(listViewItem);
					return index;
				}
			}

			return -1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
