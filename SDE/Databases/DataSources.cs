using SDE.Editor;
using System;
using System.Collections.Generic;

namespace SDE.Databases {
	public static class DataSources {
		static DataSources() {
			foreach (var db in AllSources) {
				if (db.ImportTable != null)
					db.ImportTable.IsImport = true;
			}
		}

		public static List<DataSource> AllSources = new List<DataSource>();
		public static readonly DataSource Zero = new DataSource("null");

		// Client
		public static readonly DataSource ClientItem = new DataSource("client_items") {
			DisplayName = "Client Item",
			IsClientSide = true,
			ClientSidePath = delegate {
				// These paths are only used for detecting if the database is enabled or not.
				// If one of these is missing, then the tab will be disabled.
				// The actual paths are in the parser itself (ClientItemReader*.cs)
				if (ProjectConfiguration.UseLuaFiles) {
					return ProjectConfiguration.ClientItemInfo;
				}

				return ProjectConfiguration.ClientItemIdentifiedDescription;
			}
		};

		public static DataSource ClientQuest { get; } = new DataSource("OngoingQuestInfoList") {
			DisplayName = "Client Quest",
			IsClientSide = true, 
			ClientSidePath = () => ProjectConfiguration.UseLuaFiles ? ProjectConfiguration.ClientQuestLua : ProjectConfiguration.ClientQuest
		};
		
		public static DataSource ClientAchievement { get; } = new DataSource("achievement_list") {
			DisplayName = "Client Achievement",
			IsClientSide = true,
			ClientSidePath = () => ProjectConfiguration.ClientAchievement
		};

		public static DataSource ItemFlagImport { get; } = new DataSource("item_flag2") {
			DisplayName = "Item Flag Import",
			Paths = {
				@"import\item_flag.txt",
				@"item_flag2.txt",
			},
		};

		public static DataSource ItemFlag { get; } = new DataSource("item_flag") {
			DisplayName = "Item Flag",
			Paths = {
				@"{DBPATH}\item_flag.txt",
				@"item_flag.txt",
			},
			ImportTable = ItemFlagImport,
		};

		public static DataSource ItemAvailImport { get; } = new DataSource("item_avail2") {
			DisplayName = "Item Avail Import",
			Paths = {
				@"import\item_avail.txt",
				@"item_avail2.txt",
			},
		};

		public static DataSource ItemAvail { get; } = new DataSource("item_avail") {
			DisplayName = "Item Avail",
			Paths = {
				@"{DBPATH}\item_avail.txt",
				@"item_avail.txt",
			},
			ImportTable = ItemAvailImport,
		};
		
		public static DataSource ItemNoUseImport { get; } = new DataSource("item_nouse2") {
			DisplayName = "Item NoUse Import",
			Paths = {
				@"import\item_nouse.txt",
				@"item_nouse2.txt",
			},
		};
		
		public static DataSource ItemNoUse { get; } = new DataSource("item_nouse") {
			DisplayName = "Item NoUse",
			Paths = {
				@"{DBPATH}\item_nouse.txt",
				@"item_nouse.txt",
			},
			ImportTable = ItemNoUseImport,
		};

		public static DataSource ItemDelayImport { get; } = new DataSource("item_delay2") {
			DisplayName = "Item Delay Import",
			Paths = {
				@"import\item_delay.txt",
				@"item_delay2.txt",
			},
		};

		public static DataSource ItemDelay { get; } = new DataSource("item_delay") {
			DisplayName = "Item Delay",
			Paths = {
				@"{DBPATH}\item_delay.txt",
			},
			ImportTable = ItemDelayImport,
		};
		
		public static DataSource ItemStackImport { get; } = new DataSource("item_stack2") {
			DisplayName = "Item Stack Import",
			Paths = {
				@"import\item_stack.txt",
				@"item_stack2.txt",
			},
		};
		
		public static DataSource ItemStack { get; } = new DataSource("item_stack") {
			DisplayName = "Item Stack",
			Paths = {
				@"{DBPATH}\item_stack.txt",
			},
			ImportTable = ItemStackImport,
		};

