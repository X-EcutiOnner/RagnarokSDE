using ErrorManager;
using SDE.ApplicationConfiguration;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Skills.Common;
using SDE.Databases.Skills.Features;
using SDE.Editor.Database;
using SDE.Editor.Files;
using SDE.Editor.Generic.Parsers.Generic;
using SDE.Editor.Parsers;
using System;
using System.IO;
using System.Linq;
using Utilities;
using Utilities.Extension;

namespace SDE.Databases.Skills.Parser {
	public class SkillReaderCsv : DatabaseReaderCsv<int> {
		public override void Loader(DbLoadContext context, BaseDatabase db) {
			bool isImport = context.Source != DataSources.Skill;

			LoadSkill(db, isImport);
			LoadSkillCast(db, isImport);
			LoadSkillNoCast(db, isImport);
			LoadSkillRequirement(db, isImport);
			LoadSkillCopyable(db, isImport);
			LoadSkillUnit(db, isImport);
			LoadSkillNoDex(db, isImport);
		}

		public void LoadSkill(BaseDatabase db, bool isImport) => Load(db, DataSources.Skill, isImport, ReadEntry);
		public void LoadSkillCast(BaseDatabase db, bool isImport) => Load(db, DataSources.SkillCast, isImport, ReadEntryCast);
		public void LoadSkillNoCast(BaseDatabase db, bool isImport) => Load(db, DataSources.SkillNoCast, isImport, ReadEntryNoCast);
		public void LoadSkillRequirement(BaseDatabase db, bool isImport) => Load(db, DataSources.SkillRequirement, isImport, ReadEntryRequirement);
		public void LoadSkillCopyable(BaseDatabase db, bool isImport) => Load(db, DataSources.SkillCopyable, isImport, ReadEntryCopyable);
		public void LoadSkillUnit(BaseDatabase db, bool isImport) => Load(db, DataSources.SkillUnit, isImport, ReadEntryUnit);
		public void LoadSkillNoDex(BaseDatabase db, bool isImport) => Load(db, DataSources.SkillNoDex, isImport, ReadEntryNoDex);

		public void Load(BaseDatabase db, DataSource source, bool isImport, Action<DbLoadContext, string[]> readEntryFunc) {
			var context = new DbLoadContext(db);

			if (isImport)
				source = source.ImportTable;

			if (!context.PrepareRead(source)) return;

			if (!File.Exists(context.FilePath)) {
				if (db.ThrowFileNotFoundException)
					DbIOErrorHandler.FileNotFound(db.Source);

				return;
			}

			foreach (string[] elements in TextFileHelper.GetElementsByCommasAll(File.ReadAllBytes(context.FilePath))) {
				try {
					readEntryFunc(context, elements);
				}
				catch {
					if (elements.Length <= 0) {
						if (!context.ReportIdException("#")) return;
					}
					else if (!context.ReportIdException(elements[0])) return;
				}
			}
		}

		public void ReadEntryCast(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			model.CastTime = DbReader.SetEmptyIfZero(elements[1]);
			model.AfterCastActDelay = DbReader.SetEmptyIfZero(elements[2]);
			model.AfterCastWalkDelay = DbReader.SetEmptyIfZero(elements[3]);
			model.Duration1 = DbReader.SetEmptyIfZero(elements[4]);
			model.Duration2 = DbReader.SetEmptyIfZero(elements[5]);
			model.Cooldown = DbReader.SetEmptyIfZero(elements[6]);

			if (7 < elements.Length)
				model.FixedCastTime = DbReader.SetEmptyIfZero(elements[7]);
		}

		public void ReadEntryNoCast(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			model.NoCastFlags = elements[1];
		}

		public void ReadEntryRequirement(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);
			int eleIdx = 1;
			int intValue;
			var require = model.Require;

			require.HpCost = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			require.MaxHpTrigger = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			require.SpCost = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			require.HpRateCost = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			require.SpRateCost = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			require.ZenyCost = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			require.Weapon = DbReader.SetEmptyIfZero(elements[eleIdx++]);

			long weaponFlag = 0;

			foreach (var w in require.Weapon.Split(':')) {
				if (Int32.TryParse(w, out intValue)) {
					if (intValue == 99)
						weaponFlag = 0;
					else
						weaponFlag |= (long)(1 << intValue);
				}
			}

			require.Weapon = weaponFlag.ToString();

			require.Ammo = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			require.AmmoAmount = DbReader.SetEmptyIfZero(elements[eleIdx++]);

			var e = elements[eleIdx++].ToLower();

