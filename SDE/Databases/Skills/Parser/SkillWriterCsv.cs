using Database;
using GRF.IO;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Skills.Common;
using SDE.Databases.Skills.Features;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.Editor.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extension;

namespace SDE.Databases.Skills.Parser {
	public class SkillWriterCsv : DatabaseWriterCsv {
		public override DbAttribute FileKeyRef => SkillAttributes.CopyableFileKeyRef;
		public override string KeyField => "Id";
		public override bool SplitDatabaseFiles => true;

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			bool isImport = context.Source != DataSources.Skill;

			WriteSkill(db, isImport);
			WriteSkillCast(db, isImport);
			WriteSkillNoCast(db, isImport);
			WriteSkillRequirement(db, isImport);
			WriteSkillCopyable(db, isImport);
			WriteSkillUnit(db, isImport);
			WriteSkillNoDex(db, isImport);
		}

		public void WriteSkill(BaseDatabase db, bool isImport) => Write(db, DataSources.Skill, isImport, WriteEntry);
		public void WriteSkillCast(BaseDatabase db, bool isImport) => Write(db, DataSources.SkillCast, isImport, WriteEntryCast);
		public void WriteSkillNoCast(BaseDatabase db, bool isImport) => Write(db, DataSources.SkillNoCast, isImport, WriteEntryNoCast);
		public void WriteSkillRequirement(BaseDatabase db, bool isImport) => Write(db, DataSources.SkillRequirement, isImport, WriteEntryRequirement);
		public void WriteSkillCopyable(BaseDatabase db, bool isImport) => Write(db, DataSources.SkillCopyable, isImport, WriteEntryCopyable);
		public void WriteSkillUnit(BaseDatabase db, bool isImport) => Write(db, DataSources.SkillUnit, isImport, WriteEntryUnit);
		public void WriteSkillNoDex(BaseDatabase db, bool isImport) => Write(db, DataSources.SkillNoDex, isImport, WriteEntryNoDex);

		public void Write(BaseDatabase db, DataSource source, bool isImport, Func<ReadableTuple, string> writeEntryFunc) {
			var context = new DbSaveContext(db);
			bool isSkillCopyable = source == DataSources.SkillCopyable;

			if (isImport)
				source = source.ImportTable;

			// Create file if it doesn't exist
			TkPath path = DbPathLocator.DetectPath(source);
			string dbPath = ProjectConfiguration.DatabaseDbPath;
			string subPath = ProjectConfiguration.DatabaseSubDbPath;

			if (path == null || !path.IsFile) {
				var sourcePathCsv = source.Paths.First(p => p.IsExtension(".txt"));
				File.WriteAllBytes(GrfPath.Combine(dbPath, sourcePathCsv.Replace("{DBPATH}", subPath)), new byte[] { 0 });
			}

			if (context.PrepareWrite(source) != SaveContextState.Valid) return;

			CsvWriter lines = new CsvWriter(context.OldPath, useUniqueId: isSkillCopyable, fileKeyRef: FileKeyRef);

			if (isSkillCopyable) {
				Dictionary<string, int> key2Id = new Dictionary<string, int>();

				foreach (var tuple in db.Table.FastItems) {
					key2Id[tuple.GetModel<Skill>().Name] = tuple.Key;
				}

				lines.Key2Id = k => {
					if (key2Id.TryGetValue(k, out int intKey))
						return intKey;
					return -1;
				};

				lines.Id2Key = k => {
					return db.Table.GetTuple(k).GetModel<Skill>().Name;
				};
			}

			lines.Remove(db);

			foreach (ReadableTuple tuple in db.Table.FastItems.Where(p => !p.Normal).OrderBy(p => p.GetKey<int>())) {
			//foreach (ReadableTuple tuple in db.Table.FastItems.OrderBy(p => p.GetKey<int>())) {
				int key = tuple.GetKey<int>();

				string line = writeEntryFunc(tuple);

				// Why is skill_copyable_db using string keys instead of integers... sigh.
				if (isSkillCopyable) {
					var keyRef = tuple.GetValue<string>(SkillAttributes.CopyableFileKeyRef);
					
					if (line == null)
						lines.Delete(key, tuple);
					else
						lines.Write(keyRef ?? line, line);
				}
				else {
					if (line == null)
						lines.Delete(key);
					else
						lines.Write(key, line);
				}
			}

			lines.WriteFile(context.FilePath);
		}

