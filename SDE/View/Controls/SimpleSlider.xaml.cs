using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utilities;

namespace SDE.View.Controls {
	/// <summary>
	/// Interaction logic for SimpleSlider.xaml
	/// </summary>
	public partial class SimpleSlider : UserControl {
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(SimpleSlider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public double Value {
			get => (double)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(SimpleSlider), new PropertyMetadata(0.0, null));

		public double Minimum {
			get => (double)_slider.GetValue(Slider.MinimumProperty);
			set => _slider.SetValue(Slider.MinimumProperty, value);
		}

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(SimpleSlider), new PropertyMetadata(1.0, null));

		public double Maximum {
			get => (double)_slider.GetValue(Slider.MaximumProperty);
			set => _slider.SetValue(Slider.MaximumProperty, value);
		}

		public delegate void DragStartEventHandler(object sender, double value);
		public event DragStartEventHandler ValueDragStart;
		public void OnValueDragStart(double value) => ValueDragStart?.Invoke(this, value);

		public event DragStartEventHandler ValueDragEnd;
		public void OnValueDragEnd(double value) => ValueDragEnd?.Invoke(this, value);

		public event RoutedPropertyChangedEventHandler<double> ValueChanged;

		public SimpleSlider() {
			InitializeComponent();

			_slider.ValueChanged += _slider_ValueChanged;
		}

		private void _slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			ValueChanged?.Invoke(sender, e);
		}

		private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			OnValueDragStart(Value);
			CaptureMouse();
			//UpdateSliderValue(e.GetPosition(_slider));
		}

		private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			if (IsMouseCaptured) {
				ReleaseMouseCapture();
				OnValueDragEnd(Value);
				ValueChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<double>(Value, Value));
			}
		}

		private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e) {
			if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed) {
				UpdateSliderValue(e.GetPosition(_slider));
			}
		}

		private void UpdateSliderValue(Point mousePos) {
			double realWidth = _slider.ActualWidth - 8;
			double realX = mousePos.X - 4;
			realX = Methods.Clamp(realX, 0, realWidth);

			double relativePosition = realX / realWidth;

			if (_slider.IsSnapToTickEnabled) {
				var distance = (_slider.Maximum - _slider.Minimum);
				double interval = _slider.TickFrequency;
				relativePosition = Math.Round(relativePosition * distance) / distance;
			}

			var newValue = _slider.Minimum + relativePosition * (_slider.Maximum - _slider.Minimum);

			if (newValue == Value)
				return;

			Value = newValue;
		}
	}
}
