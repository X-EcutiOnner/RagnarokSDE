using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Skills.Common;
using SDE.Databases.Skills.Features;
using SDE.Editor;
using SDE.Editor.Database;
using System;
using System.Text;

namespace SDE.Databases.Skills.Parser {
	public class SkillWriterYaml : DatabaseWriterYaml {
		public override string KeyField => "Id";

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<Skill>();
			int intValue = 0;
			long flagValue = 0;

			builder.AppendLine($"  - Id: " + tuple.Key);
			builder.AppendLine($"    Name: {model.Name}");
			builder.AppendLine($"    Description: {model.Description}");
			builder.AppendLine($"    MaxLevel: {DbReader.ToInt(model.MaxLevel)}");

			if (model.BF_Type != BattleFlagType.BF_NONE)
				builder.AppendLine($"    Type: " + EnumInfos.ToYamlString(model.BF_Type));

			if (model.INF_TargetType != SkillTargetType.INF_PASSIVE_SKILL)
				builder.AppendLine($"    TargetType: " + EnumInfos.ToYamlString(model.INF_TargetType));

			if (DbReader.ToLong(model.NK_DamageFlags, out flagValue) && flagValue != 0)
				DbWriter.ExpandFlagToBool<DamageFlag>(builder, flagValue, "DamageFlags", "    ");

			if (DbReader.ToLong(model.INF2_Flags, out flagValue) && flagValue != 0)
				DbWriter.ExpandFlagToBool<Inf2Flag>(builder, flagValue, "Flags", "    ");

			if (DbReader.IsExpandString(model.Range))
				DbWriter.ExpandLevelList(builder, model.Range, "Range", "Level", "Size", "    ");

			if (model.DMG_Hit != DamageType.DMG_NORMAL)
				builder.AppendLine($"    Hit: " + EnumInfos.ToYamlString(model.DMG_Hit));

			if (DbReader.IsExpandString(model.HitCount))
				DbWriter.ExpandLevelList(builder, model.HitCount, "HitCount", "Level", "Count", "    ");

			if (model.Element != "Neutral")
				DbWriter.ExpandLevelList(builder, model.Element, "Element", "Level", "Element", "    ");

			if (DbReader.IsExpandString(model.SplashArea))
				DbWriter.ExpandLevelList(builder, model.SplashArea, "SplashArea", "Level", "Area", "    ");

			if (DbReader.IsExpandString(model.ActiveInstance))
				DbWriter.ExpandLevelList(builder, model.ActiveInstance, "ActiveInstance", "Level", "Max", "    ");

			if (DbReader.IsExpandString(model.Knockback))
				DbWriter.ExpandLevelList(builder, model.Knockback, "Knockback", "Level", "Amount", "    ");

			if (DbReader.IsExpandString(model.GiveAp))
				DbWriter.ExpandLevelList(builder, model.GiveAp, "GiveAp", "Level", "Amount", "    ");

			if (!DbReader.IsNullOrEmpty(model.CopyFlagsSkill) || !DbReader.IsNullOrEmpty(model.CopyFlagsRemoveRequirement)) {
				builder.AppendLine($"    CopyFlags:");

				if (DbReader.ToLong(model.CopyFlagsSkill, out flagValue) && flagValue != 0)
					DbWriter.ExpandFlagToBool<SkillCopyFlag>(builder, flagValue, "Skill", "      ");

				if (DbReader.ToLong(model.CopyFlagsRemoveRequirement, out flagValue) && flagValue != 0)
					DbWriter.ExpandFlagToBool<SkillRequireFlag>(builder, flagValue, "RemoveRequirement", "      ");
			}

