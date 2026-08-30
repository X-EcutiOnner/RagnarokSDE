using SDE.Databases.MobSkills.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.Editor.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.TabCommands {
	public class MobSkillSelectFromTable : TabCommand {
		private DataSource _targetSource;

		public MobSkillSelectFromTable(DataSource targetSource) {
			_targetSource = targetSource;

			AllowMultipleSelection = false;
			DisplayName = $"Select in [{targetSource.DisplayName}]";
			ImagePath = "arrowdown.png";
			AddToCommandsStack = false;
			GenericCommand = DoAction;
		}

		public void DoAction(List<ReadableTuple> tuples) {
			if (_targetSource == DataSources.MobSkill) {
				TabNavigation.SelectList(_targetSource, tuples.Select(p => {
					Int32.TryParse(p.GetModel<MobSkill>().MobId, out int id);
					return id;
				}));
			}
			else if (_targetSource == DataSources.Skill) {
				TabNavigation.SelectList(_targetSource, tuples.Select(p => {
					Int32.TryParse(p.GetModel<MobSkill>().SkillId, out int id);
					return id;
				}));
			}
		}
	}
}
