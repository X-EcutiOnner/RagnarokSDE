using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GrfToWpfBridge;
using SDE.ApplicationConfiguration;
using SDE.View.Dialogs;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities.Extension;

namespace SDE.View.Editors {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class RateEditDialog : TkWindow, IInputWindow {
		private int _value;

		public RateEditDialog(string text)
			: base("Rate edit", "cde.ico", SizeToContent.WidthAndHeight, ResizeMode.CanResize) {
			InitializeComponent();

			_value = text.ToInt();

			_gpRate.Minimum = 0;
			_gpRate.Maximum = 10000;
			_gpRate.Value = _value;

			WindowStartupLocation = WindowStartupLocation.CenterOwner;

			Binder.Bind(_cbInc1, () => SdeAppConfiguration.RateIncrementBy1, v => SdeAppConfiguration.RateIncrementBy1 = v, delegate {
				if (SdeAppConfiguration.RateIncrementBy1 == true) {
					_cbInc5.IsChecked = false;
				}
			});
			Binder.Bind(_cbInc5, () => SdeAppConfiguration.RateIncrementBy5, v => SdeAppConfiguration.RateIncrementBy5 = v, delegate {
				if (SdeAppConfiguration.RateIncrementBy5 == true) {
					_cbInc1.IsChecked = false;
				}
			});

			WpfUtilities.AddMouseInOutUnderline(_cbInc1);
			WpfUtilities.AddMouseInOutUnderline(_cbInc5);

			Loaded += delegate {
				_gpRate.ValueChanged += _gpRate_ValueChanged;
			};
		}

		bool _subEvents = true;
		public int RateIncrement = 100;

		private void _gpRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (!_subEvents || _value == e.NewValue) return;
			Console.WriteLine("Value changed: " + e.NewValue);
			int previousValue = _value;
			_value = (int)Math.Round(e.NewValue);

			RateIncrement = 1;

			if (SdeAppConfiguration.RateIncrementBy5) {
				RateIncrement = 500;
			}

			if (SdeAppConfiguration.RateIncrementBy1) {
				RateIncrement = 100;
			}

			if (_value % RateIncrement != 0) {
				_subEvents = false;

				try {
					_value = (int)(Math.Round(_value / (float)RateIncrement, MidpointRounding.AwayFromZero) * RateIncrement);
					_gpRate.Value = _value;

					if (previousValue != _value) {
						OnValueChanged();
					}

					return;
				}
				finally {
					_subEvents = true;
				}
			}

			if (previousValue != _value)
				OnValueChanged();
		}

		public string Text {
			get { return _value.ToString(CultureInfo.InvariantCulture); }
		}

		public Grid Footer { get { return _footerGrid; } }
		public event Action ValueChanged;

		public void OnValueChanged() {
			ValueChanged?.Invoke();
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			if (!SdeAppConfiguration.UseIntegratedDialogsForFlags)
				DialogResult = true;
			Close();
		}
	}
}
