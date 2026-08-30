using SDE.Databases.Generic.Parser;
using SDE.Databases.Skills.Common;
using SDE.Databases.Skills.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using System;
using System.Linq;
using System.Text;
using Utilities;

namespace SDE.Databases.Skills.Parser {
	public class SkillReaderYaml : DatabaseReaderYaml<int> {
		public override string KeyField => "Id";

		public override void ReadEntry(DbLoadContext context, ParserObject skill) {
			int id = Int32.Parse(skill[KeyField]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Skill>();
			Skill previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (Skill)model.Clone();
			}

			foreach (var entry in skill.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "Name":
						model.Name = entry.ObjectValue;
						break;
					case "Description":
						model.Description = entry.ObjectValue;
						break;
					case "MaxLevel":
						model.MaxLevel = entry.ObjectValue;
						break;
					case "Type":
						model.BF_Type = DbReader.LoadEnum(entry, BattleFlagType.BF_NONE);
						break;
					case "TargetType":
						model.INF_TargetType = DbReader.LoadEnum(entry, SkillTargetType.INF_PASSIVE_SKILL);
						break;
					case "DamageFlags":
						model.NK_DamageFlags = DbReader.LoadFlag<DamageFlag>(entry.Value);
						break;
					case "Flags":
						model.INF2_Flags = DbReader.LoadFlag<Inf2Flag>(entry.Value);
						break;
					case "Range":
						DbReader.LevelListToString(ref model.Range, entry, "Level", "Size");
						break;
					case "Hit":
						model.DMG_Hit = DbReader.LoadEnum(entry, DamageType.DMG_NORMAL);
						break;
					case "HitCount":
						DbReader.LevelListToString(ref model.HitCount, entry, "Level", "Count");
						break;
					case "Element":
						DbReader.LevelListToString(ref model.Element, entry, "Level", "Element");
						break;
					case "SplashArea":
						DbReader.LevelListToString(ref model.SplashArea, entry, "Level", "Area");
						break;
					case "ActiveInstance":
						DbReader.LevelListToString(ref model.ActiveInstance, entry, "Level", "Max");
						break;
					case "Knockback":
						DbReader.LevelListToString(ref model.Knockback, entry, "Level", "Amount");
						break;
					case "GiveAp":
						DbReader.LevelListToString(ref model.GiveAp, entry, "Level", "Amount");
						break;
					case "CopyFlags":
						foreach (ParserKeyValue copyFlagEntry in entry.Value) {
							switch (copyFlagEntry.Key) {
								case "Skill":
									model.CopyFlagsSkill = DbReader.LoadFlag<SkillCopyFlag>(copyFlagEntry.Value);
									break;
								case "RemoveRequirement":
									model.CopyFlagsRemoveRequirement = DbReader.LoadFlag<SkillRequireFlag>(copyFlagEntry.Value);
									break;
							}
						}
						break;
					case "NoNearNPC":
						foreach (ParserKeyValue noNearNpcEntry in entry.Value) {
							switch (noNearNpcEntry.Key) {
								case "AdditionalRange":
									model.NoNearNPCRange = noNearNpcEntry.ObjectValue;
									break;
								case "Type":
									model.NoNearNPCType = DbReader.LoadFlag<NoNearNpcFlag>(noNearNpcEntry.Value);
									break;
							}
						}

						break;
					case "CastCancel":
						model.CastCancel = Boolean.Parse(entry.ObjectValue);
						break;
					case "CastDefenseReduction":
						model.CastDefenseReduction = entry.ObjectValue;
						break;
					case "CastTime":
						DbReader.LevelListToString(ref model.CastTime, entry.Value, "Level", "Time");
						break;
					case "AfterCastActDelay":
						DbReader.LevelListToString(ref model.AfterCastActDelay, entry.Value, "Level", "Time");
						break;
					case "AfterCastWalkDelay":
						DbReader.LevelListToString(ref model.AfterCastWalkDelay, entry.Value, "Level", "Time");
						break;
					case "Duration1":
						DbReader.LevelListToString(ref model.Duration1, entry.Value, "Level", "Time");
						break;
					case "Duration2":
						DbReader.LevelListToString(ref model.Duration2, entry.Value, "Level", "Time");
						break;
					case "Cooldown":
						DbReader.LevelListToString(ref model.Cooldown, entry.Value, "Level", "Time");
						break;
					case "FixedCastTime":
						DbReader.LevelListToString(ref model.FixedCastTime, entry.Value, "Level", "Time");
						break;
					case "CastTimeFlags":
						model.CastTimeFlags = DbReader.LoadFlag<SkillCastFlag>(entry.Value);
						break;
					case "CastDelayFlags":
						model.CastDelayFlags = DbReader.LoadFlag<SkillCastFlag>(entry.Value);
						break;
					case "Requires":
						var requires = entry.Value;

						foreach (var requireEntry in requires.OfType<ParserKeyValue>()) {
							switch (requireEntry.Key) {
								case "HpCost":
									DbReader.LevelListToString(ref model.Require.HpCost, requireEntry.Value, "Level", "Amount");
									break;
								case "SpCost":
									DbReader.LevelListToString(ref model.Require.SpCost, requireEntry.Value, "Level", "Amount");
									break;
								case "ApCost":
									DbReader.LevelListToString(ref model.Require.ApCost, requireEntry.Value, "Level", "Amount");
									break;
								case "HpRateCost":
									DbReader.LevelListToString(ref model.Require.HpRateCost, requireEntry.Value, "Level", "Amount");
									break;
								case "SpRateCost":
									DbReader.LevelListToString(ref model.Require.SpRateCost, requireEntry.Value, "Level", "Amount");
									break;
								case "ApRateCost":
									DbReader.LevelListToString(ref model.Require.ApRateCost, requireEntry.Value, "Level", "Amount");
									break;
								case "MaxHpTrigger":
									DbReader.LevelListToString(ref model.Require.MaxHpTrigger, requireEntry.Value, "Level", "Amount");
									break;
								case "ZenyCost":
									DbReader.LevelListToString(ref model.Require.ZenyCost, requireEntry.Value, "Level", "Amount");
									break;
								case "Weapon":
									model.Require.Weapon = DbReader.LoadFlag<WeaponFlag>(requireEntry.Value);
									break;
								case "Ammo":
									model.Require.Ammo = DbReader.LoadFlag<AmmoFlag>(requireEntry.Value);
									break;
								case "AmmoAmount":
									model.Require.AmmoAmount = requireEntry.ObjectValue;
									break;
								case "State":
									model.Require.State = DbReader.LoadEnum(requireEntry.Value, RequiredStateType.ST_NONE);
									break;
								case "Status":
									model.Require.Status = Methods.Aggregate(requireEntry.Value.OfType<ParserKeyValue>().Select(p => p.Key).ToList(), ":");
									break;
								case "SpiritSphereCost":
									DbReader.LevelListToString(ref model.Require.SpiritSphereCost, requireEntry.Value, "Level", "Amount");
									break;
								case "ItemCost": {
										StringBuilder b = new StringBuilder();
										var itemList = requireEntry.Value;

										foreach (var item in itemList) {
											string key = item["Item"];
											int value = Int32.Parse(item["Amount"]);
											var itemLevel = item["Level"];
											int level = 0;

											if (itemLevel != null)
												level = Int32.Parse(itemLevel);

											key = CachedDbs.AegisNameItem.ToStringId(key);
											b.Append(key);
											b.Append(":");
											b.Append(value);
											b.Append(":");
											b.Append(level);
											b.Append(":");
										}

										model.Require.ItemCost = b.ToString().Trim(':');
									}
									break;
								case "Equipment": {
										StringBuilder b = new StringBuilder();
										var itemList = requireEntry.Value;

										foreach (var item in itemList.OfType<ParserKeyValue>()) {
											string key = item.Key;

											key = CachedDbs.AegisNameItem.ToStringId(key);
											b.Append(key);
											b.Append(":");
										}

										model.Require.Equipment = b.ToString().Trim(':');
									}
									break;
							}
						}

						break;
					case "Unit":
						var unit = entry.Value;

						foreach (var unitEntry in unit.OfType<ParserKeyValue>()) {
							switch (unitEntry.Key) {
								case "Id":
									model.Unit.Id = unitEntry.ObjectValue;
									break;
								case "AlternateId":
									model.Unit.AlternateId = unitEntry.ObjectValue;
									break;
								case "Layout":
									DbReader.LevelListToString(ref model.Unit.Layout, unitEntry.Value, "Level", "Size");
									break;
								case "Range":
									DbReader.LevelListToString(ref model.Unit.Range, unitEntry.Value, "Level", "Size");
									break;
								case "Interval":
									model.Unit.Interval = unitEntry.ObjectValue;
									break;
								case "Target":
									model.Unit.Target = DbReader.LoadEnum(unitEntry.Value, BattleCheckTargetType.BCT_ALL);
									break;
								case "Flag":
									model.Unit.Flag = DbReader.LoadFlag<SkillUnitFlag>(unitEntry.Value);
									break;
							}
						}

						break;
					case "Status":
						model.Status = entry.ObjectValue;
						break;
				}
			}

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, SkillAttributes.Model, model, false);
			}
		}
	}
}
