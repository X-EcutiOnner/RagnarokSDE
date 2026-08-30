using SDE.View.Dialogs;
using SDE.View.Editors.ScriptEdit;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class ScriptEditButton : UserControl {
		public ScriptEditButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(nameof(PreviewText), typeof(string), typeof(ScriptEditButton), new PropertyMetadata(string.Empty, OnPreviewTextChanged));

		private static void OnPreviewTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var scriptEdit = (ScriptEditButton)d;

			scriptEdit._button.Content = e.NewValue.ToString();
		}

		public string PreviewText {
			get => (string)GetValue(PreviewTextProperty);
			set => SetValue(PreviewTextProperty, value);
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(ScriptEditButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			var dialog = new ScriptEditDialog(SourceField);
			InputWindowHelper.Edit(dialog, v => SourceField = v, _button);
		}
	}
}
