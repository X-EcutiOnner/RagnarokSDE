using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Editor.Shops {
	[Register(typeof(ShopTypeInfo))]
	public enum ShopType {
		Shop,
		Trader,
	}

	public static class ShopTypeInfo {
		public const string Marker = "NOCAST_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ShopTypeInfo() {
			All.Add(new EnumInfoBase(ShopType.Shop, "shop", Marker));
			All.Add(new EnumInfoBase(ShopType.Trader, "trader", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ShopType>(All, TypeToInfo, Marker);
		}
	}
}
