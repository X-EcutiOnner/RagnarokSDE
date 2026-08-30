using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Mobs.Common {
	[Flags]
	[Register(typeof(ModeFlagInfo))]
	public enum ModeFlag : Int64 {
		MD_NONE = 0x0000000,
		MD_CANMOVE = 0x0000001,
		MD_LOOTER = 0x0000002,
		MD_AGGRESSIVE = 0x0000004,
		MD_ASSIST = 0x0000008,
		MD_CASTSENSORIDLE = 0x0000010,
		MD_NORANDOMWALK = 0x0000020,
		MD_NOCAST = 0x0000040,
		MD_CANATTACK = 0x0000080,
		//FREE					= 0x0000100,
		MD_CASTSENSORCHASE = 0x0000200,
		MD_CHANGECHASE = 0x0000400,
		MD_ANGRY = 0x0000800,
		MD_CHANGETARGETMELEE = 0x0001000,
		MD_CHANGETARGETCHASE = 0x0002000,
		MD_TARGETWEAK = 0x0004000,
		MD_RANDOMTARGET = 0x0008000,
		MD_IGNOREMELEE = 0x0010000,
		MD_IGNOREMAGIC = 0x0020000,
		MD_IGNORERANGED = 0x0040000,
		MD_MVP = 0x0080000,
		MD_IGNOREMISC = 0x0100000,
		MD_KNOCKBACKIMMUNE = 0x0200000,
		MD_TELEPORTBLOCK = 0x0400000,
		//FREE					= 0x0800000,
		MD_FIXEDITEMDROP = 0x1000000,
		MD_DETECTOR = 0x2000000,
		MD_STATUSIMMUNE = 0x4000000,
		MD_SKILLIMMUNE = 0x8000000,
	}

	public static class ModeFlagInfo {
		public const string Marker = "MD_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static ModeFlagInfo() {
			All.Add(new EnumInfoBase(ModeFlag.MD_NONE, "None", Marker, "None") { Visible = false });
			All.Add(new EnumInfoBase(ModeFlag.MD_CANMOVE, "Can move", Marker, "CanMove") { ToolTip = "Enables the mob to move/chase characters." });
			All.Add(new EnumInfoBase(ModeFlag.MD_LOOTER, "Looter", Marker, "Looter") { ToolTip = "The mob will loot up nearby items on the ground when it's on idle state." });
			All.Add(new EnumInfoBase(ModeFlag.MD_AGGRESSIVE, "Aggressive", Marker, "Aggressive") { ToolTip = "Normal aggressive mob, will look for a close-by player to attack." });
			All.Add(new EnumInfoBase(ModeFlag.MD_ASSIST, "Assist", Marker, "Assist") { ToolTip = "When a nearby mob of the same class attacks, assist types will join them." });
			All.Add(new EnumInfoBase(ModeFlag.MD_CASTSENSORIDLE, "Cast sensor", Marker, "CastSensorIdle") { ToolTip = "Will go after characters who start casting on them if idle or walking (without a target)." });
			All.Add(new EnumInfoBase(ModeFlag.MD_NORANDOMWALK, "No random walk", Marker, "NoRandomWalk") { ToolTip = "The mob will not walk randomly." });
			All.Add(new EnumInfoBase(ModeFlag.MD_NOCAST, "No cast skill", Marker, "NoCast") { ToolTip = "The mob will not cast skills." });
			All.Add(new EnumInfoBase(ModeFlag.MD_CANATTACK, "Can attack", Marker, "CanAttack") { ToolTip = "Enables the mob to attack/retaliate when you are within attack range. Note that this only enables them to use normal attacks, skills are always allowed." });
			All.Add(new EnumInfoBase(ModeFlag.MD_CASTSENSORCHASE, "Cast sensor chase", Marker, "CastSensorChase") { ToolTip = "Will go after characters who start casting on them if idle or chasing other players (they switch chase targets)." });
			All.Add(new EnumInfoBase(ModeFlag.MD_CHANGECHASE, "Change chase", Marker, "ChangeChase") { ToolTip = "Allows chasing mobs to switch targets if another player happens to be within attack range (handy on ranged attackers, for example)." });
			All.Add(new EnumInfoBase(ModeFlag.MD_ANGRY, "Angry", Marker, "Angry") { ToolTip = "These mobs are 'hyper-active'. Apart from 'chase'/'attack', they have the states 'follow'/'angry'. Once hit, they stop using these states and use the normal ones. The new states are used to determine a different skill-set for their 'before attacked' and 'after attacked' states. Also, when 'following', they automatically switch to whoever character is closest." });
			All.Add(new EnumInfoBase(ModeFlag.MD_CHANGETARGETMELEE, "Change target melee", Marker, "ChangeTargetMelee") { ToolTip = "Enables a mob to switch targets when attacked while attacking someone else." });
			All.Add(new EnumInfoBase(ModeFlag.MD_CHANGETARGETCHASE, "Change target chase", Marker, "ChangeTargetChase") { ToolTip = "Enables a mob to switch targets when attacked while chasing another character." });
			All.Add(new EnumInfoBase(ModeFlag.MD_TARGETWEAK, "Target weak", Marker, "TargetWeak") { ToolTip = "Allows aggressive monsters to only be aggressive against  characters that are five levels below it's own level. For example, a monster of level 104 will not pick fights with a level 99." });
			All.Add(new EnumInfoBase(ModeFlag.MD_RANDOMTARGET, "Random target", Marker, "RandomTarget") { ToolTip = "Picks a new random target in range on each attack / skill." });
			All.Add(new EnumInfoBase(ModeFlag.MD_IGNOREMELEE, "Ignore melee", Marker, "IgnoreMelee") { ToolTip = "The mob will take 1 HP damage from physical attacks." });
			All.Add(new EnumInfoBase(ModeFlag.MD_IGNOREMAGIC, "Ignore magic", Marker, "IgnoreMagic") { ToolTip = "The mob will take 1 HP damage from magic attacks." });
			All.Add(new EnumInfoBase(ModeFlag.MD_IGNORERANGED, "Ignore ranged", Marker, "IgnoreRanged") { ToolTip = "The mob will take 1 HP damage from ranged attacks." });
			All.Add(new EnumInfoBase(ModeFlag.MD_MVP, "MVP", Marker, "Mvp") { ToolTip = "Flagged as MVP which makes mobs resistance to Coma." });
			All.Add(new EnumInfoBase(ModeFlag.MD_IGNOREMISC, "Ignore misc", Marker, "IgnoreMisc") { ToolTip = "The mob will take 1 HP damage from 'none' attack type." });
			All.Add(new EnumInfoBase(ModeFlag.MD_KNOCKBACKIMMUNE, "Knockback immune", Marker, "KnockBackImmune") { ToolTip = "The mob will be unable to be knocked back." });
			All.Add(new EnumInfoBase(ModeFlag.MD_TELEPORTBLOCK, "No teleport block", Marker, "TeleportBlock") { ToolTip = "Allows the monster to teleport on noteleport maps." });
			All.Add(new EnumInfoBase(ModeFlag.MD_FIXEDITEMDROP, "Fixed item drop", Marker , "FixedItemDrop") { ToolTip = "The mob's drops are not affected by item drop modifiers." });
			All.Add(new EnumInfoBase(ModeFlag.MD_DETECTOR, "Detector", Marker, "Detector") { ToolTip = "Enables mob to detect and attack characters who are in hiding/cloak." });
			All.Add(new EnumInfoBase(ModeFlag.MD_STATUSIMMUNE, "Status immune", Marker, "StatusImmune") { ToolTip = "Immune to being affected by statuses." });
			All.Add(new EnumInfoBase(ModeFlag.MD_SKILLIMMUNE, "Skill immune", Marker, "SkillImmune") { ToolTip = "Immune to being affected by skills." });

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<ModeFlag>(All, TypeToInfo, Marker);
		}
	}
}
