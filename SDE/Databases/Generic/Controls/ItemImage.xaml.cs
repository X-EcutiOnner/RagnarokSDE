using GRF.Image;
using GRF.IO;
using SDE.Core;
using SDE.View;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Utilities.Extension;
using Utilities.Services;

namespace SDE.Databases.Generic.Controls {
	public partial class ItemImage : UserControl {
		public Image Image => _image;
		private readonly GrfImageWrapper _wrapper = new GrfImageWrapper();

		public ItemImage() {
			InitializeComponent();

			VirtualFileDataObject.SetDraggable(_image, _wrapper);
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(ItemImage),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSourceFieldChanged));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public bool DataValid {
			get => (bool)GetValue(DataValidProperty);
			set => SetValue(DataValidProperty, value);
		}

		public static readonly DependencyProperty DataValidProperty =
			DependencyProperty.Register(
				nameof(DataValid),
				typeof(bool),
				typeof(ItemImage),
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		private static void OnSourceFieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (ItemImage)d;

			var path = e.NewValue as string;

			edit.SetIcon(path);
		}

		public async void SetIcon(string path) {
			try {
				const string IconPath = @"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\item";
				var current = (SourceField ?? "").ExpandString();
				
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

				DataValid = result.isValid;

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
				DataValid = false;
				_wrapper.Image = null;
				_image.Source = null;
			}
		}
	}
}
