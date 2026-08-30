using ActEditor.Core.WPF.EditorControls.ActSelectorComponents;
using GrfToWpfBridge.ActRenderer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TokeiLibrary;

namespace SDE.Tools.ActViewer {
	/// <summary>
	/// Interaction logic for FrameScrollBar.xaml
	/// </summary>
	public partial class FrameScrollBar : UserControl {
		private IActIndexSelector _selector;
		private IFrameRendererEditor _editor;
		private ScrollBarHelper _sbHelper = new ScrollBarHelper();
		private bool _eventsEnabled = true;

		public FrameScrollBar() {
			InitializeComponent();

			_sbFrameIndex.SmallChange = 1;
			_sbFrameIndex.LargeChange = 1;
		}

		public void Init(IActIndexSelector selector, IFrameRendererEditor editor) {
			CleanupEvents();

			_selector = selector;
			_editor = editor;

			_selector.FrameChanged += _frameChanged;
			_selector.AnimationPlaying += _selector_AnimationPlaying;
			_selector.ActionChanged += _selector_ActionChanged;
			_sbHelper.OverrideMouseIncrement(_sbFrameIndex, () => selector.SelectedFrame++, () => selector.SelectedFrame--);

			_editor.ActLoaded += _editor_ActLoaded;

			if (_editor.Act != null)
				_editor_ActLoaded(null);
		}

		public void CleanupEvents() {
			if (_selector != null) {
				_selector.FrameChanged -= _frameChanged;
				_selector.AnimationPlaying -= _selector_AnimationPlaying;
				_selector.ActionChanged -= _selector_ActionChanged;
			}

			if (_editor != null) {
				_editor.ActLoaded += _editor_ActLoaded;

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

		private void _commands_CommandIndexChanged(object sender, GRF.FileFormats.ActFormat.Commands.IActCommand command) {
			_updateUI();
		}

		private void _selector_ActionChanged(int index) {
			_updateUI();
			_sbFrameIndex.Value = 0;
		}

		private void _updateUI() {
			int max = _editor.Act[_selector.SelectedAction].NumberOfFrames - 1;
			max = max < 0 ? 0 : max;

			_sbFrameIndex.Minimum = 0;
			_sbFrameIndex.Maximum = max;
		}

		private void _selector_AnimationPlaying(AnimationState state) {
			this.Dispatch(delegate {
				switch (state) {
					case AnimationState.Stopped:
						_sbFrameIndex.IsEnabled = true;
						break;
					case AnimationState.StartThread:
						_sbFrameIndex.IsEnabled = false;
						break;
				}
			});
		}

		private void _sbFrameIndex_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			if (_editor.Act == null)
				return;

			_selector.OnAnimationPlaying(AnimationState.StartScrollBar);
		}

		private void _frameChanged(int frameIndex) {
			try {
				_eventsEnabled = false;
				_sbFrameIndex.Value = frameIndex;
			}
			finally {
				_eventsEnabled = true;
			}
		}

		private void _sbFrameIndex_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			_selector.OnAnimationPlaying(AnimationState.Stopped);
		}

		private void _sbFrameIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (!_eventsEnabled) return;
			if (_editor.Act == null) return;

			int value = (int)Math.Round(_sbFrameIndex.Value);

			_selector.SelectedFrame = value;
			_sbFrameIndex.Value = value;
		}

		public void SetEnabled(bool value) {
			_sbFrameIndex.IsEnabled = value;
		}
	}
}
