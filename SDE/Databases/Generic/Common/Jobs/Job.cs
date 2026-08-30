using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Extension;

namespace SDE.Databases.Generic.Common.Jobs {
	public sealed class Job {
		public static List<Job> AllJobs = new List<Job>();
		public static List<Job> PrimaryJobs = new List<Job>();
		public static List<Job> FirstJobs = new List<Job>();
		public static Dictionary<MAPIDs, Job> MapId2Job = new Dictionary<MAPIDs, Job>();
		public static Dictionary<int, List<Job>> JobUidToTree = new Dictionary<int, List<Job>>();

		//Novice And 1-1 Jobs
		public static Job Novice = new Job(-1, MAPIDs.MAPID_NOVICE, "Novice", null, "ÃÊº¸ÀÚ");
		public static Job Swordman = new Job(-1, MAPIDs.MAPID_SWORDMAN, "Swordman", null, "°Ë»ç", Novice);
		public static Job Mage = new Job(-1, MAPIDs.MAPID_MAGE, "Mage", null, "¸¶¹ý»ç", Novice);
		public static Job Archer = new Job(-1, MAPIDs.MAPID_ARCHER, "Archer", null, "±Ã¼ö", Novice);
		public static Job Acolyte = new Job(-1, MAPIDs.MAPID_ACOLYTE, "Acolyte", null, "¼ºÁ÷ÀÚ", Novice);
		public static Job Merchant = new Job(-1, MAPIDs.MAPID_MERCHANT, "Merchant", null, "»óÀÎ", Novice);
		public static Job Thief = new Job(-1, MAPIDs.MAPID_THIEF, "Thief", null, "µµµÏ", Novice);

		public static Job Taekwon = new Job(-1, MAPIDs.MAPID_TAEKWON, "Taekwon", null, "ÅÂ±Ç¼Ò³â", Novice);
		public static Job Gunslinger = new Job(-1, MAPIDs.MAPID_GUNSLINGER, "Gunslinger", null, "°Ç³Ê", Novice);
		public static Job Ninja = new Job(-1, MAPIDs.MAPID_NINJA, "Ninja", null, "´ÑÀÚ", Novice);
		public static Job Summoner = new Job(-1, MAPIDs.MAPID_SUMMONER, "Summoner (Doram)", null, "summoner");
		public static Job Druid = new Job(-1, MAPIDs.MAPID_DRUID, "Druid", null, "druid", Novice);

		//2-1 Jobs
		public static Job SuperNovice = new Job(-1, MAPIDs.MAPID_SUPER_NOVICE, "Super Novice", null, "½´ÆÛ³ëºñ½º", Novice);
		public static Job Knight = new Job(-1, MAPIDs.MAPID_KNIGHT, "Knight", null, "±â»ç", Swordman);
		public static Job Wizard = new Job(-1, MAPIDs.MAPID_WIZARD, "Wizard", null, "À§Àúµå", Mage);
		public static Job Hunter = new Job(-1, MAPIDs.MAPID_HUNTER, "Hunter", null, "ÇåÅÍ", Archer);
		public static Job Priest = new Job(-1, MAPIDs.MAPID_PRIEST, "Priest", null, "¼ºÅõ»ç", Acolyte);
		public static Job Blacksmith = new Job(-1, MAPIDs.MAPID_BLACKSMITH, "Blacksmith", null, "Á¦Ã¶°ø", Merchant);
		public static Job Assassin = new Job(-1, MAPIDs.MAPID_ASSASSIN, "Assassin", null, "¾î¼¼½Å", Thief);
		public static Job StarGladiator = new Job(-1, MAPIDs.MAPID_STAR_GLADIATOR, "Star Gladiator", null, "±Ç¼º", Taekwon);
		public static Job Rebellion = new Job(-1, MAPIDs.MAPID_REBELLION, "Rebellion", null, "rebellion", Gunslinger);
		public static Job KagerouOboro = new Job(-1, MAPIDs.MAPID_KAGEROUOBORO, "Kagerou", "Oboro", "kagerou", Ninja);
		public static Job SpiritHandler = new Job(-1, MAPIDs.MAPID_SPIRIT_HANDLER, "Spirit Handler", null, "spirit_handler", Summoner);
		public static Job Karnos = new Job(-1, MAPIDs.MAPID_KARNOS, "Karnos", null, "karnos", Druid);

