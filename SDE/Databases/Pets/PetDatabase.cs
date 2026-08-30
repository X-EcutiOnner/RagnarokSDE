using SDE.Databases.Generic.TabCommands;
using SDE.Databases.Pets.Features;
using SDE.Databases.Pets.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;

namespace SDE.Databases.Pets {
	public class PetDatabase : ModelDatabase {
		public PetDatabase() : base(PetAttributes.Model) {
			Source = DataSources.Pet;
			AttributeList = PetAttributes.AttributeList;
			Parser = new PetParserProvider();
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToImportTable(this),
					new CopyToClipboard(this),
					new CopyToClipboardOther(this),
					new SelectFromTable(DataSources.Mob)
				);
			};
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					return new PetViewYaml();
				case FileType.Txt:
					return new PetViewCsv();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString() ?? "", "Mob ID", true);
					SearchDescriptor.AddTuple(v => DbUtilities.MobId2Name(((ReadableTuple)v).Key), "Display name", true);
					SearchDescriptor.Add(v => ((Pet)v).TameItem ?? "", "Tame ID");
					SearchDescriptor.Add(v => ((Pet)v).EquipItem ?? "", "Equip ID");
					SearchDescriptor.Add(v => ((Pet)v).EggItem ?? "", "Egg ID");
					SearchDescriptor.Add(v => ((Pet)v).FoodItem ?? "", "Food ID");
					SearchDescriptor.Add(v => ((Pet)v).Script ?? "", "Script");
					SearchDescriptor.Add(v => ((Pet)v).SupportScript ?? "", "Support script");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
				case FileType.Txt:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString() ?? "", "Mob ID", true);
					SearchDescriptor.AddTuple(v => DbUtilities.MobId2Name(((ReadableTuple)v).Key), "Display name", true);
					SearchDescriptor.Add(v => ((Pet)v).AegisName ?? "", "Sprite name");
					SearchDescriptor.Add(v => ((Pet)v).EquipItem ?? "", "Equip ID");
					SearchDescriptor.Add(v => ((Pet)v).EggItem ?? "", "Egg ID");
					SearchDescriptor.Add(v => ((Pet)v).FoodItem ?? "", "Food ID");
					SearchDescriptor.Add(v => ((Pet)v).Script ?? "", "Script");
					SearchDescriptor.Add(v => ((Pet)v).SupportScript ?? "", "Loyal script");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}
	}

	public class PetDatabaseImport : PetDatabase {
		public PetDatabaseImport() {
			Source = DataSources.PetImport;
			ThrowFileNotFoundException = false;
		}
	}
}