		public static DataSource ItemTradeImport { get; } = new DataSource("item_trade2") {
			DisplayName = "Item Trade Import",
			Paths = {
				@"import\item_trade.txt",
				@"item_trade2.txt",
			},
		};

		public static DataSource ItemTrade { get; } = new DataSource("item_trade") {
			DisplayName = "Item Trade",
			Paths = {
				@"{DBPATH}\item_trade.txt",
			},
			ImportTable = ItemTradeImport
		};
		
		public static DataSource ItemBuyingStoreImport { get; } = new DataSource("item_buyingstore2") {
			DisplayName = "Item Buying Store Import",
			Paths = {
				@"import\item_buyingstore.txt",
				@"item_buyingstore2.txt",
			},
		};
		
		public static DataSource ItemsBuyingStore { get; } = new DataSource("item_buyingstore") {
			DisplayName = "Item Buying Store",
			Paths = {
				@"{DBPATH}\item_buyingstore.txt",
			},
			ImportTable = ItemBuyingStoreImport
		};
		
		public static DataSource ItemImport { get; } = new DataSource("item_db2") {
			DisplayName = "Item Import",
			Paths = {
				@"import\item_db.yml",
				@"import\item_db.txt",
				@"item_db2.txt",
			},
		};

		public static DataSource Item { get; } = new DataSource("item_db") {
			DisplayName = "Item",
			Paths = {
				@"{DBPATH}\item_db.yml",
				@"{DBPATH}\item_db.txt",
			},
			ImportTable = ItemImport,
		};

		public static DataSource MobImport { get; } = new DataSource("mob_db") {
			DisplayName = "Mob Import",
			Paths = {
				@"import\mob_db.yml",
				@"import\mob_db.txt",
				@"mob_db2.txt",
			},
		};

		public static DataSource Mob { get; } = new DataSource("mob_db2") {
			DisplayName = "Mob",
			Paths = {
				@"{DBPATH}\mob_db.yml",
				@"{DBPATH}\mob_db.txt",
			},
			ImportTable = MobImport,
		};

		public static DataSource MobAvail { get; } = new DataSource("mob_avail") { DisplayName = "Mob>Avail", UseSubPath = false };
		
		public static DataSource SkillImport { get; } = new DataSource("skill_db2") {
			DisplayName = "Skill Import",
			Paths = {
				@"import\skill_db.yml",
				@"import\skill_db.txt",
				@"skill_db2.txt",
			},
		};

		public static DataSource Skill { get; } = new DataSource("skill_db") {
			DisplayName = "Skill",
			Paths = {
				@"{DBPATH}\skill_db.yml",
				@"{DBPATH}\skill_db.txt",
			},
			ImportTable = SkillImport,
		};

		public static DataSource SkillNoDexImport { get; } = new DataSource("skill_castnodex_db2") {
			DisplayName = "Skill CastNoDex Import",
			Paths = {
				@"import\skill_castnodex_db.txt",
				@"skill_castnodex_db2.txt",
			},
		};

		public static DataSource SkillNoDex { get; } = new DataSource("skill_castnodex_db") {
			DisplayName = "Skill CastNoDex",
			Paths = {
				@"{DBPATH}\skill_castnodex_db.txt",
			},
			ImportTable = SkillNoDexImport
		};

		public static DataSource SkillNoCastImport { get; } = new DataSource("skill_nocast_db2") {
			DisplayName = "Skill NoCast Import",
			Paths = {
				@"import\skill_nocast_db.txt",
				@"skill_nocast_db2.txt",
			},
		};

		public static DataSource SkillNoCast { get; } = new DataSource("skill_nocast_db") {
			DisplayName = "Skill NoCast",
			Paths = {
				@"{DBPATH}\skill_nocast_db.txt",
			},
			ImportTable = SkillNoCastImport
		};