			if (!DbReader.IsNullOrEmpty(model.NoNearNPCRange) || !DbReader.IsNullOrEmpty(model.NoNearNPCType)) {
				builder.AppendLine($"    NoNearNPC:");

				if (DbReader.ToInt(model.NoNearNPCRange, out intValue) && intValue != 0)
					builder.AppendLine($"      AdditionalRange: {intValue}");

				if (DbReader.ToLong(model.NoNearNPCType, out flagValue) && flagValue != 0)
					DbWriter.ExpandFlagToBool<NoNearNpcFlag>(builder, flagValue, "Type", "      ");
			}
			if (DbReader.ToInt(model.CastDefenseReduction, out intValue) && intValue != 0)
				builder.AppendLine($"    CastDefenseReduction: {intValue}");

			if (DbReader.IsExpandString(model.CastTime))
				DbWriter.ExpandLevelList(builder, model.CastTime, "CastTime", "Level", "Time", "    ");

			if (DbReader.IsExpandString(model.AfterCastActDelay))
				DbWriter.ExpandLevelList(builder, model.AfterCastActDelay, "AfterCastActDelay", "Level", "Time", "    ");

			if (DbReader.IsExpandString(model.AfterCastWalkDelay))
				DbWriter.ExpandLevelList(builder, model.AfterCastWalkDelay, "AfterCastWalkDelay", "Level", "Time", "    ");

			if (DbReader.IsExpandString(model.Duration1))
				DbWriter.ExpandLevelList(builder, model.Duration1, "Duration1", "Level", "Time", "    ");

			if (DbReader.IsExpandString(model.Duration2))
				DbWriter.ExpandLevelList(builder, model.Duration2, "Duration2", "Level", "Time", "    ");

			if (DbReader.IsExpandString(model.Cooldown))
				DbWriter.ExpandLevelList(builder, model.Cooldown, "Cooldown", "Level", "Time", "    ");

			if (ProjectConfiguration.IsRenewal) {
				if (DbReader.IsExpandString(model.FixedCastTime))
					DbWriter.ExpandLevelList(builder, model.FixedCastTime, "FixedCastTime", "Level", "Time", "    ");
			}

			if (DbReader.ToLong(model.CastTimeFlags, out flagValue) && flagValue != 0)
				DbWriter.ExpandFlagToBool<SkillCastFlag>(builder, flagValue, "CastTimeFlags", "    ");

			if (DbReader.ToLong(model.CastDelayFlags, out flagValue) && flagValue != 0)
				DbWriter.ExpandFlagToBool<SkillCastFlag>(builder, flagValue, "CastDelayFlags", "    ");

			string require = WriteRequire(model.Require);
			
			if (!String.IsNullOrEmpty(require)) {
				builder.AppendLine("    Requires:");
				builder.Append(require);
			}

			string unit = WriteUnit(model.Unit);

			if (!String.IsNullOrEmpty(unit)) {
				builder.AppendLine("    Unit:");
				builder.Append(unit);
			}

			if (!DbReader.IsNullOrEmpty(model.Status))
				builder.AppendLine("    Status: " + model.Status);

			if (!model.CastCancel)
				builder.AppendLine("    CastCancel: false");
		}

