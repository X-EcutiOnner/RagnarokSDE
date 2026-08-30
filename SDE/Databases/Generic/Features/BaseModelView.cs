using SDE.Editor;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace SDE.Databases.Generic.Features {
	public interface IBaseViewModel {
		object GetModel();
	}

	public class BaseModelView<TModel> : INotifyPropertyChanged, IBaseViewModel, INotifyDataErrorInfo where TModel : new() {
		public TModel Model;
		public DbTab Tab { get; protected set; }
		public ReadableTuple Tuple { get; set; }
		public object GetModel() => Model;

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		#endregion

		#region INotifyDataErrorInfo
		private readonly Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();
		public bool HasErrors => _errorsByPropertyName.Any();
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public IEnumerable GetErrors(string propertyName) {
			if (string.IsNullOrEmpty(propertyName) || !_errorsByPropertyName.ContainsKey(propertyName))
				return null;

			return _errorsByPropertyName[propertyName];
		}

		protected void AddError(string propertyName, string error) {
			if (!_errorsByPropertyName.ContainsKey(propertyName)) {
				_errorsByPropertyName[propertyName] = new List<string>();
			}

			if (!_errorsByPropertyName[propertyName].Contains(error)) {
				_errorsByPropertyName[propertyName].Add(error);
				OnErrorsChanged(propertyName);
			}
		}

		protected void ClearErrors() {
			foreach (var error in _errorsByPropertyName) {
				error.Value.Clear();
				OnErrorsChanged(error.Key);
			}
		}

		protected void ClearErrors(string propertyName) {
			if (_errorsByPropertyName.ContainsKey(propertyName)) {
				_errorsByPropertyName.Remove(propertyName);
				OnErrorsChanged(propertyName);
			}
		}

		protected void OnErrorsChanged(string propertyName) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
		#endregion

		public bool Normal => true;
		public bool IsModelValid => Model != null;

		public bool Renewal => ProjectConfiguration.IsRenewal;
		public bool PreRenewal => !Renewal;

		public void Copy<T, TWriter>(List<T> entries, Action<T, TWriter, StringBuilder> write) where TWriter : class, new() {
			StringBuilder b = new StringBuilder();
			var writer = new TWriter();

			foreach (var entry in entries)
				write(entry, writer, b);

			Clipboard.SetDataObject(b.ToString());
		}

		public void Execute<TFieldValue>(object model, TFieldValue value, string fieldName, Action<bool> isLockedSetter) {
			try {
				isLockedSetter(true);
				var tab = SdeEditor.Instance.FindTopmostTab();

				if (tab.List.SelectedItems.Count > 1) {
					tab.Table.Commands.SetModelsValue<TModel, TFieldValue>(tab.List.SelectedItems.OfType<ReadableTuple>().ToList(), fieldName, value);
				}
				else {
					var tuple = tab.List.SelectedItem as ReadableTuple;
					if (tuple == null)
						return;
					tab.Table.Commands.SetModelValue(tuple, Model, fieldName, value);
				}
			}
			finally {
				isLockedSetter(false);
			}

			OnPropertyChanged(fieldName);
		}

		public void Execute<TFieldValue>(object model, Func<TModel, TFieldValue> value, string fieldName, Action<bool> isLockedSetter) {
			try {
				isLockedSetter(true);
				var tab = SdeEditor.Instance.FindTopmostTab();

				if (tab.List.SelectedItems.Count > 1) {
					tab.Table.Commands.SetModelsValue(tab.List.SelectedItems.OfType<ReadableTuple>().ToList(), fieldName, value);
				}
				else {
					var tuple = tab.List.SelectedItem as ReadableTuple;
					if (tuple == null)
						return;
					tab.Table.Commands.SetModelValue(tuple, Model, fieldName, value(Model));
				}
			}
			finally {
				isLockedSetter(false);
			}

			OnPropertyChanged(fieldName);
		}

		public void Execute<TFieldValue, TBaseModel>(
			TFieldValue value, string fieldName, 
			Func<List<ReadableTuple>, List<TBaseModel>> tupleToBaseModelFunc, 
			List<BaseModelView<TModel>> viewModelList,
			Func<TBaseModel, List<TModel>> baseModelToListModelsFunc,
			DbTab tab,
			Action<bool> isLockedSetter,
			Func<TModel, TFieldValue> valueSetter = null) {
			try {
				isLockedSetter(true);
				if (tab.List.SelectedItems.Count > 1) {
					int index = viewModelList.IndexOf(this);
					var tuples = tab.List.SelectedItems.OfType<ReadableTuple>().ToList();

					if (valueSetter != null)
						tab.Table.Commands.SetModelsValue(tuples, fieldName, valueSetter, p => baseModelToListModelsFunc((TBaseModel)p), index);
					else
						tab.Table.Commands.SetModelsValue(tuples, fieldName, value, p => baseModelToListModelsFunc((TBaseModel)p), index);
				}
				else {
					tab.Table.Commands.SetModelValue(tab.List.SelectedItem as ReadableTuple, Model, fieldName, valueSetter != null ? valueSetter(Model) : value);
				}
			}
			finally {
				isLockedSetter(false);
			}

			OnPropertyChanged(fieldName);
		}
	}
}
