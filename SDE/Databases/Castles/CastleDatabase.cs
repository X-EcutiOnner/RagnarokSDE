using SDE.Databases.Castles.Features;
using SDE.Databases.Castles.Parser;
using SDE.Databases.Generic.TabCommands;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;

namespace SDE.Databases.Castles {
	public class CastleDatabase : ModelDatabase {
		public CastleDatabase() : base(CastleAttributes.Model) {
			Source = DataSources.Castle;
			AttributeList = CastleAttributes.AttributeList;
			Parser = new CastleParserProvider();
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToImportTable(this),
					new CopyToClipboard(this),
					new CopyToClipboardOther(this)
				);
			};
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					return new CastleViewYaml();
				case FileType.Txt:
					return new CastleViewTxt();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString() ?? "", "Castle ID", true);
					SearchDescriptor.Add(v => ((Castle)v).Name ?? "", "Castle name");
					SearchDescriptor.Add(v => ((Castle)v).Map ?? "", "Map name");
					SearchDescriptor.Add(v => ((Castle)v).Npc ?? "", "NPC");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
				case FileType.Txt:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString() ?? "", "Castle ID", true);
					SearchDescriptor.Add(v => ((Castle)v).Name ?? "", "Castle name");
					SearchDescriptor.Add(v => ((Castle)v).Map ?? "", "Map name");
					SearchDescriptor.Add(v => ((Castle)v).Npc ?? "", "NPC");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}
	}

	public class CastleDatabaseImport : CastleDatabase {
		public CastleDatabaseImport() {
			Source = DataSources.CastleImport;
			ThrowFileNotFoundException = false;
		}
	}
}
