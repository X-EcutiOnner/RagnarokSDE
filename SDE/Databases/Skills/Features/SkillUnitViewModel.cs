using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Databases.Skills.Common;
using SDE.Databases.Skills.Features;
using System.Runtime.CompilerServices;

namespace SDE.Databases.Pets.Features {
	public class SkillUnitViewModel : BaseModelView<SkillUnit> {
		private SkillViewModel _vm;

		public SkillUnitViewModel(SkillViewModel viewModel, SkillUnit model) {
			_vm = viewModel;
			Model = model;
		}

		public string Id { get => Model?.Id; set => ExecuteCommand(value); }
		public string AlternateId { get => Model?.AlternateId; set => ExecuteCommand(value); }
		public string Layout { get => Model?.Layout; set => ExecuteCommand(value); }
		public string Range { get => Model?.Range; set => ExecuteCommand(value); }
		public string Interval { get => Model?.Interval; set => ExecuteCommand(value); }
		public EnumInfoBase Target { get => EnumInfos.GetEnumBase(Model?.Target); set => ExecuteCommand((BattleCheckTargetType)value.Value); }
		public string Flag { get => Model?.Flag; set => ExecuteCommand(value); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => _vm.IsLocked = v);
		}
	}
}
