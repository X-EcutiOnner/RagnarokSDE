using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Database;
using ErrorManager;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.SearchDescriptors;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.SearchFeature;
using SDE.View;
using SDE.View.Controls;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF;
using Utilities;
using Tuple = Database.Tuple;

namespace SDE.Editor.SearchFeature {
	/// <summary>
	/// This class is responsible for sorting the items. It also
	/// generates the UI for the search panel.
	/// </summary>
	public partial class SearchEngine {
		#region Delegates
		public delegate void CDEEventHandler(object sender, List<ReadableTuple> modified);
		#endregion

		public static string LastSearch = "";

		protected readonly object _filterLock = new object();
		protected readonly SearchSettings _itemsSearchSettings;
		private readonly BaseDatabase _db;
		private readonly TabSettings _settings;
		private readonly Dictionary<DbAttribute, bool> _states = new Dictionary<DbAttribute, bool>();
		private SearchDescriptor _searchDescriptor;
		private DbAttribute[] _attributes;
		private SearchField[] _searchFields;
		private ComboBox _cbSearchItemsMode;
		private DatabaseItemSorter<ReadableTuple> _entryComparer;
		private bool _isLoaded;
		private ListView _items;
		private Grid _searchDrop;
		private string _searchItemsFilter = "";
		private TextBox _tbItemsRange;
		private TextBox _tbSearchItems;
		private CheckBox _cbAdded;
		private CheckBox _cbModified;
		private Func<List<ReadableTuple>> _getItemsFunction;
		private readonly List<ComboBox> _resetFields = new List<ComboBox>();

		public SearchEngine(DataSource source, TabSettings settings) {
			_db = SdeEditor.Project.GetDb(source);
			_settings = settings;
			_itemsSearchSettings = new SearchSettings(ProjectConfiguration.ConfigAsker, source.UidName);
		}

		public bool IsLoaded {
			get => _isLoaded;
			set => _isLoaded = value;
		}

		public bool IsFiltering { get; protected set; }
		public Func<Tuple, object> SetupImageDataGetter { get; set; }

		public RangeObservableCollection<ReadableTuple> Collection {
			get {
				_validateLoaded();
				return _items.ItemsSource as RangeObservableCollection<ReadableTuple>;
			}
		}

		protected void _validateLoaded() {
			if (!_isLoaded) {
				if (_entryComparer == null) {
					_entryComparer = new DatabaseItemSorter<ReadableTuple>(_settings.AttributeList);
					_entryComparer.SetSort(_settings.AttId.AttributeName, ListSortDirection.Ascending);
				}

				_isLoaded = true;
				_load();
			}
		}

