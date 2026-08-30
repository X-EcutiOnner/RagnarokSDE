using SDE.View.Dialogs;
using SDE.View.Editors.TimeEdit;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class TimeEditButton : UserControl {
		public TimeEditButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(nameof(PreviewText), typeof(string), typeof(TimeEditButton), new PropertyMetadata(string.Empty, OnPreviewTextChanged));

		private static void OnPreviewTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var timeEdit = (TimeEditButton)d;

			timeEdit._button.Content = e.NewValue.ToString();
		}

		public string PreviewText {
			get => (string)GetValue(PreviewTextProperty);
			set => SetValue(PreviewTextProperty, value);
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(TimeEditButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSourceFieldChanged));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		private static void OnSourceFieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (TimeEditButton)d;

			if (e.NewValue == null) {
				edit.PreviewText = "0s";
				return;
			}

			if (edit.Millisecond)
				edit.PreviewText = Core.Extensions.ParseToTimeMs(e.NewValue.ToString());
			else
				edit.PreviewText = Core.Extensions.ParseToTimeSeconds(e.NewValue.ToString());
		}

		public bool Millisecond {
			get => (bool)GetValue(MillisecondProperty);
			set => SetValue(MillisecondProperty, value);
		}

		public static readonly DependencyProperty MillisecondProperty =
			DependencyProperty.Register(
				nameof(Millisecond),
				typeof(bool),
				typeof(TimeEditButton),
				new FrameworkPropertyMetadata(false));

		private void _button_Click(object sender, RoutedEventArgs e) {
			TimeEditDialog dialog = new TimeEditDialog(SourceField, !Millisecond);
			InputWindowHelper.Edit(dialog, v => SourceField = v, _button);
		}
	}
}
