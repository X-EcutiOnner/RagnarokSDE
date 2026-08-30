using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using ErrorManager;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Generic;
using SDE.Editor.Generic.DbTabs;
using SDE.View.Editors;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities;
using Tuple = Database.Tuple;

namespace SDE.View.Dialogs {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class AddRangeDialog : TkWindow {
		private Tuple _sourceTuple;
		private readonly DbTab _tab;

		public AddRangeDialog(SdeEditor editor)
			: base("Add range...", "add.png", SizeToContent.WidthAndHeight, ResizeMode.NoResize) {
			InitializeComponent();

			_tab = editor.FindTopmostTab();

			if (_tab == null) {
				throw new Exception("No table selected.");
			}

			if (!(_tab is DbTab)) {
				throw new Exception("This table doesn't support this operation.");
			}

			List<DataSource> sources = new List<DataSource>();

			sources.Add(_tab.Database.Source);

			if (_tab.Database.Source.ImportTable != null) {
				sources.Add(_tab.Database.Source.ImportTable);
			}

			_destTable.ItemsSource = sources;
			_destTable.SelectedIndex = 0;
			
			WpfUtilities.AddMouseInOutHandEffect(_imReset);

			this.Loaded += delegate {
				_tbRange.Text = "1";
				_tbFrom.Text = "0";

				if (_tab.SelectedItem != null) {
					_sourceTuple = _tab.SelectedItem;
					_tbBasedOn.Text = _sourceTuple.GetKey<int>().ToString(CultureInfo.InvariantCulture);
					_imReset.Visibility = System.Windows.Visibility.Visible;

					_tbFrom.Text = (_sourceTuple.GetKey<int>() + 1).ToString(CultureInfo.InvariantCulture);
				}
			};

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = WpfUtilities.TopWindow;
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			try {
				_addRange();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _addRange() {
			var tab = _tab;

			var range = FormatConverters.IntOrHexConverter(_tbRange.Text);
			var from = FormatConverters.IntOrHexConverter(_tbFrom.Text);
			var table = SdeEditor.Project.GetTable((DataSource)_destTable.SelectedItem);

			try {
				table.Commands.Begin();

				for (int i = 0; i < range; i++) {
					var tuple = new ReadableTuple(i + from, tab.Database.AttributeList);

					if (_sourceTuple != null) {
						tuple.Copy(_sourceTuple);
						tuple.SetRawValue(0, i + from);
					}

					tuple.Added = true;
					table.Commands.AddTuple(i + from, tuple);
				}
			}
			catch (Exception err) {
				table.Commands.CancelEdit();
				ErrorHandler.HandleException(err);
			}
			finally {
				table.Commands.End();
				tab.Filter();
			}
		}

		private void _imReset_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			_tbBasedOn.Text = "None";
			_imReset.Visibility = Visibility.Collapsed;
			_sourceTuple = null;
		}

		private void _buttonSearch_Click(object sender, RoutedEventArgs e) {
			try {
				SelectTupleDialog dialog = new SelectTupleDialog(_tab.Table, _tab.Database.Source, _tab.SelectedItem == null ? "" : _tab.SelectedItem.Key.ToString());
				
				if (dialog.ShowDialog() == true) {
					var id = Int32.Parse(dialog.Id);

					_sourceTuple = SdeEditor.Project.GetMergedTable(_tab.Database.Source).TryGetTuple(id);

					if (_sourceTuple != null) {
						_tbBasedOn.Text = id.ToString(CultureInfo.InvariantCulture);
						_imReset.Visibility = Visibility.Visible;
					}
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}
