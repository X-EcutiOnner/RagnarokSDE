using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Databases.MobSkills.Common;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SDE.Databases.MobSkills.Features {
	public class MobSkillViewModel : BaseModelView<MobSkill> {
		public bool IsLocked { get; set; }

		public MobSkillViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, MobSkill model) {
			if (IsLocked)
				return;

			Model = model;
			Tuple = tuple;

			OnPropertyChanged("");
		}

		public Brush ForegroundBrush => ReadableTupleBrush.TextForeground;

		public string MobId { get => Model?.MobId; set => ExecuteCommand(value); }
		public string FriendlyDisplay { get => Model?.FriendlyDisplay; set => ExecuteCommand(value); }
		public EnumInfoBase State { get => EnumInfos.GetEnumBase(Model?.State, MobSkillStateType.MSS_ANY); set => ExecuteCommand((MobSkillStateType)value.Value); }
		public string SkillId { get => Model?.SkillId; set => ExecuteCommand(value); }
		public string SkillLv { get => Model?.SkillLv; set => ExecuteCommand(value); }
		public string Rate { get => Model?.Rate; set => ExecuteCommand(value); }
		public string CastTime { get => Model?.CastTime; set => ExecuteCommand(value); }
		public string Delay { get => Model?.Delay; set => ExecuteCommand(value); }
		public bool Cancelable { get => Model == null ? false : Model.Cancelable; set => ExecuteCommand(value); }
		public EnumInfoBase Target { get => EnumInfos.GetEnumBase(Model?.Target, MobSkillTargetType.MST_TARGET); set => ExecuteCommand((MobSkillTargetType)value.Value); }
		
		public EnumInfoBase Cond1 {
			get => EnumInfos.GetEnumBase(Model?.Cond1, MobSkillCond1Type.MSC_ALWAYS);
			set {
				ExecuteCommand((MobSkillCond1Type)value.Value);
				OnPropertyChanged(nameof(IsCValueEnum));
			}
		}

		public bool IsCValueEnum {
			get {
				if (Model == null)
					return false;

				switch (Model.Cond1) {
					case MobSkillCond1Type.MSC_MYSTATUSON:
					case MobSkillCond1Type.MSC_MYSTATUSOFF:
					case MobSkillCond1Type.MSC_FRIENDSTATUSON:
					case MobSkillCond1Type.MSC_FRIENDSTATUSOFF:
						return true;
				}

				return false;
			}
		}

		public object CValue {
			get {
				if (Model == null)
					return null;

				if (IsCValueEnum) {
					if (Model.CValue is MobSkillCond2Type cond2)
						return EnumInfos.GetEnumBase(cond2, MobSkillCond2Type.SC_ANYBAD);

					return null;
				}
				else {
					return Model.CValue as string;
				}
			}
			set {
				if (IsCValueEnum) {
					if (value is EnumInfoBase) {
						ExecuteCommand((MobSkillCond2Type)((EnumInfoBase)value).Value);
					}
				}
				else if (value is string) {
					ExecuteCommand(value);
				}
			}
		}

		public string Val1 { get => Model?.Val1; set => ExecuteCommand(value); }
		public string Val2 { get => Model?.Val2; set => ExecuteCommand(value); }
		public string Val3 { get => Model?.Val3; set => ExecuteCommand(value); }
		public string Val4 { get => Model?.Val4; set => ExecuteCommand(value); }
		public string Val5 { get => Model?.Val5; set => ExecuteCommand(value); }
		public string Emotion { get => Model?.Emotion; set => ExecuteCommand(value); }
		public string Chat { get => Model?.Chat; set => ExecuteCommand(value); }

		public string SkillNamePreview {
			get {
				var r = DbUtilities.SkillId2Description(Model?.SkillId);
				return String.IsNullOrEmpty(r) ? "#Not found - " + SkillId : r;
			}
		}
		public string ConditionPreview {
			get {
				string condition = EnumInfos.GetEnumTypeToInfo<MobSkillCond1Type>()[Model?.Cond1].DisplayName;
				var r = condition.
					Replace("[CValue]", (Model?.CValue ?? "").ToString()).
					Replace("[Val1]", (Model?.Val1 ?? "").ToString());
				return r;
			}
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}
	}
}
