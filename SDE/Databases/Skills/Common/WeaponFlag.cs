using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(WeaponFlagInfo))]
	public enum WeaponFlag : Int64 {
		W_FIST = 1L << 0, //Bare hands
		W_DAGGER = 1L << 1,   //1
		W_1HSWORD = 1L << 2,  //2
		W_2HSWORD = 1L << 3,  //3
		W_1HSPEAR = 1L << 4,  //4
		W_2HSPEAR = 1L << 5,  //5
		W_1HAXE = 1L << 6,    //6
		W_2HAXE = 1L << 7,    //7
		W_MACE = 1L << 8, //8
		W_2HMACE = 1L << 9,   //9 (unused)
		W_STAFF = 1L << 10,    //10
		W_BOW = 1L << 11,  //11
		W_KNUCKLE = 1L << 12,  //12
		W_MUSICAL = 1L << 13,  //13
		W_WHIP = 1L << 14, //14
		W_BOOK = 1L << 15, //15
		W_KATAR = 1L << 16,    //16
		W_REVOLVER = 1L << 17, //17
		W_RIFLE = 1L << 18,    //18
		W_GATLING = 1L << 19,  //19
		W_SHOTGUN = 1L << 20,  //20
		W_GRENADE = 1L << 21,  //21
		W_HUUMA = 1L << 22,    //22
		W_2HSTAFF = 1L << 23,  //23
		MAX_WEAPON_TYPE = 1L << 24,
		// dual-wield constants
		W_DOUBLE_DD = 1L << 25, // 2 daggers
		W_DOUBLE_SS = 1L << 26, // 2 swords
		W_DOUBLE_AA = 1L << 27, // 2 axes
		W_DOUBLE_DS = 1L << 28, // dagger + sword
		W_DOUBLE_DA = 1L << 29, // dagger + axe
		W_DOUBLE_SA = 1L << 30, // sword + axe
		MAX_WEAPON_TYPE_ALL = 1L << 31,
		W_SHIELD = MAX_WEAPON_TYPE,

		SDE_ALL = W_FIST | W_DAGGER | W_1HSWORD | W_2HSWORD | W_1HSPEAR | W_2HSPEAR | W_1HAXE | W_2HAXE | W_MACE | W_2HMACE | W_STAFF | W_BOW | W_KNUCKLE | W_MUSICAL | W_WHIP | W_BOOK | W_KATAR | W_REVOLVER | W_RIFLE | W_GATLING | W_SHOTGUN | W_GRENADE | W_HUUMA | W_2HSTAFF | W_SHIELD,
	}

	public static class WeaponFlagInfo {
		public const string Marker = "W_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static WeaponFlagInfo() {
			All.Add(new EnumInfoBase(WeaponFlag.W_FIST, "Fist", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_DAGGER, "Dagger", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_1HSWORD, "1hSword", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_2HSWORD, "2hSword", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_1HSPEAR, "1hSpear", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_2HSPEAR, "2hSpear", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_1HAXE, "1hAxe", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_2HAXE, "2hAxe", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_MACE, "Mace", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_2HMACE, "2hMace", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_STAFF, "Staff", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_BOW, "Bow", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_KNUCKLE, "Knuckle", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_MUSICAL, "Musical", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_WHIP, "Whip", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_BOOK, "Book", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_KATAR, "Katar", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_REVOLVER, "Revolver", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_RIFLE, "Rifle", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_GATLING, "Gatling", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_SHOTGUN, "Shotgun", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_GRENADE, "Grenade", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_HUUMA, "Huuma", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_2HSTAFF, "2hStaff", Marker));
			All.Add(new EnumInfoBase(WeaponFlag.W_SHIELD, "Shield", Marker));

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<WeaponFlag>(All, TypeToInfo, Marker);
		}
	}
}
