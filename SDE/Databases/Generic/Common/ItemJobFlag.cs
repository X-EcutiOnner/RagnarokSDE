using SDE.Databases.Generic.Common.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDE.Databases.Generic.Common {
	[Flags]
	[Register(typeof(ItemJobFlagInfo))]
	public enum ItemJobFlag : Int64 {
		ITEMJ_NONE = 0x00,
		ITEMJ_NORMAL = 0x01,
		ITEMJ_UPPER = 0x02,
		ITEMJ_BABY = 0x04,
		ITEMJ_THIRD = 0x08,
		ITEMJ_THIRD_UPPER = 0x10,
		ITEMJ_THIRD_BABY = 0x20,
		ITEMJ_FOURTH = 0x40,
		ITEMJ_MAX = 0xFF,

		ITEMJ_ALL_UPPER = ITEMJ_UPPER | ITEMJ_THIRD_UPPER | ITEMJ_FOURTH,
		ITEMJ_ALL_BABY = ITEMJ_BABY | ITEMJ_THIRD_BABY,
		ITEMJ_ALL_THIRD = ITEMJ_THIRD | ITEMJ_THIRD_UPPER | ITEMJ_THIRD_BABY,
		ITEMJ_ALL = ITEMJ_NORMAL | ITEMJ_UPPER | ITEMJ_BABY | ITEMJ_THIRD | ITEMJ_THIRD_UPPER | ITEMJ_THIRD_BABY | ITEMJ_FOURTH,
		ITEMJ_ALL_EXCEPT_FOURTH = ITEMJ_NORMAL | ITEMJ_UPPER | ITEMJ_BABY | ITEMJ_THIRD | ITEMJ_THIRD_UPPER | ITEMJ_THIRD_BABY,

		// Custom definitions
		AllBelowThird = ITEMJ_NORMAL | ITEMJ_UPPER | ITEMJ_BABY,
		Renewal = ITEMJ_THIRD | ITEMJ_THIRD_UPPER | ITEMJ_THIRD_BABY,
		PreRenewal = ITEMJ_NORMAL | ITEMJ_UPPER | ITEMJ_BABY,
		ThirdAbove = Renewal | ITEMJ_FOURTH,
		Trans = ITEMJ_UPPER | ITEMJ_THIRD_UPPER,
		TransAndThird = ITEMJ_ALL_THIRD | ITEMJ_UPPER,
	}

	public static class ItemJobFlagInfo {
		public const string Marker = "ITEMJ_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ItemJobFlagInfo() {
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_NONE, "None", Marker, null, false));
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_NORMAL, "Normal", Marker) { FlagDisplay = "1 - Normal" });
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_UPPER, "Upper", Marker) { FlagDisplay = "2 - Reborn/Trans. Classes (excl. Trans-3rd classes)" });
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_BABY, "Baby", Marker) { FlagDisplay = "4 - Baby Classes (excl. 3rd Baby Classes)" });
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_THIRD, "Third", Marker) { FlagDisplay = "8 - 3rd Classes (excl. Trans-3rd classes and 3rd Baby classes)" });
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_THIRD_UPPER, "Third Upper", Marker) { FlagDisplay = "16 - Trans-3rd Classes" });
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_THIRD_BABY, "Third Baby", Marker) { FlagDisplay = "32 - Baby 3rd Classes" });
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_FOURTH, "Fourth", Marker) { FlagDisplay = "64 - 4th Classes" });
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_MAX, "Max", Marker, null, false));
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_ALL_UPPER, "All Upper", Marker, null, false));
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_ALL_BABY, "All Baby", Marker, null, false));
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_ALL_THIRD, "All Third", Marker, null, false));
			All.Add(new EnumInfoBase(ItemJobFlag.ITEMJ_ALL, "All", Marker, null, false));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ItemJobFlag>(All, TypeToInfo, Marker);
		}

		public static bool ProcessFlagToName(ItemJobFlag flag, StringBuilder builder) {
			var r = JobOperations.GetStringFormat(JobGroups.EveryJob, flag, GenderType.SEX_BOTH, 0);

			if (r != "" && r != "Every Job" && r.StartsWith("Every")) {
				builder.Append(r.Replace("Every ", "").Replace(" or ", " and "));
				return true;
			}

			if (flag == ItemJobFlag.AllBelowThird) {
				builder.Append("3rd");
				return true;
			}

			if (flag == ItemJobFlag.ITEMJ_ALL) {
				builder.Append("All");
				return true;
			}

			if (flag == ItemJobFlag.ITEMJ_ALL_EXCEPT_FOURTH) {
				builder.Append("All except 4th");
				return true;
			}

			if ((flag & ItemJobFlag.ITEMJ_ALL_THIRD) == ItemJobFlag.ITEMJ_ALL_THIRD) {
				flag &= ~ItemJobFlag.ITEMJ_ALL_THIRD;
				builder.Append("3rd, ");
			}

			if ((flag & ItemJobFlag.ITEMJ_ALL_BABY) == ItemJobFlag.ITEMJ_ALL_BABY) {
				flag &= ~ItemJobFlag.ITEMJ_ALL_BABY;
				builder.Append("All Baby, ");
			}

			if ((flag & ItemJobFlag.ITEMJ_ALL_UPPER) == ItemJobFlag.ITEMJ_ALL_UPPER) {
				flag &= ~ItemJobFlag.ITEMJ_ALL_UPPER;
				builder.Append("Trans and 4th, ");
			}

			if ((flag & ItemJobFlag.ITEMJ_NORMAL) == ItemJobFlag.ITEMJ_NORMAL)
				builder.Append("Normal, ");
			if ((flag & ItemJobFlag.ITEMJ_UPPER) == ItemJobFlag.ITEMJ_UPPER)
				builder.Append("Trans 2nd, ");
			if ((flag & ItemJobFlag.ITEMJ_BABY) == ItemJobFlag.ITEMJ_BABY)
				builder.Append("Baby 2nd, ");
			if ((flag & ItemJobFlag.ITEMJ_THIRD) == ItemJobFlag.ITEMJ_THIRD)
				builder.Append("3rd, ");
			if ((flag & ItemJobFlag.ITEMJ_THIRD_UPPER) == ItemJobFlag.ITEMJ_THIRD_UPPER)
				builder.Append("Trans 3rd, ");
			if ((flag & ItemJobFlag.ITEMJ_THIRD_BABY) == ItemJobFlag.ITEMJ_THIRD_BABY)
				builder.Append("Baby 3rd, ");
			if ((flag & ItemJobFlag.ITEMJ_FOURTH) == ItemJobFlag.ITEMJ_FOURTH)
				builder.Append("4th, ");
			if (flag == 0)
				builder.Append("None");

			return true;
		}
	}
}
