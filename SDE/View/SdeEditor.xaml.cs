using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Database;
using ErrorManager;
using GRF.Core.GroupedGrf;
using GRF.Image;
using GRF.IO;
using GRF.Threading;
using GrfToWpfBridge;
using GrfToWpfBridge.Application;
using Microsoft.Scripting.Utils;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.ClientItems.Features;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.Editor.Database.Commands;
using SDE.Editor.Generic.Parsers.Generic;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.Navigation;
using SDE.Tools.SDEMapcache;
using SDE.View.Controls;
using SDE.View.Dialogs;
using SDE.View.ObjectView;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;
using TokeiLibrary.WpfBugFix;
using Utilities;
using Utilities.CommandLine;
using Utilities.Services;
using Lua;

namespace SDE.View {
	/// <summary>
	/// Interaction logic for CDEditor.xaml
	/// </summary>
	public partial class SdeEditor : TkWindow, IProgress, IErrorListener {
		public readonly List<DbTab> GdTabs = new List<DbTab>();
		internal readonly AsyncOperation _asyncOperation;
		private readonly ProjectManager _sdb;
		private DbHolder _holder;
		private TabNavigation _tabNavigation;
		public static SdeEditor Instance;
		public bool NoErrorsFound { get; set; }
		private EditorPosition _editorPosition = new EditorPosition();
		public int ErrorCount => _errorConsole.ErrorCount;

		public static ProjectManager Project;
		public static MultiGrfReader MetaGrf => Project.MetaGrf;

		public SdeEditor() : base("Server database editor", "cde.ico", SizeToContent.Manual, ResizeMode.CanResize) {
			_parseCommandLineArguments(true);

			SplashDialog loading = new SplashDialog();
			loading.Show();
			Loaded += delegate {
				loading.Terminate();
			};

			try {
				ApplicationShortcut.OverrideBindings(SdeAppConfiguration.Remapper);
			}
			catch (Exception err) {
				SdeAppConfiguration.Remapper.Clear();
				ApplicationShortcut.OverrideBindings(SdeAppConfiguration.Remapper);
				ErrorHandler.HandleException("Failed to load the custom key bindings. The bindings will be reset to their default values.", err);
			}

			string configFile = _parseCommandLineArguments();
			GrfPath.Delete(ProjectConfiguration.DefaultFileName);

			UIElement.IsEnabledProperty.OverrideMetadata(typeof(RangeListView), new UIPropertyMetadata(true, List_IsEnabledChanged, CoerceIsEnabled));
			UIElement.IsEnabledProperty.OverrideMetadata(typeof(ListView), new UIPropertyMetadata(true, List_IsEnabledChanged, CoerceIsEnabled));

			InitializeComponent();
			Instance = this;
			ShowInTaskbar = true;

			_asyncOperation = new AsyncOperation(_progressBar);
			_sdb = new ProjectManager(_metaGrf);
			Project = _sdb;

			_loadMenu();
			
			if (configFile == null) {
				ProjectConfiguration.ConfigAsker = new ConfigAsker(ProjectConfiguration.DefaultFileName);

				if (SdeAppConfiguration.AlwaysReopenLatestProject) {
					if (_recentFilesManager.Files.Count > 0 && File.Exists(_recentFilesManager.Files[0])) {
						ProjectConfiguration.ConfigAsker = new ConfigAsker(configFile = _recentFilesManager.Files[0]);
					}
				}
			}
			else if (File.Exists(configFile)) {
				ProjectConfiguration.ConfigAsker = new ConfigAsker(configFile);
			}

			_loadSettingsTab();
			if (configFile != null) { ReloadSettings(configFile); }
			_loadGenericTab();

			_sdb.Commands.ModifiedStateChanged += _commands_ModifiedStateChanged;

			ApplicationShortcut.Link(SdeCommands.Undo, () => _sdb.Commands.Undo(), this);
			ApplicationShortcut.Link(SdeCommands.UndoGlobal, () => _sdb.Commands.Undo(), this);
			ApplicationShortcut.Link(SdeCommands.Redo, () => _sdb.Commands.Redo(), this);
			ApplicationShortcut.Link(SdeCommands.RedoGlobal, () => _sdb.Commands.Redo(), this);
			ApplicationShortcut.Link(SdeCommands.Search, () => _execute(v => v.Search()), this);
			ApplicationShortcut.Link(SdeCommands.Delete, () => _execute(v => v.Commands.DeleteItems()), this);
			ApplicationShortcut.Link(SdeCommands.Rename, () => _execute(v => v.Commands.ChangeId()), this);
			ApplicationShortcut.Link(SdeCommands.NavigationBackward, () => _tabNavigation.Undo(), this);
			ApplicationShortcut.Link(SdeCommands.NavigationForward, () => _tabNavigation.Redo(), this);
			ApplicationShortcut.Link(SdeCommands.Change, () => _execute(v => v.Commands.ChangeId()), this);
			ApplicationShortcut.Link(SdeCommands.Restrict, () => _execute(v => v.Commands.ShowSelectedOnly()), this);
			ApplicationShortcut.Link(SdeCommands.CopyTo, () => _execute(v => v.Commands.CopyItemTo()), this);
			ApplicationShortcut.Link(SdeCommands.Save, () => _menuItemDatabaseSave_Click(this, null), this);
			ApplicationShortcut.Link(SdeCommands.DbFocusNextEntry, () => _execute(v => v.SelectNext()), this);
			ApplicationShortcut.Link(SdeCommands.DbFocusPreviousEntry, () => _execute(v => v.SelectPrevious()), this);

			ApplicationShortcut.Link(SdeCommands.DbReload, _menuItemReload, this);
			ApplicationShortcut.Link(SdeCommands.Replace, _menuItemReplaceAll, this);
			ApplicationShortcut.Link(SdeCommands.DbCopyAll, _menuItemCopyAll, this);
			ApplicationShortcut.Link(SdeCommands.DbAdd, _menuItemAddItem, this);
			ApplicationShortcut.Link(SdeCommands.DbAddRange, _menuItemAddItemRage, this);
			ApplicationShortcut.Link(SdeCommands.DbAddRaw, _menuItemAddItemRaw, this);
			ApplicationShortcut.Link(SdeCommands.DbChangeId, _menuItemChangeId, this);
			ApplicationShortcut.Link(SdeCommands.DbCopyItemTo, _menuItemCopyItemTo, this);
			ApplicationShortcut.Link(SdeCommands.DbDelete, _menuItemDeleteItem, this);

			PreviewMouseUp += _sdeEditor_PreviewMouseUp;

			Configuration.EnableDebuggerTrace = false;
			
			_tnbUndo.SetUndo(_tabNavigation);
			_tnbRedo.SetRedo(_tabNavigation);
			
			_tmbUndo.SetUndo(_sdb.Commands);
			_tmbRedo.SetRedo(_sdb.Commands);

			DbIOErrorHandler.ClearListeners();
			DbIOErrorHandler.AddListener(this);

			_sdb.PreviewReloaded += delegate {
				this.BeginDispatch(delegate {
					foreach (TabItem tabItem in _mainTabControl.Items) {
						tabItem.IsEnabled = true;

						var tabItemHeader = tabItem.Header as DisplayLabel;
						tabItemHeader?.ResetEnabled();
					}
				});
			};

			_sdb.Reloaded += delegate {
				_mainTabControl.Dispatch(p => p.RaiseEvent(new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), _mainTabControl.SelectedItem == null ? new List<object>() : new List<object> { _mainTabControl.SelectedItem })));
				//ServerType serverType = DbPathLocator.GetServerType();
				//bool renewal = DbPathLocator.GetIsRenewal();
				//string header = String.Format("Current ({0} - {1})", serverType == ServerType.RAthena ? "rA" : "Herc", renewal ? "Renewal" : "Pre-Renewal");

