using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Items.Common {
	[Register(typeof(AmmoTypeInfo))]
	public enum AmmoType {
		AMMO_NONE = 0,
		AMMO_ARROW,
		AMMO_DAGGER,
		AMMO_BULLET,
		AMMO_SHELL,
		AMMO_GRENADE,
		AMMO_SHURIKEN,
		AMMO_KUNAI,
		AMMO_CANNONBALL,
		AMMO_THROWWEAPON,
		MAX_AMMO_TYPE
	}

	public static class AmmoTypeInfo {
		public const string Marker = "AMMO_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static AmmoTypeInfo() {
			All.Add(new EnumInfoBase(AmmoType.AMMO_NONE, "None", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_ARROW, "Arrow", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_DAGGER, "Dagger", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_BULLET, "Bullet", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_SHELL, "Shell", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_GRENADE, "Grenade", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_SHURIKEN, "Shuriken", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_KUNAI, "Kunai", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_CANNONBALL, "Cannon Ball", Marker));
			All.Add(new EnumInfoBase(AmmoType.AMMO_THROWWEAPON, "Throw Weapon", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<AmmoType>(All, TypeToInfo, Marker);
		}
	}
}
