using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GRF.Image;
using GRF.IO;
using SDE.ApplicationConfiguration;
using SDE.Core;
using SDE.View;
using TokeiLibrary;
using Utilities.Extension;
using Utilities.Services;

namespace SDE.Databases.ClientItems.Controls {
	/// <summary>
	/// Interaction logic for ClientItemDescriptionPreview.xaml
	/// </summary>
	public partial class ClientItemDescriptionPreview : UserControl {
		private readonly GrfImageWrapper _wrapper = new GrfImageWrapper();

		public ClientItemDescriptionPreview() {
			InitializeComponent();

			VirtualFileDataObject.SetDraggable(_image, _wrapper);
			ApplicationManager.ThemeChanged += _applicationManager_ThemeChanged;
			_applicationManager_ThemeChanged();
		}

		private void _applicationManager_ThemeChanged() {
			if (DesignerProperties.GetIsInDesignMode(this))
				return;
			
			if (SdeAppConfiguration.ThemeIndex == 0) {
				_resImage.Source = ApplicationManager.GetResourceImage("collection_bg.png");
			}
			else {
				_resImage.Source = ApplicationManager.GetResourceImage("collection_bg_dark.png");
			}
		}

		public static readonly DependencyProperty ResourceProperty =
			DependencyProperty.Register(
				nameof(Resource),
				typeof(string),
				typeof(ClientItemDescriptionPreview),
				new FrameworkPropertyMetadata(default(string), OnResourceChanged));

		public string Resource {
			get => (string)GetValue(ResourceProperty);
			set => SetValue(ResourceProperty, value);
		}

		public static readonly DependencyProperty DisplayNameProperty =
			DependencyProperty.Register(
				nameof(DisplayName),
				typeof(string),
				typeof(ClientItemDescriptionPreview),
				new FrameworkPropertyMetadata(default(string)));

		public string DisplayName {
			get => (string)GetValue(DisplayNameProperty);
			set => SetValue(DisplayNameProperty, value);
		}

		private static void OnResourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (ClientItemDescriptionPreview)d;

			if (e.NewValue == null) {
				edit.Image.Source = null;
				return;
			}

			var path = e.NewValue.ToString();

			edit.SetCollection(path);
		}

		public static readonly DependencyProperty PreviewDescriptionProperty =
			DependencyProperty.Register(
				nameof(PreviewDescription),
				typeof(string),
				typeof(ClientItemDescriptionPreview),
				new FrameworkPropertyMetadata(default(string), OnPreviewDescriptionChanged));

		public string PreviewDescription {
			get => (string)GetValue(PreviewDescriptionProperty);
			set => SetValue(PreviewDescriptionProperty, value);
		}

		private static void OnPreviewDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (ClientItemDescriptionPreview)d;
			edit.SetPreviewDescription((e.NewValue as string) ?? "");
		}

		public void SetPreviewDescription(string newValue) {
			WpfUtilities.UpdateRtb(_rtbItemDescription, newValue, true);
		}

		public async void SetCollection(string path) {
			try {
				const string IconPath = @"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\collection";
				var current = (path ?? "").ExpandString();

				var result = await Task.Run(() => {
					bool isValid = false;
					GrfImage grfImage = null;
					BitmapSource bitmap = null;

					try {
						var data = SdeEditor.MetaGrf.GetDataBuffered(EncodingService.FromAnyToDisplayEncoding(GrfPath.Combine(IconPath, current) + ".bmp"));

						if (data != null) {
							grfImage = ImageProvider.GetImage(data, ".bmp");
							grfImage.MakePinkShadeTransparent();

							if (grfImage.GrfImageType == GrfImageType.Bgr24) {
								grfImage.Convert(GrfImageType.Bgra32);
							}

							bitmap = grfImage.Cast<BitmapSource>();
							isValid = true;
						}
					}
					catch {
					}

					return (isValid, grfImage, bitmap);
				});

				if (result.isValid) {
					_wrapper.Image = result.grfImage;
					_image.Tag = path;
					_image.Source = result.bitmap;
				}
				else {
					_wrapper.Image = null;
					_image.Source = null;
				}
			}
			catch {
				_wrapper.Image = null;
				_image.Source = null;
			}
		}

		public Image Image {
			get { return _image; }
		}

		public RichTextBox PreviewDescriptionTextBox {
			get { return _rtbItemDescription; }
		}
	}
}