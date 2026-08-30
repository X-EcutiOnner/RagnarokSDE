using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErrorManager;
using GRF.Core;
using GRF.IO;
using GRF.Threading;
using GrfToWpfBridge;
using GrfToWpfBridge.Application;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Validation;
using SDE.View;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;

namespace SDE.Editor.Validation {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class ValidationDialog : TkWindow {
		private readonly string[] _advancedView = new string[] { "List view", "Show the list view" };
		private readonly string[] _rawView = new string[] { "Raw view", "Show the raw text view" };
		private List<ValidationErrorView> _errors;
		private AsyncOperation _asyncOperation;
		private ProjectManager _sdb;
		private DbValidationEngine _validation;

		public ValidationDialog() : base("Table validation", "validity.png", SizeToContent.Manual, ResizeMode.CanResize) {
			InitializeComponent();

			_asyncOperation = new AsyncOperation(_progressBar);

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = WpfUtilities.TopWindow;

			_initializeErrorView();

			_sdb = SdeEditor.Project;
			_validation = new DbValidationEngine(_sdb);

			_addSelectAll(_findErrors, _cbResSelectAll);
			_addSelectAll(_scanClientItems, _cbCiSelectAll);
			
			_listViewResults.MouseDoubleClick += (s, e) => _select();
			_listViewResults.KeyUp += (s, e) => {
				if (e.Key == Key.Enter)
					_select();
			};
		}

		private void _addSelectAll(Grid grid, CheckBox selectAll) {
			List<CheckBox> boxes = Core.Extensions.FindChildren<CheckBox>(grid).ToList();
			boxes.ForEach(WpfUtilities.AddMouseInOutUnderline);
			bool eventsActive = true;
			WpfUtilities.AddMouseInOutUnderline(selectAll);

			foreach (var box in boxes) {
				box.Click += delegate {
					if (eventsActive)
						_checkAllSelected(boxes, selectAll, box.IsChecked == true);
				};
			}

			selectAll.Click += delegate {
				eventsActive = false;

				bool target = selectAll.IsChecked == true;

				foreach (var box in boxes) {
					if (box.IsChecked == !target) {
						box.IsChecked = target;
					}
				}

				eventsActive = true;
			};

			_checkAllSelected(boxes, selectAll, true);
		}

		private void _checkAllSelected(List<CheckBox> boxes, CheckBox selectAll, bool state) {
			bool current = true;

			foreach (var box in boxes) {
				if (box.IsChecked != state) {
					current = false;
					break;
				}
			}

			if (current) {
				selectAll.IsChecked = state;
			}
		}

		private void _select() {
			var item = _listViewResults.SelectedItem as ValidationErrorView;

			if (item != null) {
				var commands = new HashSet<ValidationCommand>();
				item.GetCommands(commands);
				commands.First().Execute(item, _listViewResults.SelectedItems.OfType<ValidationErrorView>().ToList());
			}
		}

