using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Database;
using Database.Commands;
using ErrorManager;
using GRF.GrfSystem;
using Microsoft.Scripting.Utils;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Navigation;
using SDE.Editor.SearchFeature;
using SDE.View;
using SDE.View.Controls;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles.ListView;
using TokeiLibrary.WpfBugFix;
using Utilities;

namespace SDE.Editor.Generic.DbTabs {
	/// <summary>
	/// Interaction logic for DbTab.xaml
	/// </summary>
	public partial class DbTab : TabItem {
		public static ReadableTuple LastSelectedTuple;
		public DbAttribute DisplayAttribute => Settings.AttDisplay;
		public DbAttribute IdAttribute => Settings.AttId;
		public BaseDatabase Database { get; set; }
		public SearchEngine SearchEngine { get; private set; }
		public Table<int, ReadableTuple> Table { get; set; }
		public TkDictionary<string, object> AttachedProperty = new TkDictionary<string, object>();
		public TabSettings Settings { get; private set; }
		public Grid PropertiesGrid => _displayGrid;
		public ListView List => _listView;
		internal Grid _displayGrid = new Grid();
		public bool IsDetached => AttachedProperty["AttachedWindow"] != null;
		public Action<ReadableTuple> UpdateAction;
		public RangeListView ListView => _listView;
		public bool DelayedReload { get; set; }
		public ReadableTuple SelectedItem => _listView.SelectedItem as ReadableTuple;
		private UpdateDispatcher _updateDispatcher = new UpdateDispatcher(50);
		private readonly object _lock = new object();
		private ReadableTuple _currentSelectedItem;

		public DbTabDatabaseEventManager EventManager;
		public DbTabCommandManager Commands;
		public DbTabMenuManager MenuManager;

		public new object Content {
			get { return base.Content ?? ((Window)AttachedProperty["AttachedWindow"]).Content; }
			set { base.Content = value; }
		}

		public new bool IsSelected {
			get {
				if (IsDetached)
					return ((Window)AttachedProperty["AttachedWindow"]).IsActive;
				return base.IsSelected;
			}
			set {
				if (IsDetached)
					((Window)AttachedProperty["AttachedWindow"]).Activate();
				else
					base.IsSelected = value;
			}
		}

		public bool IsFiltering {
			get => SearchEngine.IsFiltering;
			set => IsFiltering = value;
		}

		public DbTab() {
			InitializeComponent();
		}