		public string WriteRequire(SkillRequire require) {
			StringBuilder builder = new StringBuilder();

			int intValue = 0;
			long flagValue = 0;

			if (DbReader.IsExpandString(require.HpCost))
				DbWriter.ExpandLevelList(builder, require.HpCost, "HpCost", "Level", "Amount", "      ");

			if (DbReader.IsExpandString(require.SpCost))
				DbWriter.ExpandLevelList(builder, require.SpCost, "SpCost", "Level", "Amount", "      ");

			if (DbReader.IsExpandString(require.ApCost))
				DbWriter.ExpandLevelList(builder, require.ApCost, "ApCost", "Level", "Amount", "      ");

			if (DbReader.IsExpandString(require.HpRateCost))
				DbWriter.ExpandLevelList(builder, require.HpRateCost, "HpRateCost", "Level", "Amount", "      ");

			if (DbReader.IsExpandString(require.SpRateCost))
				DbWriter.ExpandLevelList(builder, require.SpRateCost, "SpRateCost", "Level", "Amount", "      ");

			if (DbReader.IsExpandString(require.ApRateCost))
				DbWriter.ExpandLevelList(builder, require.ApRateCost, "ApRateCost", "Level", "Amount", "      ");

			if (DbReader.IsExpandString(require.MaxHpTrigger))
				DbWriter.ExpandLevelList(builder, require.MaxHpTrigger, "MaxHpTrigger", "Level", "Amount", "      ");

			if (DbReader.IsExpandString(require.ZenyCost))
				DbWriter.ExpandLevelList(builder, require.ZenyCost, "ZenyCost", "Level", "Amount", "      ");

			if (DbReader.ToLong(require.Weapon, out flagValue) && flagValue != 0)
				DbWriter.ExpandFlagToBool<WeaponFlag>(builder, flagValue, "Weapon", "      ");

			if (DbReader.ToLong(require.Ammo, out flagValue) && flagValue != 0)
				DbWriter.ExpandFlagToBool<AmmoFlag>(builder, flagValue, "Ammo", "      ");

			if (DbReader.IsExpandString(require.AmmoAmount))
				DbWriter.ExpandLevelList(builder, require.AmmoAmount, "AmmoAmount", "Level", "Amount", "      ");

			if (require.State != RequiredStateType.ST_NONE)
				builder.AppendLine($"      State: " + EnumInfos.ToYamlString(require.State));

			if (!DbReader.IsNullOrEmpty(require.Status))
				DbWriter.ExpandArrayToBool(builder, require.Status, "Status", "      ");

			if (DbReader.IsExpandString(require.SpiritSphereCost))
				DbWriter.ExpandLevelList(builder, require.SpiritSphereCost, "SpiritSphereCost", "Level", "Amount", "      ");

			if (!DbReader.IsNullOrEmpty(require.ItemCost)) {
				var data = require.ItemCost.Split(':');

				if ((data.Length % 3) == 0 && data.Length > 0) {
					builder.AppendLine("      ItemCost:");

					for (int i = 0; i < data.Length; i += 3) {
						builder.AppendLine("        - Item: " + DbUtilities.ItemId2AegisName(data[i]));
						builder.AppendLine("          Amount: " + DbReader.ToInt(data[i + 1]));

						if (DbReader.ToInt(data[i + 2], out intValue) && intValue != 0)
							builder.AppendLine("          Level: " + intValue);
					}
				}
			}

			if (!DbReader.IsNullOrEmpty(require.Equipment)) {
				var data = require.Equipment.Split(':');

				builder.AppendLine("      Equipment:");

				foreach (var item in data) {
					builder.AppendLine("        " + DbUtilities.ItemId2AegisName(item) + ": true");
				}
			}

			return builder.ToString();
		}

		public string WriteUnit(SkillUnit unit) {
			StringBuilder builder = new StringBuilder();

			int intValue = 0;
			long flagValue = 0;

			if (String.IsNullOrEmpty(unit.Id))
				return "";

			builder.AppendLine($"      Id: {unit.Id}");

			if (!String.IsNullOrEmpty(unit.AlternateId))
				builder.AppendLine($"      AlternateId: {unit.AlternateId}");

			if (DbReader.IsExpandString(unit.Layout))
				DbWriter.ExpandLevelList(builder, unit.Layout, "Layout", "Level", "Size", "      ");

			if (DbReader.IsExpandString(unit.Range))
				DbWriter.ExpandLevelList(builder, unit.Range, "Range", "Level", "Size", "      ");

			if (DbReader.ToInt(unit.Interval, out intValue) && intValue != 0)
				builder.AppendLine($"      Interval: {intValue}");

			if (unit.Target != BattleCheckTargetType.BCT_ALL)
				builder.AppendLine($"      Target: " + EnumInfos.ToYamlString(unit.Target));

			if (DbReader.ToLong(unit.Flag, out flagValue) && flagValue != 0)
				DbWriter.ExpandFlagToBool<SkillUnitFlag>(builder, flagValue, "Flag", "      ");

			return builder.ToString();
		}
	}
}