		//2-2 Jobs
		public static Job Crusader = new Job(-1, MAPIDs.MAPID_CRUSADER, "Crusader", null, "Å©·ç¼¼ÀÌ´õ", Swordman);
		public static Job Sage = new Job(-1, MAPIDs.MAPID_SAGE, "Sage", null, "¼¼ÀÌÁö", Mage);
		public static Job BardDancer = new Job(-1, MAPIDs.MAPID_BARDDANCER, "Bard", "Dancer", "¹Ùµå:¹«Èñ", Archer);
		public static Job Monk = new Job(-1, MAPIDs.MAPID_MONK, "Monk", null, "¸ùÅ©", Acolyte);
		public static Job Alchemist = new Job(-1, MAPIDs.MAPID_ALCHEMIST, "Alchemist", null, "¿¬±Ý¼ú»ç", Merchant);
		public static Job Rogue = new Job(-1, MAPIDs.MAPID_ROGUE, "Rogue", null, "·Î±×", Thief);
		public static Job SoulLinker = new Job(-1, MAPIDs.MAPID_SOUL_LINKER, "Soul Linker", null, "¼Ò¿ï¸µÄ¿", Taekwon);

		//Trans Novice And Trans 1-1 Jobs
		public static Job HighNovice = new Job(MAPIDs.MAPID_NOVICE_HIGH, "High Novice", null, "ÃÊº¸ÀÚ", Novice);
		public static Job HighSwordman = new Job(MAPIDs.MAPID_SWORDMAN_HIGH, "High Swordman", null, "°Ë»ç", HighNovice);
		public static Job HighMage = new Job(MAPIDs.MAPID_MAGE_HIGH, "High Mage", null, "¸¶¹ý»ç", HighNovice);
		public static Job HighArcher = new Job(MAPIDs.MAPID_ARCHER_HIGH, "High Archer", null, "±Ã¼ö", HighNovice);
		public static Job HighAcolyte = new Job(MAPIDs.MAPID_ACOLYTE_HIGH, "High Acolyte", null, "¼ºÁ÷ÀÚ", HighNovice);
		public static Job HighMerchant = new Job(MAPIDs.MAPID_MERCHANT_HIGH, "High Merchant", null, "»óÀÎ", HighNovice);
		public static Job HighThief = new Job(MAPIDs.MAPID_THIEF_HIGH, "High Thief", null, "µµµÏ", HighNovice);

		//Trans 2-1 Jobs
		public static Job LordKnight = new Job(MAPIDs.MAPID_LORD_KNIGHT, "Lord Knight", null, "·Îµå³ªÀÌÆ®", HighSwordman);
		public static Job HighWizard = new Job(MAPIDs.MAPID_HIGH_WIZARD, "High Wizard", null, "ÇÏÀÌÀ§Àúµå", HighMage);
		public static Job Sniper = new Job(MAPIDs.MAPID_SNIPER, "Sniper", null, "½º³ªÀÌÆÛ", HighArcher);
		public static Job HighPriest = new Job(MAPIDs.MAPID_HIGH_PRIEST, "High Priest", null, "ÇÏÀÌÇÁ¸®", HighAcolyte);
		public static Job Whitesmith = new Job(MAPIDs.MAPID_WHITESMITH, "Whitesmith", null, "È­ÀÌÆ®½º¹Ì½º", HighMerchant);
		public static Job AssassinCross = new Job(MAPIDs.MAPID_ASSASSIN_CROSS, "Assassin Cross", null, "¾î½Ø½ÅÅ©·Î½º", HighThief);

		//Trans 2-2 Jobs
		public static Job Paladin = new Job(MAPIDs.MAPID_PALADIN, "Paladin", null, "ÆÈ¶óµò", HighSwordman);
		public static Job Professor = new Job(MAPIDs.MAPID_PROFESSOR, "Professor", null, "ÇÁ·ÎÆä¼­", HighMage);
		public static Job ClowyGypsy = new Job(MAPIDs.MAPID_CLOWNGYPSY, "Clown", "Gypsy", "Å¬¶ó¿î:Áý½Ã:Áý½Ã", HighArcher);
		public static Job Champion = new Job(MAPIDs.MAPID_CHAMPION, "Champion", null, "Ã¨ÇÇ¿Â", HighAcolyte);
		public static Job Creator = new Job(MAPIDs.MAPID_CREATOR, "Creator", null, "Å©¸®¿¡ÀÌÅÍ", HighMerchant);
		public static Job Stalker = new Job(MAPIDs.MAPID_STALKER, "Stalker", null, "½ºÅäÄ¿", HighThief);