				//this.BeginDispatch(delegate {
				//	_menuItemExportDbCurrent.IsEnabled = true;
				//	_menuItemExportDbCurrent.Header = header;
				//
				//	_menuItemExportSqlCurrent.IsEnabled = true;
				//	_menuItemExportSqlCurrent.Header = header;
				//});
			};

			SelectionChanged += _sdeEditor_SelectionChanged;

			_editorPosition.Load(this);
		}

		private void _sdeEditor_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.XButton1) {
				ApplicationShortcut.ExecuteCommand(SdeCommands.NavigationBackward, this);
			}
			else if (e.ChangedButton == MouseButton.XButton2) {
				ApplicationShortcut.ExecuteCommand(SdeCommands.NavigationForward, this);
			}
		}

		private static WeakDictionary<ListView, bool> _fixed = new WeakDictionary<ListView, bool>();
		private static void List_IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var lv = d as ListView;

			if (lv != null) {
				if (_fixed.ContainsKey(lv))
					return;

				_fixed[lv] = true;
				Core.Extensions.FixDarkThemeListView(lv);
			}
		}

		private static object CoerceIsEnabled(DependencyObject d, object basevalue) {
			return basevalue;
		}

		public AsyncOperation AsyncOperation => _asyncOperation;
		public List<DbTab> Tabs => GdTabs;

		#region IErrorListener Members

		public void Handle(Exception err, string exception) {
			Handle(err, exception, ErrorLevel.Warning);
		}

		public void Handle(Exception err, string exception, ErrorLevel errorLevel) {
			Dispatcher.Invoke(new Action(delegate {
				if (_mainTabControl.SelectedIndex != 1 && ((TabItem)_mainTabControl.Items[1]).Header.ToString() != "Error console *")
					((TabItem)_mainTabControl.Items[1]).Header = new DisplayLabel { DisplayText = "Error console *", FontWeight = FontWeights.Bold };

				_errorConsole.AddError(err, exception, errorLevel);
			}), DispatcherPriority.Background);
		}

		#endregion

		#region IProgress Members

		public float Progress { get; set; }
		public bool IsCancelling { get; set; }
		public bool IsCancelled { get; set; }
		public void CancelOperation() {
			IsCancelling = true;
		}

		#endregion

		private string _parseCommandLineArguments(bool load = false) {
			List<GenericCLOption> options = CommandLineParser.GetOptions(Environment.CommandLine, false);

			foreach (GenericCLOption option in options) {
				if (!load) {
					if (option.CommandName == "-REM" || option.CommandName == "REM") {
						break;
					}
					if (option.Args.Count <= 0) {
						continue;
					}
					else if (option.Args[0].EndsWith(".sde")) {
						return options[0].Args[0];
					}
				}
				else {
					if (option.CommandName == "-mapcache" || option.CommandName == "mapcache") {
						new MapcacheDialog(option.Args.Count > 0 ? option.Args[0] : null).ShowDialog();
						ApplicationManager.Shutdown();
						break;
					}
				}
			}

			return null;
		}

		private void _commands_ModifiedStateChanged(object sender, IGenericDbCommand command) {
			_setTitle(Methods.CutFileName(ProjectConfiguration.ConfigAsker.ConfigFile), _sdb.IsModified);
		}

		private void _setTitle(string name, bool isModified) {
			this.BeginDispatch(() => {
				Title = "Server database editor" + (String.IsNullOrEmpty(name) ? "" : " - " + name) + (isModified ? " *" : "");
			});
		}

		private void _loadGenericTab() {
			try {
				ProjectConfiguration.ConfigAsker.IsAutomaticSaveEnabled = false;

				_holder = new DbHolder();
				_holder.Instantiate(_sdb);
				GdTabs.AddRange(_holder.GetTabs(_mainTabControl));

				foreach (var tab in _sdb.AllTables) {
					if (tab.Value.DoNotLoadInEditor)
						continue;

					var copy = tab.Value;

					BaseDatabase db = copy;
					db.Table.Commands.CommandIndexChanged += (e, a) => UpdateTabHeader(db);
					db.Table.Commands.ModifiedStateChanged += (e, a) => UpdateTabHeader(db);
				}

				foreach (var tab in GdTabs) {
					var copy = tab;
					copy._listView.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args) {
						if (sender is ListView) {
							ListView view = (ListView)sender;
							_tabNavigation.StoreAndExecute(new SelectionChangedCommand(copy.Header.ToString(), view.SelectedItem, view, copy));
						}
					};
				}

				foreach (DbTab tab in GdTabs) {
					DbTab tabCopy = tab;
					_mainTabControl.Items.Insert(_mainTabControl.Items.Count, tabCopy);
				}
			}
			finally {
				ProjectConfiguration.ConfigAsker.IsAutomaticSaveEnabled = true;
			}
		}

		public void UpdateTabHeader(BaseDatabase db) {
			Table<int, ReadableTuple> table = db.Table;

			if (table != null) {
				string header = db.Source.IsImport ? "imp" : db.Source.DisplayName;

				if (table.Commands.IsModified) {
					header += " *";
				}

				this.BeginDispatch(delegate {
					var gdt = _mainTabControl.Items.OfType<DbTab>().FirstOrDefault(p => p.Header.ToString() == db.Source.UidName);

					if (gdt != null) {
						((DisplayLabel) gdt.Header).Text = header;
					}
				});
			}
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) { }

		public bool DisableSelectionChangedEvents { get; set; }

		private void _sdeEditor_SelectionChanged(object sender, TabItem olditem, TabItem newitem) {
			if (DisableSelectionChangedEvents)
				return;

			if (newitem == null) {
				NoErrorsFound = true;
				return;
			}

			if (newitem == olditem) {
				NoErrorsFound = true;
				return;
			}

			bool isOldErrorConsole = WpfUtilities.IsTab(olditem, "Error console *") || WpfUtilities.IsTab(olditem, "Error console");
			bool isCurrentErrorConsole = WpfUtilities.IsTab(newitem, "Error console *") || WpfUtilities.IsTab(newitem, "Error console");

			if (_delayedReloadDatabase && (WpfUtilities.IsTab(olditem, "Settings") || isOldErrorConsole) &&
				(!isCurrentErrorConsole && !WpfUtilities.IsTab(newitem, "Settings"))) {
				if (!ReloadDatabase()) {
					_mainTabControl.SelectedIndex = 0;
				}

				NoErrorsFound = false;
				return;
			}

			if (WpfUtilities.IsTab(newitem, "Error console *")) {
				_mainTabControl.Dispatch(p => ((TabItem)_mainTabControl.Items[1]).Header = new DisplayLabel { DisplayText = "Error console", FontWeight = FontWeights.Bold });
				NoErrorsFound = false;
				return;
			}

			NoErrorsFound = true;
		}

		private void _mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e == null || e.RemovedItems.Count <= 0 || e.RemovedItems[0] as TabItem == null || (e.AddedItems.Count > 0 && e.AddedItems[0] as TabItem == null))
				return;

			OnSelectionChanged(e.RemovedItems[0] as TabItem, _mainTabControl.SelectedItem as TabItem);
		}

		public delegate void SdeSelectionChangedEventHandler(object sender, TabItem oldItem, TabItem newItem);
		public event SdeSelectionChangedEventHandler SelectionChanged;

		public void OnSelectionChanged() {
			SdeSelectionChangedEventHandler handler = SelectionChanged;
			TabItem olditem = _mainTabControl.SelectedItem as TabItem;
			TabItem newitem = _mainTabControl.SelectedItem as TabItem;
			handler?.Invoke(this, olditem, newitem);
		}

		public void OnSelectionChanged(TabItem olditem, TabItem newitem) {
			SdeSelectionChangedEventHandler handler = SelectionChanged;
			olditem = olditem ?? _mainTabControl.SelectedItem as TabItem;
			newitem = newitem ?? _mainTabControl.SelectedItem as TabItem;
			if (ReferenceEquals(olditem, newitem)) return;
			handler?.Invoke(this, olditem, newitem);
		}

		public void Update() {
			_execute(v => v.Update());
		}

		public DbTab FindTopmostTab() {
			var window = WpfUtilities.TopWindow;
			if (window == null) return null;

			DbTab tab = null;

			if (window.Tag is DbTab) {
				return window.Tag as DbTab;
			}

			if ((_mainTabControl.SelectedIndex >= 0 && _mainTabControl.Items[_mainTabControl.SelectedIndex] is DbTab) || (tab != null)) {
				return (DbTab)_mainTabControl.Items[_mainTabControl.SelectedIndex];
			}

			return null;
		}

		private void _execute(Action<DbTab> func) {
			var window = WpfUtilities.TopWindow;
			if (window == null) return;

			DbTab tab = null;

			if (window.Tag is DbTab) {
				tab = window.Tag as DbTab;
			}

			if ((_mainTabControl.SelectedIndex >= 0 && _mainTabControl.Items[_mainTabControl.SelectedIndex] is DbTab) || (tab != null)) {
				tab = tab ?? (DbTab)_mainTabControl.Items[_mainTabControl.SelectedIndex];

				try {
					func(tab);
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			}
		}

		private bool _isClientSyncConvert() {
			return ProjectConfiguration.SynchronizeWithClientDatabases;
		}

		private void _exportImages(string grfpath, int mode) {
			_execute(tab => {
				// 
				if (tab.Database.Source != DataSources.ClientItem)
					throw new Exception("This feature can only be used on the Client Items tab.");

				string extractionPath = PathRequest.FolderExtractDb();

				if (extractionPath == null)
					return;

				var selector = (tab.ListView.SelectedItems.Count > 0 ? tab.ListView.SelectedItems : tab.ListView.Items).Cast<ReadableTuple>().ToList();
				Exception exception = null;
				
				foreach (var tuple in selector) {
					var resourceName = tuple.GetModel<ClientItem>().IdentifiedResourceName ?? "";
					var resourcePath = GrfPath.Combine(grfpath, resourceName + ".bmp");
					var data = Project.MetaGrf.GetData(resourcePath);
				
					if (data != null) {
						try {
							int id = tuple.Key;

							GrfImage image = new GrfImage(data);
							image.MakeFirstPixelTransparent();
							
							if (mode == 0) {
								image.MakePinkShadeTransparent();
							}
							
							image.Convert(GrfImageType.Bgra32);
							image.Save(GrfPath.Combine(extractionPath, id + ".png"));
						}
						catch (Exception err) {
							exception = new Exception("Failed to decompress image for item id: " + tuple.Key, err);
						}
					}
				}

				GC.Collect();

				if (exception != null)
					throw exception;
			});
		}

		private void _menuItemInventoryExport_Click(object sender, RoutedEventArgs e) {
			_exportImages(@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\item\", 0);
		}

		private void _menuItemIllustrationExport_Click(object sender, RoutedEventArgs e) {
			_exportImages(@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\collection\", 1);
		}
	}
}
