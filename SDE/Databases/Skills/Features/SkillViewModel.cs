using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Pets.Features;
using SDE.Databases.Skills.Common;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SDE.Databases.Skills.Features {
	public class SkillViewModel : BaseModelView<Skill> {
		private SkillRequireViewModel _require;
		private SkillUnitViewModel _unit;
		public bool IsLocked { get; set; }

		public SkillViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, Skill model) {
			if (IsLocked)
				return;

			Model = model;
			_require = new SkillRequireViewModel(this, Model?.Require);
			_unit = new SkillUnitViewModel(this, Model?.Unit);

			Tuple = tuple;

			OnPropertyChanged("");
		}

		public string Id {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple?.Key.ToString() == value)
					return;

				if (int.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Id));
			}
		}

		public string Name { get => Model?.Name; set => ExecuteCommand(value); }
		public string Description { get => Model?.Description; set => ExecuteCommand(value); }
		public string MaxLevel { get => Model?.MaxLevel; set => ExecuteCommand(value); }
		public EnumInfoBase BF_Type { get => EnumInfos.GetEnumBase(Model?.BF_Type); set => ExecuteCommand((BattleFlagType)value.Value); }
		public EnumInfoBase INF_TargetType { get => EnumInfos.GetEnumBase(Model?.INF_TargetType); set => ExecuteCommand((SkillTargetType)value.Value); }
		public string NK_DamageFlags { get => Model?.NK_DamageFlags; set => ExecuteCommand(value); }
		public string INF2_Flags { get => Model?.INF2_Flags; set => ExecuteCommand(value); }
		public string Range { get => Model?.Range; set => ExecuteCommand(value); }
		public EnumInfoBase DMG_Hit { get => EnumInfos.GetEnumBase(Model?.DMG_Hit); set => ExecuteCommand((DamageType)value.Value); }
		public string HitCount { get => Model?.HitCount; set => ExecuteCommand(value); }
		
		public string Element {
			get => Model?.Element;
			set {
				ExecuteCommand(value);
				OnPropertyChanged(nameof(IsAllIdenticalElement));
				OnPropertyChanged(nameof(IsNotAllIdenticalElement));
			}
		}

		public EnumInfoBase Element_ComboBoxSelected {
			get {
				var enumData = EnumInfos.GetEnumTypeToInfo<SkillElementType>();

				if (Model == null)
					return enumData[SkillElementType.ELE_NEUTRAL];

				var element = Model.Element.Split(':').FirstOrDefault();

				if (element == null)
					return enumData[SkillElementType.ELE_NEUTRAL];

				SkillElementType r = SkillElementType.ELE_NEUTRAL;
				
				if (!DbReader.LoadEnum(ref r, element, false))
					return null;

				return enumData[r];
			}
			set {
				ExecuteCommand(value.YamlName, nameof(Element));
				OnPropertyChanged(nameof(Element_ComboBoxSelected));
			}
		}

		public bool IsNotAllIdenticalElement {
			get => !IsAllIdenticalElement;
		}

		public bool IsAllIdenticalElement {
			get => Model == null ? true : !(Model.Element ?? "").Contains(":");
		}

		public string SplashArea { get => Model?.SplashArea; set => ExecuteCommand(value); }
		public string ActiveInstance { get => Model?.ActiveInstance; set => ExecuteCommand(value); }
		public string Knockback { get => Model?.Knockback; set => ExecuteCommand(value); }
		public string GiveAp { get => Model?.GiveAp; set => ExecuteCommand(value); }
		public string CopyFlagsSkill { get => Model?.CopyFlagsSkill; set => ExecuteCommand(value); }
		public string CopyFlagsRemoveRequirement { get => Model?.CopyFlagsRemoveRequirement; set => ExecuteCommand(value); }
		public string NoNearNPCRange { get => Model?.NoNearNPCRange; set => ExecuteCommand(value); }
		public string NoNearNPCType { get => Model?.NoNearNPCType; set => ExecuteCommand(value); }
		public bool CastCancel { get => Model == null ? true : Model.CastCancel; set => ExecuteCommand(value); }		
		public string CastDefenseReduction { get => Model?.CastDefenseReduction; set => ExecuteCommand(value); }
		public string CastTime { get => Model?.CastTime; set => ExecuteCommand(value); }
		public string AfterCastActDelay { get => Model?.AfterCastActDelay; set => ExecuteCommand(value); }
		public string AfterCastWalkDelay { get => Model?.AfterCastWalkDelay; set => ExecuteCommand(value); }
		public string Duration1 { get => Model?.Duration1; set => ExecuteCommand(value); }
		public string Duration2 { get => Model?.Duration2; set => ExecuteCommand(value); }
		public string Cooldown { get => Model?.Cooldown; set => ExecuteCommand(value); }
		public string FixedCastTime { get => Model?.FixedCastTime; set => ExecuteCommand(value); }
		public string CastTimeFlags { get => Model?.CastTimeFlags; set => ExecuteCommand(value); }
		public string CastDelayFlags { get => Model?.CastDelayFlags; set => ExecuteCommand(value); }
		public SkillRequireViewModel Require { get => _require; }
		public SkillUnitViewModel Unit { get => _unit; }
		public string Status { get => Model?.Status; set => ExecuteCommand(value); }
		public string NoCastFlags { get => Model?.NoCastFlags; set => ExecuteCommand(value); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}
	}
}