		protected void _load() {
			_searchDrop.Dispatch(delegate {
				_searchDrop.Children.Clear();

				try {
					if (_searchDescriptor != null) {
						foreach (var field in _searchDescriptor.Fields) {
							if (field == null)
								continue;

							_itemsSearchSettings[field.DisplayName] = field.IsActive;
						}
					}
					else {
						foreach (var attribute in _states) {
							_itemsSearchSettings[attribute.Key] = attribute.Value;
						}
					}

					_tbSearchItems.TextChanged += _tbSearchItems_TextChanged;
					int row = 0;
					int column = 0;

					_addSearch(_searchDrop, "Search options", null, row, column, true);

					_nextRow2(ref row, ref column);
					column = -2;

					if (_searchDescriptor != null) {
						foreach (var field in _searchDescriptor.Fields) {
							_advance(ref row, ref column);

							if (field == null) {
								continue;
							}

							var attributeCopy = field;
							_addSearchAttribute(_searchDrop, attributeCopy, row, column);
						}

						_searchFields = _searchDescriptor.Fields.Where(p => p != null).ToArray();
					}
					else {
						foreach (DbAttribute attribute in _attributes) {
							_advance(ref row, ref column);

							if (attribute == null) {
								continue;
							}

							DbAttribute attributeCopy = attribute;
							_addSearchAttribute(_searchDrop, attributeCopy, row, column);
						}

						_attributes = _attributes.Where(p => p != null).ToArray();
					}

					_itemsSearchSettings[SearchSettings.TupleAdded] = false;
					_itemsSearchSettings[SearchSettings.TupleModified] = false;
					_nextRow(ref row, ref column);
					_cbAdded = _addSearchAttributeSub(_searchDrop, SearchSettings.TupleAdded, row, column);
					_advance(ref row, ref column);
					_cbModified = _addSearchAttributeSub(_searchDrop, SearchSettings.TupleModified, row, column);

					_tbItemsRange = new TextBox();

					_cbSearchItemsMode = new ComboBox();
					_cbSearchItemsMode.MinWidth = 120;

					_cbSearchItemsMode.PreviewMouseDown += delegate(object sender, MouseButtonEventArgs args) {
						ComboBoxItem item = WpfUtilities.FindParentControl<ComboBoxItem>((Mouse.DirectlyOver as DependencyObject));

						if (item != null) {
							StackPanel panel = WpfUtilities.FindParentControl<StackPanel>(item);

							if (panel != null) {
								if (panel.Children.Count == 1)
									return;
							}

							item.IsSelected = true;
							args.Handled = true;
						}
					};

					_cbSearchItemsMode.SelectionChanged += _cbSearchItemsMode_SelectionChanged;
					_cbSearchItemsMode.Items.Add("OR search");
					_cbSearchItemsMode.Items.Add("AND search");
					_advance(ref row, ref column);
					_addSearch(_searchDrop, "Mode", _cbSearchItemsMode, row, column);
					_nextRow2(ref row, ref column);
					_addSearch(_searchDrop, "Range (5-10;-4;15+)", _tbItemsRange, row, column);

					_itemsSearchSettings[SearchSettings.TupleRange] = false;
					//_itemsSearchSettings[] = false;
					_tbItemsRange.TextChanged += (sender, e) => _itemsSearchSettings[SearchSettings.TupleRange] = _tbItemsRange.Text.Trim() != "";
					_cbSearchItemsMode.SelectedIndex = 1;

					_itemsSearchSettings.Modified += _filter;
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			});
		}

		public event CDEEventHandler FilterFinished;

		public void OnFilterFinished(List<ReadableTuple> items) {
			FilterFinished?.Invoke(this, items);
		}

		protected void _advance(ref int row, ref int column) {
			if (column >= 2) {
				column = 0;
				row++;
			}
			else {
				column += 2;
			}
		}

		protected void _nextRow(ref int row, ref int column) {
			if (column != 0) {
				column = 0;
				row++;
			}
		}

		protected void _nextRow2(ref int row, ref int column) {
			column = 0;
			row++;
		}

		protected void _addSearch(Grid searchGrid, string display, FrameworkElement element, int row, int column, bool isItalic = false) {
			Label label = new Label();
			label.Content = display;
			label.SetResourceReference(TextBlock.ForegroundProperty, "TextForeground");

			if (isItalic)
				label.FontStyle = FontStyles.Italic;

			while (searchGrid.RowDefinitions.Count <= row)
				searchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(-1, GridUnitType.Auto) });

			WpfUtilities.SetGridPosition(label, row, column);

			if (element != null) {
				WpfUtilities.SetGridPosition(element, row, column + 2);
				element.Margin = new Thickness(2);

				searchGrid.Children.Add(element);
			}

			searchGrid.Children.Add(label);
		}

		protected void _addSearchAttribute(Grid searchGrid, object attribute, int row, int column) {
			Type searchDataType = null;
			string displayName = "";
			Action<object> setter = null;

			if (attribute is SearchField searchField) {
				if (searchField.EnumType != null) {
					searchDataType = searchField.EnumType;
					setter = v => searchField.ActiveEnum = (Enum)v;
				}
				
				displayName = searchField.DisplayName;
			}
			else if (attribute is DbAttribute dbAttribute) {
				searchDataType = dbAttribute.DataType;
				displayName = dbAttribute.DisplayName;
				setter = v => dbAttribute.AttachedAttribute = v;
			}
			else {
				throw new Exception("Unrecognized attribute type.");
			}

			if (searchDataType != null &&
				((searchDataType.BaseType == typeof(Enum) && EnumInfos.Exists(searchDataType)) ||
				(searchDataType.BaseType == typeof(Enum)))) {
				_addEnumSearchAttributeSub(searchGrid, displayName, row, column, searchDataType, setter);
			}
			else {
				_addSearchAttributeSub(searchGrid, displayName, row, column);
			}
		}

