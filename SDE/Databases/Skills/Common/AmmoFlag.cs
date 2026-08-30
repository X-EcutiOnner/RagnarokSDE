using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(AmmoFlagInfo))]
	public enum AmmoFlag : Int64 {
		AMMO_NONE = 1L << 0,
		AMMO_ARROW = 1L << 1,
		AMMO_DAGGER = 1L << 2,
		AMMO_BULLET = 1L << 3,
		AMMO_SHELL = 1L << 4,
		AMMO_GRENADE = 1L << 5,
		AMMO_SHURIKEN = 1L << 6,
		AMMO_KUNAI = 1L << 7,
		AMMO_CANNONBALL = 1L << 8,
		AMMO_THROWWEAPON = 1L << 9,
		MAX_AMMO_TYPE = 1L << 10,
	}

	public static class AmmoFlagInfo {
		public const string Marker = "AMMO_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static AmmoFlagInfo() {
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_NONE, "None", Marker) { Visible = false });
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_ARROW, "Arrow", Marker));
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_DAGGER, "Dagger", Marker));
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_BULLET, "Bullet", Marker));
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_SHELL, "Shell", Marker));
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_GRENADE, "Grenade", Marker));
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_SHURIKEN, "Shuriken", Marker));
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_KUNAI, "Kunai", Marker));
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_CANNONBALL, "Cannon Ball", Marker));
			All.Add(new EnumInfoBase(AmmoFlag.AMMO_THROWWEAPON, "Throw Weapon", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<AmmoFlag>(All, TypeToInfo, Marker);
		}
	}
}
