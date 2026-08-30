using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErrorManager;
using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Files;
using SDE.View;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities;
using Utilities.Services;

namespace SDE.Editor.Generic.DbTabs {
	/// <summary>
	/// Utility class to help generate tabs
	/// </summary>
	public static class TabsMaker {
		public const int MinOptions = 3;

		public static void SInit(DbTab tab, TabSettings settings, BaseDatabase db) {
			if (db.Source.IsImport) {
				settings.Style = "DatabaseTabImportStyle";
			}
			else {
				if (db.Source.ImportTable != null && db.Source.ImportTable.IsImport)
					settings.Style = "DatabaseTabLeftImportStyle";
				else
					settings.Style = "DatabaseTabStyle";
			}

			settings.ContextMenu = new ContextMenu();
			var menuItem = new MenuItem { Header = "Select '" + db.Source.UidName.Replace("_", "__") + "' in explorer", Icon = new Image { Source = ApplicationManager.PreloadResourceImage("arrowdown.png") } };

			menuItem.Click += delegate {
				if (db.Source != null) {
					try {
						TkPath path = DbPathLocator.DetectPath(db.Source);

						if (path != null) {
							if (path.IsFile)
								OpeningService.FilesOrFolders(path.FilePath);
							else
								ErrorHandler.HandleException("The file cannot be opened because it is not stored locally.");
						}
						else
							ErrorHandler.HandleException("File not found.");
					}
					catch (Exception err) {
						ErrorHandler.HandleException(err);
					}
				}
				else {
					ErrorHandler.HandleException("File not found.");
				}
			};

			settings.ContextMenu.Items.Add(menuItem);

			settings.Loaded += _loaded;

			if (tab == null || db == null)
				return;

			tab.Visibility = settings.Visibility;

			if (db.AttributeList.Attributes.Any(p => p.IsSkippable)) {
				foreach (var attributeIntern in db.AttributeList.Attributes.Where(p => p.IsSkippable)) {
					var attribute = attributeIntern;
					var menuItemSkippable = new MenuItem { Header = attribute.DisplayName + " [" + attribute.AttributeName + ", " + attribute.Index + "]", Icon = new Image { Source = ApplicationManager.PreloadResourceImage("add.png") } };
					menuItemSkippable.IsEnabled = false;
					menuItemSkippable.Click += delegate {
						db.Attached["EntireRewrite"] = true;
						db.Attached[attribute.DisplayName] = db.Attached[attribute.DisplayName] != null && !(bool)db.Attached[attribute.DisplayName];
						db.TabGenerator.OnTabVisualUpdate(tab, settings, db);
					};
					settings.ContextMenu.Items.Add(menuItemSkippable);
				}

				db.Attached.CollectionChanged += delegate {
					int index = MinOptions;

					foreach (var attributeIntern in db.AttributeList.Attributes.Where(p => p.IsSkippable)) {
						var attribute = attributeIntern;
						int index1 = index;
						settings.ContextMenu.Dispatch(delegate {
							var menuItemSkippable = (MenuItem)settings.ContextMenu.Items[index1];
							menuItemSkippable.IsEnabled = true;
							bool isSet = db.Attached[attribute.DisplayName] == null || (bool)db.Attached[attribute.DisplayName];

							menuItemSkippable.Icon = new Image { Source = ApplicationManager.PreloadResourceImage(isSet ? "delete.png" : "add.png") };
						});

						index++;
					}
				};
			}
		}

