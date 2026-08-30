using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActEditor.Core.WPF.EditorControls.ActSelectorComponents;
using ErrorManager;
using GRF.IO;
using GRF.Threading;
using GrfToWpfBridge.ActRenderer;
using GrfToWpfBridge.ActRenderer.ActSelectorComponents;
using SDE.ApplicationConfiguration;
using SDE.Databases.Items.Features;
using SDE.Editor.LuaTables;
using SDE.Tools.ActViewer;
using SDE.View;
using SDE.View.Editors;
using TokeiLibrary;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles;
using Utilities.Extension;
using Utilities.Services;
using static GrfToWpfBridge.ActRenderer.FrameRendererEventDelegates;

namespace SDE.Tools.ActViewer {
	/// <summary>
	/// Interaction logic for FrameSelector.xaml
	/// </summary>
	public partial class CompactActIndexSelector : UserControl, IActIndexSelector {
		private bool _handlersEnabled = true;
		private IFrameRendererEditor _editor;
		private FancyButton _play = new FancyButton();
		private bool _firstInitDone;

		public CompactActIndexSelector() {
			InitializeComponent();

			_setupPlayButtonUI();
			
			MouseEnter += (s, e) => Opacity = 1f;
			MouseLeave += (s, e) => Opacity = 0.7f;

			Unloaded += delegate {
				Stop();
			};
		}

		private void _setupPlayButtonUI() {
			_directionalControl.DirectionalGrid.Children.Add(_play);
			_play.SetValue(Grid.ColumnProperty, 1);
			_play.SetValue(Grid.RowProperty, 1);
			_play.Width = 16;
			_play.Height = 16;

			_updatePlay();
			_play.Click += _play_Click;
		}

		private int _selectedFrame;
		private int _selectedAction;
		private ActRenderThread _thread;
		private FrameRendererConfiguration _config;

		public int SelectedAction {
			get => _selectedAction;
			set {
				if (value == _selectedAction)
					return;

				int max = _editor.Act.NumberOfActions;
				_selectedAction = (value % max + max) % max;

				// This should always be done on the main UI thread
				this.Dispatch(_ => {
					if (SelectedFrame >= _editor.Act[_selectedAction].Frames.Count)
						SelectedFrame = 0;

					OnActionChanged(_selectedAction);
				});
			}
		}

		public int SelectedFrame {
			get => _selectedFrame;
			set {
				if (value == _selectedFrame)
					return;

				int max = _editor.Act[SelectedAction].NumberOfFrames;
				_selectedFrame = (value % max + max) % max;

				// This should always be done on the main UI thread
				this.Dispatch(_ => {
					OnFrameChanged(_selectedFrame);
				});
			}
		}

		public event IndexChangedDelegate ActionChanged;
		public event IndexChangedDelegate FrameChanged;
		public event IndexChangedDelegate SpecialFrameChanged;

		public bool IsPlaying { get; private set; }

		public void OnSpecialFrameChanged(int frameIndex) {
			if (!_handlersEnabled) return;
			SpecialFrameChanged?.Invoke(frameIndex);
		}

		public event AnimationStateEventHandler AnimationPlaying;

		public void OnAnimationPlaying(AnimationState state) {
			AnimationPlaying?.Invoke(state);
		}

		public void OnFrameChanged(int frameIndex) {
			if (!_handlersEnabled) return;
			FrameChanged?.Invoke(frameIndex);
		}

		public void OnActionChanged(int actionIndex) {
			_updateAction();
			if (!_handlersEnabled) return;
			ActionChanged?.Invoke(actionIndex);
		}

		private void _play_Click(object sender, RoutedEventArgs e) {
			if (IsPlaying)
				Stop();
			else
				Play();
		}

		public void Play() {
			if (IsPlaying) return;

			_play.Dispatch(delegate {
				_play.IsPressed = true;
				IsPlaying = true;
				_updatePlay();
				_sbFrameIndex.IsEnabled = false;
				_thread.Resume();
			});
		}

		public void Stop() {
			if (!IsPlaying) return;

			_play.Dispatch(delegate {
				_play.IsPressed = false;
				_sbFrameIndex.IsEnabled = true;
				IsPlaying = false;
				_updatePlay();
			});
		}

		public class ActRenderThread : PausableThread {
			private IActIndexSelector _selector;
			private IFrameRendererEditor _editor;

			public bool IsEnabled { get; set; } = true;

			public ActRenderThread(IActIndexSelector selector, IFrameRendererEditor editor) {
				_selector = selector;
				_editor = editor;
			}

