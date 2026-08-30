using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Database;
using ErrorManager;
using SDE.Editor.Generic.Parsers.Generic;
using TokeiLibrary;
using Utilities;
using Utilities.Extension;
using Tuple = Database.Tuple;

namespace SDE.Editor.Database {
	public class ReadableTupleBrush {
		public static Brush CellBrushRemoved;
		public static Brush CellBrushModified;
		public static Brush CellBrushAdded;
		public static Brush CellBrushMvp;
		public static Brush CellBrushLzma;
		public static Brush CellBrushEncrypted;
		public static Brush TextForeground;

		static ReadableTupleBrush() {
			ApplicationManager.ThemeChanged += ApplicationManager_ThemeChanged;
			ApplicationManager_ThemeChanged();
		}

		private static void ApplicationManager_ThemeChanged() {
			CellBrushRemoved = (Brush)Application.Current.TryFindResource("CellBrushRemoved");
			CellBrushModified = (Brush)Application.Current.TryFindResource("CellBrushModified");
			CellBrushAdded = (Brush)Application.Current.TryFindResource("CellBrushAdded");
			CellBrushMvp = (Brush)Application.Current.TryFindResource("CellBrushMvp");
			CellBrushLzma = (Brush)Application.Current.TryFindResource("CellBrushLzma");
			CellBrushEncrypted = (Brush)Application.Current.TryFindResource("CellBrushEncrypted");
			TextForeground = (Brush)Application.Current.TryFindResource("TextForeground");
		}
	}
	/// <summary>
	/// Tuple view item (to be displayed in a list view)
	/// </summary>
	public class ReadableTuple : Tuple, INotifyPropertyChanged {
		public Brush ForegroundBrush {
			get {
				if (Added)
					return ReadableTupleBrush.CellBrushAdded;
				if (Modified)
					return ReadableTupleBrush.CellBrushModified;
				if (Deleted)
					return ReadableTupleBrush.CellBrushRemoved;

				return ReadableTupleBrush.TextForeground;
			}
		}

		//private bool _isSelected;
		//
		//public bool IsSelected {
		//	get => _isSelected;
		//	set {
		//		if (_isSelected == value)
		//			return;
		//
		//		_isSelected = value;
		//		OnPropertyChanged(nameof(IsSelected));
		//	}
		//}

		public ReadableTuple(int key, AttributeList list) : base(key, list) {
		}

		public int Key => GetKey<int>();
		public override bool Default => false;

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		public int GetIntValue(int index) {
			return (int)GetValue(index);
		}

		// Using these methods are not allowed; they will crash the filter engines
		//public int GetIntValue(DbAttribute attribute) {
		//    return (int)GetValue(attribute.Index);
		//}
		//public string GetStringValue(DbAttribute attribute) {
		//    return (string)GetValue(attribute.Index);
		//}

		public string GetStringValue(int index) {
			var r = GetValue(index);
			return r is string v ? v : r.ToString();
		}

		public int GetIntNoThrow(DbAttribute attibute) {
			object obj = GetValue(attibute.Index);
			return obj is int ? (int)obj : FormatConverters.IntOrHexConverter((string)obj);
		}

		public int GetIntNoThrow(int index) {
			object obj = GetValue(index);
			return obj is int ? (int)obj : FormatConverters.IntOrHexConverter((string)obj);
		}

		public override void OnTupleModified(bool value) {
			base.OnTupleModified(value);
			OnPropertyChanged("");
		}

		public override void SetValue(DbAttribute attribute, object value) {
			bool sameValue;

			try {
				var v = GetValue(attribute.Index);

				if (v == null || value == null)
					sameValue = v == value;
				else
					sameValue = v.Equals(value);
			}
			catch {
				sameValue = false;
			}

			try {
				base.SetValue(attribute, value);
			}
			catch (Exception err) {
				DbIOErrorHandler.Handle(err, ("Failed to set or parse the value for [" + Key + "] at '" + attribute.DisplayName + "'. Value entered is : " + (value ?? "")).RemoveBreakLines(), ErrorLevel.NotSpecified);
				base.SetValue(attribute, attribute.Default);
			}

			if (!sameValue) {
				Modified = true;
			}
		}

		protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}