using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Database;
using ErrorManager;
using GrfToWpfBridge;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.Navigation;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace SDE.View.Dialogs {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class CopyToDialog : TkWindow {
		private List<ReadableTuple> _tuples;
		private readonly BaseDatabase _srcDb;
		private readonly BaseDatabase _dstDb;

		public CopyToDialog(DbTab tab, List<ReadableTuple> tuples, BaseDatabase currentDb, BaseDatabase destDb)
			: base("Copy to advanced...", "imconvert.png", SizeToContent.WidthAndHeight, ResizeMode.NoResize) {
			_tab = tab;
			_tuples = tuples;
			_srcDb = currentDb;
			_dstDb = destDb;

			InitializeComponent();
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = WpfUtilities.TopWindow;

			_tbNewId.Loaded += delegate {
				_tuples = _tuples.OrderBy(p => p.GetKey<int>()).ToList();
				_tbNewId.Text = _tuples[0].GetKey<int>().ToString(CultureInfo.InvariantCulture);
				_tbNewId.Focus();
				_tbNewId.SelectAll();
			};

			_gridItems.IsEnabled = (DataSources.ServerItems & destDb.Source) != 0;

			Binder.Bind(_cbOverwrite, () => SdeAppConfiguration.CmdCopyToOverwrite);

			WpfUtilities.AddMouseInOutUnderline(_cbOverwrite);
		}

		private readonly DbTab _tab;

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			_copy();
			Close();
		}

		private void _copy() {
			try {
				int newKey;
				int firstId = Int32.Parse(_tbNewId.Text);

				try {
					_dstDb.Table.Commands.BeginNoDelay();

					for (int i = 0; i < _tuples.Count; i++) {
						var item = _tuples[i];
						int oldId = item.GetKey<int>();
						newKey = firstId + i;

						if (!SdeAppConfiguration.CmdCopyToOverwrite) {
							if (_dstDb.Table.ContainsKey(newKey))
								continue;
						}

						if (i == _tuples.Count - 1)
							_dstDb.Table.Commands.CopyTupleTo(_srcDb.Table, oldId, newKey, (a, b, c, d, e) => _copyToCallback2(_dstDb, c, d, e));
						else
							_dstDb.Table.Commands.CopyTupleTo(_srcDb.Table, oldId, newKey, (a, b, c, d, e) => _copyToCallback3(c, d, e));
					}
				}
				catch (Exception err) {
					_dstDb.Table.Commands.CancelEdit();
					ErrorHandler.HandleException(err);
				}
				finally {
					_dstDb.Table.Commands.End();
					_tab.Filter();
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _copyToCallback3(Table<int, ReadableTuple> tableDest, int newKey, bool executed) {
			if (executed) {
				tableDest.GetTuple(newKey).Added = true;
			}
		}

		private void _copyToCallback2(BaseDatabase dbDest, Table<int, ReadableTuple> tableDest, int newkey, bool executed) {
			if (executed) {
				tableDest.GetTuple(newkey).Added = true;
				TabNavigation.Select(dbDest.Source, newkey);
			}
		}
	}
}