		//Baby Novice And Baby 1-1 Jobs
		public static Job BabyNovice = new Job(MAPIDs.MAPID_BABY, "Baby Novice", null, "ÃÊº¸ÀÚ");
		public static Job BabySwordman = new Job(MAPIDs.MAPID_BABY_SWORDMAN, "Baby Swordman", null, "°Ë»ç", BabyNovice);
		public static Job BabyMage = new Job(MAPIDs.MAPID_BABY_MAGE, "Baby Mage", null, "¸¶¹ý»ç", BabyNovice);
		public static Job BabyArcher = new Job(MAPIDs.MAPID_BABY_ARCHER, "Baby Archer", null, "±Ã¼ö", BabyNovice);
		public static Job BabyAcolyte = new Job(MAPIDs.MAPID_BABY_ACOLYTE, "Baby Acolyte", null, "¼ºÁ÷ÀÚ", BabyNovice);
		public static Job BabyMerchant = new Job(MAPIDs.MAPID_BABY_MERCHANT, "Baby Merchant", null, "»óÀÎ", BabyNovice);
		public static Job BabyThief = new Job(MAPIDs.MAPID_BABY_THIEF, "Baby Thief", null, "µµµÏ", BabyNovice);
		public static Job BabyTaekwon = new Job(MAPIDs.MAPID_BABY_TAEKWON, "Baby Taekwon", null, "ÅÂ±Ç¼Ò³â", BabyNovice);
		public static Job BabyGunslinger = new Job(MAPIDs.MAPID_BABY_GUNSLINGER, "Baby Gunslinger", null, "°Ç³Ê", BabyNovice);
		public static Job BabyNinja = new Job(MAPIDs.MAPID_BABY_NINJA, "Baby Ninja", null, "´ÑÀÚ", BabyNovice);
		public static Job BabySummoner = new Job(MAPIDs.MAPID_BABY_SUMMONER, "Baby Summoner", null, "summoner");
		public static Job BabyDruid = new Job(MAPIDs.MAPID_BABY_DRUID, "Baby Druid", null, "druid", BabyNovice);

		//Baby 2-1 Jobs
		public static Job SuperBaby = new Job(MAPIDs.MAPID_SUPER_BABY, "Super Baby", null, "½´ÆÛ³ëºñ½º", BabyNovice);
		public static Job BabyKnight = new Job(MAPIDs.MAPID_BABY_KNIGHT, "Baby Knight", null, "±â»ç", BabySwordman);
		public static Job BabyWizard = new Job(MAPIDs.MAPID_BABY_WIZARD, "Baby Wizard", null, "À§Àúµå", BabyMage);
		public static Job BabyHunter = new Job(MAPIDs.MAPID_BABY_HUNTER, "Baby Hunter", null, "ÇåÅÍ", BabyArcher);
		public static Job BabyPriest = new Job(MAPIDs.MAPID_BABY_PRIEST, "Baby Priest", null, "¼ºÅõ»ç", BabyAcolyte);
		public static Job BabyBlacksmith = new Job(MAPIDs.MAPID_BABY_BLACKSMITH, "Baby Blacksmith", null, "Á¦Ã¶°ø", BabyMerchant);
		public static Job BabyAssassin = new Job(MAPIDs.MAPID_BABY_ASSASSIN, "Baby Assassin", null, "¾î¼¼½Å", BabyThief);
		public static Job BabyStarGladiator = new Job(MAPIDs.MAPID_BABY_STAR_GLADIATOR, "Baby Star Gladiator", null, "±Ç¼º", BabyTaekwon);
		public static Job BabyRebellion = new Job(MAPIDs.MAPID_BABY_REBELLION, "Baby Rebellion", null, "rebellion", BabyGunslinger);
		public static Job BabyKagerouOboro = new Job(MAPIDs.MAPID_BABY_KAGEROUOBORO, "Baby Kagerou", "Baby Oboro", "kagerou", BabyNinja);
		public static Job BabyKarnos = new Job(MAPIDs.MAPID_BABY_KARNOS, "Baby Karnos", null, "karnos", BabyDruid);