		public string WriteEntryCast(ReadableTuple tuple) {
			var model = tuple.GetModel<Skill>();

			if (IsNotSet(model.CastTime) &&
				IsNotSet(model.AfterCastActDelay) &&
				IsNotSet(model.AfterCastWalkDelay) &&
				IsNotSet(model.Duration1) &&
				IsNotSet(model.Duration2) &&
				IsNotSet(model.Cooldown) &&
				(!ProjectConfiguration.IsRenewal || IsNotSet(model.FixedCastTime)))
				return null;

			var output = new List<string>();
			output.Add(tuple.Key.ToString());
			output.Add(DbWriter.SetZeroDefault(model.CastTime));
			output.Add(DbWriter.SetZeroDefault(model.AfterCastActDelay));
			output.Add(DbWriter.SetZeroDefault(model.AfterCastWalkDelay));
			output.Add(DbWriter.SetZeroDefault(model.Duration1));
			output.Add(DbWriter.SetZeroDefault(model.Duration2));
			output.Add(DbWriter.SetZeroDefault(model.Cooldown));

			if (ProjectConfiguration.IsRenewal)
				output.Add(DbWriter.SetZeroDefault(model.FixedCastTime));

			return string.Join(",", output);
		}

		public string WriteEntryNoCast(ReadableTuple tuple) {
			var model = tuple.GetModel<Skill>();
			var flag = model.NoCastFlags;
			
			if (flag.ToLong() == 0)
				return null;

			return tuple.Key.ToString() + "," + flag + "\t//" + model.Name;
		}

		public string WriteEntryRequirement(ReadableTuple tuple) {
			var model = tuple.GetModel<Skill>();
			var require = model.Require;

			if (IsNotSet(require.HpCost) &&
				IsNotSet(require.MaxHpTrigger) &&
				IsNotSet(require.SpCost) &&
				IsNotSet(require.HpRateCost) &&
				IsNotSet(require.SpRateCost) &&
				IsNotSet(require.ZenyCost) &&
				IsNotSet(require.Weapon) &&
				IsNotSet(require.Ammo) &&
				IsNotSet(require.AmmoAmount) &&
				require.State == RequiredStateType.ST_NONE &&
				IsNotSet(require.Status) &&
				IsNotSet(require.SpiritSphereCost) &&
				IsNotSet(require.ItemCost) &&
				IsNotSet(require.Equipment))
				return null;

			var weaponFlag = require.Weapon.ToLong();

			string weapon = "";

			if (weaponFlag == 0) {
				weapon = "99";
			}
			else {
				for (int i = 0; i < 32; i++) {
					if (((1 << i) & weaponFlag) != 0) {
						weapon += i + ":";
					}
				}
			}

			weapon = weapon.TrimEnd(':');
			
			if (weapon == "")
				weapon = "0";

			string state = "none";

			switch (require.State) {
				case RequiredStateType.ST_NONE: state = "none"; break;
				case RequiredStateType.ST_HIDDEN: state = "hidden"; break;
				case RequiredStateType.ST_RIDING: state = "riding"; break;
				case RequiredStateType.ST_FALCON: state = "falcon"; break;
				case RequiredStateType.ST_CART: state = "cart"; break;
				case RequiredStateType.ST_SHIELD: state = "shield"; break;
				case RequiredStateType.ST_RECOVER_WEIGHT_RATE: state = "recover_weight_rate"; break;
				case RequiredStateType.ST_MOVE_ENABLE: state = "move_enable"; break;
				case RequiredStateType.ST_WATER: state = "water"; break;
				case RequiredStateType.ST_RIDINGDRAGON: state = "dragon"; break;
				case RequiredStateType.ST_WUG: state = "warg"; break;
				case RequiredStateType.ST_RIDINGWUG: state = "ridingwarg"; break;
				case RequiredStateType.ST_MADO: state = "mado"; break;
				case RequiredStateType.ST_ELEMENTALSPIRIT: state = "elementalspirit"; break;
				case RequiredStateType.ST_ELEMENTALSPIRIT2: state = "elementalspirit2"; break;
				case RequiredStateType.ST_PECO: state = "peco"; break;
			}

			string status = "";

			foreach (var v in (require.Status ?? "").Split(':')) {
				if (v.Length > 0 && !v.StartsWith("SC_", StringComparison.OrdinalIgnoreCase))
					status += "SC_" + v + ":";
				else
					status += v.ToUpper();
			}

			if (status == "")
				status = "0";

			StringBuilder itemCostBuilder = new StringBuilder();
			var data = (require.ItemCost ?? "").Split(':');

			for (int i = 0; i < 10; i++) {
				if (3 * i + 2 < data.Length) {
					itemCostBuilder.Append(data[3 * i + 0] + "," + data[3 * i + 1] + ",");
				}
				else {
					itemCostBuilder.Append("0,0,");
				}
			}

			itemCostBuilder.Remove(itemCostBuilder.Length - 1, 1);

			string[] output = {
				tuple.Key.ToString(),
				DbWriter.SetZeroDefault(require.HpCost),
				DbWriter.SetZeroDefault(require.MaxHpTrigger),
				DbWriter.SetZeroDefault(require.SpCost),
				DbWriter.SetZeroDefault(require.HpRateCost),
				DbWriter.SetZeroDefault(require.SpRateCost),
				DbWriter.SetZeroDefault(require.ZenyCost),
				weapon,
				DbWriter.SetZeroDefault(require.Ammo),
				DbWriter.SetZeroDefault(require.AmmoAmount),
				state,
				status,
				DbWriter.SetZeroDefault(require.SpiritSphereCost),
				itemCostBuilder.ToString(),
				DbWriter.SetZeroDefault(require.Equipment)
			};

			return string.Join(",", output) + "\t//" + model.Name;
		}

