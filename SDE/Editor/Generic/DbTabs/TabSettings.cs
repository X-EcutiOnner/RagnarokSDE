using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Database;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.SearchFeature;
using SDE.View.Controls;

namespace SDE.Editor.Generic.DbTabs {
	public enum TabCommandAnchors {
		Delete,
		ChangeId,
		CopyTo,
		Cut,
		SelectInNotepad,
		ShowSelectedOnly,
	}

	/// <summary>
	/// The tab settings contains all the information to generate a tab.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class TabSettings {
		public List<TabCommand> AddedCommands = new List<TabCommand>();
		public Action CustomAddItemMethod;
		public TabGenerator.TabGeneratorDelegate Loaded;
		public string Style = "TabItemStyled";

		public TabSettings(DataSource source, BaseDatabase db) {
			TabName = new DisplayLabel(source, db);
			GenerateSearchPopUp = true;
			HasUniqueId = false;
			AttIdWidth = 60;
			SearchEngine = new SearchEngine(source, this);

			AddedCommands.Add(TabDefaultCommands.Delete);
			AddedCommands.Add(TabDefaultCommands.ChangeId);
			AddedCommands.Add(TabDefaultCommands.CopyTo);
			AddedCommands.Add(TabDefaultCommands.Cut);
			AddedCommands.Add(TabDefaultCommands.SelectInNotepad);
			AddedCommands.Add(TabDefaultCommands.ShowSelectedOnly);
		}

		public TabSettings(BaseDatabase db) : this(db.Source, db) {
		}

		public TabControl Control { get; set; }

		public int AttIdWidth { get; set; }
		public bool HasUniqueId { get; set; } = false;
		public bool CanChangeId => !HasUniqueId;
		public bool GenerateSearchPopUp { get; set; }
		public object TabName { get; set; }
		public ContextMenu ContextMenu { get; set; }
		public Action<ReadableTuple> NewItemAddedFunction { get; set; }
		public SearchEngine SearchEngine { get; set; }
		public Table<int, ReadableTuple> Table { get; set; }
		public DbAttribute AttId { get; set; }
		public DbAttribute AttDisplay { get; set; }
		public AttributeList AttributeList { get; set; }
		public bool UseModel;
		public DbAttribute ModelAttribute;
		public Visibility Visibility { get; set; } = Visibility.Visible;

		public TextWrapping AttDisplayWrap { get; set; } = TextWrapping.Wrap;

		public void AddCommand(TabCommand command, TabCommand before = null, TabCommand after = null) {
			if (after != null && before != null)
				throw new Exception("Cannot use both before and after modes at once. Choose one of the two options.");

			if (after == null && before == null)
				AddedCommands.Add(command);

			int insertIndex = AddedCommands.IndexOf(after ?? before);

			if (after != null)
				AddedCommands.Insert(insertIndex + 1, command);
			else
				AddedCommands.Insert(insertIndex, command);
		}

		public void AddCommand(TabCommand[] commands, TabCommand before = null, TabCommand after = null) {
			AddCommand(commands.ToList(), before, after);
		}

		public void AddCommand(List<TabCommand> commands, TabCommand before = null, TabCommand after = null) {
			if (after != null && before != null)
				throw new Exception("Cannot use both before and after modes at once. Choose one of the two options.");

			if (after == null && before == null)
				AddedCommands.AddRange(commands);

			int insertIndex = AddedCommands.IndexOf(after ?? before);

			if (after != null)
				AddedCommands.InsertRange(insertIndex + 1, commands);
			else
				AddedCommands.InsertRange(insertIndex, commands);
		}

		public void AddCommand(TabCommandAnchors after, params TabCommand[] commands) {
			AddCommand(commands, after: AnchorToCommand(after));
		}

		public TabCommand AnchorToCommand(TabCommandAnchors anchor) {
			switch (anchor) {
				case TabCommandAnchors.Delete:
					return TabDefaultCommands.Delete;
				case TabCommandAnchors.ChangeId:
					return TabDefaultCommands.ChangeId;
				case TabCommandAnchors.CopyTo:
					return TabDefaultCommands.CopyTo;
				case TabCommandAnchors.Cut:
					return TabDefaultCommands.Cut;
				case TabCommandAnchors.SelectInNotepad:
					return TabDefaultCommands.SelectInNotepad;
				case TabCommandAnchors.ShowSelectedOnly:
					return TabDefaultCommands.ShowSelectedOnly;
			}

			return null;
		}

		public void RemoveCommand(TabCommandAnchors anchor) {
			RemoveCommand(AnchorToCommand(anchor));
		}

		public void RemoveCommand(TabCommand command) {
			AddedCommands.Remove(command);
		}
	}
}