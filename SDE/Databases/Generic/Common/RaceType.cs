using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.Common {
	[Register(typeof(RaceTypeInfo))]
	public enum RaceType {
		RC_NONE_ = -1, //don't give us bonus
		RC_FORMLESS = 0,
		RC_UNDEAD,
		RC_BRUTE,
		RC_PLANT,
		RC_INSECT,
		RC_FISH,
		RC_DEMON,
		RC_DEMIHUMAN,
		RC_ANGEL,
		RC_DRAGON,
		RC_PLAYER_HUMAN,
		RC_PLAYER_DORAM,
		RC_ALL,
		RC_MAX, //auto upd enum for array Race
	}

	public static class RaceTypeInfo {
		public const string Marker = "RC_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static RaceTypeInfo() {
			All.Add(new EnumInfoBase(RaceType.RC_FORMLESS, "Formless", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_UNDEAD, "Undead", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_BRUTE, "Brute", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_PLANT, "Plant", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_INSECT, "Insect", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_FISH, "Fish", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_DEMON, "Demon", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_DEMIHUMAN, "Demi Human", Marker, "Demihuman"));
			All.Add(new EnumInfoBase(RaceType.RC_ANGEL, "Angel", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_DRAGON, "Dragon", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_PLAYER_HUMAN, "Player Human", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_PLAYER_DORAM, "Player Doram", Marker));
			All.Add(new EnumInfoBase(RaceType.RC_ALL, "All", Marker) { Visible = false });

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<RaceType>(All, TypeToInfo, Marker);
		}
	}
}
