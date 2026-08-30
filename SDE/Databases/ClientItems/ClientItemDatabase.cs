using SDE.Databases.ClientItems.Features;
using SDE.Databases.ClientItems.Parser;
using SDE.Databases.ClientItems.TabCommands;
using SDE.Databases.Generic.TabCommands;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.Parsers;
using System;
using System.IO;
using System.Windows;
using Utilities.Services;

namespace SDE.Databases.ClientItems {
	public class ClientItemDatabase : ModelDatabase {
		public ClientItemDatabase() : base(ClientItemAttributes.Model) {
			Source = DataSources.ClientItem;
			AttributeList = ClientItemAttributes.AttributeList;
			Parser = new ClientItemParserProvider();
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToClipboardLua(),
					new SelectFromTable(DataSources.Item),
					new ClientItemAutocomplete()
				);
			};
			TabGenerator.OnInitSettings += (tab, settings, db) => {
				settings.SearchEngine.SetupImageDataGetter = delegate (Database.Tuple tuple) {
					return Core.Extensions.GetImage(@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\item\", (tuple.GetModel<ClientItem>().IdentifiedResourceName ?? "") + ".bmp");
				};
			};
		}

		public override void OnLoadDataFromClipboard(DbLoadContext context, string text, string path, BaseDatabase db) {
			// The text comes from UTF-8 (clipboard), needs to be converted back to its proper encoding.
			if (EncodingService.Ansi.GetString(EncodingService.Ansi.GetBytes(text)) == text) {
				File.WriteAllText(path, text, EncodingService.Ansi);
			}
			if (EncodingService.Korean.GetString(EncodingService.Korean.GetBytes(text)) == text) {
				File.WriteAllText(path, text, EncodingService.Korean);
			}

			var reader = new ClientItemReaderLua();
			reader.LoadFile(this, path);
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Lua:
				case FileType.Txt:
					return new ClientItemViewLua();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Lua:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Item ID", true);
					SearchDescriptor.AddSpacer();
					SearchDescriptor.Add(v => ((ClientItem)v).IdentifiedDisplayName ?? "", "Id. display name", true);
					SearchDescriptor.Add(v => ((ClientItem)v).UnidentifiedDisplayName ?? "", "Un. display name");
					SearchDescriptor.Add(v => ((ClientItem)v).IdentifiedResourceName ?? "", "Id. resource name");
					SearchDescriptor.Add(v => ((ClientItem)v).UnidentifiedResourceName ?? "", "Un. resource name");
					SearchDescriptor.Add(v => ((ClientItem)v).IdentifiedDescription ?? "", "Id. description");
					SearchDescriptor.Add(v => ((ClientItem)v).UnidentifiedDescription ?? "", "Un. description");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}
	}
}
