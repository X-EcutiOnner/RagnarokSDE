using SDE.Databases.Items.Features;
using SDE.View.Dialogs;
using SDE.View.Editors;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Items.Controls {
	public partial class JobEditButton : UserControl {
		public JobEditButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(nameof(PreviewText), typeof(string), typeof(JobEditButton), new PropertyMetadata(string.Empty, OnPreviewTextChanged));

		private static void OnPreviewTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var JobEditButton = (JobEditButton)d;

			JobEditButton._button.Content = e.NewValue.ToString();
		}

		public string PreviewText {
			get => (string)GetValue(PreviewTextProperty);
			set => SetValue(PreviewTextProperty, value);
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(JobEditButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public bool Millisecond {
			get => (bool)GetValue(MillisecondProperty);
			set => SetValue(MillisecondProperty, value);
		}

		public static readonly DependencyProperty MillisecondProperty =
			DependencyProperty.Register(
				nameof(Millisecond),
				typeof(bool),
				typeof(JobEditButton),
				new FrameworkPropertyMetadata(false));

		private void _button_Click(object sender, RoutedEventArgs e) {
			if (DataContext is ItemViewModel vm) {
				JobEditDialog dialog = new JobEditDialog(vm.Model);
				InputWindowHelper.Edit(dialog, v => SourceField = v, _button);
			}
		}
	}
}
