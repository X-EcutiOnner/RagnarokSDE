using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using SDE.View;
using System;
using System.Windows;

namespace SDE.Databases.Generic.Controls {
	public partial class MobToAegisNameButton : MultiApplyBase {
		private MergedTable _mobDb;

		public MobToAegisNameButton() {
			InitializeComponent();
		}

		protected override void _preExecute() {
			_mobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
		}

		protected override string _getNewValue(ReadableTuple tuple, object oModel, string srcValue, string oldValue, string newValue) {
			if (!string.IsNullOrEmpty(srcValue)) {
				if (Int32.TryParse(srcValue, out int res)) {
					var mob = _mobDb.TryGetTuple(res);

					if (mob != null) {
						return mob.GetModel<Mob>().AegisName;
					}
				}
			}

			return newValue;
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			base.Execute();
		}
	}
}
