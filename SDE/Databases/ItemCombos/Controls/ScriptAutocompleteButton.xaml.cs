using SDE.Databases.Generic.Controls;
using SDE.Databases.Generic.Parser;
using SDE.Databases.ItemCombos.Features;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.View.Editors.ScriptEdit;
using System.Windows;

namespace SDE.Databases.ItemCombos.Controls {
	public partial class ScriptAutocompleteButton : MultiApplyBase {
		public ScriptAutocompleteButton() {
			InitializeComponent();
		}

		public override void Execute() {
			if (!ProjectConfiguration.SynchronizeWithClientDatabases)
				return;

			base.Execute();
		}

		protected override string _getNewValue(ReadableTuple tuple, object oModel, string srcValue, string oldValue, string newValue) {
			var script = LuaEquipmentProperties.CreateAthenaScript((ItemCombo)oModel);

			if (script != null)
				script = DbWriter.ScriptToSingleLineScript(script);

			return script ?? srcValue;
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			base.Execute();
		}
	}
}
