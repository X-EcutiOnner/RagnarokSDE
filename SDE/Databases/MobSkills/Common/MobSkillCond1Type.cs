using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.MobSkills.Common {
	[Register(typeof(MobSkillCond1TypeInfo))]
	public enum MobSkillCond1Type {
		MSC_NONE = -1,
		MSC_ALWAYS = 0x0000,
		MSC_MYHPLTMAXRATE,
		MSC_MYHPINRATE,
		MSC_FRIENDHPLTMAXRATE,
		MSC_FRIENDHPINRATE,
		MSC_MYSTATUSON,
		MSC_MYSTATUSOFF,
		MSC_FRIENDSTATUSON,
		MSC_FRIENDSTATUSOFF,
		MSC_ATTACKPCGT,
		MSC_ATTACKPCGE,
		MSC_SLAVELT,
		MSC_SLAVELE,
		MSC_CLOSEDATTACKED,
		MSC_LONGRANGEATTACKED,
		MSC_AFTERSKILL,
		MSC_SKILLUSED,
		MSC_CASTTARGETED,
		MSC_RUDEATTACKED,
		MSC_MASTERHPLTMAXRATE,
		MSC_MASTERATTACKED,
		MSC_ALCHEMIST,
		MSC_SPAWN,
		MSC_MOBNEARBYGT,
		MSC_GROUNDATTACKED,
		MSC_DAMAGEDGT,
		MSC_TRICKCASTING,

		MSC_ONSPAWN = MSC_SPAWN,
	}

	public static class MobSkillCond1TypeInfo {
		public const string Marker = "MSC_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static MobSkillCond1TypeInfo() {
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_ALWAYS, "Always", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_ONSPAWN, "On spawn", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_MYHPLTMAXRATE, "HP% < [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_MYHPINRATE, "[CValue] <= HP% <= [Val1]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_MYSTATUSON, "Has [CValue] status on", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_MYSTATUSOFF, "Has [CValue] status off", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_FRIENDHPLTMAXRATE, "Friend_HP% < [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_FRIENDHPINRATE, "[CValue] <= Friend_HP% <= [Val1]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_FRIENDSTATUSON, "Friend has [CValue] status on", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_FRIENDSTATUSOFF, "Friend has [CValue] status off", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_ATTACKPCGT, "Attack PCs > [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_ATTACKPCGE, "Attack PCs >= [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_SLAVELT, "Num of slaves < [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_SLAVELE, "Num of slaves <= [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_CLOSEDATTACKED, "Melee attacked (BF_SHORT)", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_LONGRANGEATTACKED, "Range attacked (excluding BF_MAGIC)", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_SKILLUSED, "Skill [CValue] used on mob", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_AFTERSKILL, "After skill [CValue] used by mob", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_CASTTARGETED, "Skill used on mob", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_RUDEATTACKED, "Rude attacked", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_MASTERHPLTMAXRATE, "Master_HP% < [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_MASTERATTACKED, "Master attacked", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_ALCHEMIST, "Is a summoned monster and HP% < 100", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_MOBNEARBYGT, "Monster nearby count >= [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_GROUNDATTACKED, "Ground skill hit monster", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_DAMAGEDGT, "Damaged for more than [CValue]", Marker));
			All.Add(new EnumInfoBase(MobSkillCond1Type.MSC_TRICKCASTING, "Has fake cast bar (trickcasting)", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<MobSkillCond1Type>(All, TypeToInfo, Marker);
		}
	}
}
