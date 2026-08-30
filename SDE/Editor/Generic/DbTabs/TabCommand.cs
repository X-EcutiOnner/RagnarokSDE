using System;
using System.Collections.Generic;
using System.Windows;
using Database.Commands;
using SDE.Editor.Database;
using TokeiLibrary.Shortcuts;

namespace SDE.Editor.Generic.DbTabs {
	/// <summary>
	/// Custom menu item to add to a tab's list view
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class TabCommand {
		#region Delegates
		public delegate void GenericCommandDelegate(List<ReadableTuple> toList);
		#endregion

		public GenericCommandDelegate GenericCommand;
		public Func<string> GetDisplayName;
		public string DisplayName { get; set; }
		public string ImagePath { get; set; }
		public bool AllowMultipleSelection { get; set; }
		public Func<ReadableTuple, ITableCommand<int, ReadableTuple>> Command { get; set; }

		public bool AddToCommandsStack { get; set; } = true;
		public TkCommand Shortcut { get; set; }
		public Visibility Visibility { get; set; } = Visibility.Visible;
	}
}