		//Baby 2-2 Jobs
		public static Job BabyCrusader = new Job(MAPIDs.MAPID_BABY_CRUSADER, "Baby Crusader", null, "Å©·ç¼¼ÀÌ´õ", BabySwordman);
		public static Job BabySage = new Job(MAPIDs.MAPID_BABY_SAGE, "Baby Sage", null, "¼¼ÀÌÁö", BabyMage);
		public static Job BabyBardDancer = new Job(MAPIDs.MAPID_BABY_BARDDANCER, "Baby Bard", "Baby Dancer", "¹Ùµå:¹«Èñ", BabyArcher);
		public static Job BabyMonk = new Job(MAPIDs.MAPID_BABY_MONK, "Baby Monk", null, "¸ùÅ©", BabyAcolyte);
		public static Job BabyAlchemist = new Job(MAPIDs.MAPID_BABY_ALCHEMIST, "Baby Alchemist", null, "¿¬±Ý¼ú»ç", BabyMerchant);
		public static Job BabyRogue = new Job(MAPIDs.MAPID_BABY_ROGUE, "Baby Rogue", null, "·Î±×", BabyThief);
		public static Job BabySoulLinker = new Job(MAPIDs.MAPID_BABY_SOUL_LINKER, "Baby Soul Linker", null, "¼Ò¿ï¸µÄ¿", BabyTaekwon);

		//3-1 Jobs
		public static Job SuperNoviceExtended = new Job(MAPIDs.MAPID_SUPER_NOVICE_E, "Super Novice", null, "½´ÆÛ³ëºñ½º", SuperNovice);
		public static Job RuneKnight = new Job(MAPIDs.MAPID_RUNE_KNIGHT, "Rune Knight", null, "·é³ªÀÌÆ®", LordKnight);
		public static Job Warlock = new Job(MAPIDs.MAPID_WARLOCK, "Warlock", null, "¿ö·Ï", HighWizard);
		public static Job Ranger = new Job(MAPIDs.MAPID_RANGER, "Ranger", null, "·¹ÀÎÁ®", Sniper);
		public static Job ArchBishop = new Job(MAPIDs.MAPID_ARCH_BISHOP, "Arch Bishop", null, "¾ÆÅ©ºñ¼ó", HighPriest);
		public static Job Mechanic = new Job(MAPIDs.MAPID_MECHANIC, "Mechanic", null, "¹ÌÄÉ´Ð", Whitesmith);
		public static Job GuillotineCross = new Job(MAPIDs.MAPID_GUILLOTINE_CROSS, "Guillotine Cross", null, "±æ·ÎÆ¾Å©·Î½º", AssassinCross);
		public static Job StarEmperor = new Job(MAPIDs.MAPID_STAR_EMPEROR, "Star Emperor", null, "±Ç¼º", StarGladiator);
		// Night Watch and Shinkiro/Shiranui/Hyper Novice are 4th classes though...?
		// rAthena messed up their class hierarchy I think, official servers treat them as 4th classes in item bonuses and restrictions.
		public static Job NightWatch = new Job(MAPIDs.MAPID_NIGHT_WATCH, "Night Watch", null, "night_watch", Rebellion);
		public static Job ShinkiroShiranui = new Job(MAPIDs.MAPID_SHINKIROSHIRANUI, "Shinkiro", "Shiranui", "shinkiro:shiranui", KagerouOboro);
		public static Job Alitea = new Job(MAPIDs.MAPID_ALITEA, "Alitea", null, "alitea", Karnos);

		//3-2 Jobs
		public static Job RoyalGuard = new Job(MAPIDs.MAPID_ROYAL_GUARD, "Royal Guard", null, "°¡µå", Paladin);
		public static Job Sorcerer = new Job(MAPIDs.MAPID_SORCERER, "Sorcerer", null, "¼Ò¼­·¯", Professor);
		public static Job MinstrelWanderer = new Job(MAPIDs.MAPID_MINSTRELWANDERER, "Minstrel", "Wanderer", "¹Î½ºÆ®·²:¿ø´õ·¯", ClowyGypsy);
		public static Job Shura = new Job(MAPIDs.MAPID_SURA, "Sura", null, "½´¶ó", Champion);
		public static Job Genetic = new Job(MAPIDs.MAPID_GENETIC, "Genetic", null, "Á¦³×¸¯", Creator);
		public static Job ShadowChaser = new Job(MAPIDs.MAPID_SHADOW_CHASER, "Shadow Chaser", null, "½¦µµ¿ìÃ¼ÀÌ¼­", Stalker);
		public static Job SoulReaper = new Job(MAPIDs.MAPID_SOUL_REAPER, "Soul Reaper", null, "¼Ò¿ï¸µÄ¿", SoulLinker);

