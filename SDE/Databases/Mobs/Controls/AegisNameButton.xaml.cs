using SDE.Databases.Generic.Controls;
using SDE.Databases.Mobs.Features;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.Editor.LuaTables;
using SDE.View;
using System;
using System.Linq;
using System.Windows;

namespace SDE.Databases.Mobs.Controls {
	public partial class AegisNameButton : MultiApplyBase {
		private MergedTable _mobDb;

		public AegisNameButton() {
			InitializeComponent();
		}

		public override void Execute() {
			if (!ProjectConfiguration.SynchronizeWithClientDatabases)
				return;

			base.Execute();
		}

		protected override void _preExecute() {
			_mobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
		}

		protected override string _getNewValue(ReadableTuple tuple, object oModel, string srcValue, string oldValue, string newValue) {
			var key = tuple.Key;
			var current = LuaHelper.LatinOnly(srcValue);
			var count = _mobDb.FastItems.Count(p => p.Key != key && String.Compare(p.GetModel<Mob>().AegisName, current) == 0) - 1;

			if (count <= 0)
				return current;

			current = current + "_";

			count = _mobDb.FastItems.Count(p => p.Key != key && String.Compare(p.GetModel<Mob>().AegisName, current) == 0);

			if (count <= 0)
				return current;

			var sprite = current + "{0}";

			int i = 0;
			var output = String.Format(sprite, i);
			while (_mobDb.FastItems.Count(p => p.Key != key && String.Compare(p.GetModel<Mob>().AegisName, output) == 0) != 0) {
				i++;
				output = String.Format(sprite, i);
			}

			return output;
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			base.Execute();
		}
	}
}
