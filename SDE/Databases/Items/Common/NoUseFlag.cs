using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Items.Common {
	[Flags]
	[Register(typeof(NoUseFlagInfo))]
	public enum NoUseFlag : Int64 {
		Sitting = 1 << 0,
	}

	public static class NoUseFlagInfo {
		public const string Marker = "";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static NoUseFlagInfo() {
			All.Add(new EnumInfoBase(NoUseFlag.Sitting, "Sitting", Marker, "Sitting") { ToolTip = "Cannot use the item while sitting." });

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<NoUseFlag>(All, TypeToInfo, Marker);
		}
	}
}