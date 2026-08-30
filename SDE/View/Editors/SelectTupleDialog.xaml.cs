using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Database;
using SDE.Core;
using SDE.Databases;
using SDE.Databases.AchievementIcons;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.SearchFeature;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;

namespace SDE.View.Editors {
	/// <summary>
	/// Interaction logic for SelectTupleDialog.xaml
	/// </summary>
	public partial class SelectTupleDialog : TkWindow {
		protected int _idIndex = 0;

		public SelectTupleDialog() {
			InitializeComponent();
		}

		public SelectTupleDialog(Table<int, ReadableTuple> table, DataSource source, string text) : base("Select item in [" + source.UidName + "]", "cde.ico", SizeToContent.Manual, ResizeMode.CanResize) {
			InitializeComponent();

			Extensions.SetMinimalSize(this);

			DbAttribute attId = table.AttributeList.PrimaryAttribute;
			DbAttribute attDisplay = table.AttributeList.Attributes.FirstOrDefault(p => p.IsDisplayAttribute) ?? table.AttributeList.Attributes[1];

			if (source == DataSources.AchievementIcon) {
				attId = AchvIconAttributes.StringId;
				_idIndex = attId.Index;

				ListViewDataTemplateHelper.GenerateListViewTemplateNew(_listView, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
					new ListViewDataTemplateHelper.GeneralColumnInfo {Header = attId.DisplayName, DisplayExpression = "[" + AchvIconAttributes.StringId.Index + "]", SearchGetAccessor = attId.AttributeName, FixedWidth = 120, TextAlignment = TextAlignment.Right, ToolTipBinding = "[" + attId.Index + "]"},
					new ListViewDataTemplateHelper.RangeColumnInfo {Header = attDisplay.DisplayName, DisplayExpression = "[" + attDisplay.Index + "]", SearchGetAccessor = attDisplay.AttributeName, IsFill = true, ToolTipBinding = "[" + attDisplay.Index + "]", MinWidth = 100, TextWrapping = TextWrapping.Wrap }
				}, new DatabaseItemSorter(table.AttributeList), new string[] { "Deleted", "{DynamicResource CellBrushRemoved}", "Modified", "{DynamicResource CellBrushModified}", "Added", "{DynamicResource CellBrushAdded}", "Normal", "{DynamicResource TextForeground}" });
			}
			else {
				ListViewDataTemplateHelper.GenerateListViewTemplateNew(_listView, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
					new ListViewDataTemplateHelper.GeneralColumnInfo {Header = attId.DisplayName, DisplayExpression = "[" + attId.Index + "]", SearchGetAccessor = attId.AttributeName, FixedWidth = 70, TextAlignment = TextAlignment.Right, ToolTipBinding = "[" + attId.Index + "]"},
					new ListViewDataTemplateHelper.RangeColumnInfo {Header = attDisplay.DisplayName, DisplayExpression = "[" + attDisplay.Index + "]", SearchGetAccessor = attDisplay.AttributeName, IsFill = true, ToolTipBinding = "[" + attDisplay.Index + "]", MinWidth = 100, TextWrapping = TextWrapping.Wrap }
				}, new DatabaseItemSorter(table.AttributeList), new string[] { "Deleted", "{DynamicResource CellBrushRemoved}", "Modified", "{DynamicResource CellBrushModified}", "Added", "{DynamicResource CellBrushAdded}", "Normal", "{DynamicResource TextForeground}" });
			}

			TabSettings gTabSettings = new TabSettings(source, null);
			gTabSettings.AttributeList = table.AttributeList;
			gTabSettings.AttId = attId;
			gTabSettings.AttDisplay = attDisplay;

			SearchEngine gSearchEngine = new SearchEngine(source, gTabSettings);

			var attributes = new DbAttribute[] { attId, attDisplay }.Concat(table.AttributeList.Attributes.Skip(2).Where(p => p.IsSearchable != null)).ToList();

			if (attributes.Count % 2 != 0) {
				attributes.Add(null);
			}

			gSearchEngine.SetAttributes(attributes);
			gSearchEngine.SetSettings(attId, true);
			gSearchEngine.SetSettings(attDisplay, true);
			gSearchEngine.Init(_dbSearchPanel, _listView, table);

			_listView.MouseDoubleClick += _listView_MouseDoubleClick;

			Loaded += delegate {
				gSearchEngine.Filter(this);
			};

			bool first = true;
			gSearchEngine.FilterFinished += delegate {
				if (!first)
					return;

				try {
					if (Int32.TryParse(text, out int ival)) {
						_listView.Dispatch(delegate {
							_listView.SelectedItem = table.TryGetTuple(ival);
							TokeiLibrary.WPF.Extensions.ScrollToCenterOfView(_listView, _listView.SelectedItem);
						});
					}
				}
				finally {
					first = false;
				}
			};

			Loaded += delegate {
				_dbSearchPanel._searchTextBox.Focus();
				_dbSearchPanel._searchTextBox.SelectAll();
			};
		}

		public string Id {
			get {
				return ((ReadableTuple)_listView.SelectedItem).GetValue(_idIndex).ToString();
			}
		}

		public ReadableTuple Tuple => _listView.SelectedItem as ReadableTuple;

		protected void _listView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (_listView.GetObjectAtPoint<ListViewItem>(e.GetPosition(_listView)) == null)
				return;

			if (_listView.SelectedItem != null && ((IList)_listView.ItemsSource).Contains(_listView.SelectedItem))
				DialogResult = true;

			Close();
		}

		protected void _buttonOk_Click(object sender, RoutedEventArgs e) {
			if (_listView.SelectedItem != null && ((IList) _listView.ItemsSource).Contains(_listView.SelectedItem))
				DialogResult = true;

			Close();
		}

		protected void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