			switch (e) {
				case "none": require.State = RequiredStateType.ST_NONE; break;
				case "hidden": require.State = RequiredStateType.ST_HIDDEN; break;
				case "riding": require.State = RequiredStateType.ST_RIDING; break;
				case "falcon": require.State = RequiredStateType.ST_FALCON; break;
				case "cart": require.State = RequiredStateType.ST_CART; break;
				case "shield": require.State = RequiredStateType.ST_SHIELD; break;
				case "recover_weight_rate": require.State = RequiredStateType.ST_RECOVER_WEIGHT_RATE; break;
				case "move_enable": require.State = RequiredStateType.ST_MOVE_ENABLE; break;
				case "water": require.State = RequiredStateType.ST_WATER; break;
				case "dragon": require.State = RequiredStateType.ST_RIDINGDRAGON; break;
				case "warg": require.State = RequiredStateType.ST_WUG; break;
				case "ridingwarg": require.State = RequiredStateType.ST_RIDINGWUG; break;
				case "mado": require.State = RequiredStateType.ST_MADO; break;
				case "elementalspirit": require.State = RequiredStateType.ST_ELEMENTALSPIRIT; break;
				case "elementalspirit2": require.State = RequiredStateType.ST_ELEMENTALSPIRIT2; break;
				case "peco": require.State = RequiredStateType.ST_PECO; break;
				default:
					throw new Exception("Unknown state");
			}

			require.Status = DbReader.SetEmptyIfZero(Methods.Aggregate(elements[eleIdx++].Split(':').Select(p => p.ReplaceFirst("SC_", "")).ToList(), ":"));
			require.SpiritSphereCost = DbReader.SetEmptyIfZero(elements[eleIdx++]);

			require.ItemCost = "";

			for (int i = 0; i < 10; i++, eleIdx += 2) {
				if (Int32.TryParse(elements[eleIdx], out intValue) && intValue != 0) {
					// ItemId:Amount:Level
					require.ItemCost += intValue + ":" + DbReader.ToInt(elements[eleIdx + 1]) + ":0:";
				}
			}

			require.ItemCost = require.ItemCost.TrimEnd(':');
			require.Equipment = DbReader.SetEmptyIfZero(elements[eleIdx++]);
		}

		public void ReadEntryCopyable(DbLoadContext context, string[] elements) {
			var idString = CachedDbs.SkillName.ToStringId(elements[0].Trim());

			if (!Int32.TryParse(idString, out int id))
				return;

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Skill>();

			model.CopyFlagsSkill = elements[1];

			if (2 < elements.Length) {
				model.CopyJobAllowed = elements[2];
			}

			if (3 < elements.Length) {
				model.CopyFlagsRemoveRequirement = elements[3];
			}

			tuple.SetRawValue(SkillAttributes.CopyableFileKeyRef, TextFileHelper.LastLineRead2);
		}

		public void ReadEntryUnit(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			int eleIdx = 1;
			var unit = model.Unit;
			unit.Id = DbReader.SetEmptyIfZero(elements[eleIdx++].Trim());
			unit.AlternateId = DbReader.SetEmptyIfZero(elements[eleIdx++].Trim());
			unit.Layout = DbReader.SetEmptyIfZero(elements[eleIdx++].Trim());
			unit.Range = DbReader.SetEmptyIfZero(elements[eleIdx++].Trim());
			unit.Interval = DbReader.SetEmptyIfZero(elements[eleIdx++].Trim());

			// This is not actually accurate, for example, "friend" redirects to BCT_NOENEMY, same as "noenemy"
			// But let's keep it simple... Yaml doesn't recognize this as a flag, but an enum.
			// So it's not really possible to convert one with another.
			switch (elements[eleIdx++].Trim().ToLower()) {
				case "noenemy": unit.Target = BattleCheckTargetType.BCT_NOENEMY; break;
				case "friend": unit.Target = BattleCheckTargetType.BCT_FRIEND; break;
				case "party": unit.Target = BattleCheckTargetType.BCT_PARTY; break;
				case "ally": unit.Target = BattleCheckTargetType.BCT_ALLY; break;
				case "guild": unit.Target = BattleCheckTargetType.BCT_GUILD; break;
				case "all": unit.Target = BattleCheckTargetType.BCT_ALL; break;
				case "enemy": unit.Target = BattleCheckTargetType.BCT_ENEMY; break;
				case "self": unit.Target = BattleCheckTargetType.BCT_SELF; break;
				case "sameguild": unit.Target = BattleCheckTargetType.BCT_SAMEGUILD; break;
				case "noone": unit.Target = BattleCheckTargetType.BCT_NOONE; break;
				default: unit.Target = (BattleCheckTargetType)elements[eleIdx - 1].ToInt(); break;
			}

			unit.Flag = "0x" + (elements[eleIdx++].Trim().ToLong() << 1).ToString("X");
		}

		public void ReadEntryNoDex(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			model.CastTimeFlags = elements[1];

			if (2 < elements.Length) {
				model.CastDelayFlags = elements[2];
			}
		}

		public override void ReadEntry(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);
			
			int eleIdx = 1;

			model.Range = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			model.DMG_Hit = (DamageType)elements[eleIdx++].ToInt();
			model.INF_TargetType = (SkillTargetType)elements[eleIdx++].ToInt();

