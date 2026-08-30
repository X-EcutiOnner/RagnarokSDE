using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Items.Common {
	[Register(typeof(ItemTypeInfo))]
	public enum ItemType {
		IT_HEALING = 0,
		IT_UNKNOWN, //1
		IT_USABLE,  //2
		IT_ETC,     //3
		IT_ARMOR,   //4
		IT_WEAPON,  //5
		IT_CARD,    //6
		IT_PETEGG,  //7
		IT_PETARMOR,//8
		IT_UNKNOWN2,//9
		IT_AMMO,    //10
		IT_DELAYCONSUME,//11
		IT_SHADOWGEAR,  //12
		IT_CASH = 18,
	}

	public static class ItemTypeInfo {
		public const string Marker = "IT_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ItemTypeInfo() {
			All.Add(new EnumInfoBase(ItemType.IT_HEALING, "Healing item", Marker, "Healing"));
			All.Add(new EnumInfoBase(ItemType.IT_USABLE, "Usable item", Marker, "Usable"));
			All.Add(new EnumInfoBase(ItemType.IT_ETC, "Misc item", Marker, "Etc"));
			All.Add(new EnumInfoBase(ItemType.IT_ARMOR, "Armor", Marker, "Armor"));
			All.Add(new EnumInfoBase(ItemType.IT_WEAPON, "Weapon", Marker, "Weapon"));
			All.Add(new EnumInfoBase(ItemType.IT_CARD, "Card", Marker, "Card"));
			All.Add(new EnumInfoBase(ItemType.IT_PETEGG, "Pet egg", Marker, "Petegg"));
			All.Add(new EnumInfoBase(ItemType.IT_PETARMOR, "Pet equipment", Marker, "Petarmor"));
			All.Add(new EnumInfoBase(ItemType.IT_AMMO, "Arrow and ammunition", Marker, "Ammo"));
			All.Add(new EnumInfoBase(ItemType.IT_DELAYCONSUME, "Usable with delayed consumption", Marker, "DelayConsume"));
			All.Add(new EnumInfoBase(ItemType.IT_SHADOWGEAR, "Shadow equipment", Marker, "Shadowgear"));
			All.Add(new EnumInfoBase(ItemType.IT_CASH, "Cash", Marker, "Cash"));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ItemType>(All, TypeToInfo, Marker);
		}
	}
}
