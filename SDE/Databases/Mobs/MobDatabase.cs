using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.SearchDescriptors;
using SDE.Databases.Generic.TabCommands;
using SDE.Databases.Mobs.Common;
using SDE.Databases.Mobs.Features;
using SDE.Databases.Mobs.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.LuaTables;
using System;
using System.Windows;

namespace SDE.Databases.Mobs {
	public class MobDatabase : ModelDatabase {
		public MobDatabase() : base(MobAttributes.Model) {
			Source = DataSources.Mob;
			Parser = new MobParserProvider();
			AttributeList = MobAttributes.AttributeList;
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToClipboard(this),
					new CopyToClipboardOther(this),
					new CopyToImportTable(this)
				);
			};

			WeakEventManager<BaseDatabase, EventArgs>.AddHandler(this, nameof(TableLoaded), OnTableModified);
			WeakEventManager<BaseDatabase, EventArgs>.AddHandler(this, nameof(TableModified), OnTableModified);
		}

		public void OnTableModified(object sender, EventArgs args) {
			CachedDbs.AegisNameMob.Dirty();
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					return new MobViewYaml();
				case FileType.Txt:
					return new MobViewCsv();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					SearchDescriptor = new SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Mob ID", true);
					SearchDescriptor.Add(v => ((Mob)v).Name ?? "", "Name", true);
					SearchDescriptor.Add(v => ((Mob)v).AegisName ?? "", "Aegis name", true);
					SearchDescriptor.Add(v => ((Mob)v).JapaneseName, "jRO name");
					SearchDescriptor.Add<SizeType>(v => ((Mob)v).Size, "Size");
					SearchDescriptor.Add<RaceType>(v => ((Mob)v).Race, "Race");
					SearchDescriptor.Add<ElementType>(v => ((Mob)v).Element, "Element");
					SearchDescriptor.Add<ClassType>(v => ((Mob)v).Class, "Class");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
				case FileType.Txt:
					SearchDescriptor = new SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Mob ID", true);
					SearchDescriptor.Add(v => ((Mob)v).Name ?? "", "Name", true);
					SearchDescriptor.Add(v => ((Mob)v).AegisName ?? "", "Aegis name", true);
					SearchDescriptor.Add(v => ((Mob)v).JapaneseName, "jRO name");
					SearchDescriptor.Add<SizeType>(v => ((Mob)v).Size, "Size");
					SearchDescriptor.Add<RaceType>(v => ((Mob)v).Race, "Race");
					SearchDescriptor.Add<ElementType>(v => ((Mob)v).Element, "Element");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}

		protected override void _loadDb() {
			base._loadDb();
			LuaHelper.ReloadJobTable(this);
		}
	}

	public class MobDatabaseImport : MobDatabase {
		public MobDatabaseImport() {
			Source = DataSources.MobImport;
			ThrowFileNotFoundException = false;
		}
	}
}
