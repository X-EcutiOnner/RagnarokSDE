namespace SDE.Databases.Generic.Common.Jobs {
	public static class JobHelper {
		public static MAPIDs JobId2MapId(JOBs job) {
			switch (job) {
			//Novice And 1-1 Jobs
				case JOBs.JOB_NOVICE:                return MAPIDs.MAPID_NOVICE;
				case JOBs.JOB_SWORDMAN:              return MAPIDs.MAPID_SWORDMAN;
				case JOBs.JOB_MAGE:                  return MAPIDs.MAPID_MAGE;
				case JOBs.JOB_ARCHER:                return MAPIDs.MAPID_ARCHER;
				case JOBs.JOB_ACOLYTE:               return MAPIDs.MAPID_ACOLYTE;
				case JOBs.JOB_MERCHANT:              return MAPIDs.MAPID_MERCHANT;
				case JOBs.JOB_THIEF:                 return MAPIDs.MAPID_THIEF;
				case JOBs.JOB_TAEKWON:               return MAPIDs.MAPID_TAEKWON;
				case JOBs.JOB_GUNSLINGER:            return MAPIDs.MAPID_GUNSLINGER;
				case JOBs.JOB_NINJA:                 return MAPIDs.MAPID_NINJA;
				case JOBs.JOB_SUMMONER:              return MAPIDs.MAPID_SUMMONER;
				case JOBs.JOB_GANGSI:                return MAPIDs.MAPID_GANGSI;
				case JOBs.JOB_WEDDING:               return MAPIDs.MAPID_WEDDING;
				case JOBs.JOB_XMAS:                  return MAPIDs.MAPID_XMAS;
				case JOBs.JOB_SUMMER:                return MAPIDs.MAPID_SUMMER;
				case JOBs.JOB_HANBOK:                return MAPIDs.MAPID_HANBOK;
				case JOBs.JOB_OKTOBERFEST:           return MAPIDs.MAPID_OKTOBERFEST;
				case JOBs.JOB_SUMMER2:               return MAPIDs.MAPID_SUMMER2;
				case JOBs.JOB_DRUID:                 return MAPIDs.MAPID_DRUID;
			//2-1 Jobs
				case JOBs.JOB_SUPER_NOVICE:          return MAPIDs.MAPID_SUPER_NOVICE;
				case JOBs.JOB_KNIGHT:                return MAPIDs.MAPID_KNIGHT;
				case JOBs.JOB_WIZARD:                return MAPIDs.MAPID_WIZARD;
				case JOBs.JOB_HUNTER:                return MAPIDs.MAPID_HUNTER;
				case JOBs.JOB_PRIEST:                return MAPIDs.MAPID_PRIEST;
				case JOBs.JOB_BLACKSMITH:            return MAPIDs.MAPID_BLACKSMITH;
				case JOBs.JOB_ASSASSIN:              return MAPIDs.MAPID_ASSASSIN;
				case JOBs.JOB_STAR_GLADIATOR:        return MAPIDs.MAPID_STAR_GLADIATOR;
				case JOBs.JOB_REBELLION:             return MAPIDs.MAPID_REBELLION;
				case JOBs.JOB_KAGEROU:
				case JOBs.JOB_OBORO:                 return MAPIDs.MAPID_KAGEROUOBORO;
				case JOBs.JOB_SPIRIT_HANDLER:        return MAPIDs.MAPID_SPIRIT_HANDLER;
				case JOBs.JOB_DEATH_KNIGHT:          return MAPIDs.MAPID_DEATH_KNIGHT;
				case JOBs.JOB_KARNOS:                return MAPIDs.MAPID_KARNOS;
			//2-2 Jobs
				case JOBs.JOB_CRUSADER:              return MAPIDs.MAPID_CRUSADER;
				case JOBs.JOB_SAGE:                  return MAPIDs.MAPID_SAGE;
				case JOBs.JOB_BARD:
				case JOBs.JOB_DANCER:                return MAPIDs.MAPID_BARDDANCER;
				case JOBs.JOB_MONK:                  return MAPIDs.MAPID_MONK;
				case JOBs.JOB_ALCHEMIST:             return MAPIDs.MAPID_ALCHEMIST;
				case JOBs.JOB_ROGUE:                 return MAPIDs.MAPID_ROGUE;
				case JOBs.JOB_SOUL_LINKER:           return MAPIDs.MAPID_SOUL_LINKER;
				case JOBs.JOB_DARK_COLLECTOR:        return MAPIDs.MAPID_DARK_COLLECTOR;
			//Trans Novice And Trans 1-1 Jobs
				case JOBs.JOB_NOVICE_HIGH:           return MAPIDs.MAPID_NOVICE_HIGH;
				case JOBs.JOB_SWORDMAN_HIGH:         return MAPIDs.MAPID_SWORDMAN_HIGH;
				case JOBs.JOB_MAGE_HIGH:             return MAPIDs.MAPID_MAGE_HIGH;
				case JOBs.JOB_ARCHER_HIGH:           return MAPIDs.MAPID_ARCHER_HIGH;
				case JOBs.JOB_ACOLYTE_HIGH:          return MAPIDs.MAPID_ACOLYTE_HIGH;
				case JOBs.JOB_MERCHANT_HIGH:         return MAPIDs.MAPID_MERCHANT_HIGH;
				case JOBs.JOB_THIEF_HIGH:            return MAPIDs.MAPID_THIEF_HIGH;
			//Trans 2-1 Jobs
				case JOBs.JOB_LORD_KNIGHT:           return MAPIDs.MAPID_LORD_KNIGHT;
				case JOBs.JOB_HIGH_WIZARD:           return MAPIDs.MAPID_HIGH_WIZARD;
				case JOBs.JOB_SNIPER:                return MAPIDs.MAPID_SNIPER;
				case JOBs.JOB_HIGH_PRIEST:           return MAPIDs.MAPID_HIGH_PRIEST;
				case JOBs.JOB_WHITESMITH:            return MAPIDs.MAPID_WHITESMITH;
				case JOBs.JOB_ASSASSIN_CROSS:        return MAPIDs.MAPID_ASSASSIN_CROSS;
			//Trans 2-2 Jobs
				case JOBs.JOB_PALADIN:               return MAPIDs.MAPID_PALADIN;
				case JOBs.JOB_PROFESSOR:             return MAPIDs.MAPID_PROFESSOR;
				case JOBs.JOB_CLOWN:
				case JOBs.JOB_GYPSY:                 return MAPIDs.MAPID_CLOWNGYPSY;
				case JOBs.JOB_CHAMPION:              return MAPIDs.MAPID_CHAMPION;
				case JOBs.JOB_CREATOR:               return MAPIDs.MAPID_CREATOR;
				case JOBs.JOB_STALKER:               return MAPIDs.MAPID_STALKER;
			//Baby Novice And Baby 1-1 Jobs
				case JOBs.JOB_BABY:                  return MAPIDs.MAPID_BABY;
				case JOBs.JOB_BABY_SWORDMAN:         return MAPIDs.MAPID_BABY_SWORDMAN;
				case JOBs.JOB_BABY_MAGE:             return MAPIDs.MAPID_BABY_MAGE;
				case JOBs.JOB_BABY_ARCHER:           return MAPIDs.MAPID_BABY_ARCHER;
				case JOBs.JOB_BABY_ACOLYTE:          return MAPIDs.MAPID_BABY_ACOLYTE;
				case JOBs.JOB_BABY_MERCHANT:         return MAPIDs.MAPID_BABY_MERCHANT;
				case JOBs.JOB_BABY_THIEF:            return MAPIDs.MAPID_BABY_THIEF;
				case JOBs.JOB_BABY_TAEKWON:          return MAPIDs.MAPID_BABY_TAEKWON;
				case JOBs.JOB_BABY_GUNSLINGER:       return MAPIDs.MAPID_BABY_GUNSLINGER;
				case JOBs.JOB_BABY_NINJA:            return MAPIDs.MAPID_BABY_NINJA;
				case JOBs.JOB_BABY_SUMMONER:         return MAPIDs.MAPID_BABY_SUMMONER;
				case JOBs.JOB_BABY_DRUID:            return MAPIDs.MAPID_BABY_DRUID;
			//Baby 2-1 Jobs
				case JOBs.JOB_SUPER_BABY:            return MAPIDs.MAPID_SUPER_BABY;
				case JOBs.JOB_BABY_KNIGHT:           return MAPIDs.MAPID_BABY_KNIGHT;
				case JOBs.JOB_BABY_WIZARD:           return MAPIDs.MAPID_BABY_WIZARD;
				case JOBs.JOB_BABY_HUNTER:           return MAPIDs.MAPID_BABY_HUNTER;
				case JOBs.JOB_BABY_PRIEST:           return MAPIDs.MAPID_BABY_PRIEST;
				case JOBs.JOB_BABY_BLACKSMITH:       return MAPIDs.MAPID_BABY_BLACKSMITH;
				case JOBs.JOB_BABY_ASSASSIN:         return MAPIDs.MAPID_BABY_ASSASSIN;
				case JOBs.JOB_BABY_STAR_GLADIATOR:   return MAPIDs.MAPID_BABY_STAR_GLADIATOR;
				case JOBs.JOB_BABY_REBELLION:        return MAPIDs.MAPID_BABY_REBELLION;
				case JOBs.JOB_BABY_KAGEROU:
				case JOBs.JOB_BABY_OBORO:            return MAPIDs.MAPID_BABY_KAGEROUOBORO;
				case JOBs.JOB_BABY_KARNOS:           return MAPIDs.MAPID_BABY_KARNOS;
			//Baby 2-2 Jobs
				case JOBs.JOB_BABY_CRUSADER:         return MAPIDs.MAPID_BABY_CRUSADER;
				case JOBs.JOB_BABY_SAGE:             return MAPIDs.MAPID_BABY_SAGE;
				case JOBs.JOB_BABY_BARD:
				case JOBs.JOB_BABY_DANCER:           return MAPIDs.MAPID_BABY_BARDDANCER;
				case JOBs.JOB_BABY_MONK:             return MAPIDs.MAPID_BABY_MONK;
				case JOBs.JOB_BABY_ALCHEMIST:        return MAPIDs.MAPID_BABY_ALCHEMIST;
				case JOBs.JOB_BABY_ROGUE:            return MAPIDs.MAPID_BABY_ROGUE;
				case JOBs.JOB_BABY_SOUL_LINKER:      return MAPIDs.MAPID_BABY_SOUL_LINKER;
			//3-1 Jobs
				case JOBs.JOB_SUPER_NOVICE_E:        return MAPIDs.MAPID_SUPER_NOVICE_E;
				case JOBs.JOB_RUNE_KNIGHT:           return MAPIDs.MAPID_RUNE_KNIGHT;
				case JOBs.JOB_WARLOCK:               return MAPIDs.MAPID_WARLOCK;
				case JOBs.JOB_RANGER:                return MAPIDs.MAPID_RANGER;
				case JOBs.JOB_ARCH_BISHOP:           return MAPIDs.MAPID_ARCH_BISHOP;
				case JOBs.JOB_MECHANIC:              return MAPIDs.MAPID_MECHANIC;
				case JOBs.JOB_GUILLOTINE_CROSS:      return MAPIDs.MAPID_GUILLOTINE_CROSS;
				case JOBs.JOB_STAR_EMPEROR:          return MAPIDs.MAPID_STAR_EMPEROR;
				case JOBs.JOB_NIGHT_WATCH:           return MAPIDs.MAPID_NIGHT_WATCH;
				case JOBs.JOB_SHINKIRO:
				case JOBs.JOB_SHIRANUI:              return MAPIDs.MAPID_SHINKIROSHIRANUI;
				case JOBs.JOB_ALITEA:                return MAPIDs.MAPID_ALITEA;
			//3-2 Jobs
				case JOBs.JOB_ROYAL_GUARD:           return MAPIDs.MAPID_ROYAL_GUARD;
				case JOBs.JOB_SORCERER:              return MAPIDs.MAPID_SORCERER;
				case JOBs.JOB_MINSTREL:
				case JOBs.JOB_WANDERER:              return MAPIDs.MAPID_MINSTRELWANDERER;
				case JOBs.JOB_SURA:                  return MAPIDs.MAPID_SURA;
				case JOBs.JOB_GENETIC:               return MAPIDs.MAPID_GENETIC;
				case JOBs.JOB_SHADOW_CHASER:         return MAPIDs.MAPID_SHADOW_CHASER;
				case JOBs.JOB_SOUL_REAPER:           return MAPIDs.MAPID_SOUL_REAPER;
			//Trans 3-1 Jobs
				case JOBs.JOB_RUNE_KNIGHT_T:         return MAPIDs.MAPID_RUNE_KNIGHT_T;
				case JOBs.JOB_WARLOCK_T:             return MAPIDs.MAPID_WARLOCK_T;
				case JOBs.JOB_RANGER_T:              return MAPIDs.MAPID_RANGER_T;
				case JOBs.JOB_ARCH_BISHOP_T:         return MAPIDs.MAPID_ARCH_BISHOP_T;
				case JOBs.JOB_MECHANIC_T:            return MAPIDs.MAPID_MECHANIC_T;
				case JOBs.JOB_GUILLOTINE_CROSS_T:    return MAPIDs.MAPID_GUILLOTINE_CROSS_T;
			//Trans 3-2 Jobs
				case JOBs.JOB_ROYAL_GUARD_T:         return MAPIDs.MAPID_ROYAL_GUARD_T;
				case JOBs.JOB_SORCERER_T:            return MAPIDs.MAPID_SORCERER_T;
				case JOBs.JOB_MINSTREL_T:
				case JOBs.JOB_WANDERER_T:            return MAPIDs.MAPID_MINSTRELWANDERER_T;
				case JOBs.JOB_SURA_T:                return MAPIDs.MAPID_SURA_T;
				case JOBs.JOB_GENETIC_T:             return MAPIDs.MAPID_GENETIC_T;
				case JOBs.JOB_SHADOW_CHASER_T:       return MAPIDs.MAPID_SHADOW_CHASER_T;
			//Baby 3-1 Jobs
				case JOBs.JOB_SUPER_BABY_E:          return MAPIDs.MAPID_SUPER_BABY_E;
				case JOBs.JOB_BABY_RUNE_KNIGHT:      return MAPIDs.MAPID_BABY_RUNE_KNIGHT;
				case JOBs.JOB_BABY_WARLOCK:          return MAPIDs.MAPID_BABY_WARLOCK;
				case JOBs.JOB_BABY_RANGER:           return MAPIDs.MAPID_BABY_RANGER;
				case JOBs.JOB_BABY_ARCH_BISHOP:      return MAPIDs.MAPID_BABY_ARCH_BISHOP;
				case JOBs.JOB_BABY_MECHANIC:         return MAPIDs.MAPID_BABY_MECHANIC;
				case JOBs.JOB_BABY_GUILLOTINE_CROSS: return MAPIDs.MAPID_BABY_GUILLOTINE_CROSS;
				case JOBs.JOB_BABY_STAR_EMPEROR:     return MAPIDs.MAPID_BABY_STAR_EMPEROR;
			//Baby 3-2 Jobs
				case JOBs.JOB_BABY_ROYAL_GUARD:      return MAPIDs.MAPID_BABY_ROYAL_GUARD;
				case JOBs.JOB_BABY_SORCERER:         return MAPIDs.MAPID_BABY_SORCERER;
				case JOBs.JOB_BABY_MINSTREL:
				case JOBs.JOB_BABY_WANDERER:         return MAPIDs.MAPID_BABY_MINSTRELWANDERER;
				case JOBs.JOB_BABY_SURA:             return MAPIDs.MAPID_BABY_SURA;
				case JOBs.JOB_BABY_GENETIC:          return MAPIDs.MAPID_BABY_GENETIC;
				case JOBs.JOB_BABY_SHADOW_CHASER:    return MAPIDs.MAPID_BABY_SHADOW_CHASER;
				case JOBs.JOB_BABY_SOUL_REAPER:      return MAPIDs.MAPID_BABY_SOUL_REAPER;
			//4-1 Jobs
				case JOBs.JOB_HYPER_NOVICE:          return MAPIDs.MAPID_HYPER_NOVICE;
				case JOBs.JOB_DRAGON_KNIGHT:         return MAPIDs.MAPID_DRAGON_KNIGHT;
				case JOBs.JOB_ARCH_MAGE:             return MAPIDs.MAPID_ARCH_MAGE;
				case JOBs.JOB_WINDHAWK:              return MAPIDs.MAPID_WINDHAWK;
				case JOBs.JOB_CARDINAL:              return MAPIDs.MAPID_CARDINAL;
				case JOBs.JOB_MEISTER:               return MAPIDs.MAPID_MEISTER;
				case JOBs.JOB_SHADOW_CROSS:          return MAPIDs.MAPID_SHADOW_CROSS;
				case JOBs.JOB_SKY_EMPEROR:           return MAPIDs.MAPID_SKY_EMPEROR;
			//4-2 Jobs
				case JOBs.JOB_IMPERIAL_GUARD:        return MAPIDs.MAPID_IMPERIAL_GUARD;
				case JOBs.JOB_ELEMENTAL_MASTER:      return MAPIDs.MAPID_ELEMENTAL_MASTER;
				case JOBs.JOB_TROUBADOUR:
				case JOBs.JOB_TROUVERE:              return MAPIDs.MAPID_TROUBADOURTROUVERE;
				case JOBs.JOB_INQUISITOR:            return MAPIDs.MAPID_INQUISITOR;
				case JOBs.JOB_BIOLO:                 return MAPIDs.MAPID_BIOLO;
				case JOBs.JOB_ABYSS_CHASER:          return MAPIDs.MAPID_ABYSS_CHASER;
				case JOBs.JOB_SOUL_ASCETIC:          return MAPIDs.MAPID_SOUL_ASCETIC;
			//None
				default:
					unchecked {
						return (MAPIDs)(-1);
					}
			}
		}

		public static JOBs MapId2JobId(MAPIDs mapid, GenderType gender) {
			switch(mapid) {
			//Novice And 1-1 Jobs
				case MAPIDs.MAPID_NOVICE:                return JOBs.JOB_NOVICE;
				case MAPIDs.MAPID_SWORDMAN:              return JOBs.JOB_SWORDMAN;
				case MAPIDs.MAPID_MAGE:                  return JOBs.JOB_MAGE;
				case MAPIDs.MAPID_ARCHER:                return JOBs.JOB_ARCHER;
				case MAPIDs.MAPID_ACOLYTE:               return JOBs.JOB_ACOLYTE;
				case MAPIDs.MAPID_MERCHANT:              return JOBs.JOB_MERCHANT;
				case MAPIDs.MAPID_THIEF:                 return JOBs.JOB_THIEF;
				case MAPIDs.MAPID_TAEKWON:               return JOBs.JOB_TAEKWON;
				case MAPIDs.MAPID_GUNSLINGER:            return JOBs.JOB_GUNSLINGER;
				case MAPIDs.MAPID_NINJA:                 return JOBs.JOB_NINJA;
				case MAPIDs.MAPID_SUMMONER:              return JOBs.JOB_SUMMONER;
				case MAPIDs.MAPID_GANGSI:                return JOBs.JOB_GANGSI;
				case MAPIDs.MAPID_WEDDING:               return JOBs.JOB_WEDDING;
				case MAPIDs.MAPID_XMAS:                  return JOBs.JOB_XMAS;
				case MAPIDs.MAPID_SUMMER:                return JOBs.JOB_SUMMER;
				case MAPIDs.MAPID_HANBOK:                return JOBs.JOB_HANBOK;
				case MAPIDs.MAPID_OKTOBERFEST:           return JOBs.JOB_OKTOBERFEST;
				case MAPIDs.MAPID_SUMMER2:               return JOBs.JOB_SUMMER2;
				case MAPIDs.MAPID_DRUID:                 return JOBs.JOB_DRUID;
			//2-1 Jobs
				case MAPIDs.MAPID_SUPER_NOVICE:          return JOBs.JOB_SUPER_NOVICE;
				case MAPIDs.MAPID_KNIGHT:                return JOBs.JOB_KNIGHT;
				case MAPIDs.MAPID_WIZARD:                return JOBs.JOB_WIZARD;
				case MAPIDs.MAPID_HUNTER:                return JOBs.JOB_HUNTER;
				case MAPIDs.MAPID_PRIEST:                return JOBs.JOB_PRIEST;
				case MAPIDs.MAPID_BLACKSMITH:            return JOBs.JOB_BLACKSMITH;
				case MAPIDs.MAPID_ASSASSIN:              return JOBs.JOB_ASSASSIN;
				case MAPIDs.MAPID_STAR_GLADIATOR:        return JOBs.JOB_STAR_GLADIATOR;
				case MAPIDs.MAPID_REBELLION:             return JOBs.JOB_REBELLION;
				case MAPIDs.MAPID_KAGEROUOBORO:          return gender == GenderType.SEX_MALE ? JOBs.JOB_KAGEROU : JOBs.JOB_OBORO;
				case MAPIDs.MAPID_SPIRIT_HANDLER:        return JOBs.JOB_SPIRIT_HANDLER;
				case MAPIDs.MAPID_DEATH_KNIGHT:          return JOBs.JOB_DEATH_KNIGHT;
				case MAPIDs.MAPID_KARNOS:                return JOBs.JOB_KARNOS;
			//2-2 Jobs
				case MAPIDs.MAPID_CRUSADER:              return JOBs.JOB_CRUSADER;
				case MAPIDs.MAPID_SAGE:                  return JOBs.JOB_SAGE;
				case MAPIDs.MAPID_BARDDANCER:            return gender == GenderType.SEX_MALE ? JOBs.JOB_BARD : JOBs.JOB_DANCER;
				case MAPIDs.MAPID_MONK:                  return JOBs.JOB_MONK;
				case MAPIDs.MAPID_ALCHEMIST:             return JOBs.JOB_ALCHEMIST;
				case MAPIDs.MAPID_ROGUE:                 return JOBs.JOB_ROGUE;
				case MAPIDs.MAPID_SOUL_LINKER:           return JOBs.JOB_SOUL_LINKER;
				case MAPIDs.MAPID_DARK_COLLECTOR:        return JOBs.JOB_DARK_COLLECTOR;
			//Trans Novice And Trans 2-1 Jobs
				case MAPIDs.MAPID_NOVICE_HIGH:           return JOBs.JOB_NOVICE_HIGH;
				case MAPIDs.MAPID_SWORDMAN_HIGH:         return JOBs.JOB_SWORDMAN_HIGH;
				case MAPIDs.MAPID_MAGE_HIGH:             return JOBs.JOB_MAGE_HIGH;
				case MAPIDs.MAPID_ARCHER_HIGH:           return JOBs.JOB_ARCHER_HIGH;
				case MAPIDs.MAPID_ACOLYTE_HIGH:          return JOBs.JOB_ACOLYTE_HIGH;
				case MAPIDs.MAPID_MERCHANT_HIGH:         return JOBs.JOB_MERCHANT_HIGH;
				case MAPIDs.MAPID_THIEF_HIGH:            return JOBs.JOB_THIEF_HIGH;
			//Trans 2-1 Jobs
				case MAPIDs.MAPID_LORD_KNIGHT:           return JOBs.JOB_LORD_KNIGHT;
				case MAPIDs.MAPID_HIGH_WIZARD:           return JOBs.JOB_HIGH_WIZARD;
				case MAPIDs.MAPID_SNIPER:                return JOBs.JOB_SNIPER;
				case MAPIDs.MAPID_HIGH_PRIEST:           return JOBs.JOB_HIGH_PRIEST;
				case MAPIDs.MAPID_WHITESMITH:            return JOBs.JOB_WHITESMITH;
				case MAPIDs.MAPID_ASSASSIN_CROSS:        return JOBs.JOB_ASSASSIN_CROSS;
			//Trans 2-2 Jobs
				case MAPIDs.MAPID_PALADIN:               return JOBs.JOB_PALADIN;
				case MAPIDs.MAPID_PROFESSOR:             return JOBs.JOB_PROFESSOR;
				case MAPIDs.MAPID_CLOWNGYPSY:            return gender == GenderType.SEX_MALE ? JOBs.JOB_CLOWN : JOBs.JOB_GYPSY;
				case MAPIDs.MAPID_CHAMPION:              return JOBs.JOB_CHAMPION;
				case MAPIDs.MAPID_CREATOR:               return JOBs.JOB_CREATOR;
				case MAPIDs.MAPID_STALKER:               return JOBs.JOB_STALKER;
			//Baby Novice And Baby 1-1 Jobs
				case MAPIDs.MAPID_BABY:                  return JOBs.JOB_BABY;
				case MAPIDs.MAPID_BABY_SWORDMAN:         return JOBs.JOB_BABY_SWORDMAN;
				case MAPIDs.MAPID_BABY_MAGE:             return JOBs.JOB_BABY_MAGE;
				case MAPIDs.MAPID_BABY_ARCHER:           return JOBs.JOB_BABY_ARCHER;
				case MAPIDs.MAPID_BABY_ACOLYTE:          return JOBs.JOB_BABY_ACOLYTE;
				case MAPIDs.MAPID_BABY_MERCHANT:         return JOBs.JOB_BABY_MERCHANT;
				case MAPIDs.MAPID_BABY_THIEF:            return JOBs.JOB_BABY_THIEF;
				case MAPIDs.MAPID_BABY_TAEKWON:          return JOBs.JOB_BABY_TAEKWON;
				case MAPIDs.MAPID_BABY_GUNSLINGER:       return JOBs.JOB_BABY_GUNSLINGER;
				case MAPIDs.MAPID_BABY_NINJA:            return JOBs.JOB_BABY_NINJA;
				case MAPIDs.MAPID_BABY_SUMMONER:         return JOBs.JOB_BABY_SUMMONER;
				case MAPIDs.MAPID_BABY_DRUID:            return JOBs.JOB_BABY_DRUID;
			//Baby 2-1 Jobs
				case MAPIDs.MAPID_SUPER_BABY:            return JOBs.JOB_SUPER_BABY;
				case MAPIDs.MAPID_BABY_KNIGHT:           return JOBs.JOB_BABY_KNIGHT;
				case MAPIDs.MAPID_BABY_WIZARD:           return JOBs.JOB_BABY_WIZARD;
				case MAPIDs.MAPID_BABY_HUNTER:           return JOBs.JOB_BABY_HUNTER;
				case MAPIDs.MAPID_BABY_PRIEST:           return JOBs.JOB_BABY_PRIEST;
				case MAPIDs.MAPID_BABY_BLACKSMITH:       return JOBs.JOB_BABY_BLACKSMITH;
				case MAPIDs.MAPID_BABY_ASSASSIN:         return JOBs.JOB_BABY_ASSASSIN;
				case MAPIDs.MAPID_BABY_STAR_GLADIATOR:   return JOBs.JOB_BABY_STAR_GLADIATOR;
				case MAPIDs.MAPID_BABY_REBELLION:        return JOBs.JOB_BABY_REBELLION;
				case MAPIDs.MAPID_BABY_KAGEROUOBORO:     return gender == GenderType.SEX_MALE ? JOBs.JOB_BABY_KAGEROU : JOBs.JOB_BABY_OBORO;
				case MAPIDs.MAPID_BABY_KARNOS:           return JOBs.JOB_BABY_KARNOS;
			//Baby 2-2 Jobs
				case MAPIDs.MAPID_BABY_CRUSADER:         return JOBs.JOB_BABY_CRUSADER;
				case MAPIDs.MAPID_BABY_SAGE:             return JOBs.JOB_BABY_SAGE;
				case MAPIDs.MAPID_BABY_BARDDANCER:       return gender == GenderType.SEX_MALE ? JOBs.JOB_BABY_BARD : JOBs.JOB_BABY_DANCER;
				case MAPIDs.MAPID_BABY_MONK:             return JOBs.JOB_BABY_MONK;
				case MAPIDs.MAPID_BABY_ALCHEMIST:        return JOBs.JOB_BABY_ALCHEMIST;
				case MAPIDs.MAPID_BABY_ROGUE:            return JOBs.JOB_BABY_ROGUE;
				case MAPIDs.MAPID_BABY_SOUL_LINKER:      return JOBs.JOB_BABY_SOUL_LINKER;
			//3-1 Jobs
				case MAPIDs.MAPID_SUPER_NOVICE_E:        return JOBs.JOB_SUPER_NOVICE_E;
				case MAPIDs.MAPID_RUNE_KNIGHT:           return JOBs.JOB_RUNE_KNIGHT;
				case MAPIDs.MAPID_WARLOCK:               return JOBs.JOB_WARLOCK;
				case MAPIDs.MAPID_RANGER:                return JOBs.JOB_RANGER;
				case MAPIDs.MAPID_ARCH_BISHOP:           return JOBs.JOB_ARCH_BISHOP;
				case MAPIDs.MAPID_MECHANIC:              return JOBs.JOB_MECHANIC;
				case MAPIDs.MAPID_GUILLOTINE_CROSS:      return JOBs.JOB_GUILLOTINE_CROSS;
				case MAPIDs.MAPID_STAR_EMPEROR:          return JOBs.JOB_STAR_EMPEROR;
				case MAPIDs.MAPID_NIGHT_WATCH:           return JOBs.JOB_NIGHT_WATCH;
				case MAPIDs.MAPID_SHINKIROSHIRANUI:      return gender == GenderType.SEX_MALE ? JOBs.JOB_SHINKIRO : JOBs.JOB_SHIRANUI;
				case MAPIDs.MAPID_ALITEA:                return JOBs.JOB_ALITEA;
			//3-2 Jobs
				case MAPIDs.MAPID_ROYAL_GUARD:           return JOBs.JOB_ROYAL_GUARD;
				case MAPIDs.MAPID_SORCERER:              return JOBs.JOB_SORCERER;
				case MAPIDs.MAPID_MINSTRELWANDERER:      return gender == GenderType.SEX_MALE ? JOBs.JOB_MINSTREL : JOBs.JOB_WANDERER;
				case MAPIDs.MAPID_SURA:                  return JOBs.JOB_SURA;
				case MAPIDs.MAPID_GENETIC:               return JOBs.JOB_GENETIC;
				case MAPIDs.MAPID_SHADOW_CHASER:         return JOBs.JOB_SHADOW_CHASER;
				case MAPIDs.MAPID_SOUL_REAPER:           return JOBs.JOB_SOUL_REAPER;
			//Trans 3-1 Jobs
				case MAPIDs.MAPID_RUNE_KNIGHT_T:         return JOBs.JOB_RUNE_KNIGHT_T;
				case MAPIDs.MAPID_WARLOCK_T:             return JOBs.JOB_WARLOCK_T;
				case MAPIDs.MAPID_RANGER_T:              return JOBs.JOB_RANGER_T;
				case MAPIDs.MAPID_ARCH_BISHOP_T:         return JOBs.JOB_ARCH_BISHOP_T;
				case MAPIDs.MAPID_MECHANIC_T:            return JOBs.JOB_MECHANIC_T;
				case MAPIDs.MAPID_GUILLOTINE_CROSS_T:    return JOBs.JOB_GUILLOTINE_CROSS_T;
			//Trans 3-2 Jobs
				case MAPIDs.MAPID_ROYAL_GUARD_T:         return JOBs.JOB_ROYAL_GUARD_T;
				case MAPIDs.MAPID_SORCERER_T:            return JOBs.JOB_SORCERER_T;
				case MAPIDs.MAPID_MINSTRELWANDERER_T:    return gender == GenderType.SEX_MALE ? JOBs.JOB_MINSTREL_T : JOBs.JOB_WANDERER_T;
				case MAPIDs.MAPID_SURA_T:                return JOBs.JOB_SURA_T;
				case MAPIDs.MAPID_GENETIC_T:             return JOBs.JOB_GENETIC_T;
				case MAPIDs.MAPID_SHADOW_CHASER_T:       return JOBs.JOB_SHADOW_CHASER_T;
			//Baby 3-1 Jobs
				case MAPIDs.MAPID_SUPER_BABY_E:          return JOBs.JOB_SUPER_BABY_E;
				case MAPIDs.MAPID_BABY_RUNE_KNIGHT:      return JOBs.JOB_BABY_RUNE_KNIGHT;
				case MAPIDs.MAPID_BABY_WARLOCK:          return JOBs.JOB_BABY_WARLOCK;
				case MAPIDs.MAPID_BABY_RANGER:           return JOBs.JOB_BABY_RANGER;
				case MAPIDs.MAPID_BABY_ARCH_BISHOP:      return JOBs.JOB_BABY_ARCH_BISHOP;
				case MAPIDs.MAPID_BABY_MECHANIC:         return JOBs.JOB_BABY_MECHANIC;
				case MAPIDs.MAPID_BABY_GUILLOTINE_CROSS: return JOBs.JOB_BABY_GUILLOTINE_CROSS;
				case MAPIDs.MAPID_BABY_STAR_EMPEROR:     return JOBs.JOB_BABY_STAR_EMPEROR;
			//Baby 3-2 Jobs
				case MAPIDs.MAPID_BABY_ROYAL_GUARD:      return JOBs.JOB_BABY_ROYAL_GUARD;
				case MAPIDs.MAPID_BABY_SORCERER:         return JOBs.JOB_BABY_SORCERER;
				case MAPIDs.MAPID_BABY_MINSTRELWANDERER: return gender == GenderType.SEX_MALE ? JOBs.JOB_BABY_MINSTREL : JOBs.JOB_BABY_WANDERER;
				case MAPIDs.MAPID_BABY_SURA:             return JOBs.JOB_BABY_SURA;
				case MAPIDs.MAPID_BABY_GENETIC:          return JOBs.JOB_BABY_GENETIC;
				case MAPIDs.MAPID_BABY_SHADOW_CHASER:    return JOBs.JOB_BABY_SHADOW_CHASER;
				case MAPIDs.MAPID_BABY_SOUL_REAPER:      return JOBs.JOB_BABY_SOUL_REAPER;
			//4-1 Jobs
				case MAPIDs.MAPID_HYPER_NOVICE:          return JOBs.JOB_HYPER_NOVICE;
				case MAPIDs.MAPID_DRAGON_KNIGHT:         return JOBs.JOB_DRAGON_KNIGHT;
				case MAPIDs.MAPID_ARCH_MAGE:             return JOBs.JOB_ARCH_MAGE;
				case MAPIDs.MAPID_WINDHAWK:              return JOBs.JOB_WINDHAWK;
				case MAPIDs.MAPID_CARDINAL:              return JOBs.JOB_CARDINAL;
				case MAPIDs.MAPID_MEISTER:               return JOBs.JOB_MEISTER;
				case MAPIDs.MAPID_SHADOW_CROSS:          return JOBs.JOB_SHADOW_CROSS;
				case MAPIDs.MAPID_SKY_EMPEROR:           return JOBs.JOB_SKY_EMPEROR;
			//4-2 Jobs
				case MAPIDs.MAPID_IMPERIAL_GUARD:        return JOBs.JOB_IMPERIAL_GUARD;
				case MAPIDs.MAPID_ELEMENTAL_MASTER:      return JOBs.JOB_ELEMENTAL_MASTER;
				case MAPIDs.MAPID_TROUBADOURTROUVERE:    return gender == GenderType.SEX_MALE ? JOBs.JOB_TROUBADOUR : JOBs.JOB_TROUVERE;
				case MAPIDs.MAPID_INQUISITOR:            return JOBs.JOB_INQUISITOR;
				case MAPIDs.MAPID_BIOLO:                 return JOBs.JOB_BIOLO;
				case MAPIDs.MAPID_ABYSS_CHASER:          return JOBs.JOB_ABYSS_CHASER;
				case MAPIDs.MAPID_SOUL_ASCETIC:          return JOBs.JOB_SOUL_ASCETIC;
			//None
				default: 
					unchecked {
						return (JOBs)(-1);
					}
			}
		}
	}
}
