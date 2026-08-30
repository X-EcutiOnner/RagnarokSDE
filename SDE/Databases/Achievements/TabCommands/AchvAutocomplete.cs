using SDE.ApplicationConfiguration;
using SDE.Editor.Achievement;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System;
using System.Collections.Generic;

namespace SDE.Databases.Achievements.TabCommands {
	public class AchvAutocomplete : TabCommand {
		public AchvAutocomplete() {
			AllowMultipleSelection = true;
			DisplayName = String.Format("Add in [{0}]", DataSources.ClientAchievement.DisplayName);
			ImagePath = "add.png";
			Shortcut = SdeCommands.DbAutocompleteNew;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			var cCheevoDb = SdeEditor.Project.GetDb(DataSources.ClientAchievement);

			try {
				cCheevoDb.Table.Commands.Begin();

				foreach (var item in tuples) {
					int key = item.GetKey<int>();

					if (!cCheevoDb.Table.ContainsKey(key)) {
						var cmds = AchievementAutocomplete.Autocomplete(item.Key, item, true);

						if (cmds != null)
							cCheevoDb.Table.Commands.StoreAndExecute(cmds);
					}
				}
			}
			finally {
				cCheevoDb.Table.Commands.EndEdit();
			}
		}
	}
}
