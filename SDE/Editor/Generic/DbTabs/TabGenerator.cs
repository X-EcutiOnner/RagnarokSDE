using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Database;
using ErrorManager;
using SDE.Editor.Database;
using SDE.View;
using TokeiLibrary;

namespace SDE.Editor.Generic.DbTabs {
	public class TabGenerator {
		#region Delegates
		public delegate DbTab GDbTabMakerDelegate(ProjectManager sdb, TabControl control, BaseDatabase db);
		public delegate bool TabEnabledDelegate(TabSettings settings, BaseDatabase db);
		public delegate void TabGeneratorDelegate(DbTab tab, TabSettings settings, BaseDatabase db);
		#endregion

		public TabGenerator() {
			GDbTabMaker = _gDbTabMaker;
			SetSettings = _setSettings;
			OnSetCustomCommands = null;
			OnTabVisualUpdate = TgOnTabVisualUpdate;
			IsTabEnabledMethod = IsTabEnabled;
		}

		public GDbTabMakerDelegate GDbTabMaker { get; set; }

		public TabGeneratorDelegate OnInitSettings { get; set; }
		public TabGeneratorDelegate SetSettings { get; set; }
		public TabGeneratorDelegate OnSetCustomCommands { get; set; }
		public TabGeneratorDelegate OnPreviewTabVisualUpdate { get; set; }
		public TabGeneratorDelegate OnTabVisualUpdate { get; set; }
		public TabEnabledDelegate IsTabEnabledMethod { get; set; }

		public static bool IsTabEnabled(TabSettings settings, BaseDatabase db) {
			if (DbPathLocator.DetectPath(db.Source) == null) {
				return false;
			}

			if (!db.IsEnabled) {
				return false;
			}

			if (!Boolean.Parse(ProjectConfiguration.ConfigAsker["[Server database editor - Enabled state - " + db.Source.DisplayName + "]", true.ToString()])) {
				return false;
			}

			return true;
		}

		public static void TgOnTabVisualUpdate(DbTab tab, TabSettings settings, BaseDatabase db) {
			Exception exception = null;

			bool success = tab.Dispatch(delegate {
				try {
					UIElement content = (UIElement)tab.Content; // (UIElement)(tab.Content ?? ((Window)tab.AttachedProperty["AttachedWindow"]).Content);

					if (db.TabGenerator == null || db.TabGenerator.IsTabEnabledMethod == null)
						content.IsEnabled = IsTabEnabled(settings, db);
					else {
						content.IsEnabled = db.TabGenerator.IsTabEnabledMethod(settings, db);
					}
					return true;
				}
				catch (Exception err) {
					exception = err;
					return false;
				}
			});

			if (!success)
				throw exception;

			// Potential SearchDescriptor update?
		}

		private DbTab _gDbTabMaker(ProjectManager sdb, TabControl control, BaseDatabase db) {
			TabSettings settings = new TabSettings(db);
			DbTab tab = new DbTab();
			Table<int, ReadableTuple> table = db.Table;
			settings.Table = table;
			settings.Control = control;

			InitStyle(tab, settings, db);
			InitAttributes(tab, settings, db);
			OnInitSettings?.Invoke(tab, settings, db);

			SdeEditor.Instance.SelectionChanged += (sender, oldTab, newTab) => {
				try {
					TabItem item = newTab;

					if (WpfUtilities.IsTab(item, db.Source)) {
						OnPreviewTabVisualUpdate?.Invoke(tab, settings, db);
						OnTabVisualUpdate?.Invoke(tab, settings, db);
					}
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			};

			SetSettings?.Invoke(tab, settings, db);
			OnSetCustomCommands?.Invoke(tab, settings, db);
			tab.Initialize(settings, db);
			return tab;
		}

		private void _setSettings(DbTab tab, TabSettings settings, BaseDatabase db) {
			if (db.SearchDescriptor != null) {
				if (db.SearchDescriptor.Fields.Count % 2 != 0)
					db.SearchDescriptor.Fields.Add(null);

				settings.SearchEngine.SetSettings(db.SearchDescriptor);
				return;
			}

			List<DbAttribute> attributes = new DbAttribute[] { settings.AttId, settings.AttDisplay }.Concat(db.AttributeList.Attributes.Skip(1).Where(p => p.IsSearchable != null && p != settings.AttId && p != settings.AttDisplay)).ToList();

			if (attributes.Count % 2 != 0) {
				attributes.Add(null);
			}

			settings.SearchEngine.SetAttributes(attributes);
			settings.SearchEngine.SetSettings(settings.AttId, true);
			settings.SearchEngine.SetSettings(settings.AttDisplay, true);

			foreach (DbAttribute attribute in attributes) {
				if (attribute != null && attribute.IsSearchable == true) {
					settings.SearchEngine.SetSettings(attribute, true);
				}
			}
		}

		public void InitAttributes(DbTab tab, TabSettings settings, BaseDatabase db) {
			settings.AttributeList = db.AttributeList;
			settings.AttId = db.AttributeList.PrimaryAttribute;
			settings.AttDisplay = db.AttributeList.Attributes.FirstOrDefault(p => p.IsDisplayAttribute) ?? db.AttributeList.Attributes[1];
		}

		public void InitStyle(DbTab tab, TabSettings settings, BaseDatabase db) {
			TabsMaker.SInit(tab, settings, db);
		}

		public DbTab GenerateTab(ProjectManager sdb, TabControl control, BaseDatabase db) {
			return GDbTabMaker(sdb, control, db);
		}
	}
}