using SDE.View.Dialogs;
using SDE.View.Editors.TimeEdit;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class TimeEditSolveButton : UserControl {
		public TimeEditSolveButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(nameof(PreviewText), typeof(string), typeof(TimeEditSolveButton), new PropertyMetadata(string.Empty, OnPreviewTextChanged));

		private static void OnPreviewTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (TimeEditSolveButton)d;

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
				typeof(TimeEditSolveButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSourceFieldChanged));

		private static void OnSourceFieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (TimeEditSolveButton)d;

			if (e.NewValue == null)
				edit.PreviewText = "0s";
			else {
				var r = Time.Parse(e.NewValue.ToString()).ToString();

				if (r == "")
					r = "0s";

				edit.PreviewText = r;
			}
		}

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			TimeEditSolveDialog dialog = new TimeEditSolveDialog(SourceField);
			InputWindowHelper.Edit(dialog, t => SourceField = t, _button, true, false);
		}
	}
}