		public void Initialize(TabSettings settings, BaseDatabase db) {
			var database = SdeEditor.Project;
			Database = db;
			Settings = settings;

			_displayGrid = new Grid();
			_displayGrid.SetValue(Grid.ColumnProperty, 2);

			if (Database.Source == DataSources.ClientItem ||
				Database.Source == DataSources.Pet ||
				Database.Source == DataSources.PetImport ||
				Database.Source == DataSources.Mob ||
				Database.Source == DataSources.MobImport ||
				Database.Source == DataSources.ItemCombo ||
				Database.Source == DataSources.ItemComboImport ||
				Database.Source == DataSources.Item ||
				Database.Source == DataSources.ItemImport) {
				_viewGrid.Children.Add(_displayGrid);
			}
			else {
				ScrollViewer sv = new ScrollViewer();
				sv.SetValue(Grid.ColumnProperty, 2);
				sv.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
				sv.Focusable = false;
				sv.Content = _displayGrid;
				_viewGrid.Children.Add(sv);
			}

			Table = Settings.Table;
			Header = Settings.TabName;
			Style = TryFindResource(settings.Style) as Style ?? Style;
			SearchEngine = settings.SearchEngine;
			SearchEngine.Init(_dbSearchPanel, this);

			if (Settings.SearchEngine.SetupImageDataGetter != null) {
				ListViewDataTemplateHelper.GenerateListViewTemplateNew(_listView, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
					new ListViewDataTemplateHelper.GeneralColumnInfo { Header = Settings.AttId.DisplayName, DisplayExpression = "[" + Settings.AttId.Index + "]", SearchGetAccessor = Settings.AttId.AttributeName, FixedWidth = Settings.AttIdWidth, TextAlignment = TextAlignment.Right, ToolTipBinding = "[" + Settings.AttId.Index + "]" },
					//new ListViewDataTemplateHelper.ImageColumnInfo { Header = "", DisplayExpression = "DataImage, IsAsync=True", SearchGetAccessor = Settings.AttId.AttributeName, FixedWidth = 26, MaxHeight = 24, MinHeight = 24 },
					new ListViewDataTemplateHelper.ImageColumnInfo { Header = "", DisplayExpression = "DataImage", SearchGetAccessor = Settings.AttId.AttributeName, FixedWidth = 26, MaxHeight = 24 },
					new ListViewDataTemplateHelper.RangeColumnInfo { Header = Settings.AttDisplay.DisplayName, DisplayExpression = "[" + Settings.AttDisplay.Index + "]", SearchGetAccessor = Settings.AttDisplay.AttributeName, IsFill = true, ToolTipBinding = "[" + Settings.AttDisplay.Index + "]", MinWidth = 100, TextWrapping = Settings.AttDisplayWrap }
				}, new DatabaseItemSorter(Settings.AttributeList), null, defaultBrushOverride: "{Binding ForegroundBrush}");
			}
			else {
				ListViewDataTemplateHelper.GenerateListViewTemplateNew(_listView, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
					new ListViewDataTemplateHelper.GeneralColumnInfo { Header = Settings.AttId.DisplayName, DisplayExpression = "[" + Settings.AttId.Index + "]", SearchGetAccessor = Settings.AttId.AttributeName, FixedWidth = Settings.AttIdWidth, TextAlignment = TextAlignment.Right, ToolTipBinding = "[" + Settings.AttId.Index + "]" },
					new ListViewDataTemplateHelper.RangeColumnInfo { Header = Settings.AttDisplay.DisplayName, DisplayExpression = "[" + Settings.AttDisplay.Index + "]", SearchGetAccessor = Settings.AttDisplay.AttributeName, IsFill = true, ToolTipBinding = "[" + Settings.AttDisplay.Index + "]", MinWidth = 100, TextWrapping = Settings.AttDisplayWrap }
				}, new DatabaseItemSorter(Settings.AttributeList), null, defaultBrushOverride: "{Binding ForegroundBrush}");
			}

			EventManager = new DbTabDatabaseEventManager(this);
			EventManager.Subscribe(Table);
			MenuManager = new DbTabMenuManager(this);

			if (Settings.ContextMenu != null) {
				if (Header is DisplayLabel label)
					label.ContextMenu = Settings.ContextMenu;
			}

			Settings.Loaded?.Invoke(this, Settings, database.GetDb(Database.Source));

			Loaded += delegate {
				TabControl parent = WpfUtilities.FindParentControl<TabControl>(this);

				if (parent != null) {
					parent.SelectionChanged += _parent_SelectionChanged;
				}
			};

			_listView.PreviewMouseDown += delegate { _listView.Focus(); };

			_listView.Loaded += delegate {
				try {
					if (IsVisible) {
						Keyboard.Focus(_listView);
					}
				}
				catch {
				}
			};

			ApplicationShortcut.Link(SdeCommands.Paste, () => ImportFromFile("clipboard"), _listView);
			ApplicationShortcut.Link(SdeCommands.AdvancedPaste, () => ImportFromFile("clipboard", true), this);
			ApplicationShortcut.Link(SdeCommands.AdvancedPaste2, () => ImportFromFile("clipboard", true), this);
			ApplicationShortcut.Link(SdeCommands.DbSearchNextEmptyEntry, () => {
				if (_listView.SelectedItems.Count > 0) {
					ReadableTuple item = _listView.SelectedItems[_listView.SelectedItems.Count - 1] as ReadableTuple;
					var original = item;

					if (item != null) {
						int id = item.Key;

						while (true) {
							id++;
							var idGeneric = id;

							var tuple = Table.TryGetTuple(idGeneric);

							if (tuple == null && item != original && item != null) {
								TabNavigation.SelectList(Database.Source, new int[] { item.Key });
								break;
							}

							item = tuple;
						}
					}
				}
			}, _listView);

