using Database.Commands;
using ErrorManager;
using SDE.ApplicationConfiguration;
using SDE.Editor.Achievement;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System;

namespace SDE.Databases.ClientAchievements.TabCommands {
	public class ClientAchvAutocomplete : TabCommand {
		public ClientAchvAutocomplete() {
			AllowMultipleSelection = true;
			DisplayName = "Autocomplete (from Server data)";
			ImagePath = "imconvert.png";
			Shortcut = SdeCommands.DbAutocomplete;
			AddToCommandsStack = true;
			Command = DoAction;
		}

		public ITableCommand<int, ReadableTuple> DoAction(ReadableTuple item) {
			try {
				int id = item.GetKey<int>();

				var achivementDb = SdeEditor.Project.GetDb(DataSources.Achievement);
				var tupleServer = achivementDb.Table.TryGetTuple(id);

				if (tupleServer == null)
					return null;

				return AchievementAutocomplete.Autocomplete(item.Key, tupleServer);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
				return null;
			}
		}
	}
}
