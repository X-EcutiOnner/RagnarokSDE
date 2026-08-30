using SDE.ApplicationConfiguration;
using SDE.Databases.ClientItems;
using SDE.Databases.ClientItems.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.Items;
using SDE.View;
using System.Collections.Generic;

namespace SDE.Databases.Items.TabCommands {
	public class ClientItemAutocomplete : TabCommand {
		public ClientItemAutocomplete() {
			AllowMultipleSelection = true;
			DisplayName = string.Format("Add in [{0}]", DataSources.ClientItem.DisplayName);
			ImagePath = "add.png";
			Shortcut = SdeCommands.DbAutocompleteNew;
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			var tab = SdeEditor.Instance.FindTopmostTab();
			var project = SdeEditor.Project;
			BaseDatabase citemDb = project.GetDb(DataSources.ClientItem);
			BaseDatabase petDb1 = project.GetDb(DataSources.Pet);
			BaseDatabase petDb2 = project.GetDb(DataSources.PetImport);
			BaseDatabase mobDb1 = project.GetDb(DataSources.Mob);
			BaseDatabase mobDb2 = project.GetDb(DataSources.MobImport);
			ItemGeneratorEngine<int> itemGen = new ItemGeneratorEngine<int>();

			try {
				citemDb.Table.Commands.Begin();

				foreach (var item in tuples) {
					int key = item.GetKey<int>();

					if (!citemDb.Table.ContainsKey(key)) {
						ReadableTuple tuple = new ReadableTuple(key, ClientItemAttributes.AttributeList);
						tuple.SetRawValue(ClientItemAttributes.Model, new ClientItem());
						tuple.Added = true;
						citemDb.Table.Commands.AddTuple(key, tuple, false);

						var cmds = itemGen.Generate(tuple, item, mobDb1, mobDb2, petDb1, petDb2);

						if (cmds != null)
							citemDb.Table.Commands.StoreAndExecute(cmds);
					}
				}
			}
			finally {
				citemDb.Table.Commands.EndEdit();
			}
		}
	}
}
