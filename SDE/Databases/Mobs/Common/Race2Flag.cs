using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Mobs.Common {
	[Flags]
	[Register(typeof(Race2FlagInfo))]
	public enum Race2Flag : Int64 {
		RC2_NONE = 0,
		RC2_GOBLIN = 1L << 0,
		RC2_KOBOLD = 1L << 1,
		RC2_ORC = 1L << 2,
		RC2_GOLEM = 1L << 3,
		RC2_GUARDIAN = 1L << 4,
		RC2_NINJA = 1L << 5,
		RC2_GVG = 1L << 6,
		RC2_BATTLEFIELD = 1L << 7,
		RC2_TREASURE = 1L << 8,
		RC2_BIOLAB = 1L << 9,
		RC2_MANUK = 1L << 10,
		RC2_SPLENDIDE = 1L << 11,
		RC2_SCARABA = 1L << 12,
		RC2_OGH_ATK_DEF = 1L << 13,
		RC2_OGH_HIDDEN = 1L << 14,
		RC2_BIO5_SWORDMAN_THIEF = 1L << 15,
		RC2_BIO5_ACOLYTE_MERCHANT = 1L << 16,
		RC2_BIO5_MAGE_ARCHER = 1L << 17,
		RC2_BIO5_MVP = 1L << 18,
		RC2_CLOCKTOWER = 1L << 19,
		RC2_THANATOS = 1L << 20,
		RC2_FACEWORM = 1L << 21,
		RC2_HEARTHUNTER = 1L << 22,
		RC2_ROCKRIDGE = 1L << 23,
		RC2_WERNER_LAB = 1L << 24,
		RC2_TEMPLE_DEMON = 1L << 25,
		RC2_ILLUSION_VAMPIRE = 1L << 26,
		RC2_MALANGDO = 1L << 27,
		RC2_EP172ALPHA = 1L << 28,
		RC2_EP172BETA = 1L << 29,
		RC2_EP172BATH = 1L << 30,
		RC2_ILLUSION_TURTLE = 1L << 31,
		RC2_RACHEL_SANCTUARY = 1L << 32,
		RC2_ILLUSION_LUANDA = 1L << 33,
		RC2_ILLUSION_FROZEN = 1L << 34,
		RC2_ILLUSION_MOONLIGHT = 1L << 35,
		RC2_EP16_DEF = 1L << 36,
		RC2_EDDA_ARUNAFELTZ = 1L << 37,
		RC2_LASAGNA = 1L << 38,
		RC2_GLAST_HEIM_ABYSS = 1L << 39,
		RC2_DESTROYED_VALKYRIE_REALM = 1L << 40,
		RC2_ENCROACHED_GEPHENIA = 1L << 41,
	}

	public static class Race2FlagInfo {
		public const string Marker = "RC2_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static Race2FlagInfo() {
			All.Add(new EnumInfoBase(Race2Flag.RC2_NONE, "None", Marker) { Visible = false });
			All.Add(new EnumInfoBase(Race2Flag.RC2_GOBLIN, "Goblin", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_KOBOLD, "Kobold", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_ORC, "Orc", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_GOLEM, "Golem", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_GUARDIAN, "Guardian", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_NINJA, "Ninja", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_GVG, "Gvg", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_BATTLEFIELD, "Battlefield", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_TREASURE, "Treasure", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_BIOLAB, "Biolab", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_MANUK, "Manuk", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_SPLENDIDE, "Splendide", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_SCARABA, "Scaraba", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_OGH_ATK_DEF, "Ogh Atk Def", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_OGH_HIDDEN, "Ogh Hidden", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_BIO5_SWORDMAN_THIEF, "Bio5 Swordman Thief", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_BIO5_ACOLYTE_MERCHANT, "Bio5 Acolyte Merchant", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_BIO5_MAGE_ARCHER, "Bio5 Mage Archer", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_BIO5_MVP, "Bio5 Mvp", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_CLOCKTOWER, "Clocktower", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_THANATOS, "Thanatos", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_FACEWORM, "Faceworm", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_HEARTHUNTER, "Hearthunter", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_ROCKRIDGE, "Rockridge", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_WERNER_LAB, "Werner Lab", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_TEMPLE_DEMON, "Temple Demon", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_ILLUSION_VAMPIRE, "Illusion Vampire", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_MALANGDO, "Malangdo", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_EP172ALPHA, "Ep17 Alpha", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_EP172BETA, "Ep17 Beta", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_EP172BATH, "Ep17 Bath", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_ILLUSION_TURTLE, "Illusion Turtle", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_RACHEL_SANCTUARY, "Rachel Sanctuary", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_ILLUSION_LUANDA, "Illusion Luanda", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_ILLUSION_FROZEN, "Illusion Frozen", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_ILLUSION_MOONLIGHT, "Illusion Moonlight", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_EP16_DEF, "Ep16 Def", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_EDDA_ARUNAFELTZ, "Edda Arunafeltz", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_LASAGNA, "Lasagna", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_GLAST_HEIM_ABYSS, "Glast Heim Abyss", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_DESTROYED_VALKYRIE_REALM, "Destroyed Valkyrie Realm", Marker));
			All.Add(new EnumInfoBase(Race2Flag.RC2_ENCROACHED_GEPHENIA, "Encroached Gephenia", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<Race2Flag>(All, TypeToInfo, Marker);
		}
	}
}
