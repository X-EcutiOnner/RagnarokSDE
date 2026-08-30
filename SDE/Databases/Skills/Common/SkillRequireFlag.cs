using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(SkillRequireFlagInfo))]
	public enum SkillRequireFlag : Int64 {
		SKILL_REQ_HPCOST = 0x1,
		SKILL_REQ_SPCOST = 0x2,
		SKILL_REQ_HPRATECOST = 0x4,
		SKILL_REQ_SPRATECOST = 0x8,
		SKILL_REQ_MAXHPTRIGGER = 0x10,
		SKILL_REQ_ZENYCOST = 0x20,
		SKILL_REQ_WEAPON = 0x40,
		SKILL_REQ_AMMO = 0x80,
		SKILL_REQ_STATE = 0x100,
		SKILL_REQ_STATUS = 0x200,
		SKILL_REQ_SPIRITSPHERECOST = 0x400,
		SKILL_REQ_ITEMCOST = 0x800,
		SKILL_REQ_EQUIPMENT = 0x1000,
		SKILL_REQ_APCOST = 0x2000,
		SKILL_REQ_APRATECOST = 0x4000,
	}

	public static class SkillRequireFlagInfo {
		public const string Marker = "SKILL_REQ_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static SkillRequireFlagInfo() {
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_HPCOST, "HpCost", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_SPCOST, "SpCost", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_HPRATECOST, "HpRateCost", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_SPRATECOST, "SpRateCost", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_MAXHPTRIGGER, "MaxHpTrigger", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_ZENYCOST, "ZenyCost", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_WEAPON, "Weapon", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_AMMO, "Ammo", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_STATE, "State", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_STATUS, "Status", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_SPIRITSPHERECOST, "SpiritSphereCost", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_ITEMCOST, "ItemCost", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_EQUIPMENT, "Equipment", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_APCOST, "ApCost", Marker));
			All.Add(new EnumInfoBase(SkillRequireFlag.SKILL_REQ_APRATECOST, "ApRateCost", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<SkillRequireFlag>(All, TypeToInfo, Marker);
		}
	}
}
