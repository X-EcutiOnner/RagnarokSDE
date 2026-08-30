using System.Windows.Media;
using GRF.Image;
using GrfToWpfBridge;
using GrfToWpfBridge.ActRenderer;
using SDE.ApplicationConfiguration;
using SDE.View.Controls;
using TokeiLibrary.WPF.Styles;
using static GrfToWpfBridge.ActRenderer.FrameRendererConfiguration;

namespace SDE.Tools.ActViewer {
	/// <summary>
	/// Interaction logic for SettingsDialog.xaml
	/// </summary>
	public partial class PreviewSettingsDialog : TkWindow {
		private IFrameRendererEditor _editor;
		private FrameRendererConfiguration _config;

		public PreviewSettingsDialog(IFrameRendererEditor editor, FrameRendererConfiguration config) : base("Advanced settings", "settings.png") {
			_editor = editor;
			_config = config;

			InitializeComponent();

			_colorPreviewPanelBakground.Color = SdeAppConfiguration.ActEditorBackgroundColor;
			_colorPreviewPanelBakground.Init(SdeAppConfiguration.ConfigAsker.RetrieveSetting(() => SdeAppConfiguration.ActEditorBackgroundColor));

			_colorPreviewPanelBakground.ColorChanged += delegate(object sender, Color value) {
				config.ActEditorBackgroundColor = value;
				editor.FrameRenderer.Update();
			};

			_colorPreviewPanelBakground.PreviewColorChanged += delegate(object sender, Color value) {
				config.ActEditorBackgroundColor = value;
				editor.FrameRenderer.Update();
			};

			_set(_colorGridLH, _config.ActEditorGridLineHorizontal);
			_set(_colorGridLV, _config.ActEditorGridLineVertical);
			_set(_colorSpriteBorder, _config.ActEditorSpriteSelectionBorder);
			_set(_colorSpriteOverlay, _config.ActEditorSpriteSelectionBorderOverlay);
			_set(_colorSelectionBorder, _config.ActEditorSelectionBorder);
			_set(_colorSelectionOverlay, _config.ActEditorSelectionBorderOverlay);
		}

		private void _set(QuickColorSelector qcs, QuickSetting<GrfColor> setting) {
			qcs.Color = setting.Get().ToColor();
			qcs.Init(setting);

			qcs.ColorChanged += delegate(object sender, Color value) {
				setting.Set(value.ToGrfColor());
				_editor.FrameRenderer.Update();
			};

			qcs.PreviewColorChanged += delegate(object sender, Color value) {
				SdeAppConfiguration.ConfigAsker.IsAutomaticSaveEnabled = false;
				setting.Set(value.ToGrfColor());
				SdeAppConfiguration.ConfigAsker.IsAutomaticSaveEnabled = true;
				_editor.FrameRenderer.Update();
			};
		}
	}
}
