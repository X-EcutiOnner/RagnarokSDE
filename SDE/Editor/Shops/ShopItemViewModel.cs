using GRF.Image;
using GrfToWpfBridge;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.Generic.Features;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Features;
using SDE.Editor.Database;
using SDE.View;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using Utilities;
using Utilities.Extension;

namespace SDE.Editor.Shops {
	public class ShopItemViewModel : BaseModelView<ShopItem> {
		private static readonly GrfImage _imgNumbers;
		private static readonly BitmapSource _imgShadow;

		static ShopItemViewModel() {
			var imageData = ApplicationManager.GetResource("numbers.bmp");
			_imgNumbers = new GrfImage(imageData);
			_imgNumbers.Convert(GrfImageType.Bgra32);
			_imgNumbers.MakePinkShadeTransparent();

			_imgShadow = new GrfImage(ApplicationManager.GetResource("shop_back.bmp")).Cast<BitmapSource>();
		}

		private readonly ShopViewModel _vm;

		public ShopItemViewModel(ShopViewModel viewModel, ShopItem model) {
			Model = model;
			_vm = viewModel;
		}

		public string Item {
			get => Model.Item;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(ImageItem));
				OnPropertyChanged(nameof(PreviewItemName));
				OnPropertyChanged(nameof(PreviewItemNameShadow));

