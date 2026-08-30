using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErrorManager;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities;
using static SDE.View.Dialogs.ReplaceDialog;

namespace SDE.View.Dialogs {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class CopyDialog : TkWindow {
		private readonly SdeEditor _editor;

		public CopyDialog(SdeEditor editor)
			: base("Copy all...", "imconvert.png", SizeToContent.WidthAndHeight, ResizeMode.NoResize) {
			_editor = editor;
			_editor.SelectionChanged += _editor_SelectionChanged;

			InitializeComponent();
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = WpfUtilities.TopWindow;

			_cbSelectAll.Checked += delegate {
				_boxes.ForEach(p => p.IsChecked = true);
			};

			_cbSelectAll.Unchecked += delegate {
				_boxes.ForEach(p => p.IsChecked = false);
			};

			WpfUtilities.AddMouseInOutUnderline(_cbSelectAll);

			_update();
		}

		private void _editor_SelectionChanged(object sender, TabItem olditem, TabItem newitem) {
			_update();
		}

		private DbTab _tab;

		private readonly List<CheckBox> _boxes = new List<CheckBox>();

		private void _update() {
			var tab = _editor.FindTopmostTab();

			if (tab != null) {
				try {
					if (_boxes.Count > 0 && _tab == tab) {
						_buttonOk.IsEnabled = true;
						return;
					}

					_tab = tab;

					_gridCopy.Children.Clear();
					_boxes.Clear();

					int index = 0;

					var modelType = tab.Settings.ModelAttribute.DataType;
					var objectTree = TypeTreeHelper.GetObjectTree(modelType);

					foreach (var vm in objectTree.FieldsOrMembers.Where(p => p.Value.Member is FieldInfo).Select(p => new ObjectTreeViewModel(p.Key, p.Value)).OrderBy(p => p.FieldName)) {
						CheckBox box = new CheckBox { Margin = new Thickness(3, 3, 10, 3) };
						box.Content = vm.FieldName;
						box.Tag = vm;
						box.SetValue(Grid.RowProperty, index / _gridCopy.ColumnDefinitions.Count);
						box.SetValue(Grid.ColumnProperty, index % _gridCopy.ColumnDefinitions.Count);
						box.IsChecked = _cbSelectAll.IsChecked;
						WpfUtilities.AddMouseInOutUnderline(box);
						_gridCopy.Children.Add(box);
						_boxes.Add(box);
						index++;
					}

					_buttonOk.IsEnabled = true;
					return;
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			}

			_gridCopy.Children.Clear();
			_boxes.Clear();
			_buttonOk.IsEnabled = false;
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			_replace();
		}

		private void _replace(DbTab tab, ReadableTuple tuple) {
			var commands = tab.Table.Commands;
			commands.Begin();

			try {
				List<ObjectTreeViewModel> fields = _boxes.Where(p => p.IsChecked == true).Select(p => (ObjectTreeViewModel)p.Tag).ToList();
				var modelType = tab.Settings.ModelAttribute.DataType;
				var tuples = tab.List.SelectedItems.Cast<ReadableTuple>().ToList();
				tuples.Remove(tuple);
				var model = tuple.GetModel();

				var method = commands.GetType().GetMethods().Where(p => p.Name == "SetModelsValue").FirstOrDefault(m => {
					// Must be generic with exactly 2 generic arguments: <TModel, TFieldValue>
					if (!m.IsGenericMethod || m.GetGenericArguments().Length != 2)
						return false;

					var parameters = m.GetParameters();

					if (parameters.Length != 4)
						return false;

					bool matchParameters =
						parameters[0].ParameterType.IsGenericType && // List<ReadableTuple>
						parameters[1].ParameterType == typeof(string) && // fieldName
						parameters[3].ParameterType == typeof(int); // modelAttributeIndex

					return matchParameters;
				});

				for (int index = 0; index < fields.Count; index++) {
					var vm = fields[index];
					var fieldName = vm.FieldName;
					var fieldInfo = vm.ObjectTree.Member as FieldInfo;
					var genericMethod = method.MakeGenericMethod(modelType, fieldInfo.FieldType);

					if (vm.ObjectTree.IsCollection)
						continue;
					else if (fieldInfo.FieldType == typeof(bool)) {
						genericMethod.Invoke(commands, new object[] { tuples, fieldName, (bool)TypeTreeHelper.GetValue(model, fieldName).First(), 1 });
					}
					else if (fieldInfo.FieldType.BaseType == typeof(Enum)) {
						genericMethod.Invoke(commands, new object[] { tuples, fieldName, TypeTreeHelper.GetValue(model, fieldName).First(), 1 });
					}
					else if (fieldInfo.FieldType == typeof(string)) {
						genericMethod.Invoke(commands, new object[] { tuples, fieldName, (string)(TypeTreeHelper.GetValue(model, fieldName).First() ?? ""), 1 });
					}
					else {
						continue;
					}
				}
			}
			catch {
				commands.CancelEdit();
			}
			finally {
				commands.End();
				tab.Update();
			}
		}

		private void _replace() {
			try {
				if (_boxes.TrueForAll(p => p.IsChecked == false))
					throw new Exception("No attribute selected.");

				DbTab tab = _tab;

				if (tab == null)
					throw new Exception("No tab selected.");

				if (tab.ListView.SelectedItems.Count == 0)
					throw new Exception("No items selected (select the items to replace in the list).");

				if (tab.ListView.SelectedItems.Count == 1)
					throw new Exception("You must select more than one item to copy (the currently selected one is the source).");

				var tuple = tab.SelectedItem;

				_replace(tab, tuple);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}
