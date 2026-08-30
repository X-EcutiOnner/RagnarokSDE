using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.Common {
	[Register(typeof(ElementTypeInfo))]
	public enum ElementType {
		ELE_NEUTRAL = 0,
		ELE_WATER,
		ELE_EARTH,
		ELE_FIRE,
		ELE_WIND,
		ELE_POISON,
		ELE_HOLY,
		ELE_DARK,
		ELE_GHOST,
		ELE_UNDEAD,
		ELE_ALL,
	}

	public static class ElementTypeInfo {
		public const string Marker = "ELE_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ElementTypeInfo() {
			All.Add(new EnumInfoBase(ElementType.ELE_NEUTRAL, "Neutral", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_WATER, "Water", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_EARTH, "Earth", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_FIRE, "Fire", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_WIND, "Wind", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_POISON, "Poison", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_HOLY, "Holy", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_DARK, "Dark", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_GHOST, "Ghost", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_UNDEAD, "Undead", Marker));
			All.Add(new EnumInfoBase(ElementType.ELE_ALL, "All", Marker) { Visible = false });

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ElementType>(All, TypeToInfo, Marker);
		}
	}
}
