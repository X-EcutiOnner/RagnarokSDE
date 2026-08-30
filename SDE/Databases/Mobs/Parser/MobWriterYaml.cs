using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Mobs.Common;
using SDE.Databases.Mobs.Features;
using SDE.Editor;
using SDE.Editor.Database;
using System;
using System.Linq;
using System.Text;
using ItemDrop = SDE.Databases.Mobs.Features.ItemDrop;

namespace SDE.Databases.Mobs.Parser {
	public class MobWriterYaml : DatabaseWriterYaml {
		public override string KeyField => "Id";

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<Mob>();
			int intValue = 0;
			long longValue = 0;
			long flagValue = 0;

			builder.AppendLine($"  - Id: " + tuple.Key);
			builder.AppendLine($"    AegisName: {model.AegisName}");
			builder.AppendLine($"    Name: {model.Name}");

			if (model.JapaneseName != model.Name)
				builder.AppendLine($"    JapaneseName: {model.JapaneseName}");

			// Default values for mobs are set in s_mob_db::s_mob_db()
			if (DbReader.ToInt(model.Level, out intValue))
				builder.AppendLine($"    Level: {intValue}");

			if (DbReader.ToLong(model.Hp, out longValue) && longValue != 1)
				builder.AppendLine($"    Hp: {longValue}");

			if (DbReader.ToLong(model.Sp, out longValue) && longValue != 1)
				builder.AppendLine($"    Sp: {longValue}");

			if (DbReader.ToLong(model.BaseExp, out longValue) && longValue != 0)
				builder.AppendLine($"    BaseExp: {longValue}");

			if (DbReader.ToLong(model.JobExp, out longValue) && longValue != 0)
				builder.AppendLine($"    JobExp: {longValue}");

			if (DbReader.ToLong(model.MvpExp, out longValue) && longValue != 0)
				builder.AppendLine($"    MvpExp: {longValue}");

			if (model.IsCsv) {
				int level = DbReader.ToInt(model.Level);
				int str = DbReader.ToInt(model.Str);
				var minAttack = DbReader.ToInt(model.Attack) - level - str;
				var maxAttack = DbReader.ToInt(model.Attack2) - level - str;

				intValue = (minAttack + maxAttack) / 2;

				if (intValue != 0)
					builder.AppendLine($"    Attack: {intValue}");
			}
			else {
				if (DbReader.ToInt(model.Attack, out intValue) && intValue != 0)
					builder.AppendLine($"    Attack: {intValue}");

				if (DbReader.ToInt(model.Attack2, out intValue) && intValue != 0)
					builder.AppendLine($"    Attack2: {intValue}");
			}

			if (DbReader.ToInt(model.Defense, out intValue) && intValue != 0)
				builder.AppendLine($"    Defense: {intValue}");

			if (DbReader.ToInt(model.MagicDefense, out intValue) && intValue != 0)
				builder.AppendLine($"    MagicDefense: {intValue}");

			if (ProjectConfiguration.IsRenewal) {
				if (DbReader.ToInt(model.Resistance, out intValue) && intValue != 0)
					builder.AppendLine($"    Resistance: {intValue}");

				if (DbReader.ToInt(model.MagicResistance, out intValue) && intValue != 0)
					builder.AppendLine($"    MagicResistance: {intValue}");
			}

			if (DbReader.ToInt(model.Str, out intValue) && intValue != 1)
				builder.AppendLine($"    Str: {intValue}");

			if (DbReader.ToInt(model.Agi, out intValue) && intValue != 1)
				builder.AppendLine($"    Agi: {intValue}");

			if (DbReader.ToInt(model.Vit, out intValue) && intValue != 1)
				builder.AppendLine($"    Vit: {intValue}");

			if (DbReader.ToInt(model.Int, out intValue) && intValue != 1)
				builder.AppendLine($"    Int: {intValue}");

			if (DbReader.ToInt(model.Dex, out intValue) && intValue != 1)
				builder.AppendLine($"    Dex: {intValue}");

			if (DbReader.ToInt(model.Luk, out intValue) && intValue != 1)
				builder.AppendLine($"    Luk: {intValue}");

			if (DbReader.ToInt(model.AttackRange, out intValue) && intValue != 0)
				builder.AppendLine($"    AttackRange: {intValue}");

			if (DbReader.ToInt(model.SkillRange, out intValue) && intValue != 0)
				builder.AppendLine($"    SkillRange: {intValue}");

			if (DbReader.ToInt(model.ChaseRange, out intValue) && intValue != 0)
				builder.AppendLine($"    ChaseRange: {intValue}");

			//if (model.Size != SizeType.Size_Small)
				builder.AppendLine($"    Size: " + EnumInfos.ToYamlString(model.Size));

			//if (model.Race != RaceType.RC_FORMLESS)
				builder.AppendLine($"    Race: " + EnumInfos.ToYamlString(model.Race));

			if (DbReader.ToLong(model.RaceGroups, out flagValue) && flagValue != 0)
				DbWriter.ExpandFlagToBool<Race2Flag>(builder, flagValue, "RaceGroups", "    ");