			public void Start() {
				GrfThread.Start(_start, "GRF - ActRenderThread thread starter");
			}

			private void _start() {
				while (!IsTerminated) {
					if (!_selector.IsPlaying)
						Pause();

					ActAnimation.DoThread(_selector, _editor);
				}
			}
		}

		private void _updatePlay() {
			if (_play.IsPressed) {
				_play.ImagePath = "stop2.png";
				_play.ImageIcon.Width = 16;
				_play.ImageIcon.Stretch = Stretch.Fill;
			}
			else {
				_play.ImagePath = "play.png";
				_play.ImageIcon.Width = 16;
				_play.ImageIcon.Stretch = Stretch.Fill;
			}
		}

		private void _updateAction() {
			if (_editor.Act == null) return;

			if (SelectedAction >= _editor.Act.NumberOfActions) {
				SelectedAction = _editor.Act.NumberOfActions - 1;
			}

			if (SelectedFrame >= _editor.Act[_editor.SelectedAction].NumberOfFrames && SelectedFrame > 0) {
				SelectedFrame = Math.Max(0, _editor.Act[SelectedAction].NumberOfFrames - 1);
			}
		}

		public void Init(IFrameRendererEditor editor, int selectedAction, int selectedFrame) {
			Init(editor, selectedAction, selectedFrame, null);
		}

		public void Init(IFrameRendererEditor editor, int selectedAction, int selectedFrame, FrameRendererConfiguration config) {
			if (config != null)
				_config = config;

			if (_thread == null) {
				_thread = new ActRenderThread(this, editor);
				_thread.Start();
			}

			if (editor.Act != null && selectedAction >= editor.Act.NumberOfActions) {
				selectedAction = ActAnimation.SafeActionIndex(editor.Act, selectedAction);
			}

			_editor = editor;
			_directionalControl.Init(this, _editor);
			_sbFrameIndex.Init(this, _editor);
			_frameTbSelect.Init(this, _editor);
			if (!_firstInitDone)
				ActSelectorHelper.InitSelectorComboBox(_editor, _comboBoxActionIndex, _comboBoxAnimationIndex);
			if (editor.Act != null)
				editor.IndexSelector.OnActionChanged(SelectedAction);
			_firstInitDone = true;
		}

		public void DisableActionChange() {
			_directionalControl.Reset();
			_comboBoxActionIndex.IsEnabled = false;
			_comboBoxAnimationIndex.IsEnabled = false;
		}

		private void _buttonRenderMode_Click(object sender, RoutedEventArgs e) {
			_config.ActEditorScalingMode.Set(_config.ActEditorScalingMode.Get() == BitmapScalingMode.NearestNeighbor ? BitmapScalingMode.Fant : BitmapScalingMode.NearestNeighbor);
		}

		private void _buttonSettings_Click(object sender, RoutedEventArgs e) {
			WindowProvider.ShowWindow(new PreviewSettingsDialog(_editor, _config), WpfUtilities.FindDirectParentControl<Window>(this));
		}

		private void _buttonExport_Click(object sender, RoutedEventArgs e) {
			try {

				var tuple = ViewIdPreviewDialog.LatestTupe;

				if (tuple == null)
					return;

				var sprite = LuaHelper.GetSpriteFromViewId(tuple.GetModel<Item>().View.ToInt(), LuaHelper.ViewIdTypes.Headgear, tuple);

				string[] files = new string[] {
					@"data\sprite\¾ÆÀÌÅÛ\" + sprite + ".spr",
					@"data\sprite\¾ÆÀÌÅÛ\" + sprite + ".act",
					@"data\sprite\¾Ç¼¼»ç¸®\³²\³²_" + sprite + ".spr",
					@"data\sprite\¾Ç¼¼»ç¸®\³²\³²_" + sprite + ".act",
					@"data\sprite\¾Ç¼¼»ç¸®\¿©\¿©_" + sprite + ".spr",
					@"data\sprite\¾Ç¼¼»ç¸®\¿©\¿©_" + sprite + ".act",
					@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\collection\" + sprite + ".bmp",
					@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\item\" + sprite + ".bmp"
				};

				string path = PathRequest.FolderEditor();

				if (path == null)
					return;

				var grf = SdeEditor.MetaGrf;

				foreach (var file in files) {
					var data = grf.GetData(file);

					if (data != null) {
						string subPath = GrfPath.Combine(path, file);
						GrfPath.CreateDirectoryFromFile(subPath);
						File.WriteAllBytes(subPath, data);
					}
				}

				OpeningService.OpenFolder(path);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}