		//Trans 3-1 Jobs
		public static Job TransRuneKnight = new Job(MAPIDs.MAPID_RUNE_KNIGHT_T, "Rune Knight", null, "·é³ªÀÌÆ®", LordKnight);
		public static Job TransWarlock = new Job(MAPIDs.MAPID_WARLOCK_T, "Warlock", null, "¿ö·Ï", HighWizard);
		public static Job TransRanger = new Job(MAPIDs.MAPID_RANGER_T, "Ranger", null, "·¹ÀÎÁ®", Sniper);
		public static Job TransArchBishop = new Job(MAPIDs.MAPID_ARCH_BISHOP_T, "Arch Bishop", null, "¾ÆÅ©ºñ¼ó", HighPriest);
		public static Job TransMechanic = new Job(MAPIDs.MAPID_MECHANIC_T, "Mechanic", null, "¹ÌÄÉ´Ð", Whitesmith);
		public static Job TransGuillotineCross = new Job(MAPIDs.MAPID_GUILLOTINE_CROSS_T, "Guillotine Cross", null, "±æ·ÎÆ¾Å©·Î½º", AssassinCross);

		//Trans 3-2 Jobs
		public static Job TransRoyalGuard = new Job(MAPIDs.MAPID_ROYAL_GUARD_T, "Royal Guard", null, "°¡µå", Paladin);
		public static Job TransSorcerer = new Job(MAPIDs.MAPID_SORCERER_T, "Sorcerer", null, "¼Ò¼­·¯", Professor);
		public static Job TransMinstrelWanderer = new Job(MAPIDs.MAPID_MINSTRELWANDERER_T, "Minstrel", "Wanderer", "¹Î½ºÆ®·²:¿ø´õ·¯", ClowyGypsy);
		public static Job TransShura = new Job(MAPIDs.MAPID_SURA_T, "Sura", null, "½´¶ó", Champion);
		public static Job TransGenetic = new Job(MAPIDs.MAPID_GENETIC_T, "Genetic", null, "Á¦³×¸¯", Creator);
		public static Job TransShadowChaser = new Job(MAPIDs.MAPID_SHADOW_CHASER_T, "Shadow Chaser", null, "½¦µµ¿ìÃ¼ÀÌ¼­", Stalker);

		//Baby 3-1 Jobs
		public static Job SuperBabyExtended = new Job(MAPIDs.MAPID_SUPER_BABY_E, "Super Baby", null, "½´ÆÛ³ëºñ½º", BabyNovice);
		public static Job BabyRuneKnight = new Job(MAPIDs.MAPID_BABY_RUNE_KNIGHT, "Baby Rune Knight", null, "·é³ªÀÌÆ®", LordKnight);
		public static Job BabyWarlock = new Job(MAPIDs.MAPID_BABY_WARLOCK, "Baby Warlock", null, "¿ö·Ï", HighWizard);
		public static Job BabyRanger = new Job(MAPIDs.MAPID_BABY_RANGER, "Baby Ranger", null, "·¹ÀÎÁ®", Sniper);
		public static Job BabyArchBishop = new Job(MAPIDs.MAPID_BABY_ARCH_BISHOP, "Baby Arch Bishop", null, "¾ÆÅ©ºñ¼ó", HighPriest);
		public static Job BabyMechanic = new Job(MAPIDs.MAPID_BABY_MECHANIC, "Baby Mechanic", null, "¹ÌÄÉ´Ð", Whitesmith);
		public static Job BabyGuillotineCross = new Job(MAPIDs.MAPID_BABY_GUILLOTINE_CROSS, "Baby Guillotine Cross", null, "±æ·ÎÆ¾Å©·Î½º", AssassinCross);

		//Baby 3-2 Jobs
		public static Job BabyRoyalGuard = new Job(MAPIDs.MAPID_BABY_ROYAL_GUARD, "Baby Royal Guard", null, "°¡µå", Paladin);
		public static Job BabySorcerer = new Job(MAPIDs.MAPID_BABY_SORCERER, "Baby Sorcerer", null, "¼Ò¼­·¯", Professor);
		public static Job BabyMinstrelWanderer = new Job(MAPIDs.MAPID_BABY_MINSTRELWANDERER, "Baby Minstrel", "Baby Wanderer", "¹Î½ºÆ®·²:¿ø´õ·¯", ClowyGypsy);
		public static Job BabyShura = new Job(MAPIDs.MAPID_BABY_SURA, "Baby Sura", null, "½´¶ó", Champion);
		public static Job BabyGenetic = new Job(MAPIDs.MAPID_BABY_GENETIC, "Baby Genetic", null, "Á¦³×¸¯", Creator);
		public static Job BabyShadowChaser = new Job(MAPIDs.MAPID_BABY_SHADOW_CHASER, "Baby Shadow Chaser", null, "½¦µµ¿ìÃ¼ÀÌ¼­", Stalker);
		public static Job BabySoulReaper = new Job(MAPIDs.MAPID_BABY_SOUL_REAPER, "Baby Soul Reaper", null, "¼Ò¿ï¸®ÆÛ", BabySoulLinker);