			//if (model.Element != ElementType.ELE_NEUTRAL)
				builder.AppendLine($"    Element: " + EnumInfos.ToYamlString(model.Element));

			//if (model.ElementLevel != ElementLevelType.ELELV_1)
				builder.AppendLine($"    ElementLevel: {(int)model.ElementLevel}");

			if (DbReader.ToInt(model.WalkSpeed, out intValue) /*&& intValue != 150*/)
				builder.AppendLine($"    WalkSpeed: {intValue}");

			if (DbReader.ToInt(model.AttackDelay, out intValue) && intValue != 100)
				builder.AppendLine($"    AttackDelay: {intValue}");

			int aMotion;
			if (DbReader.ToInt(model.AttackMotion, out aMotion) && aMotion != 100)
				builder.AppendLine($"    AttackMotion: {aMotion}");

			if (!String.IsNullOrEmpty(model.ClientAttackMotion) && DbReader.ToInt(model.ClientAttackMotion, out intValue) && intValue != aMotion)
				builder.AppendLine($"    ClientAttackMotion: {intValue}");

			if (DbReader.ToInt(model.DamageMotion, out intValue) && intValue != 0)
				builder.AppendLine($"    DamageMotion: {intValue}");

			if (DbReader.ToInt(model.DamageTaken, out intValue) && intValue != 100)
				builder.AppendLine($"    DamageTaken: {intValue}");

			if (DbReader.ToInt(model.GroupId, out intValue) && intValue != 0)
				builder.AppendLine($"    GroupId: {intValue}");

			if (!DbReader.IsNullOrEmpty(model.Title))
				builder.AppendLine($"    Title: {model.Title}");

			var aiModeResult = GetAiAndModeFlag(DbReader.ToLong(model.Modes));

			if (aiModeResult.HasAi) {
				var r = EnumInfos.ToYamlString(aiModeResult.Ai);

				// Redirect 12 -> 05, because rAthena prefers 05 (mode 12 and 05 are the same thing)
				if (r == "12")
					r = "05";

				builder.AppendLine($"    Ai: " + r);
			}
			
			if (model.Class != ClassType.CLASS_NORMAL)
				builder.AppendLine($"    Class: " + EnumInfos.ToYamlString(model.Class));

			if (aiModeResult.HasModes)
				DbWriter.ExpandFlagToBool<ModeFlag>(builder, (long)aiModeResult.Modes, "Modes", "    ");

			if (model.MvpDrops.Any(p => !String.IsNullOrEmpty(p.Item))) {
				builder.AppendLine("    MvpDrops:");

				foreach (var drop in model.MvpDrops) {
					WriteItemDrop(builder, drop);
				}
			}

			if (model.Drops.Any(p => !String.IsNullOrEmpty(p.Item))) {
				builder.AppendLine("    Drops:");

				foreach (var drop in model.Drops) {
					WriteItemDrop(builder, drop);
				}
			}
		}

		public void WriteItemDrop(StringBuilder builder, ItemDrop drop) {
			if (String.IsNullOrEmpty(drop.Item))
				return;

			builder.AppendLine("      - Item: " + DbUtilities.ItemId2AegisName(drop.Item));
			builder.AppendLine("        Rate: " + DbReader.ToInt(drop.Rate));

			if (drop.StealProtected)
				builder.AppendLine("        StealProtected: true");

			if (!String.IsNullOrEmpty(drop.RandomOptionGroup))
				builder.AppendLine("        RandomOptionGroup: " + drop.RandomOptionGroup);
		}

		public class MobFlagModesResult {
			public bool HasAi;
			public bool HasModes;

			public MonsterType Ai;
			public ModeFlag Modes;

			public void SetAi(MonsterType ai) {
				HasAi = true;
				Ai = ai;
			}

			public void SetModes(ModeFlag modes) {
				HasModes = true;
				Modes = modes;
			}
		}

		public MobFlagModesResult GetAiAndModeFlag(long modes) {
			MobFlagModesResult result = new MobFlagModesResult();

			// Test directly against the list of mob AIs to see if there's a direct match.
			// The majority of monsters use a simple AI mode, so this is much faster.
			if (MonsterTypeInfo.ValueToInfo.TryGetValue(modes, out var enumInfoData)) {
				result.SetAi((MonsterType)enumInfoData.Value);
			}
			else {
				int bestIndex = -1;
				int bestCount = int.MaxValue;
				long bestRemainModes = 0;

				for (int i = 0; i < MonsterTypeInfo.All.Count; i++) {
					long aiValue = MonsterTypeInfo.All[i].ValueLong;

					if ((modes & aiValue) != aiValue)
						continue;

					long remainModes = modes & ~aiValue;
					int count = Core.Extensions.PopCount(remainModes);

					if (count < bestCount) {
						bestCount = count;
						bestIndex = i;
						bestRemainModes = remainModes;

						// Can't do better than zero.
						if (count == 0)
							break;
					}
				}

				if (bestIndex == -1) {
					result.SetModes((ModeFlag)modes);
				}
				else {
					result.SetAi((MonsterType)MonsterTypeInfo.All[bestIndex].Value);
					result.SetModes((ModeFlag)bestRemainModes);
				}
			}

 			return result;
	  	}
	}
}
