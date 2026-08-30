using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Databases.Skills.Common;
using SDE.Databases.Skills.Features;
using SDE.Editor.Database;
using SDE.View.Dialogs;
using SDE.View.Editors.ItemCostEdit;
using System.Linq;
using System.Runtime.CompilerServices;
using Utilities;

namespace SDE.Databases.Pets.Features {
	public class SkillRequireViewModel : BaseModelView<SkillRequire> {
		private SkillViewModel _vm;

		public SkillRequireViewModel(SkillViewModel viewModel, SkillRequire model) {
			_vm = viewModel;
			Model = model;
		}

		public string HpCost { get => Model?.HpCost; set => ExecuteCommand(value); }
		public string SpCost { get => Model?.SpCost; set => ExecuteCommand(value); }
		public string ApCost { get => Model?.ApCost; set => ExecuteCommand(value); }
		public string HpRateCost { get => Model?.HpRateCost; set => ExecuteCommand(value); }
		public string SpRateCost { get => Model?.SpRateCost; set => ExecuteCommand(value); }
		public string ApRateCost { get => Model?.ApRateCost; set => ExecuteCommand(value); }
		public string MaxHpTrigger { get => Model?.MaxHpTrigger; set => ExecuteCommand(value); }
		public string ZenyCost { get => Model?.ZenyCost; set => ExecuteCommand(value); }
		public string Weapon { get => Model?.Weapon; set => ExecuteCommand(value); }
		public string Ammo { get => Model?.Ammo; set => ExecuteCommand(value); }
		public string AmmoAmount { get => Model?.AmmoAmount; set => ExecuteCommand(value); }
		public EnumInfoBase State { get => EnumInfos.GetEnumBase(Model?.State); set => ExecuteCommand((RequiredStateType)value.Value); }
		public string Status { get => Model?.Status; set => ExecuteCommand(value); }
		public string SpiritSphereCost { get => Model?.SpiritSphereCost; set => ExecuteCommand(value); }

		public string ItemCost {
			get => Model?.ItemCost;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(ItemCostPreview));
			}
		}

		public string ItemCostPreview {
			get {
				if (Model == null || Model.ItemCost == null)
					return "";

				return new ItemCostContainer(Model.ItemCost, 0).GetPreview();
			}
		}

		public string Equipment {
			get => Model?.Equipment;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(EquipmentPreview));
			}
		}

		public string EquipmentPreview {
			get {
				if (Model == null || Model.Equipment == null)
					return "";

				var data = Model.Equipment.Split(':');
				return Methods.Aggregate(data.Select(p => DbUtilities.ItemId2Name(p)).ToList(), ", ");
			}
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => _vm.IsLocked = v);
		}
	}
}
