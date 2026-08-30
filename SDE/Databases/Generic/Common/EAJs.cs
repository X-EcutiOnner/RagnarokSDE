using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.Common {
	[Flags]
	[Register(typeof(EAJsInfo))]
	public enum EAJs : UInt64 {
		//EAJ_BASEMASK = MAPIDs.MAPID_FIRSTMASK,// Update name in future update for item/scripts. (Rytech)
		//EAJ_UPPERMASK = MAPIDs.MAPID_SECONDMASK,// Update name in future update for item/scripts. (Rytech)
		//EAJ_THIRDMASK = MAPIDs.MAPID_THIRDMASK,
		//EAJ_FOURTHMASK = MAPIDs.MAPID_FOURTHMASK,

		EAJ_NOVICE = MAPIDs.MAPID_NOVICE,
		EAJ_SWORDMAN = MAPIDs.MAPID_SWORDMAN,
		EAJ_MAGE = MAPIDs.MAPID_MAGE,
		EAJ_ARCHER = MAPIDs.MAPID_ARCHER,
		EAJ_ACOLYTE = MAPIDs.MAPID_ACOLYTE,
		EAJ_MERCHANT = MAPIDs.MAPID_MERCHANT,
		EAJ_THIEF = MAPIDs.MAPID_THIEF,
		EAJ_TAEKWON = MAPIDs.MAPID_TAEKWON,
		EAJ_GUNSLINGER = MAPIDs.MAPID_GUNSLINGER,
		EAJ_NINJA = MAPIDs.MAPID_NINJA,
		EAJ_SUMMONER = MAPIDs.MAPID_SUMMONER,
		EAJ_GANGSI = MAPIDs.MAPID_GANGSI,
		EAJ_DRUID = MAPIDs.MAPID_DRUID,

		EAJ_SUPER_NOVICE = MAPIDs.MAPID_SUPER_NOVICE,
		EAJ_SUPERNOVICE = MAPIDs.MAPID_SUPER_NOVICE,
		EAJ_KNIGHT = MAPIDs.MAPID_KNIGHT,
		EAJ_WIZARD = MAPIDs.MAPID_WIZARD,
		EAJ_HUNTER = MAPIDs.MAPID_HUNTER,
		EAJ_PRIEST = MAPIDs.MAPID_PRIEST,
		EAJ_BLACKSMITH = MAPIDs.MAPID_BLACKSMITH,
		EAJ_ASSASSIN = MAPIDs.MAPID_ASSASSIN,
		EAJ_STAR_GLADIATOR = MAPIDs.MAPID_STAR_GLADIATOR,
		EAJ_STARGLADIATOR = MAPIDs.MAPID_STAR_GLADIATOR,
		EAJ_REBELLION = MAPIDs.MAPID_REBELLION,
		EAJ_KAGEROUOBORO = MAPIDs.MAPID_KAGEROUOBORO,
		EAJ_SPIRIT_HANDLER = MAPIDs.MAPID_SPIRIT_HANDLER,
		EAJ_DEATH_KNIGHT = MAPIDs.MAPID_DEATH_KNIGHT,
		EAJ_DEATHKNIGHT = MAPIDs.MAPID_DEATH_KNIGHT,
		EAJ_KARNOS = MAPIDs.MAPID_KARNOS,

		EAJ_CRUSADER = MAPIDs.MAPID_CRUSADER,
		EAJ_SAGE = MAPIDs.MAPID_SAGE,
		EAJ_BARDDANCER = MAPIDs.MAPID_BARDDANCER,
		EAJ_MONK = MAPIDs.MAPID_MONK,
		EAJ_ALCHEMIST = MAPIDs.MAPID_ALCHEMIST,
		EAJ_ROGUE = MAPIDs.MAPID_ROGUE,
		EAJ_SOUL_LINKER = MAPIDs.MAPID_SOUL_LINKER,
		EAJ_SOULLINKER = MAPIDs.MAPID_SOUL_LINKER,
		EAJ_DARK_COLLECTOR = MAPIDs.MAPID_DARK_COLLECTOR,
		EAJ_DARKCOLLECTOR = MAPIDs.MAPID_DARK_COLLECTOR,

		EAJ_NOVICE_HIGH = MAPIDs.MAPID_NOVICE_HIGH,
		EAJ_SWORDMAN_HIGH = MAPIDs.MAPID_SWORDMAN_HIGH,
		EAJ_MAGE_HIGH = MAPIDs.MAPID_MAGE_HIGH,
		EAJ_ARCHER_HIGH = MAPIDs.MAPID_ARCHER_HIGH,
		EAJ_ACOLYTE_HIGH = MAPIDs.MAPID_ACOLYTE_HIGH,
		EAJ_MERCHANT_HIGH = MAPIDs.MAPID_MERCHANT_HIGH,
		EAJ_THIEF_HIGH = MAPIDs.MAPID_THIEF_HIGH,

		EAJ_LORD_KNIGHT = MAPIDs.MAPID_LORD_KNIGHT,
		EAJ_HIGH_WIZARD = MAPIDs.MAPID_HIGH_WIZARD,
		EAJ_SNIPER = MAPIDs.MAPID_SNIPER,
		EAJ_HIGH_PRIEST = MAPIDs.MAPID_HIGH_PRIEST,
		EAJ_WHITESMITH = MAPIDs.MAPID_WHITESMITH,
		EAJ_ASSASSIN_CROSS = MAPIDs.MAPID_ASSASSIN_CROSS,

		EAJ_PALADIN = MAPIDs.MAPID_PALADIN,
		EAJ_PROFESSOR = MAPIDs.MAPID_PROFESSOR,
		EAJ_CLOWNGYPSY = MAPIDs.MAPID_CLOWNGYPSY,
		EAJ_CHAMPION = MAPIDs.MAPID_CHAMPION,
		EAJ_CREATOR = MAPIDs.MAPID_CREATOR,
		EAJ_STALKER = MAPIDs.MAPID_STALKER,

		EAJ_BABY = MAPIDs.MAPID_BABY,
		EAJ_BABY_SWORDMAN = MAPIDs.MAPID_BABY_SWORDMAN,
		EAJ_BABY_MAGE = MAPIDs.MAPID_BABY_MAGE,
		EAJ_BABY_ARCHER = MAPIDs.MAPID_BABY_ARCHER,
		EAJ_BABY_ACOLYTE = MAPIDs.MAPID_BABY_ACOLYTE,
		EAJ_BABY_MERCHANT = MAPIDs.MAPID_BABY_MERCHANT,
		EAJ_BABY_THIEF = MAPIDs.MAPID_BABY_THIEF,
		EAJ_BABY_TAEKWON = MAPIDs.MAPID_BABY_TAEKWON,
		EAJ_BABY_GUNSLINGER = MAPIDs.MAPID_BABY_GUNSLINGER,
		EAJ_BABY_NINJA = MAPIDs.MAPID_BABY_NINJA,
		EAJ_BABY_SUMMONER = MAPIDs.MAPID_BABY_SUMMONER,
		EAJ_BABY_DRUID = MAPIDs.MAPID_BABY_DRUID,

		EAJ_SUPER_BABY = MAPIDs.MAPID_SUPER_BABY,
		EAJ_BABY_KNIGHT = MAPIDs.MAPID_BABY_KNIGHT,
		EAJ_BABY_WIZARD = MAPIDs.MAPID_BABY_WIZARD,
		EAJ_BABY_HUNTER = MAPIDs.MAPID_BABY_HUNTER,
		EAJ_BABY_PRIEST = MAPIDs.MAPID_BABY_PRIEST,
		EAJ_BABY_BLACKSMITH = MAPIDs.MAPID_BABY_BLACKSMITH,
		EAJ_BABY_ASSASSIN = MAPIDs.MAPID_BABY_ASSASSIN,
		EAJ_BABY_STAR_GLADIATOR = MAPIDs.MAPID_BABY_STAR_GLADIATOR,
		EAJ_BABY_REBELLION = MAPIDs.MAPID_BABY_REBELLION,
		EAJ_BABY_KAGEROUOBORO = MAPIDs.MAPID_BABY_KAGEROUOBORO,
		EAJ_BABY_KARNOS = MAPIDs.MAPID_BABY_KARNOS,

		EAJ_BABY_CRUSADER = MAPIDs.MAPID_BABY_CRUSADER,
		EAJ_BABY_SAGE = MAPIDs.MAPID_BABY_SAGE,
		EAJ_BABY_BARDDANCER = MAPIDs.MAPID_BABY_BARDDANCER,
		EAJ_BABY_MONK = MAPIDs.MAPID_BABY_MONK,
		EAJ_BABY_ALCHEMIST = MAPIDs.MAPID_BABY_ALCHEMIST,
		EAJ_BABY_ROGUE = MAPIDs.MAPID_BABY_ROGUE,
		EAJ_BABY_SOUL_LINKER = MAPIDs.MAPID_BABY_SOUL_LINKER,

		EAJ_SUPER_NOVICE_E = MAPIDs.MAPID_SUPER_NOVICE_E,
		EAJ_RUNE_KNIGHT = MAPIDs.MAPID_RUNE_KNIGHT,
		EAJ_WARLOCK = MAPIDs.MAPID_WARLOCK,
		EAJ_RANGER = MAPIDs.MAPID_RANGER,
		EAJ_ARCH_BISHOP = MAPIDs.MAPID_ARCH_BISHOP,
		EAJ_MECHANIC = MAPIDs.MAPID_MECHANIC,
		EAJ_GUILLOTINE_CROSS = MAPIDs.MAPID_GUILLOTINE_CROSS,
		EAJ_STAR_EMPEROR = MAPIDs.MAPID_STAR_EMPEROR,

		EAJ_ROYAL_GUARD = MAPIDs.MAPID_ROYAL_GUARD,
		EAJ_SORCERER = MAPIDs.MAPID_SORCERER,
		EAJ_MINSTRELWANDERER = MAPIDs.MAPID_MINSTRELWANDERER,
		EAJ_SURA = MAPIDs.MAPID_SURA,
		EAJ_GENETIC = MAPIDs.MAPID_GENETIC,
		EAJ_SHADOW_CHASER = MAPIDs.MAPID_SHADOW_CHASER,
		EAJ_SOUL_REAPER = MAPIDs.MAPID_SOUL_REAPER,

		EAJ_RUNE_KNIGHT_T = MAPIDs.MAPID_RUNE_KNIGHT_T,
		EAJ_WARLOCK_T = MAPIDs.MAPID_WARLOCK_T,
		EAJ_RANGER_T = MAPIDs.MAPID_RANGER_T,
		EAJ_ARCH_BISHOP_T = MAPIDs.MAPID_ARCH_BISHOP_T,
		EAJ_MECHANIC_T = MAPIDs.MAPID_MECHANIC_T,
		EAJ_GUILLOTINE_CROSS_T = MAPIDs.MAPID_GUILLOTINE_CROSS_T,

		EAJ_ROYAL_GUARD_T = MAPIDs.MAPID_ROYAL_GUARD_T,
		EAJ_SORCERER_T = MAPIDs.MAPID_SORCERER_T,
		EAJ_MINSTRELWANDERER_T = MAPIDs.MAPID_MINSTRELWANDERER_T,
		EAJ_SURA_T = MAPIDs.MAPID_SURA_T,
		EAJ_GENETIC_T = MAPIDs.MAPID_GENETIC_T,
		EAJ_SHADOW_CHASER_T = MAPIDs.MAPID_SHADOW_CHASER_T,

		EAJ_SUPER_BABY_E = MAPIDs.MAPID_SUPER_BABY_E,
		EAJ_BABY_RUNE_KNIGHT = MAPIDs.MAPID_BABY_RUNE_KNIGHT,
		EAJ_BABY_WARLOCK = MAPIDs.MAPID_BABY_WARLOCK,
		EAJ_BABY_RANGER = MAPIDs.MAPID_BABY_RANGER,
		EAJ_BABY_ARCH_BISHOP = MAPIDs.MAPID_BABY_ARCH_BISHOP,
		EAJ_BABY_MECHANIC = MAPIDs.MAPID_BABY_MECHANIC,
		EAJ_BABY_GUILLOTINE_CROSS = MAPIDs.MAPID_BABY_GUILLOTINE_CROSS,
		EAJ_BABY_STAR_EMPEROR = MAPIDs.MAPID_BABY_STAR_EMPEROR,

		EAJ_BABY_ROYAL_GUARD = MAPIDs.MAPID_BABY_ROYAL_GUARD,
		EAJ_BABY_SORCERER = MAPIDs.MAPID_BABY_SORCERER,
		EAJ_BABY_MINSTRELWANDERER = MAPIDs.MAPID_BABY_MINSTRELWANDERER,
		EAJ_BABY_SURA = MAPIDs.MAPID_BABY_SURA,
		EAJ_BABY_GENETIC = MAPIDs.MAPID_BABY_GENETIC,
		EAJ_BABY_SHADOW_CHASER = MAPIDs.MAPID_BABY_SHADOW_CHASER,
		EAJ_BABY_SOUL_REAPER = MAPIDs.MAPID_BABY_SOUL_REAPER,

		EAJ_HYPER_NOVICE = MAPIDs.MAPID_HYPER_NOVICE,
		EAJ_DRAGON_KNIGHT = MAPIDs.MAPID_DRAGON_KNIGHT,
		EAJ_ARCH_MAGE = MAPIDs.MAPID_ARCH_MAGE,
		EAJ_WINDHAWK = MAPIDs.MAPID_WINDHAWK,
		EAJ_CARDINAL = MAPIDs.MAPID_CARDINAL,
		EAJ_MEISTER = MAPIDs.MAPID_MEISTER,
		EAJ_SHADOW_CROSS = MAPIDs.MAPID_SHADOW_CROSS,
		EAJ_SKY_EMPEROR = MAPIDs.MAPID_SKY_EMPEROR,
		EAJ_NIGHT_WATCH = MAPIDs.MAPID_NIGHT_WATCH,
		EAJ_SHINKIROSHIRANUI = MAPIDs.MAPID_SHINKIROSHIRANUI,

		EAJ_IMPERIAL_GUARD = MAPIDs.MAPID_IMPERIAL_GUARD,
		EAJ_ELEMENTAL_MASTER = MAPIDs.MAPID_ELEMENTAL_MASTER,
		EAJ_TROUBADOURTROUVERE = MAPIDs.MAPID_TROUBADOURTROUVERE,
		EAJ_INQUISITOR = MAPIDs.MAPID_INQUISITOR,
		EAJ_BIOLO = MAPIDs.MAPID_BIOLO,
		EAJ_ABYSS_CHASER = MAPIDs.MAPID_ABYSS_CHASER,
		EAJ_SOUL_ASCETIC = MAPIDs.MAPID_SOUL_ASCETIC,
		EAJ_ALITEA = MAPIDs.MAPID_ALITEA,
	}

	public static class EAJsInfo {
		public const string Marker = "EAJ_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static EAJsInfo() {
			All.Add(new EnumInfoBase(EAJs.EAJ_NOVICE, "Novice", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SWORDMAN, "Swordman", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MAGE, "Mage", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ARCHER, "Archer", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ACOLYTE, "Acolyte", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MERCHANT, "Merchant", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_THIEF, "Thief", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_TAEKWON, "Taekwon", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_GUNSLINGER, "Gunslinger", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_NINJA, "Ninja", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SUMMONER, "Summoner", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_GANGSI, "Gangsi", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_DRUID, "Druid", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_SUPER_NOVICE, "Super_Novice", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SUPERNOVICE, "SuperNovice", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_KNIGHT, "Knight", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_WIZARD, "Wizard", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_HUNTER, "Hunter", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_PRIEST, "Priest", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BLACKSMITH, "Blacksmith", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ASSASSIN, "Assassin", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_STAR_GLADIATOR, "Star_Gladiator", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_STARGLADIATOR, "StarGladiator", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_REBELLION, "Rebellion", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_KAGEROUOBORO, "KagerouOboro", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SPIRIT_HANDLER, "Spirit_Handler", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_DEATH_KNIGHT, "Death_Knight", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_DEATHKNIGHT, "DeathKnight", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_KARNOS, "Karnos", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_CRUSADER, "Crusader", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SAGE, "Sage", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BARDDANCER, "BardDancer", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MONK, "Monk", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ALCHEMIST, "Alchemist", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ROGUE, "Rogue", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SOUL_LINKER, "Soul_Linker", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SOULLINKER, "SoulLinker", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_DARK_COLLECTOR, "Dark_Collector", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_DARKCOLLECTOR, "DarkCollector", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_NOVICE_HIGH, "Novice_High", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SWORDMAN_HIGH, "Swordman_High", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MAGE_HIGH, "Mage_High", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ARCHER_HIGH, "Archer_High", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ACOLYTE_HIGH, "Acolyte_High", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MERCHANT_HIGH, "Merchant_High", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_THIEF_HIGH, "Thief_High", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_LORD_KNIGHT, "Lord_Knight", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_HIGH_WIZARD, "High_Wizard", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SNIPER, "Sniper", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_HIGH_PRIEST, "High_Priest", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_WHITESMITH, "Whitesmith", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ASSASSIN_CROSS, "Assassin_Cross", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_PALADIN, "Paladin", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_PROFESSOR, "Professor", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_CLOWNGYPSY, "ClownGypsy", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_CHAMPION, "Champion", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_CREATOR, "Creator", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_STALKER, "Stalker", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_BABY, "Baby", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_SWORDMAN, "Baby_Wwordman", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_MAGE, "Baby_Mage", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_ARCHER, "Baby_Archer", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_ACOLYTE, "Baby_Acolyte", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_MERCHANT, "Baby_Merchant", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_THIEF, "Baby_Thief", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_TAEKWON, "Baby_Taekwon", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_GUNSLINGER, "Baby_Gunslinger", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_NINJA, "Baby_Ninja", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_SUMMONER, "Baby_Summoner", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_DRUID, "Baby_Druid", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_SUPER_BABY, "Super_Baby", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_KNIGHT, "Baby_Knight", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_WIZARD, "Baby_Wizard", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_HUNTER, "Baby_Hunter", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_PRIEST, "Baby_Priest", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_BLACKSMITH, "Baby_Blacksmith", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_ASSASSIN, "Baby_Assassin", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_STAR_GLADIATOR, "Baby_Star_Gladiator", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_REBELLION, "Baby_Rebellion", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_KAGEROUOBORO, "Baby_Kagerouoboro", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_KARNOS, "Baby_Karnos", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_CRUSADER, "Baby_Crusader", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_SAGE, "Baby_Sage", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_BARDDANCER, "Baby_BardDancer", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_MONK, "Baby_Monk", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_ALCHEMIST, "Baby_Alchemist", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_ROGUE, "Baby_Rogue", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_SOUL_LINKER, "Baby_Soul_Linker", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_SUPER_NOVICE_E, "Super_Novice_E", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_RUNE_KNIGHT, "Rune_Knight", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_WARLOCK, "Warlock", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_RANGER, "Ranger", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ARCH_BISHOP, "Arch_Bishop", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MECHANIC, "Mechanic", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_GUILLOTINE_CROSS, "Guillotine_Cross", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_STAR_EMPEROR, "Star_Emperor", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_ROYAL_GUARD, "Royal_Guard", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SORCERER, "Sorcerer", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MINSTRELWANDERER, "MinstrelWanderer", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SURA, "Sura", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_GENETIC, "Genetic", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SHADOW_CHASER, "Shadow_Chaser", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SOUL_REAPER, "Soul_Reaper", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_RUNE_KNIGHT_T, "Rune_Knight_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_WARLOCK_T, "Warlock_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_RANGER_T, "Ranger_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ARCH_BISHOP_T, "Arch_Bishop_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MECHANIC_T, "Mechanic_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_GUILLOTINE_CROSS_T, "Guillotine_Cross_T", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_ROYAL_GUARD_T, "Royal_Guard_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SORCERER_T, "Sorcerer_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MINSTRELWANDERER_T, "MinstrelWanderer_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SURA_T, "Sura_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_GENETIC_T, "Genetic_T", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SHADOW_CHASER_T, "Shadow_Chaser_T", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_SUPER_BABY_E, "Super_Baby_E", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_RUNE_KNIGHT, "Baby_Rune_Knight", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_WARLOCK, "Baby_Warlock", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_RANGER, "Baby_Ranger", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_ARCH_BISHOP, "Baby_Arch_Bishop", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_MECHANIC, "Baby_Mechanic", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_GUILLOTINE_CROSS, "Baby_Guillotine_Cross", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_STAR_EMPEROR, "Baby_Star_Emperor", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_ROYAL_GUARD, "Baby_Royal_Guard", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_SORCERER, "Baby_Sorcerer", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_MINSTRELWANDERER, "Baby_Minstrelwanderer", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_SURA, "Baby_Sura", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_GENETIC, "Baby_Genetic", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_SHADOW_CHASER, "Baby_Shadow_Chaser", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BABY_SOUL_REAPER, "Baby_Soul_Reaper", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_HYPER_NOVICE, "Hyper_Novice", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_DRAGON_KNIGHT, "Dragon_Knight", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ARCH_MAGE, "Arch_Mage", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_WINDHAWK, "Windhawk", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_CARDINAL, "Cardinal", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_MEISTER, "Meister", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SHADOW_CROSS, "Shadow_Sross", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SKY_EMPEROR, "Sky_Emperor", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_NIGHT_WATCH, "Night_Watch", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SHINKIROSHIRANUI, "ShinkiroShiranui", Marker));

			All.Add(new EnumInfoBase(EAJs.EAJ_IMPERIAL_GUARD, "Imperial_Guard", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ELEMENTAL_MASTER, "Elemental_Master", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_TROUBADOURTROUVERE, "TroubadourTrouvere", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_INQUISITOR, "Inquisitor", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_BIOLO, "Biolo", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ABYSS_CHASER, "Abyss_Chaser", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_SOUL_ASCETIC, "Soul_Ascetic", Marker));
			All.Add(new EnumInfoBase(EAJs.EAJ_ALITEA, "Alitea", Marker));

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.AddUInt64<EAJs>(All, TypeToInfo, Marker);
		}
	}
}