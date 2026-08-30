using SDE.Databases.Generic.TabCommands;

namespace SDE.Editor.Generic.DbTabs {
	public static class TabDefaultCommands {
		public static TabCommand Delete = new Delete();
		public static TabCommand ChangeId = new ChangeId();
		public static TabCommand CopyTo = new CopyTo();
		public static TabCommand Cut = new Cut();
		public static TabCommand SelectInNotepad = new SelectInNotepad();
		public static TabCommand ShowSelectedOnly = new ShowSelectedOnly();
	}
}
