using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using SDE.Databases.AchievementIcons;
using SDE.Databases.Achievements;
using SDE.Databases.Castles;
using SDE.Databases.ClientAchievements;
using SDE.Databases.ClientItemResources;
using SDE.Databases.ClientItems;
using SDE.Databases.ClientQuests;
using SDE.Databases.ItemCombos;
using SDE.Databases.Items;
using SDE.Databases.Mobs;
using SDE.Databases.MobSkills;
using SDE.Databases.Pets;
using SDE.Databases.Quests;
using SDE.Databases.Skills;
using SDE.Databases.Titles;
using SDE.Editor.Generic.DbTabs;

namespace SDE.Editor.Database {
	/// <summary>
	/// Instantiates all the tables and loads them. This class can
	/// also be used to load more tables after the instantiation.
	/// </summary>
	public class DbHolder {
		protected readonly List<BaseDatabase> _dbs = new List<BaseDatabase>();
		public ProjectManager Database { get; private set; }

		public virtual void Instantiate(ProjectManager sdb) {
			Database = sdb;

			_dbs.Add(new ClientItemResourceDatabase());
			_dbs.Add(new ItemDatabase());
			_dbs.Add(new ItemDatabaseImport());
			_dbs.Add(new ClientItemDatabase());
			_dbs.Add(new ItemComboDatabase());
			_dbs.Add(new ItemComboDatabaseImport());
			_dbs.Add(new SkillDatabase());
			_dbs.Add(new SkillDatabaseImport());
			_dbs.Add(new MobDatabase());
			_dbs.Add(new MobDatabaseImport());
			_dbs.Add(new MobSkillDatabase());
			_dbs.Add(new MobSkillDatabaseImport());
			_dbs.Add(new QuestDatabase());
			_dbs.Add(new QuestDatabaseImport());
			_dbs.Add(new ClientQuestDatabase());
			_dbs.Add(new AchvDatabase());
			_dbs.Add(new AchvDatabaseImport());
			_dbs.Add(new ClientAchvDatabase());
			_dbs.Add(new PetDatabase());
			_dbs.Add(new PetDatabaseImport());
			_dbs.Add(new CastleDatabase());
			_dbs.Add(new CastleDatabaseImport());

			// Special tables
			_dbs.Add(new EmoteDatabase());
			_dbs.Add(new AchvIconDatabase());
			_dbs.Add(new TitleDatabase());
			
			_dbs.ForEach(p => p.Init());
		}

		public void AddTable(BaseDatabase db) {
			_dbs.Add(db);
			db.Init();
		}

		public DbTab GetTab(BaseDatabase db, TabControl control) {
			return db.IsGenerateTab ? db.GenerateTab(Database, control, db) : null;
		}

		public List<DbTab> GetTabs(TabControl control) {
			return _dbs.Where(p => p.IsGenerateTab).Select(p => p.GenerateTab(Database, control, p)).ToList();
		}

		public void RemoveTable(BaseDatabase db) {
			Database.AllTables.Remove(db.Source);
			_dbs.Remove(db);
		}
	}
}