			var modelElements = elements[eleIdx++].Split(':');
			var modelElementOutput = "";

			foreach (var ele in modelElements) {
				var element = ele.ToInt();

				if (element == -3)
					modelElementOutput += EnumInfos.ToYamlString(SkillElementType.ELE_RANDOM) + ":";
				else if (element == -2)
					modelElementOutput += EnumInfos.ToYamlString(SkillElementType.ELE_ENDOWED) + ":";
				else if (element == -1)
					modelElementOutput += EnumInfos.ToYamlString(SkillElementType.ELE_WEAPON) + ":";
				else
					modelElementOutput += EnumInfos.ToYamlString((SkillElementType)element) + ":";
			}

			model.Element = modelElementOutput.TrimEnd(':');

			model.NK_DamageFlags = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			model.SplashArea = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			model.MaxLevel = elements[eleIdx++];
			model.HitCount = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			model.CastCancel = String.Compare(elements[eleIdx++], "yes", true) == 0;
			model.CastDefenseReduction = DbReader.SetEmptyIfZero(elements[eleIdx++]);
			Inf2Flag inf2 = elements[eleIdx++].ToFlag<Inf2Flag>();
			model.ActiveInstance = DbReader.SetEmptyIfZero(elements[eleIdx++]);

			switch (elements[eleIdx++].ToLower()) {
				case "weapon": model.BF_Type = BattleFlagType.BF_WEAPON; break;
				case "magic": model.BF_Type = BattleFlagType.BF_MAGIC; break;
				case "misc": model.BF_Type = BattleFlagType.BF_MISC; break;
				default: model.BF_Type = BattleFlagType.BF_NONE; break;
			}

			model.Knockback = DbReader.SetEmptyIfZero(elements[eleIdx++]);

			var inf3 = elements[eleIdx++].ToLong();

			if ((inf3 & 0x00001) != 0)
				inf2 |= Inf2Flag.INF2_IGNORELANDPROTECTOR;
			if ((inf3 & 0x00004) != 0)
				inf2 |= Inf2Flag.INF2_ALLOWWHENHIDDEN;
			if ((inf3 & 0x00008) != 0)
				inf2 |= Inf2Flag.INF2_ALLOWWHENPERFORMING;
			if ((inf3 & 0x00010) != 0)
				inf2 |= Inf2Flag.INF2_TARGETEMPERIUM;
			// ??
			//if ((inf3 & 0x00020) != 0)
			//	inf2 |= Inf2Flag.INF3_STASIS_BL;
			if ((inf3 & 0x00040) != 0)
				inf2 |= Inf2Flag.INF2_IGNOREKAGEHUMI;
			if ((inf3 & 0x00080) != 0)
				inf2 |= Inf2Flag.INF2_ALTERRANGEVULTURE;
			if ((inf3 & 0x00100) != 0)
				inf2 |= Inf2Flag.INF2_ALTERRANGESNAKEEYE;
			if ((inf3 & 0x00200) != 0)
				inf2 |= Inf2Flag.INF2_ALTERRANGESHADOWJUMP;
			if ((inf3 & 0x00400) != 0)
				inf2 |= Inf2Flag.INF2_ALTERRANGERADIUS;
			if ((inf3 & 0x00800) != 0)
				inf2 |= Inf2Flag.INF2_ALTERRANGERESEARCHTRAP;
			if ((inf3 & 0x01000) != 0)
				inf2 |= Inf2Flag.INF2_IGNOREHOVERING;
			if ((inf3 & 0x02000) != 0)
				inf2 |= Inf2Flag.INF2_ALLOWONWARG;
			if ((inf3 & 0x04000) != 0)
				inf2 |= Inf2Flag.INF2_ALLOWONMADO;
			if ((inf3 & 0x08000) != 0)
				inf2 |= Inf2Flag.INF2_TARGETMANHOLE;
			if ((inf3 & 0x10000) != 0)
				inf2 |= Inf2Flag.INF2_TARGETHIDDEN;
			// ??
			//if ((inf3 & 0x20000) != 0)
			//	inf2 |= Inf2Flag.INF3_SC_GLOOMYDAY_SK;
			if ((inf3 & 0x40000) != 0)
				inf2 |= Inf2Flag.INF2_INCREASEDANCEWITHWUGDAMAGE;
			if ((inf3 & 0x80000) != 0)
				inf2 |= Inf2Flag.INF2_IGNOREWUGBITE;

			model.INF2_Flags = "0x" + ((long)inf2).ToString("X16");
			model.Name = elements[eleIdx++].Trim(' ', '\t');
			model.Description = elements[eleIdx++].Trim(' ', '\t');
		}

		private Skill SafeLoadModel(DbLoadContext context, string[] elements) {
			int id = int.Parse(elements[0]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			return tuple.GetModel<Skill>();
		}
	}
}
