using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErrorManager;
using SDE.Core;
using SDE.Databases.Generic.Common;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities;

namespace SDE.View.Dialogs {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class ReplaceDialog : TkWindow {
		private readonly SdeEditor _editor;

		public ReplaceDialog(SdeEditor editor)
			: base("Replace all...", "convert.png", SizeToContent.Height, ResizeMode.NoResize) {
			_editor = editor;
			_editor.SelectionChanged += _editor_SelectionChanged;

			InitializeComponent();
			Extensions.SetMinimalSize(this);
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = WpfUtilities.TopWindow;

			_update();
		}

		private void _editor_SelectionChanged(object sender, TabItem olditem, TabItem newitem) {
			_update();
		}

		public class ObjectTreeViewModel {
			public string FieldName;
			public ObjectTree ObjectTree;

			public ObjectTreeViewModel(string fieldName, ObjectTree objectTree) {
				FieldName = fieldName;
				ObjectTree = objectTree;
			}

			public override string ToString() {
				return FieldName;
			}
		}

		private DbTab _tab;

		private void _update() {
			DbTab tab = _editor.FindTopmostTab();

			if (tab != null) {
				try {
					if (_cbField.ItemsSource != null && _tab == tab) {
						_buttonOk.IsEnabled = true;
						_cbField.IsEnabled = true;
						return;
					}

					_tab = tab;
					
					var modelType = _tab.Settings.ModelAttribute.DataType;
					var objectTree = TypeTreeHelper.GetObjectTree(modelType);

					_cbField.ItemsSource = objectTree.FieldsOrMembers.Where(p => p.Value.Member is FieldInfo).Select(p => new ObjectTreeViewModel(p.Key, p.Value)).OrderBy(p => p.FieldName);
					_buttonOk.IsEnabled = true;
					_cbField.IsEnabled = true;
					return;
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			}

			_cbField.ItemsSource = null;
			_buttonOk.IsEnabled = false;
			_cbField.IsEnabled = false;
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

		private void _replace(DbTab tab, ObjectTreeViewModel objectTreeViewModel) {
			var commands = tab.Table.Commands;

			try {
				var fieldName = objectTreeViewModel.FieldName;
				var fieldInfo = objectTreeViewModel.ObjectTree.Member as FieldInfo;
				var modelType = tab.Settings.ModelAttribute.DataType;

				if (fieldInfo == null)
					return;

				var tuples = tab.List.SelectedItems.Cast<ReadableTuple>().ToList();
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

				var genericMethod = method.MakeGenericMethod(modelType, fieldInfo.FieldType);

				if (objectTreeViewModel.ObjectTree.IsCollection)
					throw new NotImplementedException();
				else if (fieldInfo.FieldType == typeof(bool)) {
					genericMethod.Invoke(commands, new object[] { tuples, fieldName, _boolNewValue.IsChecked == true, 1 });
				}
				else if (fieldInfo.FieldType.BaseType == typeof(Enum)) {
					genericMethod.Invoke(commands, new object[] { tuples, fieldName, ((EnumInfoBase)_enumNewValue.SelectedItem).Value, 1 });
				}
				else if (fieldInfo.FieldType == typeof(string)) {
					genericMethod.Invoke(commands, new object[] { tuples, fieldName, _stringNewValue.Text, 1 });
				}
				else {
					throw new Exception("Unsupported field type: " + fieldInfo.FieldType);
				}
			}
			finally {
				tab.Update();
			}
		}

		private void _replace() {
			try {
				if (!(_cbField.SelectedItem is ObjectTreeViewModel objectTree))
					throw new Exception("No field selected.");

				var fieldName = objectTree.FieldName;
				DbTab tab = _tab;

				if (tab == null)
					throw new Exception("No tab selected.");

				if (tab.ListView.SelectedItems.Count == 0)
					throw new Exception("No items selected (select the items to replace in the list).");

				_replace(tab, objectTree);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _cbField_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_stringNewValue.Visibility = Visibility.Collapsed;
			_boolNewValue.Visibility = Visibility.Collapsed;
			_enumNewValue.Visibility = Visibility.Collapsed;

			if (_cbField.SelectedItem is ObjectTreeViewModel objectTreeViewModel) {
				var fieldInfo = objectTreeViewModel.ObjectTree.Member as FieldInfo;

				if (fieldInfo.FieldType == typeof(bool)) {
					_boolNewValue.Visibility = Visibility.Visible;
				}
				else if (fieldInfo.FieldType.BaseType == typeof(Enum)) {
					_enumNewValue.Visibility = Visibility.Visible;
					_enumNewValue.ItemsSource = EnumInfos.GetEnumInfoList(fieldInfo.FieldType);
					_enumNewValue.SelectedIndex = 0;
				}
				else if (fieldInfo.FieldType == typeof(string)) {
					_stringNewValue.Visibility = Visibility.Visible;
				}
			}
		}
	}
}
