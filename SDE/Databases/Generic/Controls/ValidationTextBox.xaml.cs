using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SDE.Databases.Generic.Controls {
	/// <summary>
	/// Interaction logic for ValidationTextBox.xaml
	/// </summary>
	public partial class ValidationTextBox : UserControl {
		private bool _previewVisible;

		public ValidationTextBox() {
			InitializeComponent();

			_tbData.GotFocus += _tbData_GotFocus;
			_tbData.LostFocus += _tbData_LostFocus;
		}

		private void _tbData_LostFocus(object sender, RoutedEventArgs e) {
			ShowPreview();
		}

		private void _tbData_GotFocus(object sender, RoutedEventArgs e) {
			HidePreview();
		}

		public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(nameof(PreviewText), typeof(string), typeof(ValidationTextBox), new PropertyMetadata(string.Empty, OnPreviewTextChanged));

		public string PreviewText {
			get => (string)GetValue(PreviewTextProperty);
			set => SetValue(PreviewTextProperty, value);
		}

		private static void OnPreviewTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var control = (ValidationTextBox)d;

			if (string.IsNullOrEmpty(control.PreviewText)) {
				control.HidePreview();
				return;
			}

			control.ShowPreview();
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(
				nameof(Text),
				typeof(string),
				typeof(ValidationTextBox),
				new FrameworkPropertyMetadata(
					string.Empty,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string Text {
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty IsReadOnlyProperty =
			DependencyProperty.Register(
				nameof(IsReadOnly),
				typeof(bool),
				typeof(ValidationTextBox),
				new PropertyMetadata(false));

		public bool IsReadOnly {
			get => (bool)GetValue(IsReadOnlyProperty);
			set => SetValue(IsReadOnlyProperty, value);
		}

		public static readonly DependencyProperty TextWrappingProperty =
			DependencyProperty.Register(
				nameof(TextWrapping),
				typeof(TextWrapping),
				typeof(ValidationTextBox),
				new PropertyMetadata(TextWrapping.NoWrap));

		public TextWrapping TextWrapping {
			get => (TextWrapping)GetValue(TextWrappingProperty);
			set => SetValue(TextWrappingProperty, value);
		}

		public static readonly DependencyProperty AcceptsReturnProperty =
			DependencyProperty.Register(
				nameof(AcceptsReturn),
				typeof(bool),
				typeof(ValidationTextBox),
				new PropertyMetadata(false));

		public bool AcceptsReturn {
			get => (bool)GetValue(AcceptsReturnProperty);
			set => SetValue(AcceptsReturnProperty, value);
		}

		public static readonly DependencyProperty BoxVerticalAlignmentProperty =
			DependencyProperty.Register(
				nameof(BoxVerticalAlignment),
				typeof(VerticalAlignment),
				typeof(ValidationTextBox),
				new PropertyMetadata(VerticalAlignment.Center));

		public VerticalAlignment BoxVerticalAlignment {
			get => (VerticalAlignment)GetValue(BoxVerticalAlignmentProperty);
			set => SetValue(BoxVerticalAlignmentProperty, value);
		}

		public static readonly DependencyProperty OnlyShowPreviewOnEmptyFieldProperty =
			DependencyProperty.Register(
				nameof(OnlyShowPreviewOnEmptyField),
				typeof(bool),
				typeof(ValidationTextBox),
				new PropertyMetadata(false));

		public bool OnlyShowPreviewOnEmptyField {
			get => (bool)GetValue(OnlyShowPreviewOnEmptyFieldProperty);
			set => SetValue(OnlyShowPreviewOnEmptyFieldProperty, value);
		}

		public static readonly DependencyProperty TextAlignmentProperty =
			DependencyProperty.Register(
				nameof(TextAlignment),
				typeof(TextAlignment),
				typeof(ValidationTextBox),
				new PropertyMetadata(TextAlignment.Left));

		public TextAlignment TextAlignment {
			get => (TextAlignment)GetValue(TextAlignmentProperty);
			set => SetValue(TextAlignmentProperty, value);
		}

		public static readonly DependencyProperty TextBoxBackgroundProperty =
			DependencyProperty.Register(
				nameof(TextBoxBackground),
				typeof(Brush),
				typeof(ValidationTextBox),
				new PropertyMetadata(null));

		public Brush TextBoxBackground {
			get => (Brush)GetValue(TextBoxBackgroundProperty);
			set => SetValue(TextBoxBackgroundProperty, value);
		}

		public void ShowPreview() {
			if (String.IsNullOrEmpty(PreviewText)) {
				return;
			}

			if (OnlyShowPreviewOnEmptyField && _tbData.Text != "") {
				return;
			}

			_tbPreview.Text = PreviewText;

			if (_previewVisible || _tbData.IsFocused)
				return;

			_tbPreview.Visibility = Visibility.Visible;
			_tbData.SetResourceReference(TextBox.ForegroundProperty, "UIThemeTextBoxBackgroundBrush");
			_previewVisible = true;
		}

		public void HidePreview() {
			if (!_previewVisible)
				return;

			_tbPreview.Visibility = Visibility.Collapsed;
			_tbData.SetResourceReference(TextBox.ForegroundProperty, "TextForeground");
			_previewVisible = false;
		}

		public void ClearUndo() {
			_tbData.UndoLimit = 0;
			_tbData.UndoLimit = int.MaxValue;
		}

		public void SelectAll() {
			_tbData.SelectAll();
		}
	}
}