		public string WriteEntryCopyable(ReadableTuple tuple) {
			var model = tuple.GetModel<Skill>();
			var flags = model.CopyFlagsSkill.ToLong();

			if (flags == 0)
				return null;

			var output = new List<string>();
			output.Add(model.Name);
			output.Add(flags.ToString());

			var skjaAll = (long)SkillCopyJobAllowedFlag.SKJA_ALL;
			var skja = model.CopyJobAllowed.ToLong();

			var cfrr = model.CopyFlagsRemoveRequirement.ToLong();

			if (cfrr != 0) {
				output.Add(skja.ToString());
				output.Add(cfrr.ToString());
			}
			else if (skjaAll != skja) {
				output.Add(skja.ToString());
			}

			return string.Join(",", output) + "\t// " + model.Description;
		}

		public string WriteEntryUnit(ReadableTuple tuple) {
			var model = tuple.GetModel<Skill>();
			var unit = model.Unit;

			if (unit.Id.ToInt() == 0)
				return null;

			string target = "noone";

			switch (unit.Target) {
				//case BattleCheckTargetType.BCT_NOENEMY: target = "noenemy"; break;
				case BattleCheckTargetType.BCT_FRIEND: target = "friend"; break;
				case BattleCheckTargetType.BCT_PARTY: target = "party"; break;
				case BattleCheckTargetType.BCT_ALLY: target = "ally"; break;
				case BattleCheckTargetType.BCT_GUILD: target = "guild"; break;
				case BattleCheckTargetType.BCT_ALL: target = "all"; break;
				case BattleCheckTargetType.BCT_ENEMY: target = "enemy"; break;
				case BattleCheckTargetType.BCT_SELF: target = "self"; break;
				case BattleCheckTargetType.BCT_SAMEGUILD: target = "sameguild"; break;
				case BattleCheckTargetType.BCT_NOONE: target = "noone"; break;
				default: target = ((int)unit.Target).ToString(); break;
			}

			long flag = unit.Flag.ToLong() >> 1;

			string[] output = {
				tuple.Key.ToString(),
				String.IsNullOrEmpty(unit.Id) ? "" : "0x" + unit.Id.ToInt().ToString("x"),
				String.IsNullOrEmpty(unit.AlternateId) ? "" : "0x" + unit.AlternateId.ToInt().ToString("x"),
				DbWriter.SetZeroDefault(unit.Layout),
				DbWriter.SetZeroDefault(unit.Range),
				DbWriter.SetZeroDefault(unit.Interval),
				target,
				String.IsNullOrEmpty(unit.Flag) || flag == 0 ? "0x000" : "0x" + flag.ToString("X3")
			};

			return string.Join(",", output) + "\t//" + model.Name;
		}

		public string WriteEntryNoDex(ReadableTuple tuple) {
			var model = tuple.GetModel<Skill>();
			var castFlags = model.CastTimeFlags.ToLong();
			var delayFlags = model.CastDelayFlags.ToLong();

			if (castFlags == 0 && delayFlags == 0)
				return null;

			var output = new List<string>();
			output.Add(tuple.Key.ToString());
			output.Add(castFlags.ToString());

			if (delayFlags != 0)
				output.Add(delayFlags.ToString());

			return string.Join(",", output) + "\t//" + model.Name;
		}

