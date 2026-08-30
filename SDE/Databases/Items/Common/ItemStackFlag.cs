using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Items.Common {
	[Flags]
	[Register(typeof(ItemStackFlagInfo))]
	public enum ItemStackFlag : Int64 {
		Inventory = 1 << 0,
		Cart = 1 << 1,
		Storage = 1 << 2,
		GuildStorage = 1 << 3,
	}

	public static class ItemStackFlagInfo {
		public const string Marker = "";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ItemStackFlagInfo() {
			All.Add(new EnumInfoBase(ItemStackFlag.Inventory, "Inventory", Marker));
			All.Add(new EnumInfoBase(ItemStackFlag.Cart, "Cart", Marker));
			All.Add(new EnumInfoBase(ItemStackFlag.Storage, "Storage", Marker));
			All.Add(new EnumInfoBase(ItemStackFlag.GuildStorage, "Guild storage", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ItemStackFlag>(All, TypeToInfo, Marker);
		}
	}
}