			Commands = new DbTabCommandManager(this);
		}

		public void Update() {
			SafeExecute(delegate {
				ReadableTuple item = SelectedItem;

				_updateDispatcher.Execute(delegate {
					this.Dispatcher.BeginInvoke(new Action(delegate {
						Show(item);
					}), DispatcherPriority.Render);
				});

				if (SdeAppConfiguration.BindItemTabs) {
					if (Database.Source == DataSources.ClientItem ||
						Database.Source == DataSources.Item ||
						Database.Source == DataSources.ItemImport) {
						LastSelectedTuple = item;
					}
				}
			});
		}

		public void Undo() {
			if (SdeEditor.Project != null) {
				SdeEditor.Project.Commands.Undo();
			}
			else {
				if (Table.Commands.CanUndo) {
					Table.Commands.Undo();
				}
			}
		}

		public void Redo() {
			if (SdeEditor.Project != null) {
				SdeEditor.Project.Commands.Redo();
			}
			else {
				if (Table.Commands.CanRedo) {
					Table.Commands.Redo();
				}
			}
		}

		public void Search() {
			_dbSearchPanel._searchTextBox.SelectAll();
			Keyboard.Focus(_dbSearchPanel._searchTextBox);
		}

		public void Filter() => SearchEngine.Filter(this);
		public void IgnoreFilterOnce() => SearchEngine.IgnoreFilterOnce();
		public void ReplaceFromFile() => SafeExecute(() => ReplaceTableFields.ReplaceFields(this));

		public void SelectNext() {
			SafeExecute(delegate {
				if (_listView.SelectedItems.Count <= 1) {
					_listView.ScrollToCenterOfView(_listView.SelectedItem);
					return;
				}

				var item = SelectedItem;
				_listView.SelectedItems.Remove(item);
				_listView.SelectedItems.Add(item);

				_listView.ScrollToCenterOfView(_listView.SelectedItem);
			});
		}

		public void SelectPrevious() {
			SafeExecute(delegate {
				if (_listView.SelectedItems.Count <= 1) {
					_listView.ScrollToCenterOfView(_listView.SelectedItem);
					return;
				}

				var last = _listView.SelectedItems.OfType<ReadableTuple>().Last();
				_listView.SelectedItems.Remove(last);
				_listView.SelectedItems.Insert(0, last);

				var items = new List<ReadableTuple>(_listView.SelectedItems.OfType<ReadableTuple>());

				_listView.SelectedItem = null;

				foreach (var i in items) {
					_listView.SelectedItems.Add(i);
				}

				_listView.ScrollToCenterOfView(_listView.SelectedItem);
			});
		}

		public int GetNewItemId(int oldId, bool ignoreAlreadyExists = false) {
			InputDialog dialog = new InputDialog("Enter the new ID for this item.", "New ID", oldId.ToString(), false);
			dialog.Owner = WpfUtilities.TopWindow;
			dialog.TextBoxInput.Loaded += delegate {
				dialog.TextBoxInput.SelectAll();
				dialog.TextBoxInput.Focus();
			};

			if (dialog.ShowDialog() == true) {
				try {
					Int32.Parse(dialog.Input);
				}
				catch (Exception) {
					ErrorHandler.HandleException("Invalid ID format.");
					throw new KeyInvalidException();
				}

				int id = Int32.Parse(dialog.Input);

				if (id < 0) {
					ErrorHandler.HandleException("ID must be greater than 0.");
					throw new KeyInvalidException();
				}

				int idKey = Int32.Parse(dialog.Input);

				if (!ignoreAlreadyExists && Table.ContainsKey(idKey)) {
					ErrorHandler.HandleException("An item with this ID already exists.");
					throw new KeyInvalidException();
				}

				return idKey;
			}

			throw new KeyInvalidException();
		}

		public bool GetNewKey(out int key) {
			return GetNewKey(SelectedItem, out key);
		}

		public bool GetNewKey(ReadableTuple selectedItem, out int key) {
			key = default;

			if (!Settings.CanChangeId) {
				ErrorHandler.HandleException("This type of database does not support key edit.");
				return false;
			}

			ReadableTuple item = selectedItem;

			if (item == null) {
				ErrorHandler.HandleException("No item has been selected.", ErrorLevel.NotSpecified);
				return false;
			}

			try {
				key = GetNewItemId(item.Key, true);
				return true;
			}
			catch (KeyInvalidException) {
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}

			return false;
		}

		public void Show(ReadableTuple item) {
			try {
				_currentSelectedItem = item;
				_show(item);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void Clear() {
			PropertiesGrid.Children.Clear();
			PropertiesGrid.RowDefinitions.Clear();
		}

		public void SetupSearch(TabGenerator generator, TabSettings settings, BaseDatabase db) {
			generator.SetSettings(this, settings, db);
			SearchEngine.IsLoaded = false;
			SearchEngine.Filter(this);
		}

		public void SelectItems(List<ReadableTuple> tuples, bool focus) {
			_listView.Dispatch(delegate {
				var oldSelected = SelectedItem;

				if (tuples.Count == 0) {
					_listView.SelectedIndex = -1;
					return;
				}
				else if (tuples.Count == 1) {
					if (_listView.Items.Contains(tuples[0]))
						_listView.SelectedItem = tuples[0];

					if (focus) {
						_listView.ScrollToCenterOfView(tuples[0]);
					}

					return;
				}

				var existingItems = new HashSet<ReadableTuple>(_listView.Items.OfType<ReadableTuple>(), ReferenceEqualityComparer<ReadableTuple>.Instance);
				List<ReadableTuple> toAdd = new List<ReadableTuple>();

				for (int i = 0; i < tuples.Count; i++) {
					if (existingItems.Contains(tuples[i])) {
						toAdd.Add(tuples[i]);
					}
				}

				_listView.SelectItems(toAdd);
				ReadableTuple newSelected = toAdd.FirstOrDefault();

				if (focus) {
					if (oldSelected != null && existingItems.Contains(oldSelected))
						_listView.ScrollToCenterOfView(oldSelected);
					else if (newSelected != null)
						_listView.ScrollToCenterOfView(newSelected);
				}

				Keyboard.Focus(_listView);
			});
		}

		public void ImportFromFile(string fileDefault = null, bool autoIncrement = false) {
			try {
				string file = fileDefault ?? PathRequest.OpenFileCde("filter", "All db files|*.conf;*.txt");

				if (file == "clipboard") {
					if (!Clipboard.ContainsText())
						return;

					file = TemporaryFilesManager.GetTemporaryFilePath("clipboard_{0:0000}.txt");
					File.WriteAllText(file, Clipboard.GetText());
				}

				if (file != null) {
					try {
						Table.Commands.Begin();
						SdeEditor.Project.GetDb(Database.Source).LoadFromClipboard(file);
					}
					catch {
						Table.Commands.CancelEdit();
					}
					finally {
						Table.Commands.EndEdit();

						if (autoIncrement) {
							var cmds = Table.Commands.GetUndoCommands();

							if (cmds.Count > 0) {
								var lastCmd = cmds.Last() as GroupCommand<int, ReadableTuple>;

								if (lastCmd != null) {
									if (lastCmd.Commands.Count > 0 && lastCmd.Commands.OfType<ChangeTupleProperties<int, ReadableTuple>>().Count() == 1) {
										var firstKey = lastCmd.Commands.First().Key;

										var tuple = new ReadableTuple(firstKey, Table.AttributeList);
										var oldTuple = Table.TryGetTuple(firstKey);
										tuple.Copy(oldTuple);
										tuple.Added = true;

										SdeEditor.Project.Commands.Undo();
										Table.Commands.AddTuple(tuple.Key, tuple, false, true, null);
									}
								}
							}
						}
					}

					Update();
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void SafeExecute(Action action) {
			try {
				action();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _menuItemCopyItemTo_Click(object sender, RoutedEventArgs e) => Commands.CopyItemTo();
		private void _miChangeId_Click(object sender, RoutedEventArgs e) => Commands.ChangeId();
		private void _menuItemDeleteItem_Click(object sender, RoutedEventArgs e) => Commands.DeleteItems();
		private void _miCut_Click(object sender, RoutedEventArgs e) => Commands.Cut();
		private void _menuItemKeepSelectedItemsOnly_Click(object sender, RoutedEventArgs e) => Commands.ShowSelectedOnly();
		private void _miSelectInNotepad_Click(object sender, RoutedEventArgs e) => Commands.SelectInNotepad();
		private void _miShowSelected_Click(object sender, RoutedEventArgs e) => Commands.ShowSelectedOnly();

		private void _autoSelect() {
			if (SdeAppConfiguration.BindItemTabs) {
				if (Database.Source == DataSources.ClientItem || Database.Source == DataSources.Item || Database.Source == DataSources.ItemImport) {
					_dbSearchPanel._searchTextBox.Text = SearchEngine.LastSearch;
				}
			}

			if (SdeAppConfiguration.BindItemTabs && LastSelectedTuple != null) {
				if (LastSelectedTuple is ReadableTuple) {
					if (Database.Source == DataSources.ClientItem) {
						TabNavigation.SelectQuiet(DataSources.ClientItem, LastSelectedTuple.GetKey<int>());
					}
					else if (Database.Source == DataSources.Item) {
						TabNavigation.SelectQuiet(DataSources.Item, LastSelectedTuple.GetKey<int>());
					}
					else if (Database.Source == DataSources.ItemImport) {
						TabNavigation.SelectQuiet(DataSources.ItemImport, LastSelectedTuple.GetKey<int>());
					}
				}
			}
		}

		private void _show(ReadableTuple item) {
			lock (_lock) {
				if (item != _currentSelectedItem) return;
				if (UpdateAction == null)
					throw new Exception("No UpdateAction defined for this Tab. The UpdateAction is used to define the behavior of the current tuple after being selected.");

				UpdateAction?.Invoke(item);
			}
		}

		private void _parent_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count > 0 && e.AddedItems[0] is DbTab && ReferenceEquals(e.AddedItems[0], this)) {
				Window wnd = AttachedProperty["AttachedWindow"] as Window;

				if (wnd != null) {
					e.Handled = true;

					wnd.Dispatcher.BeginInvoke(new Action(delegate {
						wnd.Activate();
					}));
				}

				TabChanged();
			}
		}

		private bool _isMenuDeployed;

		private void _deployTabControls() {
			if (!_isMenuDeployed) {
				MenuManager.CreateContextMenu();
				ListViewExtensions.DisableContextMenuIfEmpty(_listView);
				_listView.SelectionChanged += _listView_SelectionChanged;
				_isMenuDeployed = true;
			}
		}

		private void _listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Update();
		}

		public void TabChanged() {
			_deployTabControls();

			if (DelayedReload) {
				SearchEngine.Filter(this);
				DelayedReload = false;
			}

			_autoSelect();
		}

		#region Commands callback

		public bool IsCurrentTabSelected() {
			return this.Dispatch(() => {
				try {
					TabControl tabControl = WpfUtilities.FindParentControl<TabControl>(this);
					TabItem selectedTab = tabControl.Items[tabControl.SelectedIndex] as TabItem;

					return selectedTab != null && WpfUtilities.IsTab(this, selectedTab.Header.ToString());
				}
				catch {
					return false;
				}
			});
		}
		#endregion
	}
}