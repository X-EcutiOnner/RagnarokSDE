using TokeiLibrary.Shortcuts;

namespace SDE.ApplicationConfiguration {
	public static class SdeCommands {
		public static TkCommand Save = ApplicationShortcut.Save;
		public static TkCommand New = ApplicationShortcut.New;
		public static TkCommand Open = ApplicationShortcut.Open;
		public static TkCommand Copy = ApplicationShortcut.Copy;
		public static TkCommand Copy2 = ApplicationShortcut.FromString("Ctrl-Shift-C", "Application.Copy2");
		public static TkCommand Copy3 = ApplicationShortcut.FromString("Ctrl-Alt-C", "Application.Copy3");
		public static TkCommand Paste = ApplicationShortcut.Paste;
		public static TkCommand Cut = ApplicationShortcut.Cut;
		public static TkCommand Delete = ApplicationShortcut.Delete;
		public static TkCommand Undo = ApplicationShortcut.Undo;
		public static TkCommand Redo = ApplicationShortcut.Redo;
		public static TkCommand Search = ApplicationShortcut.Search;
		public static TkCommand Rename = ApplicationShortcut.Rename;
		public static TkCommand Select = ApplicationShortcut.Select;
		public static TkCommand NavigationBackward = ApplicationShortcut.NavigationBackward;
		public static TkCommand NavigationForward = ApplicationShortcut.NavigationForward;
		public static TkCommand Change = ApplicationShortcut.FromString("Ctrl-D", "Application.Change");
		public static TkCommand Restrict = ApplicationShortcut.FromString("Ctrl-R", "Application.Restrict");
		public static TkCommand CopyTo = ApplicationShortcut.FromString(null, "Application.CopyTo");
		public static TkCommand Replace = ApplicationShortcut.FromString("Ctrl-H", "Application.Replace");
		public static TkCommand UndoGlobal = ApplicationShortcut.FromString("Ctrl-Alt-Z", "Application.UndoGlobal");
		public static TkCommand RedoGlobal = ApplicationShortcut.FromString("Ctrl-Alt-Y", "Application.RedoGlobal");
		public static TkCommand CopyTo2 = ApplicationShortcut.FromString("Ctrl-Alt-D", "Application.CopyTo2");
		public static TkCommand AdvancedPaste = ApplicationShortcut.FromString("Ctrl-Shift-V", "Application.Paste2");
		public static TkCommand AdvancedPaste2 = ApplicationShortcut.FromString("Ctrl-B", "Application.Paste3");
		public static TkCommand Edit = ApplicationShortcut.FromString(null, "Application.Edit");
		public static TkCommand MoveUp = ApplicationShortcut.FromString("Alt-Up", "Application.MoveUp");
		public static TkCommand MoveDown = ApplicationShortcut.FromString("Alt-Down", "Application.MoveDown");

		public static TkCommand DbAutocomplete = ApplicationShortcut.FromString("Ctrl-G", "Database.Autocomplete");
		public static TkCommand DbAutocompleteNew = ApplicationShortcut.FromString("Ctrl-Alt-E", "Database.AutocompleteNew");
		public static TkCommand DbFocusNextEntry = ApplicationShortcut.FromString("Ctrl-Enter", "Database.FocusNextEntry");
		public static TkCommand DbFocusPreviousEntry = ApplicationShortcut.FromString("Ctrl-Shift-Enter", "Database.FocusPreviousEntry");
		public static TkCommand DbReload = ApplicationShortcut.FromString(null, "Database.Reload");
		public static TkCommand DbCopyAll = ApplicationShortcut.FromString(null, "Database.CopyAll");
		public static TkCommand DbAdd = ApplicationShortcut.FromString("Ctrl-N", "Database.Add");
		public static TkCommand DbAddRange = ApplicationShortcut.FromString("Ctrl-Shift-N", "Database.AddRange");
		public static TkCommand DbAddRaw = ApplicationShortcut.FromString(null, "Database.AddRaw");
		public static TkCommand DbChangeId = ApplicationShortcut.FromString("Ctrl-D", "Database.ChangeId");
		public static TkCommand DbCopyItemTo = ApplicationShortcut.FromString("Ctrl-Shift-D", "Database.CopyItemTo");
		public static TkCommand DbDelete = ApplicationShortcut.FromString("Delete", "Database.Delete");
		public static TkCommand DbOpenInNotepad = ApplicationShortcut.FromString("Ctrl-W", "Database.OpenInNotepad");
		public static TkCommand DbSearchNextEmptyEntry = ApplicationShortcut.FromString("F3", "Database.SearchNextEmptyEntry");
		public static TkCommand DbAddNewDrop = ApplicationShortcut.FromString("Ctrl-N", "Database.AddNewDrop");
		public static TkCommand DbAddNewMvpDrop = ApplicationShortcut.FromString("Ctrl-Shift-N", "Database.AddNewMvpDrop");

		public static TkCommand PythonRun = ApplicationShortcut.FromString("F7", "Python.Run");
		public static TkCommand PythonRun2 = ApplicationShortcut.FromString("Ctrl-Enter", "Python.Run2");
		public static TkCommand PythonNew = ApplicationShortcut.FromString("Ctrl-N", "Python.New");
		public static TkCommand PythonOpen = ApplicationShortcut.FromString("Ctrl-O", "Python.Open");
		public static TkCommand PythonSave = ApplicationShortcut.FromString("Ctrl-S", "Python.Save");
	}
}
