using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.ClientAchievements.Common {
	[Register(typeof(ClientAchvUiTypeInfo))]
	public enum ClientAchvUiType {
		UITYPE_TEXT = 0,
		UITYPE_TEXT_AND_COUNTER,
	}

	public static class ClientAchvUiTypeInfo {
		public const string Marker = "UITYPE_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ClientAchvUiTypeInfo() {
			All.Add(new EnumInfoBase(ClientAchvUiType.UITYPE_TEXT, "Text", Marker));
			All.Add(new EnumInfoBase(ClientAchvUiType.UITYPE_TEXT_AND_COUNTER, "Text and counter", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ClientAchvUiType>(All, TypeToInfo, Marker);
		}
	}
}
