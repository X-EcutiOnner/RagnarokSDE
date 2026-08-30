using SDE.View.Dialogs;
using SDE.View.Editors;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SDE.Databases.Generic.Controls {
	public partial class FlagEditButton : UserControl {
		public FlagEditButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(FlagEditButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public Type SourceType {
			get => (Type)GetValue(SourceTypeProperty);
			set => SetValue(SourceTypeProperty, value);
		}

		public static readonly DependencyProperty SourceTypeProperty =
			DependencyProperty.Register(
				nameof(SourceType),
				typeof(Type),
				typeof(FlagEditButton));

		private void _button_Click(object sender, RoutedEventArgs e) {
			string fieldName = "NotAvailable";
			BindingExpression innerExpr = this.GetBindingExpression(SourceFieldProperty);
			DependencyObject parentControl = innerExpr.ResolvedSource as DependencyObject;

			string innerPath = innerExpr.ParentBinding.Path.Path;

			var fieldInfo = parentControl.GetType().GetField($"{innerPath}Property",
				BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

			if (fieldInfo != null && fieldInfo.GetValue(null) is DependencyProperty parentProperty) {
				BindingExpression outerExpr = BindingOperations.GetBindingExpression(parentControl, parentProperty);

				if (outerExpr != null && outerExpr.ParentBinding != null) {
					fieldName = outerExpr.ParentBinding.Path.Path;
				}
			}

			FlagEditDialog dialog = new FlagEditDialog();
			dialog.LoadFlag(SourceType, fieldName, SourceField);
			InputWindowHelper.Edit(dialog, v => SourceField = v, _button);
		}
	}
}