		public Label CreateDisplayLabel(string name) {
			Label display = new Label();
			display.Margin = new Thickness(3);
			display.Padding = new Thickness(0);
			display.Content = name;
			display.VerticalAlignment = VerticalAlignment.Center;
			display.SetResourceReference(TextBlock.ForegroundProperty, "TextForeground");
			return display;
		}

		protected CheckBox _addSearchAttributeSub(Grid searchGrid, string attribute, int row, int column) {
			CheckBox box = new CheckBox();
			box.Margin = new Thickness(3);
			box.SetResourceReference(TextBlock.ForegroundProperty, "TextForeground");

			TextBlock block = new TextBlock { Text = attribute };
			block.SetResourceReference(TextBlock.ForegroundProperty, "TextForeground");
			box.MouseEnter += delegate {
				block.Foreground = Application.Current.Resources["MouseOverTextBrush"] as Brush;
				block.Cursor = Cursors.Hand;
				block.TextDecorations = TextDecorations.Underline;
			};

			box.MouseLeave += delegate {
				block.Foreground = Application.Current.Resources["TextForeground"] as Brush;
				block.Cursor = Cursors.Arrow;
				block.TextDecorations = null;
			};
			box.Content = block;

			while (searchGrid.RowDefinitions.Count <= row)
				searchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(-1, GridUnitType.Auto) });

			WpfUtilities.SetGridPosition(box, row, column);
			_itemsSearchSettings.Link(box, attribute);
			searchGrid.Children.Add(box);

			return box;
		}

		private void _addEnumSearchAttributeSub(Grid searchGrid, string displayName, int row, int column, Type searchDataType, Action<object> setter) {
			Grid grid = new Grid();

			ComboBox box = new ComboBox();
			_resetFields.Add(box);
			box.Margin = new Thickness(3);
			box.SetValue(Grid.ColumnProperty, 1);

			bool hasEnumInfo = EnumInfos.Exists(searchDataType);
			_itemsSearchSettings[displayName] = false;

			if (hasEnumInfo) {
				Type dataType = searchDataType;

				var values = EnumInfos.GetEnumInfoList(dataType).Where(p => p.Visible).ToList();
				values.Insert(0, new EnumInfoBase(default, "All", "", "", true));

				box.ItemsSource = values;
				box.SelectedIndex = 0;

				box.SelectionChanged += delegate {
					if (box.SelectedIndex > 0) {
						setter(values[box.SelectedIndex].Value);
					}

					_itemsSearchSettings[displayName] = box.SelectedIndex != 0;
				};
			}
			else {
				List<string> items = Enum.GetValues(searchDataType).Cast<Enum>().Select(Description.GetDescription).ToList();
				items.Insert(0, "All");
				box.ItemsSource = items;
				box.SelectedIndex = 0;
				List<int> values = Enum.GetValues(searchDataType).Cast<int>().ToList();

				box.SelectionChanged += delegate {
					if (box.SelectedIndex > 0) {
						setter(values[box.SelectedIndex - 1].ToString(CultureInfo.InvariantCulture));
					}

					_itemsSearchSettings[displayName] = box.SelectedIndex != 0;
				};
			}

			box.PreviewMouseDown += delegate (object sender, MouseButtonEventArgs args) {
				ComboBoxItem item = WpfUtilities.FindParentControl<ComboBoxItem>((Mouse.DirectlyOver as DependencyObject));

				if (item != null) {
					StackPanel panel = WpfUtilities.FindParentControl<StackPanel>(item);

					if (panel != null) {
						if (panel.Children.Count == 1)
							return;
					}

					item.IsSelected = true;
					args.Handled = true;
				}
			};

			WpfUtilities.SetGridPosition(grid, row, column);
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition());

			grid.Children.Add(CreateDisplayLabel(displayName));
			grid.Children.Add(box);

			searchGrid.Children.Add(grid);
		}

		protected void _cbSearchItemsMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_itemsSearchSettings.Set(SearchSettings.Mode, _cbSearchItemsMode.SelectedIndex);
		}

		protected void _tbSearchItems_TextChanged(object sender, TextChangedEventArgs e) {
			_searchItemsFilter = _tbSearchItems.Text;
			_filter(this);

			if (SdeAppConfiguration.BindItemTabs && (_db.Source & DataSources.AllItemTables) != 0) {
				LastSearch = _tbSearchItems.Text;
			}
		}

		public void Init(DbSearchPanel panel, ListView view, Func<List<ReadableTuple>> getItemsFunction) {
			// The initialization is delayed, it will start when loading the tab
			_searchDrop = panel._gridSearchContent;
			_items = view;
			_getItemsFunction = getItemsFunction;
			_tbSearchItems = panel._searchTextBox;

			panel._buttonResetSearch.Click += (sender, args) => Reset();
			ApplicationShortcut.Link(ApplicationShortcut.Search, () => {
				panel._searchTextBox.SelectAll();
				Keyboard.Focus(panel._searchTextBox);
			}, view);
		}

		public void Init(DbSearchPanel panel, ListView view, Table<int, ReadableTuple> table) {
			Init(panel, view, () => table.FastItems);
		}

		public void Init(DbSearchPanel panel, DbTab tab) {
			Init(panel, tab.List, tab.Table);
		}

		public void SetAttributes(params DbAttribute[] attributes) {
			_attributes = attributes;
		}

		public void SetAttributes(IEnumerable<DbAttribute> attributes) {
			_attributes = attributes.ToArray();
		}

		public void SetSettings(DbAttribute attribute, bool state) {
			_states[attribute] = state;
			_searchDescriptor = null;
		}

		public void SetSettings(SearchDescriptor searchDescriptor) {
			_states.Clear();
			_searchDescriptor = searchDescriptor;
		}

		public void SetRange(List<int> indexes) {
			_validateLoaded();
			_tbItemsRange.Text = GetQuery(indexes.OrderBy(p => p).ToList());
		}

		public void AddTuple(ReadableTuple tuple) {
			_validateLoaded();
			_items.Dispatch(delegate {
				if (_items.ItemsSource == null)
					return;

				RangeObservableCollection<ReadableTuple> allItems = (RangeObservableCollection<ReadableTuple>)_items.ItemsSource;

				var index = allItems.ToList().BinarySearch(tuple, _entryComparer);
				if (index < 0) index = ~index;
				allItems.Insert(index, tuple);
			});
		}

		public void AddTuples(List<ReadableTuple> tuples) {
			_validateLoaded();
			_items.Dispatch(delegate {
				if (_items.ItemsSource == null)
					return;

				RangeObservableCollection<ReadableTuple> allItems = (RangeObservableCollection<ReadableTuple>)_items.ItemsSource;

				var list = allItems.ToList();

				foreach (var tuple in tuples) {
					var index = list.BinarySearch(tuple, _entryComparer);
					
					if (index >= 0) {
						if (!_settings.HasUniqueId)
							continue;
					}

					if (index < 0) index = ~index;
					allItems.Insert(index, tuple);
					list.Insert(index, tuple);
				}
			});
		}

		public void SetOrder(ReadableTuple tuple) {
			_validateLoaded();
			_items.Dispatch(delegate {
				if (_items.ItemsSource == null)
					return;

				RangeObservableCollection<ReadableTuple> allItems = (RangeObservableCollection<ReadableTuple>)_items.ItemsSource;

				List<ReadableTuple> selection = _items.SelectedItems.OfType<ReadableTuple>().ToList();
				List<ReadableTuple> items = allItems.ToList();
				var oldIndex = items.IndexOf(tuple);

				if (oldIndex < 0) {
					var index = items.BinarySearch(tuple, _entryComparer);
					if (index < 0) index = ~index;
					allItems.Insert(index, tuple);
				}
				else {
					items.RemoveAt(oldIndex);
					var index = items.BinarySearch(tuple, _entryComparer);
					if (index < 0) index = ~index;
					allItems.Move(oldIndex, index);

					foreach (var item in selection) {
						_items.SelectedItems.Add(item);
					}
				}
			});
		}

		public static List<Func<ReadableTuple, bool>> GetRangePredicates(string query) {
			try {
				List<string> rangeQueries = query.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
				List<Func<ReadableTuple, bool>> predicates = new List<Func<ReadableTuple, bool>>();

				foreach (string rangeQuery in rangeQueries) {
					try {
						if (rangeQuery.StartsWith("-")) {
							string queryPredicate = rangeQuery;
							int high = Int32.Parse(queryPredicate.Substring(1));

							predicates.Add(new Func<ReadableTuple, bool>(p => p.GetKey<int>() <= high));
						}
						else if (rangeQuery.Contains("-")) {
							string queryPredicate = rangeQuery;
							int low = Int32.Parse(queryPredicate.Split('-')[0]);
							int high = Int32.Parse(queryPredicate.Split('-')[1]);

							predicates.Add(new Func<ReadableTuple, bool>(p => low <= p.GetKey<int>() && p.GetKey<int>() <= high));
						}
						else if (rangeQuery.EndsWith("+")) {
							string queryPredicate = rangeQuery;
							int low = Int32.Parse(queryPredicate.Substring(0, rangeQuery.Length - 1));

							predicates.Add(new Func<ReadableTuple, bool>(p => p.GetKey<int>() >= low));
						}
						else {
							string queryPredicate = rangeQuery;
							int middle = Int32.Parse(queryPredicate);

							predicates.Add(new Func<ReadableTuple, bool>(p => p.GetKey<int>() == middle));
						}
					}
					catch {
					}
				}

				return predicates;
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
				return new List<Func<ReadableTuple, bool>>();
			}
		}

		public static string GetQuery(List<int> tupleIndexes) {
			tupleIndexes.Add(-1);

			string searchQuery = "";

			int oldIndex = -1;
			int endIndex = -1;
			int startIndex = -1;

			foreach (int tupleIndex in tupleIndexes) {
				if (startIndex == -1) {
					startIndex = tupleIndex;
					oldIndex = tupleIndex;
					if (tupleIndex == -1) break;
					continue;
				}

				if (tupleIndex == oldIndex + 1) {
					endIndex = tupleIndex;
					oldIndex = tupleIndex;
					if (tupleIndex == -1) break;
					continue;
				}

				if (endIndex != -1 && startIndex != endIndex) {
					searchQuery += startIndex + "-" + endIndex + ";";
					startIndex = tupleIndex;
					oldIndex = tupleIndex;
					endIndex = -1;
					if (tupleIndex == -1) break;
					continue;
				}

				if (startIndex != endIndex) {
					searchQuery += oldIndex + ";";
					startIndex = tupleIndex;
					oldIndex = tupleIndex;
					endIndex = -1;
					if (tupleIndex == -1) break;
				}
			}

			return searchQuery;
		}

		public void Reset() {
			if (!_isLoaded) return;

			try {
				_filterEnabled = false;

				_tbItemsRange.Text = "";
				_tbSearchItems.Text = "";
				_cbSearchItemsMode.SelectedIndex = 1;

				_cbAdded.IsChecked = false;
				_cbModified.IsChecked = false;
				_filterEnabled = true;

				_resetFields.ForEach(p => p.SelectedIndex = 0);

				_filter(this);
			}
			finally {
				_filterEnabled = true;
			}
		}
	}
}