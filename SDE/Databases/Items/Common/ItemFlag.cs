using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Items.Common {
	[Flags]
	[Register(typeof(ItemFlagInfo))]
	public enum ItemFlag : Int64 {
		BuyingStore = 1 << 0,
		DeadBranch = 1 << 1,
		Container = 1 << 2,
		UniqueId = 1 << 3,
		BindOnEquip = 1 << 4,
		DropAnnounce = 1 << 5,
		NoConsume = 1 << 6,
	}

	public static class ItemFlagInfo {
		public const string Marker = "";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ItemFlagInfo() {
			All.Add(new EnumInfoBase(ItemFlag.BuyingStore, "Buying store", Marker, "BuyingStore"));
			All.Add(new EnumInfoBase(ItemFlag.DeadBranch, "Dead branch", Marker, "DeadBranch"));
			All.Add(new EnumInfoBase(ItemFlag.Container, "Container", Marker, "Container"));
			All.Add(new EnumInfoBase(ItemFlag.UniqueId, "Unique id", Marker, "UniqueId"));
			All.Add(new EnumInfoBase(ItemFlag.BindOnEquip, "Bind on equip", Marker, "BindOnEquip"));
			All.Add(new EnumInfoBase(ItemFlag.DropAnnounce, "Drop announce", Marker, "DropAnnounce"));
			All.Add(new EnumInfoBase(ItemFlag.NoConsume, "No consume", Marker, "NoConsume"));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ItemFlag>(All, TypeToInfo, Marker);
		}
	}
}