using SDE.Editor.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Extension;

namespace SDE.Editor.Shops {
	public class Shop {
		public List<ShopItem> Items = new List<ShopItem>();
		public ShopType Type = ShopType.Shop;
		public string NpcPosition;
		public string NpcDisplayName;
		public string NpcViewId;
		public string Currency;
		public string ShopCode;

		public Shop() {
		}

		public Shop(string toParse) {
			ShopCode = toParse;

			string[] data = toParse.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
			Items.Clear();

			if (data.Length > 3) {
				NpcPosition = data[0];
				data = data.Skip(1).ToArray();
			}

			if (data.Length > 2) {
				if (data[0] == "shop")
					Type = ShopType.Shop;
				else if (data[0] == "trader")
					Type = ShopType.Trader;
				else
					Type = ShopType.Shop;
				data = data.Skip(1).ToArray();
			}

			if (data.Length > 1) {
				NpcDisplayName = data[0];
				data = data.Skip(1).ToArray();
			}

			if (data.Length > 0) {
				var shopInformation = data[0].Split(new char[] { ',' });

				if (shopInformation.Length > 0 && !shopInformation[0].Contains(":")) {
					NpcViewId = shopInformation[0];
					shopInformation = shopInformation.Skip(1).ToArray();
				}

				if (shopInformation.Length > 0 && !shopInformation[0].Contains(":")) {
					Currency = shopInformation[0];
					shopInformation = shopInformation.Skip(1).ToArray();
				}

				if (Type == ShopType.Trader) {
					foreach (var line in toParse.Split(new string[] { "\r\n" }, StringSplitOptions.None)) {
						var subLine = line.Trim('\t', ' ', ';');

						if (subLine.StartsWith("sellitem ")) {
							subLine = subLine.Substring("sellitem ".Length);

							data = subLine.Split(',');
							int itemId = 0;

							if (data.Length > 0) {
								if (Int32.TryParse(data[0], out itemId)) {
								}
								else {
									itemId = CachedDbs.AegisNameItem.ToStringId(data[0]).ToInt();
								}

								data = data.Skip(1).ToArray();
							}

							int price = -1;

							if (data.Length > 0) {
								price = Int32.Parse(data[0]);
							}

							Items.Add(new ShopItem { Item = itemId.ToString(), Price = price.ToString() });
						}
					}

					return;
				}

				foreach (var shopItem in shopInformation) {
					data = shopItem.Split(':');
					Items.Add(new ShopItem { Item = data[0], Price = data[1] });
				}
			}
		}
	}
}