		//4-1 Jobs
		public static Job HyperNovice = new Job(MAPIDs.MAPID_HYPER_NOVICE, "Hyper Novice", null, "hyper_novice", SuperNovice);
		public static Job DragonKnight = new Job(MAPIDs.MAPID_DRAGON_KNIGHT, "Dragon Knight", null, "dragon_knight", RuneKnight);
		public static Job ArchMage = new Job(MAPIDs.MAPID_ARCH_MAGE, "Arch Mage", null, "arch_mage", Warlock);
		public static Job Windhawk = new Job(MAPIDs.MAPID_WINDHAWK, "Windhawk", null, "windhawk", Ranger);
		public static Job Cardinal = new Job(MAPIDs.MAPID_CARDINAL, "Cardinal", null, "cardinal", ArchBishop);
		public static Job Meister = new Job(MAPIDs.MAPID_MEISTER, "Meister", null, "meister", Mechanic);
		public static Job ShadowCross = new Job(MAPIDs.MAPID_SHADOW_CROSS, "Shadow Cross", null, "shadow_cross", GuillotineCross);
		public static Job SkyEmperor = new Job(MAPIDs.MAPID_SKY_EMPEROR, "Sky Emperor", null, "sky_emperor", StarEmperor);

		//4-2 Jobs
		public static Job ImperialGuard = new Job(MAPIDs.MAPID_IMPERIAL_GUARD, "Imperial Guard", null, "imperial_guard", RoyalGuard);
		public static Job ElementalMaster = new Job(MAPIDs.MAPID_ELEMENTAL_MASTER, "Elemental Master", null, "elemetal_master", Sorcerer);
		public static Job TroubadourTrouvere = new Job(MAPIDs.MAPID_TROUBADOURTROUVERE, "Troubadour", "Trouvere", "troubadour:trouvere", MinstrelWanderer);
		public static Job Inquisitor = new Job(MAPIDs.MAPID_INQUISITOR, "Inquisitor", null, "inquisitor", Shura);
		public static Job Biolo = new Job(MAPIDs.MAPID_BIOLO, "Biolo", null, "biolo", Genetic);
		public static Job AbyssChaser = new Job(MAPIDs.MAPID_ABYSS_CHASER, "Abyss Chaser", null, "abyss_chaser", ShadowChaser);
		public static Job SoulAscentic = new Job(MAPIDs.MAPID_SOUL_ASCETIC, "Soul Ascetic", null, "soul_ascetic", SoulReaper);

		public JOBs JobId;
		public MAPIDs MapId;
		private string _resource;
		public string[] Names = new string[2];
		public string[] Resources = new string[2];
		public string Name => Names[(int)GenderType.SEX_MALE];

		public bool IsBaby => (MapId & (MAPIDs)JOBLs.JOBL_BABY) != 0;
		public bool IsUpper => (MapId & (MAPIDs)JOBLs.JOBL_UPPER) != 0;
		public bool IsFourth => (MapId & (MAPIDs)JOBLs.JOBL_FOURTH) != 0;
		public bool IsThird => (MapId & (MAPIDs)JOBLs.JOBL_THIRD) != 0;
		public bool IsJob2 => (MapId & (MAPIDs)JOBLs.JOBL_2) != 0;
		public bool IsJob2_1 => (MapId & (MAPIDs)JOBLs.JOBL_2_1) != 0;
		public bool IsJob2_2 => (MapId & (MAPIDs)JOBLs.JOBL_2_2) != 0;
		public bool Normal => true;

		public Job BaseJob => MapId2Job[MapId & MAPIDs.MAPID_FIRSTMASK];
		public Job SecondJob => MapId2Job[MapId & ((MAPIDs)JOBLs.JOBL_2 | MAPIDs.MAPID_FIRSTMASK)];
		public Job NormalJob => MapId2Job[MapId & ~(MAPIDs)(JOBLs.JOBL_BABY | JOBLs.JOBL_UPPER)];
		public Job Parent;
		public UInt64 JobSdeUid = 0;
		public int Uid = 0;
		public static int MaxJobUid = 0;