		private static void _loaded(DbTab tab, TabSettings settings, BaseDatabase db) {
			string property = "[Server database editor - Enabled state - " + db.Source.DisplayName + "]";

			Func<bool> getConfig = () => Boolean.Parse(ProjectConfiguration.ConfigAsker[property, true.ToString()]);
			Action<bool> setConfig = v => ProjectConfiguration.ConfigAsker[property] = v.ToString();
			Func<string> getHeader = () => getConfig() ? "Disable" : "Enable";
			Func<string> getFullHeader = () => String.Format("{0} '{1}'", getHeader(), db.Source.UidName.Replace("_", "__"));
			Func<Image> getIcon = () => getConfig() ? new Image { Source = ApplicationManager.PreloadResourceImage("error16.png") } : new Image { Source = ApplicationManager.PreloadResourceImage("validity.png") };

			var menuItem = new MenuItem { Header = getFullHeader(), Icon = getIcon() };
			menuItem.IsEnabled = false;

			menuItem.Click += delegate {
				if (db.Source != null) {
					try {
						setConfig(!getConfig());
						db.IsEnabled = getConfig();
						TabGenerator.TgOnTabVisualUpdate(tab, settings, db);

						menuItem.Dispatch(delegate {
							menuItem.Header = getFullHeader();
							menuItem.Icon = getIcon();
						});
					}
					catch (Exception err) {
						ErrorHandler.HandleException(err);
					}
				}
				else {
					ErrorHandler.HandleException("File not found.");
				}
			};

			SdeEditor.Project.Reloaded += delegate {
				menuItem.Dispatch(delegate {
					menuItem.IsEnabled = true;

					if (!getConfig()) {
						db.IsEnabled = false;
					}

					menuItem.Header = getFullHeader();
					menuItem.Icon = getIcon();
				});
			};

			settings.ContextMenu.Items.Insert(1, menuItem);

			var menuItem2 = new MenuItem { Header = "Detach", Icon = new Image { Source = ApplicationManager.PreloadResourceImage("convert.png") } };
			menuItem2.Click += delegate {
				try {
					if (menuItem2.Header.ToString() != "Detach") {
						menuItem2.Header = "Detach";
						((TkWindow)tab.AttachedProperty["AttachedWindow"]).Close();
						tab.AttachedProperty["AttachedWindow"] = null;
						return;
					}

					menuItem2.Header = "Reattach";

					TkWindow window = new TkWindow(db.Source.DisplayName, "properties.png", SizeToContent.Manual, ResizeMode.CanResize);
					window.Tag = tab;
					window.ShowInTaskbar = true;
					//window.Owner = WpfUtilities.TopWindow;

					tab.AttachedProperty["AttachedWindow"] = window;

					window.Content = tab.Content;
					window.KeyDown += delegate(object sender, KeyEventArgs e) { tab.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(tab), 0, e.Key) { RoutedEvent = UIElement.KeyDownEvent }); };

					var sde = WpfUtilities.FindParentControl<SdeEditor>(tab);

					EventHandler handler = delegate {
						var oldTab = sde._mainTabControl.SelectedItem;
						sde.OnSelectionChanged(tab, null);
						var tab2 = sde.FindTopmostTab();
						if (tab2 != null) {
							if (oldTab != tab2)
								tab2.TabChanged();
						}
					};
					sde.Activated += handler;

					window.Activated += delegate {
						sde.OnSelectionChanged(null, tab);

						if (sde.NoErrorsFound) {
							//sde.DisableSelectionChangedEvents = true;
							tab.TabChanged();
							//sde.DisableSelectionChangedEvents = false;
						}
						else {
							if (sde.FindTopmostTab() == tab) {
								return;
							}

							sde.Activate();
						}
					};

					tab.Content = null;
					window.Closed += delegate {
						tab.Content = ((TkWindow)tab.AttachedProperty["AttachedWindow"]).Content;
						tab.AttachedProperty["AttachedWindow"] = null;
						menuItem2.Header = "Detach";
						sde.Activated -= handler;
					};
					window.Show();
					window.Activate();
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			};

			settings.ContextMenu.Items.Insert(2, menuItem2);
		}

		public static void SelectInNotepadpp(string filePath, string line) {
			try {
				if (String.IsNullOrEmpty(SdeAppConfiguration.NotepadPath))
					Process.Start("notepad++.exe", "\"" + filePath + "\" -n" + line);
				else {
					Process.Start(String.Format("\"{0}\"", SdeAppConfiguration.NotepadPath), "\"" + filePath + "\" -n" + line);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}