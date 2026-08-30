using SDE.View.Dialogs;
using SDE.View.Editors;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class RateEditButton : UserControl {
		public RateEditButton() {
			InitializeComponent();

			_button.Content = String.Format(CultureInfo.InvariantCulture, "{0:0.00} %", 0f);
		}

		public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(nameof(PreviewText), typeof(string), typeof(RateEditButton), new PropertyMetadata(string.Empty, OnPreviewTextChanged));

		private static void OnPreviewTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (RateEditButton)d;

			edit._button.Content = e.NewValue.ToString();
		}

		public string PreviewText {
			get => (string)GetValue(PreviewTextProperty);
			set => SetValue(PreviewTextProperty, value);
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(RateEditButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSourceFieldChanged));

		private static void OnSourceFieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (RateEditButton)d;

			Int32.TryParse((e.NewValue ?? "").ToString(), out int val);
			edit.PreviewText = String.Format(CultureInfo.InvariantCulture, "{0:0.00} %", val / 100f);
		}

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			RateEditDialog dialog = new RateEditDialog(SourceField);
			InputWindowHelper.Edit(dialog, v => SourceField = v, _button);
		}
	}
}
