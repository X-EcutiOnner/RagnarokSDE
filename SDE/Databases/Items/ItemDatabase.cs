using Database;
using SDE.Databases.ClientItemResources;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.Generic.TabCommands;
using SDE.Databases.Items.Features;
using SDE.Databases.Items.Parser;
using SDE.Databases.Items.TabCommands;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.LuaTables;
using SDE.View;
using System;
using System.Windows;

namespace SDE.Databases.Items {
	public class ItemDatabase : ModelDatabase {
		public ItemDatabase() : base(ItemAttributes.Model) {
			Source = DataSources.Item;
			AttributeList = ItemAttributes.AttributeList;
			Parser = new ItemParserProvider();
			TabGenerator.OnInitSettings += (tab, settings, db) => {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToImportTable(this),
					new CopyToClipboard(this),
					new CopyToClipboardOther(this),
					new SelectFromTable(DataSources.ClientItem),
					new ClientItemAutocomplete()
				);

				Table<int, ReadableTuple> cDb = null;
				Table<int, ReadableTuple> citemsDb = null;

				settings.SearchEngine.SetupImageDataGetter = delegate (Database.Tuple tuple) {
					if (cDb == null) {
						cDb = SdeEditor.Project.GetTable(DataSources.ClientResourceDb);
						citemsDb = SdeEditor.Project.GetTable(DataSources.ClientItem);
					}

					if (cDb == null || citemsDb == null)
						return null;

					int id = tuple.GetKey<int>();

					var clientTuple = citemsDb.TryGetTuple(id);

					if (clientTuple != null)
						return Core.Extensions.GetImage(@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\item\", (clientTuple.GetModel<ClientItem>().IdentifiedResourceName ?? "") + ".bmp");

					if (!cDb.ContainsKey(id))
						return null;

					return Core.Extensions.GetImage(@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\item\", cDb.GetTuple(id).GetValue<string>(ClientItemResourceAttributes.ResourceName) + ".bmp");
				};
			};

			WeakEventManager<BaseDatabase, EventArgs>.AddHandler(this, nameof(TableLoaded), OnTableModified);
			WeakEventManager<BaseDatabase, EventArgs>.AddHandler(this, nameof(TableModified), OnTableModified);
		}

		public void OnTableModified(object sender, EventArgs args) {
			CachedDbs.AegisNameItem.Dirty();
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					return new ItemViewYaml();
				case FileType.Txt:
					return new ItemViewCsv();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString() ?? "", "Item ID", true);
					SearchDescriptor.Add(v => ((Item)v).Name ?? "", "Name", true);
					SearchDescriptor.Add(v => ((Item)v).AegisName ?? "", "Aegis name", true);
					SearchDescriptor.AddSpacer();
					SearchDescriptor.Add(v => ((Item)v).Jobs.ToString(), "Applicable job");
					SearchDescriptor.Add(v => ((Item)v).Script ?? "", "Script");
					SearchDescriptor.Add(v => ((Item)v).EquipScript ?? "", "On equip script");
					SearchDescriptor.Add(v => ((Item)v).UnEquipScript ?? "", "On unequip script");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
				case FileType.Txt:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString() ?? "", "Item ID", true);
					SearchDescriptor.Add(v => ((Item)v).Name ?? "", "Name", true);
					SearchDescriptor.Add(v => ((Item)v).AegisName ?? "", "Aegis name", true);
					SearchDescriptor.AddSpacer();
					SearchDescriptor.Add(v => ((Item)v).Jobs.ToString(), "Applicable job");
					SearchDescriptor.Add(v => ((Item)v).Script ?? "", "Script");
					SearchDescriptor.Add(v => ((Item)v).EquipScript ?? "", "On equip script");
					SearchDescriptor.Add(v => ((Item)v).UnEquipScript ?? "", "On unequip script");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}

		public override void WriteDb(string dbPath, string subPath, FileType fileType = FileType.Detect) {
			LuaHelper.WriteViewIds(Source, this);
			base.WriteDb(dbPath, subPath, fileType);
		}
	}

	public class ItemDatabaseImport : ItemDatabase {
		public ItemDatabaseImport() {
			Source = DataSources.ItemImport;
			ThrowFileNotFoundException = false;
		}
	}
}
