using Database;
using ErrorManager;
using SDE.Databases.Generic.SearchDescriptors;
using SDE.Databases.Generic.TabCommands;
using SDE.Databases.ItemCombos.Features;
using SDE.Databases.ItemCombos.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;
using TokeiLibrary.WPF;

namespace SDE.Databases.ItemCombos {
	public class ItemComboDatabase : ModelDatabase {
		public ItemComboDatabase() : base(ItemComboAttributes.Model) {
			Source = DataSources.ItemCombo;
			AttributeList = ItemComboAttributes.AttributeList;
			Parser = new ItemComboParserProvider();
			TabGenerator.OnInitSettings += delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.CustomAddItemMethod = delegate {
					try {
						int uid = Table.GenerateUniqueId();

						ReadableTuple item = new ReadableTuple(uid, settings.AttributeList);
						item.Added = true;
						var model = item.GetModel<ItemCombo>();

						for (int i = model.NameIds.Count; i < ItemCombo.MaxNameIdCount; i++) {
							model.NameIds.Add(new NameId());
						}

						Table.Commands.AddTuple(uid, item, false);
						tab.ListView.ScrollToCenterOfView(item);
					}
					catch (KeyInvalidException) {
					}
					catch (Exception err) {
						ErrorHandler.HandleException(err);
					}
				};
			};
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.HasUniqueId = true;
				settings.RemoveCommand(TabCommandAnchors.ChangeId);
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToImportTable(this),
					new CopyToClipboard(this),
					new CopyToClipboardOther(this)
				);
			};
			TabGenerator.OnInitSettings += (tab, settings, db) => {
				settings.AttIdWidth = 80;
				settings.AttDisplayWrap = TextWrapping.NoWrap;
				settings.AttId = ItemComboAttributes.DisplayId;
				settings.AttDisplay = ItemComboAttributes.DisplayName2;
			};
			UseUniqueId = true;
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
				case FileType.Txt:
					return new ItemComboViewYaml();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Yaml:
				case FileType.Txt:
					SearchDescriptor = new SearchDescriptor();
					SearchDescriptor.Add(v => ((ItemCombo)v).DisplayNameIds ?? "", "Combo ID", true);
					SearchDescriptor.Add(v => ((ItemCombo)v).DisplayNames ?? "", "Combo names", true);
					SearchDescriptor.Add(v => ((ItemCombo)v).Script ?? "", "Script", true);
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}
	}

	public class ItemComboDatabaseImport : ItemComboDatabase {
		public ItemComboDatabaseImport() {
			Source = DataSources.ItemComboImport;
			ThrowFileNotFoundException = false;
		}
	}
}
