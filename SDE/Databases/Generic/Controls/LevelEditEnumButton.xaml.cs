using SDE.View.Dialogs;
using SDE.View.Editors;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class LevelEditEnumButton : UserControl {
		public LevelEditEnumButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(LevelEditEnumButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public static readonly DependencyProperty MaxLevelProperty =
			DependencyProperty.Register(
				nameof(MaxLevel),
				typeof(string),
				typeof(LevelEditEnumButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string MaxLevel {
			get => (string)GetValue(MaxLevelProperty);
			set => SetValue(MaxLevelProperty, value);
		}

		public static readonly DependencyProperty SourceTypeProperty =
			DependencyProperty.Register(
				nameof(SourceType),
				typeof(Type),
				typeof(LevelEditEnumButton));

		public Type SourceType {
			get => (Type)GetValue(SourceTypeProperty);
			set => SetValue(SourceTypeProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			if (!Int32.TryParse(MaxLevel, out int maxLevel))
				maxLevel = 10;

			LevelEnumDialog dialog = new LevelEnumDialog(SourceField, maxLevel, true, SourceType);
			InputWindowHelper.Edit(dialog, v => SourceField = v, _button);
		}
	}
}
