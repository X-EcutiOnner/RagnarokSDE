using GRF.FileFormats.ActFormat.Commands;
using GrfToWpfBridge.ActRenderer;
using System;
using System.Globalization;
using System.Windows.Controls;
using TokeiLibrary;

namespace SDE.Tools.ActViewer {
	/// <summary>
	/// Interaction logic for FrameTextBoxSelector.xaml
	/// </summary>
	public partial class FrameTextBoxSelector : UserControl {
		private IActIndexSelector _selector;
		private IFrameRendererEditor _editor;
		private bool _eventsEnabled = true;

		public FrameTextBoxSelector() {
			InitializeComponent();

			WpfUtilities.AddFocus(_tbFrameIndex);
		}

		public void Init(IActIndexSelector selector, IFrameRendererEditor editor) {
			CleanupEvents();

			_selector = selector;
			_editor = editor;

			_selector.FrameChanged += _frameChanged;
			_selector.ActionChanged += _selector_ActionChanged;
			_editor.ActLoaded += _editor_ActLoaded;

			if (_editor.Act != null)
				_editor_ActLoaded(null);
		}

		public void CleanupEvents() {
			if (_selector != null) {
				_selector.FrameChanged -= _frameChanged;
				_selector.ActionChanged -= _selector_ActionChanged;
			}

			if (_editor != null) {
				_editor.ActLoaded -= _editor_ActLoaded;

				if (_editor.Act != null) {
					_editor.Act.Commands.CommandIndexChanged -= _commands_CommandIndexChanged;
				}
			}
		}

		private void _editor_ActLoaded(object sender) {
			if (_editor.Act != null) {
				_editor.Act.Commands.CommandIndexChanged += _commands_CommandIndexChanged;
			}
		}

		private void _commands_CommandIndexChanged(object sender, IActCommand command) {
			_selector_ActionChanged(_selector.SelectedAction);
		}

		private void _selector_ActionChanged(int index) {
			int max = _editor.Act[_selector.SelectedAction].NumberOfFrames - 1;
			max = max < 0 ? 0 : max;

			_labelFrameIndex.Text = "/ " + max + " frame" + (max > 1 ? "s" : "");
		}

		private void _frameChanged(int index) {
			_eventsEnabled = false;
			_tbFrameIndex.Text = index.ToString(CultureInfo.InvariantCulture);
			_eventsEnabled = true;
		}

		private void _tbFrameIndex_TextChanged(object sender, TextChangedEventArgs e) {
			if (!_eventsEnabled || _selector == null) return;

			_eventsEnabled = false;
			int ival;

			Int32.TryParse(_tbFrameIndex.Text, out ival);

			if (ival >= _editor.Act[_selector.SelectedAction].NumberOfFrames || ival < 0) {
				ival = _editor.Act[_selector.SelectedAction].NumberOfFrames - 1;
			}

			_selector.SelectedFrame = ival;
			_eventsEnabled = true;
		}
	}
}
