using SDE.Databases.Achievements.Features;
using SDE.Databases.Achievements.Parser;
using SDE.Databases.Achievements.TabCommands;
using SDE.Databases.Generic.SearchDescriptors;
using SDE.Databases.Generic.TabCommands;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;

namespace SDE.Databases.Achievements {
	public class AchvDatabase : ModelDatabase {
		public AchvDatabase() : base(AchvAttributes.Model) {
			ThrowFileNotFoundException = false;
			Source = DataSources.Achievement;
			AttributeList = AchvAttributes.AttributeList;
			Parser = new AchvParserProvider();
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToImportTable(this),
					new CopyToClipboard(this),
					new CopyToClipboardOther(this),
					new SelectFromTable(DataSources.ClientAchievement),
					new AchvAutocomplete()
				);
			};
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					return new AchvViewYaml();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					SearchDescriptor = new SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Achv ID", true);
					SearchDescriptor.Add(v => ((Achv)v).Name ?? "", "Name", true);
					SearchDescriptor.Add(v => ((Achv)v).Condition ?? "", "Condition");
					SearchDescriptor.Add(v => ((Achv)v).Map ?? "", "Map");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}
	}

	public class AchvDatabaseImport : AchvDatabase {
		public AchvDatabaseImport() {
			Source = DataSources.AchievementImport;
			ThrowFileNotFoundException = false;
		}

		//public override void WriteDb(string dbPath, string subPath, FileType fileType = FileType.Detect) {
		//	string destPath = GrfPath.Combine(dbPath, "import", "achievement_db.yml");
		//
		//	WriterContext writer = new WriterContext(this);
		//	writer.Mode = WriterMode.Normal;
		//	writer.DestPath = destPath;
		//	writer.OriginalPath = DbPathLocator.GetStoredFile(writer.DestPath);
		//
		//	Parser.Write(writer);
		//}
	}
}
