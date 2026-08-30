using SDE.Databases.Generic.Controls;
using SDE.Databases.Mobs.Features;
using SDE.Databases.Skills.Features;
using SDE.Editor.Database;
using SDE.View;
using System;
using System.Windows;

namespace SDE.Databases.MobSkills.Features {
	public partial class SetDisplayName : MultiApplyBase {
		private MergedTable _mobDb;
		private MergedTable _skillDb;

		public SetDisplayName() {
			InitializeComponent();
		}

		protected override void _preExecute() {
			_mobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
			_skillDb = SdeEditor.Project.GetMergedTable(DataSources.Skill);
		}

		protected override string _getNewValue(ReadableTuple tuple, object oModel, string srcValue, string oldValue, string newValue) {
			var model = (MobSkill)oModel;

			var tupleMob = _mobDb.TryGetTuple(model.IntMobId);
			var tupleSkill = _skillDb.TryGetTuple(model.IntSkillId);

			string mobName = "";
			string skillName = "";

			if (tupleMob != null) {
				var mobModel = tupleMob.GetModel<Mob>();
				mobName = mobModel.Name;
			
				if (String.IsNullOrEmpty(mobName)) {
					mobName = mobModel.JapaneseName;
				}
			}
			
			if (tupleSkill != null) {
				skillName = tupleSkill.GetModel<Skill>().Name;
			}
			
			return mobName + "@" + skillName;
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			base.Execute();
		}
	}
}
