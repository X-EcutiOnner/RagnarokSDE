using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Items.Common {
	[Register(typeof(WeaponTypeInfo))]
	public enum WeaponType {
		W_FIST, //Bare hands
		W_DAGGER,   //1
		W_1HSWORD,  //2
		W_2HSWORD,  //3
		W_1HSPEAR,  //4
		W_2HSPEAR,  //5
		W_1HAXE,    //6
		W_2HAXE,    //7
		W_MACE, //8
		W_2HMACE,   //9 (unused)
		W_STAFF,    //10
		W_BOW,  //11
		W_KNUCKLE,  //12
		W_MUSICAL,  //13
		W_WHIP, //14
		W_BOOK, //15
		W_KATAR,    //16
		W_REVOLVER, //17
		W_RIFLE,    //18
		W_GATLING,  //19
		W_SHOTGUN,  //20
		W_GRENADE,  //21
		W_HUUMA,    //22
		W_2HSTAFF,  //23
		MAX_WEAPON_TYPE,
		// dual-wield constants
		W_DOUBLE_DD, // 2 daggers
		W_DOUBLE_SS, // 2 swords
		W_DOUBLE_AA, // 2 axes
		W_DOUBLE_DS, // dagger + sword
		W_DOUBLE_DA, // dagger + axe
		W_DOUBLE_SA, // sword + axe
		MAX_WEAPON_TYPE_ALL,
		W_SHIELD = MAX_WEAPON_TYPE,
	}

	public static class WeaponTypeInfo {
		public const string Marker = "W_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static WeaponTypeInfo() {
			All.Add(new EnumInfoBase(WeaponType.W_FIST, "Fist", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_DAGGER, "Dagger", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_1HSWORD, "1hSword", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_2HSWORD, "2hSword", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_1HSPEAR, "1hSpear", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_2HSPEAR, "2hSpear", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_1HAXE, "1hAxe", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_2HAXE, "2hAxe", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_MACE, "Mace", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_2HMACE, "2hMace", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_STAFF, "Staff", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_BOW, "Bow", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_KNUCKLE, "Knuckle", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_MUSICAL, "Musical", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_WHIP, "Whip", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_BOOK, "Book", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_KATAR, "Katar", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_REVOLVER, "Revolver", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_RIFLE, "Rifle", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_GATLING, "Gatling", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_SHOTGUN, "Shotgun", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_GRENADE, "Grenade", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_HUUMA, "Huuma", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_2HSTAFF, "2hStaff", Marker));
			All.Add(new EnumInfoBase(WeaponType.W_SHIELD, "Shield", Marker));

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<WeaponType>(All, TypeToInfo, Marker);
		}
	}
}
