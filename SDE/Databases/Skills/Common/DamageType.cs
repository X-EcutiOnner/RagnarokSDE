using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Register(typeof(DamageTypeInfo))]
	public enum DamageType {
		DMG_NORMAL = 0,         /// damage [ damage: total damage, div: amount of hits, damage2: assassin dual-wield damage ]
		DMG_PICKUP_ITEM,        /// pick up item
		DMG_SIT_DOWN,           /// sit down
		DMG_STAND_UP,           /// stand up
		DMG_ENDURE,             /// damage (endure)
		DMG_SPLASH,             /// (splash?)
		DMG_SINGLE,             /// (skill?)
		DMG_REPEAT,             /// (repeat damage?)
		DMG_MULTI_HIT,          /// multi-hit damage
		DMG_MULTI_HIT_ENDURE,   /// multi-hit damage (endure)
		DMG_CRITICAL,           /// critical hit
		DMG_LUCY_DODGE,         /// lucky dodge
		DMG_TOUCH,              /// (touch skill?)
		DMG_MULTI_HIT_CRITICAL, /// multi-hit with critical
		DMG_SPLASH_ENDURE,      /// splash against target with endure status
	}

	public static class DamageTypeInfo {
		public const string Marker = "DMG_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static DamageTypeInfo() {
			All.Add(new EnumInfoBase(DamageType.DMG_NORMAL, "Normal", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_PICKUP_ITEM, "Pickup item", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_SIT_DOWN, "Sit down", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_STAND_UP, "Stand up", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_ENDURE, "Endure", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_SPLASH, "Splah", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_SINGLE, "Single", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_REPEAT, "Repeat", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_MULTI_HIT, "Multi-hit", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_MULTI_HIT_ENDURE, "Multi-hit endure", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_CRITICAL, "Critical", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_LUCY_DODGE, "Lucky Dodge", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_TOUCH, "Touch", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_MULTI_HIT_CRITICAL, "Multi-hit critical", Marker));
			All.Add(new EnumInfoBase(DamageType.DMG_SPLASH_ENDURE, "Splash endure", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<DamageType>(All, TypeToInfo, Marker);
		}
	}
}
