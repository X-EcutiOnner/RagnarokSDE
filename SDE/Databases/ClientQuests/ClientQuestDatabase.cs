using SDE.Databases.ClientQuests.Features;
using SDE.Databases.ClientQuests.Parser;
using SDE.Databases.Generic.TabCommands;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.Parsers;
using System;
using System.IO;
using System.Windows;
using Utilities.Services;

namespace SDE.Databases.ClientQuests {
	public class ClientQuestDatabase : ModelDatabase {
		public ClientQuestDatabase() : base(ClientQuestAttributes.Model) {
			Source = DataSources.ClientQuest;
			AttributeList = ClientQuestAttributes.AttributeList;
			Parser = new ClientQuestParserProvider();
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToClipboard(this),
					new CopyToClipboardOther(this, FileType.Lua),
					new SelectFromTable(DataSources.Quest)
				);
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

			if (context.FileType == FileType.Lua) {
				Parser.Read(context, db);
			}
			else if (context.FileType == FileType.Txt) {
				ClientQuestReaderHelper.LoadDataFromSystem(db, ClientQuestTextFileParsers.Quest, context.FilePath);
			}
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Lua:
					return new ClientQuestViewLua();
				case FileType.Txt:
					return new ClientQuestViewCsv();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Lua:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Quest ID", true);
					SearchDescriptor.Add(v => ((ClientQuest)v).Title ?? "", "Title", true);
					SearchDescriptor.Add(v => ((ClientQuest)v).Summary ?? "", "Summary");
					SearchDescriptor.Add(v => ((ClientQuest)v).Description ?? "", "Description");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
				case FileType.Txt:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Quest ID", true);
					SearchDescriptor.Add(v => ((ClientQuest)v).Title ?? "", "Title", true);
					SearchDescriptor.Add(v => ((ClientQuest)v).SG ?? "", "SG");
					SearchDescriptor.Add(v => ((ClientQuest)v).QUE ?? "", "QUE");
					SearchDescriptor.Add(v => ((ClientQuest)v).Summary ?? "", "Summary");
					SearchDescriptor.Add(v => ((ClientQuest)v).Description ?? "", "Description");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}
	}
}
