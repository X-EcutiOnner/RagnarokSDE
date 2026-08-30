using ErrorManager;
using SDE.Editor.Database;
using SDE.View;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities;

namespace SDE.Databases.Generic.Controls {
	public class MultiApplyBase : UserControl {
		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.RegisterAttached(
				"SourceField",
				typeof(BindingExpression),
				typeof(MultiApplyBase),
				new PropertyMetadata(null));

		public static void SetSourceField(DependencyObject element, BindingExpression value)
			=> element.SetValue(SourceFieldProperty, value);

		public static BindingExpression GetSourceField(DependencyObject element)
			=> (BindingExpression)element.GetValue(SourceFieldProperty);

		public static readonly DependencyProperty ApplyToAllSelectedTuplesProperty =
			DependencyProperty.Register(
				nameof(ApplyToAllSelectedTuples),
				typeof(bool),
				typeof(MultiApplyBase),
				new FrameworkPropertyMetadata(true));

		public bool ApplyToAllSelectedTuples {
			get => (bool)GetValue(ApplyToAllSelectedTuplesProperty);
			set => SetValue(ApplyToAllSelectedTuplesProperty, value);
		}

		public static readonly DependencyProperty DestinationFieldProperty =
			DependencyProperty.RegisterAttached(
				"DestinationField",
				typeof(BindingExpression),
				typeof(MultiApplyBase),
				new PropertyMetadata(null));

		public static void SetDestinationField(DependencyObject element, BindingExpression value)
			=> element.SetValue(DestinationFieldProperty, value);

		public static BindingExpression GetDestinationField(DependencyObject element)
			=> (BindingExpression)element.GetValue(DestinationFieldProperty);

		protected virtual void _preExecute() {
		}

		protected virtual string _getNewValue(ReadableTuple tuple, object oModel, string srcValue, string oldValue, string newValue) {
			return newValue;
		}

		protected virtual void _postExecute() {
		}

		public virtual void Execute() {
			var tab = SdeEditor.Instance.FindTopmostTab();

			if (tab == null)
				return;

			var table = tab.Table;
			string dstFieldName = string.Empty;
			string srcFieldName = string.Empty;

			if (GetDestinationField(this) is BindingExpression bindingExpr)
				dstFieldName = bindingExpr.ParentBinding?.Path?.Path ?? string.Empty;

			if (GetSourceField(this) is BindingExpression bindingExprSrc)
				srcFieldName = bindingExprSrc.ParentBinding?.Path?.Path ?? string.Empty;

			try {
				table.Commands.Begin();
				var modelType = table.AttributeList[1].DataType;
				var modelGetter = ReflectionOptimizer<string>.GetGetter(modelType, dstFieldName);
				var modelSetter = ReflectionOptimizer<string>.GetSetter(modelType, dstFieldName);
				var modelSrcGetter = ReflectionOptimizer<string>.GetGetter(modelType, srcFieldName);
				int count = tab.ListView.SelectedItems.Count;

				try {
					_preExecute();
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
					return;
				}

				foreach (var item in tab.ListView.SelectedItems.OfType<ReadableTuple>()) {
					var model = item.GetValue(1);

					Func<string> getter = () => modelGetter(model);
					Action<string> setter = (value) => modelSetter(model, value);

					string oldValue = getter();
					string srcValue = modelSrcGetter(model);
					string newValue = _getNewValue(item, model, srcValue, oldValue, oldValue);

					if ((oldValue != null && oldValue.Equals(newValue)) || oldValue == newValue)
						continue;

					table.Commands.SetModelValue(item, getter, setter, newValue, dstFieldName, count == 1);
				}
			}
			catch {

			}
			finally {
				table.Commands.End();
				tab.Update();
				_postExecute();
			}
		}
	}
}
