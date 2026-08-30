using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.MobSkills.Common {
	[Register(typeof(MobSkillCond2TypeInfo))]
	public enum MobSkillCond2Type {
		SC_ANYBAD = -1,
		SC_STONE = 1,
		SC_FREEZE,
		SC_STUN,
		SC_SLEEP,
		SC_POISON,
		SC_CURSE,
		SC_SILENCE,
		SC_CONFUSION,
		SC_BLIND,
		SC_HIDING,
		SC_SIGHT,
	}

	public static class MobSkillCond2TypeInfo {
		public const string Marker = "SC_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static MobSkillCond2TypeInfo() {
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_STONE, "Stone", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_FREEZE, "Freeze", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_STUN, "Stun", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_SLEEP, "Sleep", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_POISON, "Poison", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_CURSE, "Curse", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_SILENCE, "Silence", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_CONFUSION, "Confusion", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_BLIND, "Blind", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_HIDING, "Hiding", Marker));
			All.Add(new EnumInfoBase(MobSkillCond2Type.SC_SIGHT, "Sight", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<MobSkillCond2Type>(All, TypeToInfo, Marker);
		}
	}
}
