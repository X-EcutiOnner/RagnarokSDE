using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Database;
using SDE.Databases.Generic.Controls;
using SDE.Databases.Generic.Features;
using SDE.View;
using Utilities;
using Utilities.Extension;

namespace SDE.Editor.Generic.DbTabs {
	public static class DisplayablePropertyHelper {
		private static string _getTextSearch(string field, object value) {
			if (value is IList)
				return $"[{field}] > 0";

			return $"[{field}] == " + value;
		}

		public static readonly DependencyProperty SearchBindingProperty =
			DependencyProperty.RegisterAttached(
				"SearchBinding",
				typeof(BindingExpression),
				typeof(DisplayablePropertyHelper),
				new PropertyMetadata(null, OnSearchBindingChanged));

		public static void SetSearchBinding(DependencyObject element, BindingExpression value)
			=> element.SetValue(SearchBindingProperty, value);

		public static BindingExpression GetSearchBinding(DependencyObject element)
			=> (BindingExpression)element.GetValue(SearchBindingProperty);

		private static void OnSearchBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			TextBlock textBlock = d as TextBlock;

			if (textBlock == null) {
				if (d is Property property) {
					textBlock = property.LabelControl;
				}
			}

			if (textBlock != null && e.NewValue is BindingExpression bindingExpr) {
				if (textBlock.ContextMenu == null) {
					string rawPath = bindingExpr.ParentBinding?.Path?.Path ?? string.Empty;
					string truePath = ResolveTruePath(rawPath);

					var contextMenu = new ContextMenu();

					var searchItem = new MenuItem { Header = $"Search for this field [{truePath.Replace("_", "__")}]" };
					searchItem.SetValue(TextBlock.FontStyleProperty, FontStyles.Normal);
					searchItem.Click += (s, args) => _executeSearch(truePath, rawPath, bindingExpr, textBlock.DataContext, append: false);

					var appendItem = new MenuItem { Header = $"Append search for this field [{truePath.Replace("_", "__")}]" };
					appendItem.SetValue(TextBlock.FontStyleProperty, FontStyles.Normal);
					appendItem.Click += (s, args) => _executeSearch(truePath, rawPath, bindingExpr, textBlock.DataContext, append: true);

					contextMenu.Items.Add(searchItem);
					contextMenu.Items.Add(appendItem);

					textBlock.ContextMenu = contextMenu;
				}
			}
		}

		private static string ResolveTruePath(string path) {
			if (string.IsNullOrEmpty(path)) return "Unknown";

			var paths = path.Split('.');
			var newPaths = new List<string>();

			foreach (var subPathS in paths) {
				var subPath = subPathS;

				if (subPath.StartsWith("Selected")) {
					subPath = subPath.ReplaceFirst("Selected", "");
					subPath += "s";
				}

				newPaths.Add(subPath);
			}

			return Methods.Aggregate(newPaths, ".");
		}

		private static void _executeSearch(string fieldName, string viewModelPath, BindingExpression bindingExpr, object dataContext, bool append) {
			var resolvedValue = EvaluatePath(dataContext, viewModelPath);

			if (resolvedValue == null)
				resolvedValue = "0";

			var selected = SdeEditor.Instance.Tabs.FirstOrDefault(p => p.IsSelected);

			if (selected == null)
				return;

			if (!append || selected._dbSearchPanel._searchTextBox.Text == "") {
				selected._dbSearchPanel._searchTextBox.Text = _getTextSearch(fieldName, resolvedValue);
			}
			else {
				selected._dbSearchPanel._searchTextBox.Text = "(" + selected._dbSearchPanel._searchTextBox.Text + ") && " + _getTextSearch(fieldName, resolvedValue);
			}
		}

		private static object EvaluatePath(object source, string path) {
			object current = source;

			if (source is IBaseViewModel baseVm) {
				var result = TypeTreeHelper.GetValue(baseVm.GetModel(), path);

				if (result.Count > 0)
					return result[0];
			}

			foreach (string part in path.Split('.')) {
				if (current == null)
					return null;

				var prop = current.GetType().GetProperty(part);

				if (prop == null)
					return null;

				current = prop.GetValue(current);
			}

			return current;
		}

		public static void SetTextBoxesUndo(Panel grid, DbTab tab) {
			var elements = SDE.Core.Extensions.FindChildren<ValidationTextBox>(grid);

			foreach (var element in elements)
				SDE.Core.Extensions.RemoveUndoAndRedoEvents(element, tab);
		}
	}
}