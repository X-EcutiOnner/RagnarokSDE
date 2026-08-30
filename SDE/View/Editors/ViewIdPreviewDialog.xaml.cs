using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GrfToWpfBridge.ActRenderer;
using GrfToWpfBridge.DrawingComponents;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Engines.PreviewEngine;
using SDE.Editor.Generic.DbTabs;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;
using static SDE.Databases.Generic.Controls.MobSpriteImage;

namespace SDE.View.Editors {
	public class ViewIdActs {
		public Act Body;
		public Act Head;
		public bool IsGarment;
	};

	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class ViewIdPreviewDialog : TkWindow {
		public static bool IsOpened = false;
		private readonly SdeEditor _sdeEditor;
		private DbTab _tab;
		private Database.Tuple _lastTuple;
		private readonly PreviewHelper _helper;
		private ViewIdActs _viewIdActs = new ViewIdActs();

		public ViewIdPreviewDialog(SdeEditor sdeEditor, DbTab tab) : base("View ID preview", "eye.png", SizeToContent.Manual, ResizeMode.CanResize) {
			_tab = tab;
			_sdeEditor = sdeEditor;
			_sdeEditor._mainTabControl.SelectionChanged += _mainTabControl_SelectionChanged;
			Width = 400;
			Height = 300;

			InitializeComponent();
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = WpfUtilities.TopWindow;
			WindowStyle = WindowStyle.ToolWindow;

			_initializeActRenderer();

			_helper = new PreviewHelper(_listView, _tab.Database, _editor, _gridSpriteMissing, _tbSpriteMissing, _viewIdActs);
			
			this.Loaded += delegate {
				Width = 400;
				Height = 300;
				IsOpened = true;
			};

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_listView, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.RangeColumnInfo {Header = "Job Name", DisplayExpression = "Name", SearchGetAccessor = "Name", IsFill = true, ToolTipBinding = "Name", TextWrapping = TextWrapping.Wrap}
			}, null, new string[] { "Normal", "{DynamicResource TextForeground}" });

			_tupleUpdate();
		}

		private FrameRendererEditor _editor = new FrameRendererEditor();
		private FrameRendererConfiguration _config;

		private void _initializeActRenderer() {
			_editor.IndexSelector = _indexSelector;
			_editor.FrameRenderer = _renderer;
			_editor.GridPrimary = _gridActRenderer;

			_config = new FrameRendererConfiguration(SdeAppConfiguration.ConfigAsker);
			_indexSelector.Init(_editor, 0, 0, _config);

			_renderer.RelativeCenter = new Point(0.5d, 0.8d);
			_renderer.Init(_editor, _config);
			_renderer.Canvas.Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));

			_renderer.DrawingModules.Add(new DefaultDrawModule(delegate {
				var components = new List<DrawingComponent>();

				if (_viewIdActs.IsGarment) {
					switch (_renderer.SelectedAction % 8) {
						case 0:
						case 1:
						case 2:
						case 7:
							if (_editor.Act != null)
								components.Add(new ActDraw(_editor.Act));

							if (_viewIdActs.Body != null)
								components.Add(new ActDraw(_viewIdActs.Body));

							if (_viewIdActs.Head != null)
								components.Add(new ActDraw(_viewIdActs.Head));
							
							return components;
					}
				}

				if (_viewIdActs.Body != null)
					components.Add(new ActDraw(_viewIdActs.Body));

				if (_viewIdActs.Head != null)
					components.Add(new ActDraw(_viewIdActs.Head));

				if (_editor.Act != null)
					components.Add(new ActDraw(_editor.Act));

				return components;
			}));

			_editor.FrameRenderer.GridZoom.VerticalAlignment = VerticalAlignment.Bottom;
			_editor.FrameRenderer.GridZoom.HorizontalAlignment = HorizontalAlignment.Left;
		}

		public static ReadableTuple LatestTupe { get; set; }

		private void _tupleUpdate(bool bypass = false) {
			try {
				if ((_tab.Database.Source & DataSources.ServerItems) != 0) {
					var tuple = _tab.SelectedItem;

					ViewIdPreviewDialog.LatestTupe = tuple;

					if (tuple == null) return;
					if (!bypass) {
						if (tuple == _lastTuple) return;

						if (_lastTuple != null) {
							_lastTuple.TupleModified -= _tupleUpdate;
						}

						_lastTuple = tuple;
						_lastTuple.TupleModified += _tupleUpdate;
					}

					_helper.Read(tuple);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _tupleUpdate(object sender, bool value) {
			_tupleUpdate(true);
		}
		private void _mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				if (_sdeEditor._mainTabControl.SelectedIndex >= 0 && _sdeEditor._mainTabControl.Items[_sdeEditor._mainTabControl.SelectedIndex] is DbTab) {
					_tab = (DbTab)_sdeEditor._mainTabControl.Items[_sdeEditor._mainTabControl.SelectedIndex];
					_tupleUpdate();
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			_sdeEditor._mainTabControl.SelectionChanged -= _mainTabControl_SelectionChanged;
			if (_lastTuple != null) {
				_lastTuple.TupleModified -= _tupleUpdate;
			}
			base.OnClosing(e);
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}
	}
}
