using System;
using ErrorManager;
using TokeiLibrary;
using TokeiLibrary.WPF;
using Utilities;

namespace SDE.Editor.IronPython {
	/// <summary>
	/// A class used by the IronPython script interpreter.
	/// It will be added to the script as 'script' variable and give additional functions that would be hard to do through pure IronPython alone.
	/// </summary>
	public class ScriptHost {
		// ReSharper disable InconsistentNaming
		public void exit() {
			throw new OperationCanceledException();
		}

		public string input(string caption, string description, string defaultValue) {
			InputDialog diag = new InputDialog(description, caption, defaultValue);
			diag.Owner = WpfUtilities.TopWindow;
			diag.TextBoxInput.Loaded += delegate {
				diag.TextBoxInput.SelectAll();
				diag.TextBoxInput.Focus();
			};
			if (diag.ShowDialog() == true) {
				return diag.Input;
			}

			return defaultValue;
		}

		public string input(string caption, string description) {
			return input(caption, description, "");
		}

		public void show(object message) {
			ErrorHandler.HandleException(message.ToString(), ErrorLevel.NotSpecified);
		}

		public void show(string message, params object[] items) {
			ErrorHandler.HandleException(string.Format(message, items), ErrorLevel.NotSpecified);
		}

		public bool confirm(string message) {
			return ErrorHandler.YesNoRequest(message, "Information");
		}

		public void @throw(string message) {
			throw new Exception(message);
		}

		public string format(string message, params object[] items) {
			return string.Format(message, items);
		}

		public string trim(string message) {
			return message.Trim();
		}

		public int @int(object obj) {
			if (obj is int)
				return (int)obj;

			return FormatConverters.IntOrHexConverter(obj.ToString());
		}

		public string hex(object obj) {
			if (obj is string)
				return (string)obj;

			int v = FormatConverters.IntOrHexConverter(obj.ToString());

			return "0x" + v.ToString("X");
		}
		// ReSharper restore InconsistentNaming
	}
}