		public Job(int uid, MAPIDs mapid, string male, string female, string jobResource, Job parent = null) {
			if (uid == -1)
				uid = MaxJobUid++;

			Uid = uid;
			JobSdeUid = 1UL << uid;
			PrimaryJobs.Add(this);

			if ((mapid & (MAPIDs)JOBLs.JOBL_2) == 0)
				FirstJobs.Add(this);

			_init(mapid, male, female, jobResource, parent);
		}

		public Job(MAPIDs mapid, string male, string female, string jobResource, Job parent = null) {
			_init(mapid, male, female, jobResource, parent);
		}

		private void _init(MAPIDs mapid, string male, string female, string jobResource, Job parent = null) {
			JobId = JobHelper.MapId2JobId(mapid, GenderType.SEX_MALE);
			Names[(int)GenderType.SEX_FEMALE] = female ?? male;
			Names[(int)GenderType.SEX_MALE] = male;

			var resources = jobResource.Split(':');
			Resources[(int)GenderType.SEX_FEMALE] = resources.Length == 2 ? resources[1] : resources[0];
			Resources[(int)GenderType.SEX_MALE] = resources[0];
			Parent = parent;
			_resource = jobResource;

			MapId = mapid;
			MapId2Job[MapId] = this;
			AllJobs.Add(this);

			if (JobSdeUid == 0) {
				if (!MapId2Job.TryGetValue(MapId & ((MAPIDs)JOBLs.JOBL_2 | MAPIDs.MAPID_FIRSTMASK), out var job)) {
					throw new Exception("Job has no ancestor and also has no unique ID. Did you use the wrong constructor?");
				}

				JobSdeUid = job.JobSdeUid;
				Uid = job.Uid;
			}

			if (!JobUidToTree.TryGetValue(Uid, out var l)) {
				l = new List<Job>();
				JobUidToTree[Uid] = l;
			}

			l.Add(this);
		}

		public bool CanUseItem(ItemJobFlag upper) {
			while (true) {
				if ((upper & ItemJobFlag.ITEMJ_NORMAL) != 0 && (MapId & (MAPIDs)(JOBLs.JOBL_UPPER | JOBLs.JOBL_BABY | JOBLs.JOBL_THIRD | JOBLs.JOBL_FOURTH)) == 0)
					break;
				//trans. classes (exl. third-trans.)
				if ((upper & ItemJobFlag.ITEMJ_UPPER) != 0 && (MapId & (MAPIDs)JOBLs.JOBL_UPPER) != 0 && (MapId & (MAPIDs)(JOBLs.JOBL_THIRD)) == 0)
					break;
				//baby classes (exl. third-baby)
				if ((upper & ItemJobFlag.ITEMJ_BABY) != 0 && (MapId & (MAPIDs)JOBLs.JOBL_BABY) != 0 && (MapId & (MAPIDs)(JOBLs.JOBL_THIRD)) == 0)
					break;
				//third classes (exl. third-trans. and baby-third and fourth)
				if ((upper & ItemJobFlag.ITEMJ_THIRD) != 0 && (MapId & (MAPIDs)JOBLs.JOBL_THIRD) != 0 && (MapId & (MAPIDs)(JOBLs.JOBL_UPPER | JOBLs.JOBL_BABY | JOBLs.JOBL_FOURTH)) == 0)
					break;
				//trans-third classes (exl. fourth)
				if ((upper & ItemJobFlag.ITEMJ_THIRD_UPPER) != 0 && (MapId & (MAPIDs)JOBLs.JOBL_THIRD) != 0 && (MapId & (MAPIDs)JOBLs.JOBL_UPPER) != 0 && (MapId & (MAPIDs)(JOBLs.JOBL_FOURTH)) == 0)
					break;
				//third-baby classes (exl. fourth)
				if ((upper & ItemJobFlag.ITEMJ_THIRD_BABY) != 0 && (MapId & (MAPIDs)JOBLs.JOBL_THIRD) != 0 && (MapId & (MAPIDs)JOBLs.JOBL_BABY) != 0 && (MapId & (MAPIDs)(JOBLs.JOBL_FOURTH)) == 0)
					break;
				//fourth classes
				if ((upper & ItemJobFlag.ITEMJ_FOURTH) != 0 && (MapId & (MAPIDs)JOBLs.JOBL_FOURTH) != 0)
					break;

				return false;
			}

			return true;
		}

