using ErrorManager;
using SDE.Editor.Generic.DbTabs;
using SDE.View.ObjectView;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles.ListView;

namespace SDE.Editor.ErrorConsole {
	/// <summary>
	/// Interaction logic for ErrorConsoleControl.xaml
	/// </summary>
	public partial class ErrorConsoleControl : UserControl {
		public int ErrorCount => _debugList.Items.Count;
		private readonly ObservableCollection<DebugItemViewModel> _debugItems;

		public ErrorConsoleControl() {
			InitializeComponent();

			ListViewDataTemplateHelper.GenerateListViewTemplateNew(_debugList, new ListViewDataTemplateHelper.GeneralColumnInfo[] {
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "#", DisplayExpression = "ErrorNumber", SearchGetAccessor = "ErrorNumber", FixedWidth = 35, ToolTipBinding = "ErrorNumber", TextAlignment = TextAlignment.Right },
				new ListViewDataTemplateHelper.ImageColumnInfo { Header = "", DisplayExpression = "DataImage", SearchGetAccessor = "Exception", FixedWidth = 20, MaxHeight = 24 },
				new ListViewDataTemplateHelper.RangeColumnInfo { Header = "Exception", DisplayExpression = "Exception", SearchGetAccessor = "Exception", IsFill = true, TextAlignment = TextAlignment.Left, ToolTipBinding="OriginalException", TextWrapping = TextWrapping.Wrap, MinWidth = 120 },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Id", DisplayExpression = "Id", SearchGetAccessor = "Id", FixedWidth = 90, TextAlignment = TextAlignment.Left, ToolTipBinding="Id", TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "File", DisplayExpression = "FileName", SearchGetAccessor = "FilePath", FixedWidth = 145, TextAlignment = TextAlignment.Left, ToolTipBinding="FilePath", TextWrapping = TextWrapping.Wrap },
				new ListViewDataTemplateHelper.GeneralColumnInfo { Header = "Line", DisplayExpression = "Line", SearchGetAccessor = "Line", FixedWidth = 40, TextAlignment = TextAlignment.Left, ToolTipBinding="Line" },
			}, null, new string[] { "Default", "{DynamicResource TextForeground}" });

			ApplicationShortcut.Link(ApplicationShortcut.Copy, () => ListViewExtensions.CopyContent(_debugList), _debugList);

			_debugItems = new ObservableCollection<DebugItemViewModel>();
			_debugList.ItemsSource = _debugItems;

			_debugList.MouseRightButtonUp += _debugList_MouseRightButtonUp;
		}

		private void _debugList_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
			try {
				ListViewItem item = _debugList.GetObjectAtPoint<ListViewItem>(e.GetPosition(_debugList));

				if (item != null) {
					DebugItemViewModel view = item.Content as DebugItemViewModel;

					if (view != null) {
						if (view.CanSelectInTextEditor()) {
							((MenuItem)_debugList.ContextMenu.Items[0]).Visibility = Visibility.Visible;
							((MenuItem)_debugList.ContextMenu.Items[1]).Visibility = Visibility.Visible;
						}
						else {
							((MenuItem)_debugList.ContextMenu.Items[0]).Visibility = Visibility.Collapsed;
							((MenuItem)_debugList.ContextMenu.Items[1]).Visibility = Visibility.Collapsed;
						}
					}
				}
				else {
					e.Handled = true;
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _miOpen_Click(object sender, RoutedEventArgs e) {
			try {
				DebugItemViewModel view = (DebugItemViewModel)_debugList.SelectedItem;
				Process.Start(view.FilePath);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _miOpenNotepad_Click(object sender, RoutedEventArgs e) {
			try {
				DebugItemViewModel view = (DebugItemViewModel)_debugList.SelectedItem;
				TabsMaker.SelectInNotepadpp(view.FilePath, view.Line);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _miCopy_Click(object sender, RoutedEventArgs e) {
			try {
				DebugItemViewModel view = (DebugItemViewModel)_debugList.SelectedItem;
				view?.Copy();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		public void Clear() {
			_debugItems.Clear();
		}

		public void FocusLastItem() {
			_debugList.ScrollIntoView(_debugItems.Last());
		}

		public void AddError(Exception err, string exception, ErrorLevel errorLevel) {
			_debugItems.Add(new DebugItemViewModel(err, _debugItems.Count + 1, exception, errorLevel));
		}
	}
}
