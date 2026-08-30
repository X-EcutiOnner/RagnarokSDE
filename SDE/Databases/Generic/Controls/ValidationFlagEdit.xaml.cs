using System;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	/// <summary>
	/// Interaction logic for ValidationFlagEdit.xaml
	/// </summary>
	public partial class ValidationFlagEdit : UserControl {
		public ValidationFlagEdit() {
			InitializeComponent();
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(
				nameof(Text),
				typeof(string),
				typeof(ValidationFlagEdit),
				new FrameworkPropertyMetadata(
					string.Empty,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string Text {
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public Type SourceType {
			get => (Type)GetValue(SourceTypeProperty);
			set => SetValue(SourceTypeProperty, value);
		}

		public static readonly DependencyProperty SourceTypeProperty =
			DependencyProperty.Register(
				nameof(SourceType),
				typeof(Type),
				typeof(ValidationFlagEdit));
	}
}
