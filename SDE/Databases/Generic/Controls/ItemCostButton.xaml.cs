using SDE.View.Dialogs;
using SDE.View.Editors.ItemCostEdit;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class ItemCostButton : UserControl {
		public ItemCostButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(ItemCostButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public static readonly DependencyProperty MaxLevelProperty =
			DependencyProperty.Register(
				nameof(MaxLevel),
				typeof(string),
				typeof(ItemCostButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string MaxLevel {
			get => (string)GetValue(MaxLevelProperty);
			set => SetValue(MaxLevelProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			if (!Int32.TryParse(MaxLevel, out int maxLevel))
				maxLevel = 10;

			ItemCostDialog dialog = new ItemCostDialog(SourceField, maxLevel);
			InputWindowHelper.Edit(dialog, v => SourceField = v, _button, false, true);
		}
	}
}