		public static DataSource SkillCastImport { get; } = new DataSource("skill_cast_db2") {
			DisplayName = "Skill Cast Import",
			Paths = {
				@"import\skill_cast_db.txt",
				@"skill_cast_db2.txt",
			},
		};

		public static DataSource SkillCast { get; } = new DataSource("skill_cast_db") {
			DisplayName = "Skill Cast",
			Paths = {
				@"{DBPATH}\skill_cast_db.txt",
			},
			ImportTable = SkillCastImport
		};

		public static DataSource SkillRequirementImport { get; } = new DataSource("skill_require_db2") {
			DisplayName = "Skill Requirements Import",
			Paths = {
				@"import\skill_require_db.txt",
				@"skill_require_db2.txt",
			},
		};

		public static DataSource SkillRequirement { get; } = new DataSource("skill_require_db") {
			DisplayName = "Skill Requirements",
			Paths = {
				@"{DBPATH}\skill_require_db.txt",
			},
			ImportTable = SkillRequirementImport
		};

		public static DataSource SkillUnitImport { get; } = new DataSource("skill_unit_db2") {
			DisplayName = "Skill Unit Import",
			Paths = {
				@"import\skill_unit_db.txt",
				@"skill_unit_db2.txt",
			},
		};

		public static DataSource SkillUnit { get; } = new DataSource("skill_unit_db") {
			DisplayName = "Skill Unit",
			Paths = {
				@"{DBPATH}\skill_unit_db.txt",
			},
			ImportTable = SkillRequirementImport
		};

		public static DataSource SkillCopyableImport { get; } = new DataSource("skill_copyable_db2") {
			DisplayName = "Skill Copyable Import",
			Paths = {
				@"import\skill_copyable_db.txt",
				@"skill_copyable_db2.txt",
			},
		};

		public static DataSource SkillCopyable { get; } = new DataSource("skill_copyable_db") {
			DisplayName = "Skill Copyable",
			Paths = {
				@"skill_copyable_db.txt",
			},
			ImportTable = SkillCopyableImport
		};

		public static DataSource ItemComboImport { get; } = new DataSource("item_combo_db2") {
			DisplayName = "Item Combo",
			Paths = {
				@"import\item_combos.yml",
				@"import\item_combos.txt",
				@"item_combos2.txt",
			},
		};

		public static DataSource ItemCombo { get; } = new DataSource("item_combo_db") {
			DisplayName = "Item Combo",
			Paths = {
				@"{DBPATH}\item_combos.yml",
				@"{DBPATH}\item_combos.txt",
				@"{DBPATH}\item_combo_db.txt",
				@"item_combo_db.txt",
			},
			ImportTable = ItemComboImport,
		};

		public static DataSource MobSkillImport { get; } = new DataSource("mob_skill_db2") {
			DisplayName = "Mob Skill Import",
			Paths = {
				@"import\mob_skill_db.txt",
			},
		};
		
		public static DataSource MobSkill { get; } = new DataSource("mob_skill_db") {
			DisplayName = "Mob Skill",
			Paths = {
				@"{DBPATH}\mob_skill_db.txt",
			},
			ImportTable = MobSkillImport,
		};

		public static DataSource PetImport { get; } = new DataSource("pet_db2") {
			DisplayName = "Pet Import",
			Paths = {
				@"import\pet_db.yml",
				@"import\pet_db.txt",
				@"pet_db2.txt",
			},
		};
		
		public static DataSource Pet { get; } = new DataSource("pet_db") {
			DisplayName = "Pet",
			Paths = {
				@"{DBPATH}\pet_db.yml",
				@"{DBPATH}\pet_db.txt",
				@"pet_db.txt",
			},
			ImportTable = PetImport,
		};
		
		public static DataSource CastleImport { get; } = new DataSource("castle_db2") {
			DisplayName = "Castle",
			Paths = {
				@"import\castle_db.yml",
				@"import\castle_db.txt",
				@"castle_db2.txt",
			},
		};
		
