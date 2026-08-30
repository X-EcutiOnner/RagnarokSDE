using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Mobs.Common {
	[Register(typeof(ClassTypeInfo))]
	public enum ClassType {
		CLASS_NONE = -1,
		CLASS_NORMAL = 0,
		CLASS_BOSS,
		CLASS_GUARDIAN,
		CLASS_BATTLEFIELD = 4,
		CLASS_EVENT,
		CLASS_ALL,
		CLASS_MAX,
	}

	public static class ClassTypeInfo {
		public const string Marker = "CLASS_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ClassTypeInfo() {
			All.Add(new EnumInfoBase(ClassType.CLASS_NONE, "None", Marker) { Visible = false });
			All.Add(new EnumInfoBase(ClassType.CLASS_NORMAL, "Normal", Marker));
			All.Add(new EnumInfoBase(ClassType.CLASS_BOSS, "Boss", Marker));
			All.Add(new EnumInfoBase(ClassType.CLASS_GUARDIAN, "Guardian", Marker));
			All.Add(new EnumInfoBase(ClassType.CLASS_BATTLEFIELD, "Battlefield", Marker));
			All.Add(new EnumInfoBase(ClassType.CLASS_EVENT, "Event", Marker));
			All.Add(new EnumInfoBase(ClassType.CLASS_ALL, "All", Marker) { Visible = false });
			All.Add(new EnumInfoBase(ClassType.CLASS_MAX, "Max", Marker) { Visible = false });

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ClassType>(All, TypeToInfo, Marker);
		}
	}
}
