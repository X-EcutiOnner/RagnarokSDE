using SDE.Databases.Achievements.Parser;
using SDE.Databases.ClientAchievements.Features;
using SDE.Databases.ClientAchievements.TabCommands;
using SDE.Databases.Generic.TabCommands;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.IO;
using System.Windows;
using Utilities.Services;

namespace SDE.Databases.ClientAchievements {
	public class ClientAchvDatabase : ModelDatabase {
		public ClientAchvDatabase() : base(ClientAchvAttributes.Model) {
			Source = DataSources.ClientAchievement;
			AttributeList = ClientAchvAttributes.AttributeList;
			Parser = new ClientAchvParserProvider();
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToClipboard(this),
					new SelectFromTable(DataSources.Achievement),
					new ClientAchvAutocomplete()
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

			Parser.Read(context, this);
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Lua:
					return new ClientAchvViewLua();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Lua:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.AddTuple(v => ((ReadableTuple)v).Key.ToString(), "Item ID", true);
					SearchDescriptor.Add(v => ((ClientAchv)v).Title ?? "", "Title", true);
					SearchDescriptor.Add(v => ((ClientAchv)v).Summary ?? "", "Summary", true);
					SearchDescriptor.Add(v => ((ClientAchv)v).Details ?? "", "Details", true);
					SearchDescriptor.Add(v => ((ClientAchv)v).RewardTitle ?? "", "Reward title");
					SearchDescriptor.Add(v => ((ClientAchv)v).RewardBuff ?? "", "Reward buff");
					SearchDescriptor.Add(v => ((ClientAchv)v).RewardItem ?? "", "Reward item");
					SearchDescriptor.Add(v => ((ClientAchv)v).Score ?? "", "Score");
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}
	}
}