		public static DataSource Castle { get; } = new DataSource("castle_db") {
			DisplayName = "Castle",
			Paths = {
				@"{DBPATH}\castle_db.yml",
				@"{DBPATH}\castle_db.txt",
				@"castle_db.txt",
			},
			ImportTable = CastleImport,
		};
		
		public static DataSource QuestImport { get; } = new DataSource("quest_db2") {
			DisplayName = "Quest Import",
			Paths = {
				@"import\quest_db.yml",
				@"import\quest_db.txt",
				@"quest_db2.txt",
			},
		};
		
		public static DataSource Quest { get; } = new DataSource("quest_db") {
			DisplayName = "Quest",
			Paths = {
				@"{DBPATH}\quest_db.yml",
				@"{DBPATH}\quest_db.txt",
			},
			ImportTable = QuestImport
		};
		
		public static DataSource AchievementImport { get; } = new DataSource("achievement_db2") {
			DisplayName = "Achievement Import",
			Paths = {
				@"import\achievement_db.yml",
				@"import\achievement_db.txt",
				@"achievement_db2.txt",
			},
		};
		
		public static DataSource Achievement { get; } = new DataSource("achievement_db") {
			DisplayName = "Achievement",
			Paths = {
				@"{DBPATH}\achievement_db.yml",
				@"{DBPATH}\achievement_db.txt",
			},
			ImportTable = AchievementImport,
		};

		public static DataSource ClientResourceDb { get; } = new DataSource("idnum2itemresnametable") {
			DisplayName = "Fallback Resource",
			IsClientSide = true,
		};
		
		public static DataSource Emote { get; } = new DataSource("emotes") {
			DisplayName = "Emote"
		};

		public static DataSource AchievementIcon { get; } = new DataSource("achievement_icons") {
			DisplayName = "Achv Group"
		};

		public static DataSource Title { get; } = new DataSource("titles") {
			DisplayName = "Title",
			IsClientSide = true,
			ClientSidePath = () => ProjectConfiguration.ClientTitle
		};

		public static ulong AllItemTables = ClientItem | Item | ItemImport;
		public static ulong ServerItems = Item | ItemImport;
		public static ulong MobSkillsItems = MobSkill | MobSkillImport;
	}

	public sealed class DataSource {
		public List<string> Paths = new List<string>();
		private readonly ulong _subId;
		private string _displayName;

		public DataSource() {
			_subId = (ulong)1 << DataSources.AllSources.Count;
			DataSources.AllSources.Add(this);
		}

		public DataSource(string name) {
			UseSubPath = true;
			UidName = name;
			_subId = (ulong)1 << DataSources.AllSources.Count;
			DataSources.AllSources.Add(this);
		}

		public bool IsImport { get; set; }
		public bool IsClientSide { get; set; }
		public Func<string> ClientSidePath { get; set; }

		public string DisplayName {
			get { return _displayName ?? UidName; }
			set { _displayName = value; }
		}

		public string UidName { get; } = "null";
		public DataSource ImportTable { get; set; }

		// Not used anymore
		public bool UseSubPath { get; set; }

		private bool _equals(DataSource other) {
			return string.Equals(UidName, other.UidName);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is DataSource && _equals((DataSource)obj);
		}

		public override int GetHashCode() {
			return UidName != null ? UidName.GetHashCode() : 0;
		}

		public static implicit operator string(DataSource item) {
			return item.UidName;
		}

		public static implicit operator ulong(DataSource item) {
			return item._subId;
		}

		public static bool operator ==(DataSource item1, DataSource item2) {
			if (ReferenceEquals(item1, item2)) return true;
			if (ReferenceEquals(item1, null)) return false;
			if (ReferenceEquals(item2, null)) return false;
			return item1.Equals(item2);
		}

		public static bool operator !=(DataSource item1, DataSource item2) {
			return !(item1 == item2);
		}

		public static DataSource Instantiate(string fileName, string displayName) {
			return new DataSource(fileName) { DisplayName = displayName };
		}

		public override string ToString() {
			return DisplayName;
		}
	}
}