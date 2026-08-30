using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Skills.Common {
	[Flags]
	[Register(typeof(Inf2FlagInfo))]
	public enum Inf2Flag : Int64 {
		INF2_ISQUEST = 1L << 0,
		INF2_ISNPC = 1L << 1, //NPC skills are those that players can't have in their skill tree.
		INF2_ISWEDDING = 1L << 2,
		INF2_ISSPIRIT = 1L << 3,
		INF2_ISGUILD = 1L << 4,
		INF2_ISSONG = 1L << 5,
		INF2_ISENSEMBLE = 1L << 6,
		INF2_ISTRAP = 1L << 7,
		INF2_TARGETSELF = 1L << 8, //Refers to ground placed skills that will target the caster as well (like Grandcross)
		INF2_NOTARGETSELF = 1L << 9,
		INF2_PARTYONLY = 1L << 10,
		INF2_GUILDONLY = 1L << 11,
		INF2_NOTARGETENEMY = 1L << 12,
		INF2_ISAUTOSHADOWSPELL = 1L << 13, // Skill that available for SC_AUTOSHADOWSPELL
		INF2_ISCHORUS = 1L << 14, // Chorus skill
		INF2_IGNOREBGREDUCTION = 1L << 15, // Skill that ignore bg reduction
		INF2_IGNOREGVGREDUCTION = 1L << 16, // Skill that ignore gvg reduction
		INF2_DISABLENEARNPC = 1L << 17, // disable to cast skill if near with NPC [Cydh]
		INF2_TARGETTRAP = 1L << 18, // can hit trap-type skill (INF2_ISTRAP) [Cydh]
		INF2_IGNORELANDPROTECTOR = 1L << 19, // Skill that can ignore Land Protector
		INF2_ALLOWWHENHIDDEN = 1L << 20, // Skill that can be use in hiding
		INF2_ALLOWWHENPERFORMING = 1L << 21, // Skill that can be use while in dancing state
		INF2_TARGETEMPERIUM = 1L << 22, // Skill that could hit emperium
		INF2_IGNOREKAGEHUMI = 1L << 23, // Skill blocked by kagehumi
		INF2_ALTERRANGEVULTURE = 1L << 24, // Skill range affected by AC_VULTURE
		INF2_ALTERRANGESNAKEEYE = 1L << 25, // Skill range affected by GS_SNAKEEYE
		INF2_ALTERRANGESHADOWJUMP = 1L << 26, // Skill range affected by NJ_SHADOWJUMP
		INF2_ALTERRANGERADIUS = 1L << 27, // Skill range affected by WL_RADIUS
		INF2_ALTERRANGERESEARCHTRAP = 1L << 28, // Skill range affected by RA_RESEARCHTRAP
		INF2_IGNOREHOVERING = 1L << 29, // Skill that does not affect user that has SC_HOVERING active
		INF2_ALLOWONWARG = 1L << 30, // Skill that can be use while riding warg
		INF2_ALLOWONMADO = 1L << 31, // Skill that can be used while on Madogear
		INF2_TARGETMANHOLE = 1L << 32, // Skill that can be used to target while under SC__MANHOLE effect
		INF2_TARGETHIDDEN = 1L << 33, // Skill that affects hidden targets
		INF2_INCREASEDANCEWITHWUGDAMAGE = 1L << 34, // Skill that is affected by SC_DANCEWITHWUG
		INF2_IGNOREWUGBITE = 1L << 35, // Skill blocked by RA_WUGBITE
		INF2_IGNOREAUTOGUARD = 1L << 36, // Skill is not blocked by SC_AUTOGUARD (physical-skill only)
		INF2_IGNORECICADA = 1L << 37, // Skill is not blocked by SC_UTSUSEMI or SC_BUNSINJYUTSU (physical-skill only)
		INF2_SHOWSCALE = 1L << 38, // Skill shows AoE area while casting
		INF2_IGNOREGTB = 1L << 39, // Skill ignores effect of GTB
		INF2_TOGGLEABLE = 1L << 40, // Skill can be toggled on and off (won't consume HP/SP when toggled off)
		INF2_IGNORENONCRITATKBONUS = 1L << 41, // Skill ignores the bonus of bNonCritAtkRate
		INF2_MAX = 1L << 42,
	}

	public static class Inf2FlagInfo {
		public const string Marker = "INF2_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static Inf2FlagInfo() {
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISQUEST, "IsQuest", Marker) { ToolTip = "Quest skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISNPC, "IsNpc", Marker) { ToolTip = "NPC skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISWEDDING, "IsWedding", Marker) { ToolTip = "Wedding skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISSPIRIT, "IsSpirit", Marker) { ToolTip = "Spirit skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISGUILD, "IsGuild", Marker) { ToolTip = "Guild skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISSONG, "IsSong", Marker) { ToolTip = "Song/Dance skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISENSEMBLE, "IsEnsemble", Marker) { ToolTip = "Ensemble skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISTRAP, "IsTrap", Marker) { ToolTip = "Trap skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_TARGETSELF, "TargetSelf", Marker) { ToolTip = "Damages/targets self." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_NOTARGETSELF, "NoTargetSelf", Marker) { ToolTip = "Cannot target self. If TargetType is Self, changes to Attack." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_PARTYONLY, "PartyOnly", Marker) { ToolTip = "Usable on party (and enemies if offensive)." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_GUILDONLY, "GuildOnly", Marker) { ToolTip = "Usable on guild (and enemies if offensive)." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_NOTARGETENEMY, "NoTargetEnemy", Marker) { ToolTip = "Disable on enemies (for non-offensive)." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISAUTOSHADOWSPELL, "IsAutoShadowSpell", Marker) { ToolTip = "Make skill available for SC_AUTOSHADOWSPELL." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ISCHORUS, "IsChorus", Marker) { ToolTip = "Chorus skill." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNOREBGREDUCTION, "IgnoreBgReduction", Marker) { ToolTip = "Ignore Battleground reduction." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNOREGVGREDUCTION, "IgnoreGvgReduction", Marker) { ToolTip = "Ignore GvG reduction." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_DISABLENEARNPC, "DisableNearNpc", Marker) { ToolTip = "Disable self/ground skills near NPC. In tandem with NoNearNpc node." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_TARGETTRAP, "TargetTrap", Marker) { ToolTip = "Damage traps. If TargetType is Trap." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNORELANDPROTECTOR, "IgnoreLandProtector", Marker) { ToolTip = "Ignore SA_LANDPROTECTOR." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALLOWWHENHIDDEN, "AllowWhenHidden", Marker) { ToolTip = "Usable while hiding." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALLOWWHENPERFORMING, "AllowWhenPerforming", Marker) { ToolTip = "Usable while in dancing state." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_TARGETEMPERIUM, "TargetEmperium", Marker) { ToolTip = "Damages/targets Emperium." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNOREKAGEHUMI, "IgnoreKagehumi", Marker) { ToolTip = "Ignore KG_KAGEHUMI." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALTERRANGEVULTURE, "AlterRangeVulture", Marker) { ToolTip = "Skill range affected by AC_VULTURE." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALTERRANGESNAKEEYE, "AlterRangeSnakeEye", Marker) { ToolTip = "Skill range affected by GS_SNAKEEYE." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALTERRANGESHADOWJUMP, "AlterRangeShadowJump", Marker) { ToolTip = "Skill range affected by NJ_SHADOWJUMP." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALTERRANGERADIUS, "AlterRangeRadius", Marker) { ToolTip = "Skill range affected by WL_RADIUS." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALTERRANGERESEARCHTRAP, "AlterRangeResearchTrap", Marker) { ToolTip = "Skill range affected by RA_RESEARCHTRAP." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNOREHOVERING, "IgnoreHovering", Marker) { ToolTip = "Ignore SC_HOVERING." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALLOWONWARG, "AllowOnWarg", Marker) { ToolTip = "Usable while riding Warg." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_ALLOWONMADO, "AllowOnMado", Marker) { ToolTip = "Usable while on Madogear." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_TARGETMANHOLE, "TargetManHole", Marker) { ToolTip = "Target enemy with SC__MANHOLE." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_TARGETHIDDEN, "TargetHidden", Marker) { ToolTip = "Target enemy with OPTION_HIDE." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_INCREASEDANCEWITHWUGDAMAGE, "IncreaseDanceWithWugDamage", Marker) { ToolTip = "Increase SC_DANCEWITHWUG damage." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNOREWUGBITE, "IgnoreWugBite", Marker) { ToolTip = "Ignore RA_WUGBITE." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNOREAUTOGUARD, "IgnoreAutoGuard", Marker) { ToolTip = "Not blocked by SC_AUTOGUARD (When TargetType is Weapon only)." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNORECICADA, "IgnoreCicada", Marker) { ToolTip = "Not blocked by SC_UTSUSEMI or SC_BUNSINJYUTSU (When TargetType is Weapon only)." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_SHOWSCALE, "ShowScale", Marker) { ToolTip = "Shows AoE area while casting" });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNOREGTB, "IgnoreGtb", Marker) { ToolTip = "Not blocked by Golden Thief Bug card." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_TOGGLEABLE, "Toggleable", Marker) { ToolTip = "Skill can be toggled on and off. When toggled off the skill doesn't consume HP/SP." });
			All.Add(new EnumInfoBase(Inf2Flag.INF2_IGNORENONCRITATKBONUS, "IgnoreNonCritAtkBonus", Marker) { ToolTip = "Skill ignores the bonus of bNonCritAtkRate" });

			foreach (var info in All) {
				info.YamlName = info.DisplayName;
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<Inf2Flag>(All, TypeToInfo, Marker);
		}
	}
}
