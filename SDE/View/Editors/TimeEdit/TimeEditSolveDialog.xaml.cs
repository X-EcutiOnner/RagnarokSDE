using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SDE.ApplicationConfiguration;
using SDE.View.Dialogs;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace SDE.View.Editors.TimeEdit {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class TimeEditSolveDialog : TkWindow, IInputWindow {
		private TimeViewModel _viewModel;

		public TimeEditSolveDialog(string text, bool seconds = false)
			: base("Time edit", "cde.ico", SizeToContent.WidthAndHeight, ResizeMode.CanResize) {
			InitializeComponent();

			_viewModel = new TimeViewModel();
			DataContext = _viewModel;

			_viewModel.SetModel(Time.Parse(text));
			_viewModel.PropertyChanged += _viewModel_PropertyChanged;

			WpfUtilities.AddMouseInOutUnderline(_cbExactTime);

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(TimeViewModel.Output)) {
				ValueChanged?.Invoke();
			}
		}

		public string Text => _viewModel.Output;
		public Grid Footer { get { return _footerGrid; } }
		public event Action ValueChanged;

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
