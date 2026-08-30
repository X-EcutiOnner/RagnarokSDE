using GRF.FileFormats.ActFormat;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using GRF.IO;
using GrfToWpfBridge.ActRenderer;
using GrfToWpfBridge.DrawingComponents;
using SDE.Core;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using SDE.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Utilities.Extension;
using Utilities.Services;

namespace SDE.Databases.Generic.Controls {
	public partial class MobSpriteImage : UserControl {
		public class FrameRendererEditor : IFrameRendererEditor {
			public Act Act { get; set; }
			public int SelectedAction => IndexSelector.SelectedAction;
			public int SelectedFrame => IndexSelector.SelectedFrame;
			public IActIndexSelector IndexSelector { get; set; }
			public FrameRenderer FrameRenderer { get; set; }
			public Grid GridPrimary { get; set; }
			public event FrameRendererEventDelegates.ActEditorEventDelegate ActLoaded;
			public void OnActLoaded() => ActLoaded?.Invoke(Act);
			public bool IsLoading { get; set; }
			public int PreferedLoadingAction { get; set; }
			public Canvas Canvas => FrameRenderer.Canvas;
		}

		public class DefaultDrawModule : IDrawingModule {
			private readonly Func<List<DrawingComponent>> _getComponents;
			public int DrawingPriority => 0;
			public List<DrawingComponent> GetComponents() => _getComponents() ?? new List<DrawingComponent>();
			public bool Permanent => false;

			public DefaultDrawModule(Func<List<DrawingComponent>> getComponents) {
				_getComponents = getComponents;
			}
		}

		public Image Image => _image;
		private readonly GrfImageWrapper _wrapper = new GrfImageWrapper();

		public MobSpriteImage() {
			InitializeComponent();

			VirtualFileDataObject.SetDraggable(_image, _wrapper);
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(MobSpriteImage),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSourceFieldChanged));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public static readonly DependencyProperty AliasFieldProperty =
			DependencyProperty.Register(
				nameof(AliasField),
				typeof(string),
				typeof(MobSpriteImage),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSourceFieldChanged));

		public string AliasField {
			get => (string)GetValue(AliasFieldProperty);
			set => SetValue(AliasFieldProperty, value);
		}

		public bool DataValid {
			get => (bool)GetValue(DataValidProperty);
			set => SetValue(DataValidProperty, value);
		}

		public static readonly DependencyProperty DataValidProperty =
			DependencyProperty.Register(
				nameof(DataValid),
				typeof(bool),
				typeof(MobSpriteImage),
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		private static void OnSourceFieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (MobSpriteImage)d;
			string name = edit.SourceField;

			if (!String.IsNullOrEmpty(edit.AliasField)) {
				var mobIdString = CachedDbs.AegisNameMob.ToId(edit.AliasField);

				if (mobIdString is int mobId) {
					var mobDb = SdeEditor.Project.GetTable(DataSources.Mob);
					var tuple = mobDb.TryGetTuple(mobId);
					name = tuple.GetModel<Mob>().ClientSprite;
				}
				else {
					name = null;
				}
			}

			if (String.IsNullOrEmpty(name)) {
				edit.Image.Source = null;
				return;
			}

			edit.SetImage(name);
		}

		public async void SetImage(string sprite) {
			try {
				byte[] sprData = await _tryLoadAct(sprite);
				if (sprData == null) {
					_setImage(null);
					return;
				}

				_setImage(sprData);
			}
			catch {
				_setImage(null);
			}
		}

		private void _setImage(byte[] data) {
			if (data == null) {
				_wrapper.Image = null;
				_image.Source = null;
				return;
			}

			_wrapper.Image = Spr.GetFirstImage(data);
			_wrapper.Image.MakePinkShadeTransparent();
			_wrapper.Image.MakeFirstPixelTransparent();
			
			if (_wrapper.Image.GrfImageType == GrfImageType.Bgr24) {
				_wrapper.Image.Convert(GrfImageType.Bgra32);
			}

			_image.Tag = SourceField ?? "";
			_image.Source = _wrapper.Image.Cast<BitmapSource>();
		}

		private async Task<byte[]> _tryLoadAct(string sprite) {
			const string MobPath = @"data\sprite\¸ó½ºÅÍ\";
			var current = (sprite ?? "").ExpandString();

			return await Task.Run(() => {
				return SdeEditor.MetaGrf.GetDataBuffered(EncodingService.FromAnyToDisplayEncoding(GrfPath.Combine(MobPath, current) + ".spr"));
			});
		}
	}
}