		public bool Restrict(JOBLs jobl) {
			const MAPIDs mask = (MAPIDs)(JOBLs.JOBL_THIRD | JOBLs.JOBL_FOURTH | JOBLs.JOBL_UPPER | JOBLs.JOBL_BABY);

			var r = MapId & mask;
			return r == (MAPIDs)jobl;
		}

		public string GetName(GenderType gender) {
			switch (gender) {
				case GenderType.SEX_BOTH:
					return Names[(int)GenderType.SEX_MALE];
				default:
					return Names[(int)gender];
			}
		}

		public string GetResource(GenderType gender) {
			switch (gender) {
				case GenderType.SEX_BOTH:
					return Resources[(int)GenderType.SEX_MALE].ToDisplayEncoding();
				default:
					return Resources[(int)gender].ToDisplayEncoding();
			}
		}

		public static Job Get(Job inputJob, ItemJobFlag upper, int equipLevel = 0) {
			var job = inputJob;
			var mapid = job.MapId;

			if (equipLevel >= 200)
				upper = ItemJobFlag.ITEMJ_FOURTH;
			else if (equipLevel >= 100)
				upper = ItemJobFlag.ITEMJ_THIRD | (upper & ~(ItemJobFlag.ITEMJ_NORMAL | ItemJobFlag.ITEMJ_UPPER | ItemJobFlag.ITEMJ_BABY));

			var jobClass = GetJobDisplayMapId(upper);

			if (jobClass != 0 && MapId2Job.TryGetValue(inputJob.MapId | jobClass, out job))
				return job;

			// Search through available jobs
			var jobs = Job.JobUidToTree[inputJob.Uid].Where(p => p.CanUseItem(upper)).ToList();

			if (jobs.Count > 0) {
				return jobs[0];
			}

			return inputJob;
		}

		private static MAPIDs GetJobDisplayMapId(ItemJobFlag itemJobFlag) {
			if (itemJobFlag == ItemJobFlag.ITEMJ_ALL_BABY)
				return (MAPIDs)JOBLs.JOBL_BABY;
			if (itemJobFlag == ItemJobFlag.ITEMJ_THIRD_BABY)
				return (MAPIDs)(JOBLs.JOBL_BABY | JOBLs.JOBL_THIRD);
			if (itemJobFlag == ItemJobFlag.ITEMJ_BABY)
				return (MAPIDs)JOBLs.JOBL_BABY;
			if (itemJobFlag == ItemJobFlag.ITEMJ_ALL_THIRD)
				return (MAPIDs)JOBLs.JOBL_THIRD;
			if (itemJobFlag == (ItemJobFlag.ITEMJ_ALL_THIRD | ItemJobFlag.ITEMJ_FOURTH))
				return (MAPIDs)JOBLs.JOBL_THIRD;
			if (itemJobFlag == ItemJobFlag.ITEMJ_FOURTH)
				return (MAPIDs)(JOBLs.JOBL_THIRD | JOBLs.JOBL_FOURTH);
			if (itemJobFlag == ItemJobFlag.ITEMJ_THIRD_UPPER)
				return (MAPIDs)(JOBLs.JOBL_UPPER | JOBLs.JOBL_THIRD);
			if (itemJobFlag == (ItemJobFlag.ITEMJ_UPPER | ItemJobFlag.ITEMJ_ALL_THIRD))
				return (MAPIDs)JOBLs.JOBL_UPPER;
			if (itemJobFlag == ItemJobFlag.ITEMJ_UPPER)
				return (MAPIDs)JOBLs.JOBL_UPPER;
			if (itemJobFlag == ItemJobFlag.ITEMJ_ALL_UPPER)
				return (MAPIDs)JOBLs.JOBL_UPPER;
			if (itemJobFlag == ItemJobFlag.Trans)
				return (MAPIDs)JOBLs.JOBL_UPPER;

			return 0;
		}

		public override string ToString() {
			return Names[(int)GenderType.SEX_MALE];
		}

		public static ulong GetGroupId(params Job[] jobs) {
			UInt64 id = 0;

			foreach (var job in jobs)
				id |= job.JobSdeUid;

			return id;
		}

		public static Job TryGet(MAPIDs mapid) {
			MapId2Job.TryGetValue(mapid, out var job);
			return job;
		}
	}
}