				if (Price.ToInt() == -1)
					OnPropertyChanged(nameof(ImagePrice));
			}
		}
		public string Price { get => Model.Price; set { ExecuteCommand(value); OnPropertyChanged(nameof(ImagePrice)); } }
		public BitmapSource ImageItemShadow => SdeAppConfiguration.ThemeIndex == 0 ? _imgShadow : null;
		public BitmapSource ImageItem {
			get {
				var metaGrf = SdeEditor.MetaGrf;
				var citemDb = SdeEditor.Project.GetTable(DataSources.ClientItem);
				var ctuple = citemDb.TryGetTuple(Item.ToInt());

				var imagePath = (@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\item\" + (ctuple == null ? "" : ctuple.GetModel<ClientItem>().IdentifiedResourceName ?? "") + ".bmp").ToDisplayEncoding();

				if (metaGrf.Exists(imagePath)) {
					var img = new GrfImage(metaGrf.FileTable[imagePath]);
					img.MakePinkShadeTransparent();
					img.Convert(GrfImageType.Bgra32);
					return img.Cast<BitmapSource>();
				}

				return null;
			}
		}
		public string PreviewItemName => DbUtilities.ItemId2Name(Item);
		public string PreviewItemNameShadow => SdeAppConfiguration.ThemeIndex == 1 ? "" : DbUtilities.ItemId2Name(Item);
		public BitmapSource ImagePrice => CreateImagePrice();

		private BitmapSource CreateImagePrice() {
			int price = Price.ToInt();

			if (price == -1) {
				var itemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);
				var tuple = itemDb.TryGetTuple(Item.ToInt());

				if (tuple != null) {
					var model = tuple.GetModel<Item>();
					string value = model.Buy ?? "";

					if (value == "") {
						price = DbReader.ToInt(model.Sell) * 2;
					}
					else {
						Int32.TryParse(value, out price);
					}
				}
				else {
					price = 0;
				}
			}

			GrfImage img = CreateImagePriceSub(price);

			if (SdeAppConfiguration.UseDiscount) {
				var newPrice = (int)(price * .76);

				if (price == 1) {
					newPrice = 1;
				}

				if (newPrice > price)
					newPrice = price;

				if (newPrice != price) {
					GrfImage img2 = CreateImagePriceSub(newPrice);

					img = img.Extract(0, 0, img.Width - 14, 11);
					if (SdeAppConfiguration.ThemeIndex == 0) {
						_append(img, 12, GrfColor.Black);
					}
					else if (SdeAppConfiguration.ThemeIndex == 1) {
						_append(img, 12, ((Color)Application.Current.Resources["UIThemeDefaultTextColor"]).ToGrfColor());
					}
					img.SetPixelsUnrestricted(img.Width, 0, img2);
				}
			}

			return img.Cast<BitmapSource>();
		}

		private GrfImage CreateImagePriceSub(int price) {
			if (price < 0)
				price = 0;

			GrfImage img = new GrfImage(new byte[] { 0, 0, 0, 0 }, 0, 0, GrfImageType.Bgra32);

			var str = price.ToString(CultureInfo.InvariantCulture);
			GrfColor color;

			if (SdeAppConfiguration.ThemeIndex == 0) {
				color = GrfColor.Black;
			}
			else {
				color = ((Color)Application.Current.Resources["UIThemeDefaultTextColor"]).ToGrfColor();
			}

			for (int i = 0; i < str.Length; i++) {
				int v = str[i] - '0';

				if ((str.Length - i) % 3 == 0 && i != 0) {
					_append(img, 10, color);
				}

				_append(img, v, color);
			}

			var img2 = img.Copy();
			img2 = img2.Extract(1, 0, img.Width - 1, img.Height);

			if (SdeAppConfiguration.ThemeIndex == 0) {
				if (!SdeAppConfiguration.UseZenyColors) {
				}
				else if (price < 10) {
					_setColor(img, "#00ffff");
					img.SetPixelsUnrestricted(0, 0, img2, true);
				}
				else if (price < 100) {
					_setColor(img, "#ce00ce");
					_setColor(img2, "#0000ff");
					img.SetPixelsUnrestricted(0, 0, img2);
				}
				else if (price < 1000) {
					_setColor(img, "#00ffff");
					_setColor(img2, "#0000ff");
					img.SetPixelsUnrestricted(0, 0, img2, true);
				}
				else if (price < 10000) {
					_setColor(img, "#ffff00");
					_setColor(img2, "#ff0000");
					img.SetPixelsUnrestricted(0, 0, img2, true);
				}
				else if (price < 100000) {
					_setColor(img, "#ff18ff");
				}
				else if (price < 1000000) {
					_setColor(img, "#0000ff");
				}
				else if (price < 10000000) {
					_setColor(img, "#00ff00");
					img.SetPixelsUnrestricted(0, 0, img2, true);
				}
				else if (price < 100000000) {
					_setColor(img, "#ff0000");
				}
				else {
					_setColor(img, "#cece63");
					img.SetPixelsUnrestricted(0, 0, img2, true);
				}

				_append(img, 11, GrfColor.Black);
			}
			else if (SdeAppConfiguration.ThemeIndex == 1) {
				if (!SdeAppConfiguration.UseZenyColors) {
				}
				else if (price < 10) {
					_setColor(img, "#00ffff");
				}
				else if (price < 100) {
					_setColor(img, ((Color)Application.Current.Resources["UIThemePropertyAddedColor"]).ToGrfColor());
				}
				else if (price < 1000) {
					_setColor(img, ((Color)Application.Current.Resources["UIThemePropertyLzmaColor"]).ToGrfColor());
				}
				else if (price < 10000) {
					_setColor(img, ((Color)Application.Current.Resources["UIThemePropertyEncryptedColor"]).ToGrfColor());
				}
				else if (price < 100000) {
					_setColor(img, ((Color)Application.Current.Resources["UIThemePropertyLzmaColor"]).ToGrfColor());
				}
				else if (price < 1000000) {
					_setColor(img, ((Color)Application.Current.Resources["UIThemePropertyAddedColor"]).ToGrfColor());
				}
				else if (price < 10000000) {
					_setColor(img, "#00DA00");
				}
				else if (price < 100000000) {
					_setColor(img, ((Color)Application.Current.Resources["UIThemePropertyRemovedColor"]).ToGrfColor());
				}
				else {
					_setColor(img, "#cece63");
				}

				_append(img, 11, ((Color)Application.Current.Resources["UIThemeDefaultTextColor"]).ToGrfColor());
			}

			return img;
		}

		private void _append(GrfImage imgSource, int elementIndex, in GrfColor color) {
			int x = imgSource.Width;

			imgSource.SetPixelsUnrestricted(x, 0, _getElement(elementIndex, color));
		}

		private GrfImage _setColor(GrfImage img, string color) {
			return _setColor(img, new GrfColor(color));
		}

		private GrfImage _setColor(GrfImage img, in GrfColor color) {
			for (int i = 0; i < img.Pixels.Length; i += 4) {
				if (img.Pixels[i + 3] != 0) {
					img.Pixels[i + 0] = color.B;
					img.Pixels[i + 1] = color.G;
					img.Pixels[i + 2] = color.R;
				}
			}

			return img;
		}

		private GrfImage _getElement(int elementIndex, in GrfColor color) {
			if (elementIndex < 11) {
				return _setColor(_imgNumbers.Extract(7 * elementIndex, 0, elementIndex < 10 ? 7 : 3, 11), color);
			}

			if (elementIndex == 11)
				return _setColor(_imgNumbers.Extract(73, 0, 15, 11), color);
			if (elementIndex == 12)
				return _setColor(_imgNumbers.Extract(88, 0, 16, 11), color);

			return _setColor(_imgNumbers.Extract(73, 0, 15, 11), color);
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			var currentValue = TypeTreeHelper.GetValue(Model, fieldName);

			if (currentValue.ToString() == value.ToString())
				return;

			TypeTreeHelper.SetValue(Model, fieldName, value);
			_vm.UpdateShopScript();
			OnPropertyChanged(fieldName);
		}
	}
}
