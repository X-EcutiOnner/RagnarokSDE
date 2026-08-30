using SDE.Databases.Generic.SearchDescriptors;
using SDE.Databases.Generic.TabCommands;
using SDE.Databases.Quests.Features;
using SDE.Databases.Quests.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;

namespace SDE.Databases.Quests {
	public class QuestDatabase : ModelDatabase {
		public QuestDatabase() : base(QuestAttributes.Model) {
			Source = DataSources.Quest;
			AttributeList = QuestAttributes.AttributeList;
			Parser = new QuestParserProvider();
			TabGenerator.OnInitSettings += delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.NewItemAddedFunction = delegate(ReadableTuple item) {
					if (ActiveFormat == FileType.Txt) {
						var model = item.GetModel<Quest>();

						model.Targets.Add(new QuestTarget());
						model.Targets.Add(new QuestTarget());
						model.Targets.Add(new QuestTarget());

						model.Drops.Add(new QuestDrop());
						model.Drops.Add(new QuestDrop());
						model.Drops.Add(new QuestDrop());
					}
				};
			};
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToImportTable(this),
					new CopyToClipboard(this),
					new CopyToClipboardOther(this),
					new SelectFromTable(DataSources.ClientQuest)
				);
			};
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					return new QuestViewYaml();
				case FileType.Txt:
					return new QuestViewCsv();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
				case FileType.Txt:
					SearchDescriptor = new SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Quest ID", true);
					SearchDescriptor.Add(v => ((Quest)v).Title, "Title", true);
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}
	}

	public class QuestDatabaseImport : QuestDatabase {
		public QuestDatabaseImport() {
			Source = DataSources.QuestImport;
			ThrowFileNotFoundException = false;
		}
	}
}
