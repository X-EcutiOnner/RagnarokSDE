using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SDE.ApplicationConfiguration;
using SDE.Databases.Generic.Common;
using SDE.Editor.Engines;
using SDE.View.Dialogs;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities;
using Utilities.Extension;
using Utilities.IndexProviders;

namespace SDE.View.Editors {
	public class GridIndexProvider {
		private readonly int _row;
		private readonly int _col;
		private int _current;

		public int Current {
			get { return _current - 1; }
		}

		public GridIndexProvider(int row, int col) {
			_row = row;
			_col = col;
		}

		public bool Next(out int row, out int col) {
			row = _current % _row;
			col = (_current / _row) % _col;
			_current++;

			return _current <= _row * _col;
		}
	}

	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class FlagEditDialog : TkWindow, IInputWindow {
		private readonly List<CheckBox> _boxes = new List<CheckBox>();
		private long _value;
		private int _maxColWidth = 400;

		public FlagEditDialog() : base("", "cde.ico", SizeToContent.WidthAndHeight, ResizeMode.CanResize) {
			InitializeComponent();
		}

		public void LoadFlag(Type enumType, string modelField, string text) {
			_value = text.ToLong();

			Title = "Flag edit";
			var values2 = EnumInfos.GetEnumInfoList(enumType);
			var visibleValues = values2.Where(p => p.Visible).ToList();
			string[] commands = Description.GetAnyDescription(enumType).Split('#');

			if (commands.Any(p => p.StartsWith("max_col_width:"))) {
				_maxColWidth = Int32.Parse(commands.First(p => p.StartsWith("max_col_width")).Split(':')[1]);
			}

			GridIndexProvider provider = _findGrid(visibleValues);

			var toolTips = new string[visibleValues.Count];

			if (!commands.Contains("disable_tooltips")) {
				for (int i = 0; i < visibleValues.Count; i++)
					toolTips[i] = visibleValues[i].ToolTip;
			}

			AbstractProvider iProvider = new DefaultIndexProvider(0, visibleValues.Count);

			if (commands.Any(p => p.StartsWith("order:"))) {
				List<int> order = commands.First(p => p.StartsWith("order:")).Split(':')[1].Split(',').Select(Int32.Parse).ToList();

				for (int i = 0; i < visibleValues.Count; i++) {
					if (!order.Contains(i)) {
						order.Add(i);
					}
				}

				iProvider = new SpecifiedIndexProvider(order);
			}

			ToolTipsBuilder.Initialize(toolTips, this);

			int row;
			int col;

			for (int i = 0; i < visibleValues.Count; i++) {
				var enumValue = visibleValues[i];
				provider.Next(out row, out col);

				int index = (int)iProvider.Next();
				CheckBox box = new CheckBox { Content = enumValue.FlagDisplay ?? enumValue.DisplayName, Margin = new Thickness(3, 6, 3, 6), VerticalAlignment = VerticalAlignment.Center };

				var menu = new ContextMenu();
				MenuItem item = new MenuItem();
				item.Header = "Restrict search to [" + enumValue.DisplayName + "]";
				box.ContextMenu = menu;
				menu.Items.Add(item);
				item.Click += delegate {
					var selected = SdeEditor.Instance.Tabs.FirstOrDefault(p => p.IsSelected);

					if (selected != null) {
						selected._dbSearchPanel._searchTextBox.Text = "([" + modelField + "] & " + enumValue.ValueLong + ") != 0";
					}
				};

				box.Tag = enumValue.ValueLong;
				WpfUtilities.AddMouseInOutUnderline(box);
				_boxes.Add(box);
				_upperGrid.Children.Add(box);
				WpfUtilities.SetGridPosition(box, row, 2 * col);
			}

			_boxes.ForEach(_addEvents);
		}

		private GridIndexProvider _findGrid(ICollection values) {
			int maxRow;
			int maxCol;

			if (values.Count < 8) {
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });
				maxRow = values.Count;
				maxCol = 1;
			}
			else if (values.Count < 20) {
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });

				maxRow = (int)Math.Ceiling(values.Count / 2f);
				maxCol = 2;
			}
			else if (values.Count < 30) {
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });

				maxRow = (int)Math.Ceiling(values.Count / 3f);
				maxCol = 3;
			}
			else {
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto), MaxWidth = _maxColWidth });
				_upperGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 20 });

				maxRow = (int)Math.Ceiling(values.Count / 4f);
				maxCol = 4;
			}

			for (int i = 0; i < maxRow; i++) {
				_upperGrid.RowDefinitions.Add(new RowDefinition());
			}

			GridIndexProvider provider = new GridIndexProvider(maxRow, maxCol);
			return provider;
		}

		public string Text {
			get {
				if (_value == 0)
					return "";

				return _value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public Grid Footer {
			get { return _footerGrid; }
		}

		private void _addEvents(CheckBox cb) {
			ToolTipsBuilder.SetupNextToolTip(cb, this);
			cb.IsChecked = ((long)cb.Tag & _value) == (long)cb.Tag;

			cb.Checked += (e, a) => _update();
			cb.Unchecked += (e, a) => _update();
		}

		private void _update() {
			_value = 0;

			foreach (var box in _boxes) {
				if (box.IsChecked == true) {
					_value |= (long)box.Tag;
				}
			}
			
			OnValueChanged();
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			if (!SdeAppConfiguration.UseIntegratedDialogsForFlags)
				DialogResult = true;
			Close();
		}

		public event Action ValueChanged;

		public void OnValueChanged() {
			ValueChanged?.Invoke();
		}
	}
}
