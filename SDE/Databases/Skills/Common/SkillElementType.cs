using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Register(typeof(SkillElementTypeInfo))]
	public enum SkillElementType {
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
		ELE_WEAPON,
		ELE_ENDOWED,
		ELE_RANDOM,
	}

	public static class SkillElementTypeInfo {
		public const string Marker = "ELE_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static SkillElementTypeInfo() {
			All.Add(new EnumInfoBase(SkillElementType.ELE_NEUTRAL, "Neutral", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_WATER, "Water", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_EARTH, "Earth", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_FIRE, "Fire", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_WIND, "Wind", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_POISON, "Poison", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_HOLY, "Holy", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_DARK, "Dark", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_GHOST, "Ghost", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_UNDEAD, "Undead", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_WEAPON, "Use weapon element", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_ENDOWED, "Use endowed element", Marker));
			All.Add(new EnumInfoBase(SkillElementType.ELE_RANDOM, "Use random element", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<SkillElementType>(All, TypeToInfo, Marker);
		}
	}
}