		public override string WriteEntry(ReadableTuple tuple) {
			var model = tuple.GetModel<Skill>();

			string element = "";

			foreach (var ele in (model.Element ?? "").Split(':')) {
				var eleType = DbReader.LoadEnum(ele, SkillElementType.ELE_NEUTRAL);

				switch (eleType) {
					case SkillElementType.ELE_RANDOM: element += "-3:"; break;
					case SkillElementType.ELE_ENDOWED: element += "-2:"; break;
					case SkillElementType.ELE_WEAPON: element += "-1:"; break;
					default: element += (int)eleType + ":"; break;
				}
			}

			element = element.TrimEnd(':');

			if (element == "")
				element = "0";

			var inf2 = model.INF2_Flags.ToFlag<Inf2Flag>();
			var modelInf2 = model.INF2_Flags.ToLong();
			long outputInf2 = modelInf2 & 0x7FFFF;
			long outputInf3 = 0;

			string bfTarget;

			switch (model.BF_Type) {
				case BattleFlagType.BF_WEAPON: bfTarget = "weapon"; break;
				case BattleFlagType.BF_MAGIC: bfTarget = "magic"; break;
				case BattleFlagType.BF_MISC: bfTarget = "misc"; break;
				default: bfTarget = "none"; break;
			}

			if ((inf2 & Inf2Flag.INF2_IGNORELANDPROTECTOR) != 0)
				outputInf3 |= 0x00001;
			if ((inf2 & Inf2Flag.INF2_ALLOWWHENHIDDEN) != 0)
				outputInf3 |= 0x00004;
			if ((inf2 & Inf2Flag.INF2_ALLOWWHENPERFORMING) != 0)
				outputInf3 |= 0x00008;
			if ((inf2 & Inf2Flag.INF2_TARGETEMPERIUM) != 0)
				outputInf3 |= 0x00010;
			if ((inf2 & Inf2Flag.INF2_IGNOREKAGEHUMI) != 0)
				outputInf3 |= 0x00040;
			if ((inf2 & Inf2Flag.INF2_ALTERRANGEVULTURE) != 0)
				outputInf3 |= 0x00080;
			if ((inf2 & Inf2Flag.INF2_ALTERRANGESNAKEEYE) != 0)
				outputInf3 |= 0x00100;
			if ((inf2 & Inf2Flag.INF2_ALTERRANGESHADOWJUMP) != 0)
				outputInf3 |= 0x00200;
			if ((inf2 & Inf2Flag.INF2_ALTERRANGERADIUS) != 0)
				outputInf3 |= 0x00400;
			if ((inf2 & Inf2Flag.INF2_ALTERRANGERESEARCHTRAP) != 0)
				outputInf3 |= 0x00800;
			if ((inf2 & Inf2Flag.INF2_IGNOREHOVERING) != 0)
				outputInf3 |= 0x01000;
			if ((inf2 & Inf2Flag.INF2_ALLOWONWARG) != 0)
				outputInf3 |= 0x02000;
			if ((inf2 & Inf2Flag.INF2_ALLOWONMADO) != 0)
				outputInf3 |= 0x04000;
			if ((inf2 & Inf2Flag.INF2_TARGETMANHOLE) != 0)
				outputInf3 |= 0x08000;
			if ((inf2 & Inf2Flag.INF2_TARGETHIDDEN) != 0)
				outputInf3 |= 0x10000;
			if ((inf2 & Inf2Flag.INF2_INCREASEDANCEWITHWUGDAMAGE) != 0)
				outputInf3 |= 0x40000;
			if ((inf2 & Inf2Flag.INF2_IGNOREWUGBITE) != 0)
				outputInf3 |= 0x80000;

			string[] output = {
				tuple.Key.ToString(),
				DbWriter.SetZeroDefault(model.Range),
				((int)model.DMG_Hit).ToString(),
				((int)model.INF_TargetType).ToString(),
				element,
				IsNotSet(model.NK_DamageFlags) ? "0" : "0x" + model.NK_DamageFlags.ToLong().ToString("x"),
				DbWriter.SetZeroDefault(model.SplashArea),
				DbWriter.SetZeroDefault(model.MaxLevel),
				DbWriter.SetZeroDefault(model.HitCount),
				model.CastCancel ? "yes" : "no",
				DbWriter.SetZeroDefault(model.CastDefenseReduction),
				outputInf2 == 0 ? "0" : "0x" + outputInf2.ToString("x"),
				DbWriter.SetZeroDefault(model.ActiveInstance),
				bfTarget,
				DbWriter.SetZeroDefault(model.Knockback),
				"0x" + outputInf3.ToString("x"),
				"\t" + model.Name,
				model.Description
			};

			return string.Join(",", output);
		}

		private bool IsNotSet(string value) {
			return value == null || value == "" || value == "0";
		}
	}
}
