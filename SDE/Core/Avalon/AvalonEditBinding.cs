using System;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace SDE.Core.Avalon {
	public static class AvalonEditBinding {
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.RegisterAttached(
				"Text",
				typeof(string),
				typeof(AvalonEditBinding),
				new FrameworkPropertyMetadata(
					"",
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnTextChanged));

		private static readonly DependencyProperty IsUpdatingProperty =
			DependencyProperty.RegisterAttached(
				"IsUpdating",
				typeof(bool),
				typeof(AvalonEditBinding));

		public static string GetText(DependencyObject obj) =>
			(string)obj.GetValue(TextProperty);

		public static void SetText(DependencyObject obj, string value) =>
			obj.SetValue(TextProperty, value);

		private static void OnTextChanged(
			DependencyObject d,
			DependencyPropertyChangedEventArgs e) {
			if (!(d is TextEditor editor))
				return;

			editor.TextChanged -= Editor_TextChanged;

			if (!(bool)editor.GetValue(IsUpdatingProperty)) {
				editor.Text = e.NewValue as string ?? "";
			}

			editor.TextChanged += Editor_TextChanged;
		}

		private static void Editor_TextChanged(object sender, EventArgs e) {
			var editor = (TextEditor)sender;

			editor.SetValue(IsUpdatingProperty, true);
			SetText(editor, editor.Text);
			editor.SetValue(IsUpdatingProperty, false);
		}
	}
}