		private void _initializeErrorView() {
			_changeRawViewButton();

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_listViewResults, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.ImageColumnInfo { Header = "", DisplayExpression = "DataImage", SearchGetAccessor = "Error", FixedWidth = 20, MaxHeight = 24 },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Error", DisplayExpression = "ErrorString", FixedWidth = 120, ToolTipBinding = "ErrorString", TextAlignment = TextAlignment.Left },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Id", DisplayExpression = "Id", FixedWidth = 50, ToolTipBinding = "Id", TextAlignment = TextAlignment.Right },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Db", DisplayExpression = "Db", FixedWidth = 100, ToolTipBinding = "Db", TextAlignment = TextAlignment.Left },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Message", DisplayExpression = "Message", MinWidth = 150, ToolTipBinding = "Message", TextAlignment = TextAlignment.Left, IsFill = true, TextWrapping = TextWrapping.Wrap },
			}, new DefaultListViewComparer<ValidationErrorView>(), new string[] { "Default", "{DynamicResource TextForeground}" });

			_errors = new List<ValidationErrorView>();
			_listViewResults.ItemsSource = _errors;
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonFindErrors_Click(object sender, RoutedEventArgs e) {
			List<ValidationErrorView> errors = new List<ValidationErrorView>();
			_asyncOperation.SetAndRunOperation(new GrfThread(() => _validation.FindResourceErrors(errors), _validation, errors, true, true), _updateErrors);
		}

		private void _updateErrors(object state) {
			try {
				_lbResults.Dispatch(p => p.Content = "Results");

				var errors = (List<ValidationErrorView>)state;
				errors = errors.Where(p => p != null).ToList();
				
				StringBuilder builder = new StringBuilder();

				for (int index = 0; index < errors.Count; index++) {
					builder.Append(errors[index] + "\r\n");
				}

				_tbResults.Dispatch(p => p.Text = builder.ToString());
				_listViewResults.Dispatch(p => p.ItemsSource = errors);
				_tabControl.Dispatch(p => p.SelectedItem = _tabItemResults);
				_lbResults.Dispatch(p => p.Content = "Results (found " + errors.Count + String.Format(" error{0})", errors.Count == 1 ? "" : "s"));
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _buttonRawView_Click(object sender, RoutedEventArgs e) {
			SdeAppConfiguration.ValidationRawView = !SdeAppConfiguration.ValidationRawView;
			_changeRawViewButton();
		}

		private void _changeRawViewButton() {
			if (SdeAppConfiguration.ValidationRawView) {
				_buttonRawView.Dispatch(p => p.TextHeader = _advancedView[0]);
				_buttonRawView.Dispatch(p => p.TextDescription = _advancedView[1]);
				_listViewResults.Visibility = Visibility.Hidden;
				_tbResults.Visibility = Visibility.Visible;
			}
			else {
				_buttonRawView.Dispatch(p => p.TextHeader = _rawView[0]);
				_buttonRawView.Dispatch(p => p.TextDescription = _rawView[1]);
				_listViewResults.Visibility = Visibility.Visible;
				_tbResults.Visibility = Visibility.Hidden;
			}
		}

		private void _listViewResults_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
			var item = _listViewResults.GetObjectAtPoint<ListViewItem>(e.GetPosition(_listViewResults));

			if (item != null) {
				HashSet<ValidationCommand> commands = new HashSet<ValidationCommand>();

				foreach (ValidationErrorView error in _listViewResults.SelectedItems) {
					error.GetCommands(commands);
				}

				ContextMenu menu = new ContextMenu();

				foreach (var cmd in commands) {
					var lcmd = cmd;

					MenuItem mitem = new MenuItem { Header = cmd.DisplayName, Icon = new Image { Source = ApplicationManager.GetResourceImage(cmd.Icon) } };
					mitem.Click += delegate {
						var items = _listViewResults.SelectedItems.Cast<ValidationErrorView>().ToList();

						_asyncOperation.SetAndRunOperation(new GrfThread(delegate {
							List<BaseDatabase> dbs = new List<BaseDatabase>();

							foreach (var source in DataSources.AllSources) {
								var db = _sdb.TryGetDb(source);

								if (db != null) {
									dbs.Add(db);
								}
							}

							foreach (var db in dbs) {
								db.Table.Commands.BeginNoDelay();
							}

							try {
								AProgress.Init(_validation);

								_validation.Grf.Close();
								_validation.Grf.Open(GrfPath.Combine(SdeAppConfiguration.ProgramDataPath, "missing_resources.grf"), GrfLoadOptions.OpenOrNew);

								for (int i = 0; i < items.Count; i++) {
									AProgress.IsCancelling(_validation);
									if (!lcmd.Execute(items[i], items))
										return;
									_validation.Progress = (float)i / items.Count * 100f;
								}

								if (_validation.Grf.IsModified) {
									_validation.Progress = -1;
									_validation.Grf.Save();
									_validation.Grf.Reload();
									_validation.Grf.Compact();
								}
							}
							catch (OperationCanceledException) {
							}
							catch (Exception err) {
								ErrorHandler.HandleException(err);
							}
							finally {
								foreach (var db in dbs) {
									db.Table.Commands.End();
								}

								_validation.Grf.Close();
								AProgress.Finalize(_validation);
							}
						}, _validation, null, true, true));
					};

					menu.Items.Add(mitem);
				}

				item.ContextMenu = menu;
				item.ContextMenu.IsOpen = true;
			}
			else {
				e.Handled = true;
			}
		}

		private void _buttonScanClientItems_Click(object sender, RoutedEventArgs e) {
			List<ValidationErrorView> errors = new List<ValidationErrorView>();
			_asyncOperation.SetAndRunOperation(new GrfThread(() => _validation.FindClientItemErrors(errors), _validation, errors, true, true), _updateErrors);
		}

		private void _buttonTableErrors_Click(object sender, RoutedEventArgs e) {
			List<ValidationErrorView> errors = new List<ValidationErrorView>();
			_asyncOperation.SetAndRunOperation(new GrfThread(() => _validation.FindTableErrors(errors), _validation, errors, true, true), _updateErrors);
		}
	}
}
