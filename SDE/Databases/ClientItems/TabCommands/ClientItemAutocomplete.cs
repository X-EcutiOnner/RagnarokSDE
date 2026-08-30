using Database.Commands;
using ErrorManager;
using SDE.ApplicationConfiguration;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.Items;
using SDE.View;
using System;

namespace SDE.Databases.ClientItems.TabCommands {
	public class ClientItemAutocomplete : TabCommand {
		private ItemGeneratorEngine<int> _itemGen;

		public ClientItemAutocomplete() {
			AllowMultipleSelection = true;
			DisplayName = "Autocomplete (from Server data)";
			ImagePath = "imconvert.png";
			Shortcut = SdeCommands.DbAutocomplete;
			AddToCommandsStack = true;
			Command = DoAction;

			_itemGen = new ItemGeneratorEngine<int>();
		}

		public ITableCommand<int, ReadableTuple> DoAction(ReadableTuple tuple) {
			try {
				var project = SdeEditor.Project;
				var itemDb1 = project.GetDb(DataSources.Item);
				var itemDb2 = project.GetDb(DataSources.ItemImport);
				var petDb1 = project.GetDb(DataSources.Pet);
				var petDb2 = project.GetDb(DataSources.PetImport);
				var mobDb1 = project.GetDb(DataSources.Mob);
				var mobDb2 = project.GetDb(DataSources.MobImport);

				int id = tuple.GetKey<int>();

				ReadableTuple tupleSource = itemDb2.Table.TryGetTuple(id) ?? itemDb1.Table.TryGetTuple(id);

				if (tupleSource != null)
					return _itemGen.Generate(tuple, tupleSource, mobDb1, mobDb2, petDb1, petDb2);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}

			return null;
		}
	}
}
