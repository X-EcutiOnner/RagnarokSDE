using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Mobs.Common {
	[Register(typeof(MonsterTypeInfo))]
	public enum MonsterType {
		MONSTER_TYPE_01 = 0x81,
		MONSTER_TYPE_02 = 0x83,
		MONSTER_TYPE_03 = 0x1089,
		MONSTER_TYPE_04 = 0x3885,
		MONSTER_TYPE_05 = 0x2085,
		MONSTER_TYPE_06 = 0,
		MONSTER_TYPE_07 = 0x108B,
		MONSTER_TYPE_08 = 0x7085,
		MONSTER_TYPE_09 = 0x3095,
		MONSTER_TYPE_10 = 0x84,
		MONSTER_TYPE_11 = 0x84,
		MONSTER_TYPE_12 = 0x2085,
		MONSTER_TYPE_13 = 0x308D,
		//MONSTER_TYPE_14
		//MONSTER_TYPE_15
		//MONSTER_TYPE_16
		MONSTER_TYPE_17 = 0x91,
		//MONSTER_TYPE_18
		MONSTER_TYPE_19 = 0x3095,
		MONSTER_TYPE_20 = 0x3295,
		MONSTER_TYPE_21 = 0x3695,
		//MONSTER_TYPE_22
		//MONSTER_TYPE_23
		MONSTER_TYPE_24 = 0xA1,
		MONSTER_TYPE_25 = 0x1,
		MONSTER_TYPE_26 = 0xB695,
		MONSTER_TYPE_27 = 0x8084,
		// Special AI
		MONSTER_TYPE_ABR_PASSIVE = 0x21,
		MONSTER_TYPE_ABR_OFFENSIVE = 0xA5,
	}

	public static class MonsterTypeInfo {
		public const string Marker = "MONSTER_TYPE_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();
		public static Dictionary<long, EnumInfoBase> ValueToInfo = new Dictionary<long, EnumInfoBase>();

		static MonsterTypeInfo() {
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_01, "01", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_02, "02", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_03, "03", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_04, "04", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_05, "05", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_06, "06", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_07, "07", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_08, "08", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_09, "09", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_10, "10", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_11, "11", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_12, "12", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_13, "13", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_17, "17", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_19, "19", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_20, "20", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_21, "21", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_24, "24", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_25, "25", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_26, "26", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_27, "27", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_ABR_PASSIVE, "Abr Passive", Marker));
			All.Add(new EnumInfoBase(MonsterType.MONSTER_TYPE_ABR_OFFENSIVE, "Abr Offensive", Marker));
			
			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
				ValueToInfo[info.ValueLong] = info;
			}

			EnumInfos.Add<MonsterType>(All, TypeToInfo, Marker);
		}
	}
}
