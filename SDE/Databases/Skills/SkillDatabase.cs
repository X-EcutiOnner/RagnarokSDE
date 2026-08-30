using SDE.Databases.Generic.SearchDescriptors;
using SDE.Databases.Generic.TabCommands;
using SDE.Databases.Skills.Common;
using SDE.Databases.Skills.Features;
using SDE.Databases.Skills.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;
using Utilities.Extension;

namespace SDE.Databases.Skills {
	public class SkillDatabase : ModelDatabase {
		public SkillDatabase() : base(SkillAttributes.Model) {
			Source = DataSources.Skill;
			AttributeList = SkillAttributes.AttributeList;
			Parser = new SkillParserProvider();
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToImportTable(this),
					new CopyToClipboard(this, FileType.Yaml)
				);
			};

			WeakEventManager<BaseDatabase, EventArgs>.AddHandler(this, nameof(TableLoaded), OnTableModified);
			WeakEventManager<BaseDatabase, EventArgs>.AddHandler(this, nameof(TableModified), OnTableModified);
		}

		public void OnTableModified(object sender, EventArgs args) {
			CachedDbs.SkillName.Dirty();
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					return new SkillViewYaml();
				case FileType.Txt:
					return new SkillViewCsv();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
				case FileType.Txt:
					SearchDescriptor = new SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Mob ID", true);
					SearchDescriptor.Add(v => ((Skill)v).Name ?? "", "Name", true);
					SearchDescriptor.Add(v => ((Skill)v).Description ?? "", "Description", true);
					SearchDescriptor.Add<DamageType>(v => ((Skill)v).DMG_Hit, "Hit mode");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}

		public override void OnLoadDataFromClipboard(DbLoadContext context, string text, string path, BaseDatabase db) {
			if (context.FileType == FileType.Yaml) {
				Parser.Read(context, this);
			}
			else {
				base.OnLoadDataFromClipboard(context, text, path, db);
			}
		}
	}

	public class SkillDatabaseImport : SkillDatabase {
		public SkillDatabaseImport() {
			Source = DataSources.SkillImport;
			ThrowFileNotFoundException = false;
		}

		public override void WriteDb(string dbPath, string subPath, FileType fileType = FileType.Detect) {
			string path = DbPathLocator.DetectPath(Source)?.GetMostRelative();

			if (!path.IsExtension(".yml"))
				return;

			base.WriteDb(dbPath, subPath, fileType);
		}
	}
}
