using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Register(typeof(BattleCheckTargetTypeInfo))]
	public enum BattleCheckTargetType {
		BCT_NOONE = 0x000000, ///< No one
		BCT_SELF = 0x010000, ///< Self
		BCT_ENEMY = 0x020000, ///< Enemy
		BCT_PARTY = 0x040000, ///< Party members
		BCT_GUILDALLY = 0x080000, ///< Only allies, NOT guildmates
		BCT_NEUTRAL = 0x100000, ///< Neutral target
		BCT_SAMEGUILD = 0x200000, ///< Guildmates, No Guild Allies

		BCT_ALL = 0x3F0000, ///< All targets

		BCT_WOS = 0x400000, ///< Except self and your master
		BCT_SLAVE = BCT_SELF | BCT_WOS,             ///< Does not hit yourself/master, but hits your/master's slaves
		BCT_GUILD = BCT_SAMEGUILD | BCT_GUILDALLY,  ///< Guild AND Allies (BCT_SAMEGUILD|BCT_GUILDALLY)
		BCT_NOGUILD = BCT_ALL & ~BCT_GUILD,         ///< Except guildmates
		BCT_NOPARTY = BCT_ALL & ~BCT_PARTY,         ///< Except party members
		BCT_NOENEMY = BCT_ALL & ~BCT_ENEMY,         ///< Except enemy
		BCT_ALLY = BCT_PARTY | BCT_GUILD,
		BCT_FRIEND = BCT_NOENEMY,
	}

	public static class BattleCheckTargetTypeInfo {
		public const string Marker = "BCT_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static BattleCheckTargetTypeInfo() {
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_NOONE, "None", Marker) { Visible = false });
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_SELF, "Self", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_ENEMY, "Enemy", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_PARTY, "Party", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_GUILDALLY, "GuildAlly", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_NEUTRAL, "Neutral", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_SAMEGUILD, "SameGuild", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_ALL, "All", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_WOS, "WoS", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_SLAVE, "Slave", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_GUILD, "Guild", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_NOGUILD, "NoGuild", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_NOPARTY, "NoParty", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_NOENEMY, "NoEnemy", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_ALLY, "Ally", Marker));
			All.Add(new EnumInfoBase(BattleCheckTargetType.BCT_FRIEND, "Friend", Marker));

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<BattleCheckTargetType>(All, TypeToInfo, Marker);
		}
	}
}
