using SDE.View.Dialogs;
using SDE.View.Editors;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class LevelEditIntegerButton : UserControl {
		public LevelEditIntegerButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(LevelEditIntegerButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public static readonly DependencyProperty MaxLevelProperty =
			DependencyProperty.Register(
				nameof(MaxLevel),
				typeof(string),
				typeof(LevelEditIntegerButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string MaxLevel {
			get => (string)GetValue(MaxLevelProperty);
			set => SetValue(MaxLevelProperty, value);
		}

		public static readonly DependencyProperty EquipmentProperty =
			DependencyProperty.Register(
				nameof(Equipment),
				typeof(bool),
				typeof(LevelEditIntegerButton),
				new FrameworkPropertyMetadata(false));

		public bool Equipment {
			get => (bool)GetValue(EquipmentProperty);
			set => SetValue(EquipmentProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			if (!Int32.TryParse(MaxLevel, out int maxLevel))
				maxLevel = 10;

			var flags = LevelEditFlag.ShowPreview2 | LevelEditFlag.AutoFill;
			bool canBeIntegrated = true;

			if (Equipment) {
				flags = LevelEditFlag.ShowPreview2 | LevelEditFlag.ItemDbPick;
				canBeIntegrated = false;
			}

			LevelEditDialog dialog = new LevelEditDialog(SourceField, maxLevel, flags);
			InputWindowHelper.Edit(dialog, v => SourceField = v, _button, canBeIntegrated);
		}
	}
}
