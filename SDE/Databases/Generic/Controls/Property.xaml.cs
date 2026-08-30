using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	/// <summary>
	/// Interaction logic for Property.xaml
	/// </summary>
	public partial class Property : UserControl {
		public Property() {
			InitializeComponent();
		}

		internal TextBlock LabelControl => _tbPropertyLabel;
		internal Grid RootGrid => _grid;

		public static readonly DependencyProperty EditorProperty = DependencyProperty.Register(nameof(Editor), typeof(object), typeof(Property), new PropertyMetadata(null));
		
		public object Editor {
			get => GetValue(EditorProperty);
			set => SetValue(EditorProperty, value);
		}

		public ObservableCollection<UIElement> Extras { get; } = new ObservableCollection<UIElement>();

		public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(Property), new FrameworkPropertyMetadata(string.Empty, OnLabelPropertyChanged));
		
		public string Label {
			get => (string)GetValue(LabelProperty);
			set => SetValue(LabelProperty, value);
		}

		private static void OnLabelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var p = (Property)d;

			var label = e.NewValue as string;

			if (String.IsNullOrEmpty(label))
				p.LabelControl.Visibility = Visibility.Collapsed;
			else
				p.LabelControl.Visibility = Visibility.Visible;
		}
	}
}
