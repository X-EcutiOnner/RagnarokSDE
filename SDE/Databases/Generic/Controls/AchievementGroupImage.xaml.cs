using GRF.Image;
using GRF.IO;
using SDE.Core;
using SDE.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Utilities.Extension;
using Utilities.Services;

namespace SDE.Databases.Generic.Controls {
	public partial class AchievementGroupImage : UserControl {
		public Image Image => _image;
		private readonly GrfImageWrapper _wrapper = new GrfImageWrapper();

		public AchievementGroupImage() {
			InitializeComponent();

			VirtualFileDataObject.SetDraggable(_image, _wrapper);
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(AchievementGroupImage),
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
				typeof(AchievementGroupImage),
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		private static void OnSourceFieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var edit = (AchievementGroupImage)d;

			if (e.NewValue == null) {
				edit.Image.Source = null;
				return;
			}

			var path = e.NewValue.ToString();

			edit.SetIcon(path);
		}

		public void SetIcon(string path) {
			try {
				const string IconPath = @"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\achievement_re";
				var current = (SourceField ?? "").ExpandString();
				byte[] data = SdeEditor.MetaGrf.GetDataBuffered(EncodingService.FromAnyToDisplayEncoding(GrfPath.Combine(IconPath, "icon_" + current) + ".bmp"));

				if (data != null) {
					DataValid = true;
					_wrapper.Image = ImageProvider.GetImage(data, ".bmp");
					_wrapper.Image.MakePinkShadeTransparent();

					if (_wrapper.Image.GrfImageType == GrfImageType.Bgr24) {
						_wrapper.Image.Convert(GrfImageType.Bgra32);
					}

					_image.Tag = SourceField ?? "";
					_image.Source = _wrapper.Image.Cast<BitmapSource>();
				}
				else {
					DataValid = false;
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
