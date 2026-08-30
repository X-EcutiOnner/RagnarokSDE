using GRF.IO;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.Editor.Parsers.Libconfig;
using SDE.Editor.Parsers.Yaml;
using SDE.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Extension;

namespace SDE.Databases.Items.Parser {
	public class ItemWriterYaml : DatabaseWriterYaml {
		public override string KeyField => "Id";

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			try {
				if (db.Attached["Import:item_db_usable"] != null &&
					db.Attached["Import:item_db_equip"] != null &&
					db.Attached["Import:item_db_etc"] != null) {
					string dbPath = GrfPath.GetDirectoryName(ProjectConfiguration.DatabasePath);
					dbPath = GrfPath.GetDirectoryName(dbPath);
					string subPath = SdeEditor.Project.IsRenewal ? "re" : "pre-re";

					var paths = new string[] {
						GrfPath.CombineUrl(dbPath, "db", subPath, "item_db_usable.yml"),
						GrfPath.CombineUrl(dbPath, "db", subPath, "item_db_equip.yml"),
						GrfPath.CombineUrl(dbPath, "db", subPath, "item_db_etc.yml"),
					};

					var linesUsable = new YamlParser(DbPathLocator.GetStoredFile(paths[0]), ParserMode.Write);
					var linesEquip = new YamlParser(DbPathLocator.GetStoredFile(paths[1]), ParserMode.Write);
					var linesEtc = new YamlParser(DbPathLocator.GetStoredFile(paths[2]), ParserMode.Write);

					if (linesUsable.Output == null ||
						linesEquip.Output == null ||
						linesEtc.Output == null)
						return;

					linesUsable.Remove(db);
					linesEquip.Remove(db);
					linesEtc.Remove(db);

					foreach (ReadableTuple tuple in db.Table.FastItems.Where(p => !p.Normal).OrderBy(p => p.GetKey<int>())) {
						string key = tuple.Key.ToString();
						var model = tuple.GetModel<Item>();

						ItemType type = model.Type;
						StringBuilder b = new StringBuilder();

						WriteEntry(b, tuple);

						switch (type) {
							default: // Usable
								linesEquip.Delete(key);
								linesEtc.Delete(key);
								linesUsable.Write(key, b.ToString().Trim('\r', '\n'));
								break;
							case ItemType.IT_ARMOR:
							case ItemType.IT_WEAPON:
							case ItemType.IT_PETEGG:
							case ItemType.IT_PETARMOR:
							case ItemType.IT_SHADOWGEAR: // Equip
								linesEquip.Write(key, b.ToString().Trim('\r', '\n'));
								linesEtc.Delete(key);
								linesUsable.Delete(key);
								break;
							case ItemType.IT_ETC:
							case ItemType.IT_CARD:
							case ItemType.IT_AMMO: // Etc
								linesEquip.Delete(key);
								linesEtc.Write(key, b.ToString().Trim('\r', '\n'));
								linesUsable.Delete(key);
								break;
						}
					}

					linesUsable.WriteFile(paths[0]);
					linesEquip.WriteFile(paths[1]);
					linesEtc.WriteFile(paths[2]);
				}
				else {
					base.Writer(context, db);
				}
			}
			catch (Exception err) {
				context.ReportException(err);
			}
		}

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<Item>();
			int intValue = 0;
			long flagValue = 0;

			builder.AppendLine($"  - Id: " + tuple.Key);
			builder.AppendLine($"    AegisName: {DbReader.YamlString(model.AegisName)}");
			builder.AppendLine($"    Name: {DbReader.YamlString(model.Name)}");

			var type = model.Type;

			builder.AppendLine($"    Type: " + EnumInfos.ToYamlString(model.Type));

			switch (type) {
				case ItemType.IT_AMMO:
					builder.AppendLine("    SubType: " + EnumInfos.ToYamlString(model.AmmoType));
					break;
				case ItemType.IT_CARD:
					if (model.CardType != CardType.CARD_NORMAL)
						builder.AppendLine("    SubType: " + EnumInfos.ToYamlString(model.CardType));
					break;
				case ItemType.IT_WEAPON:
					builder.AppendLine("    SubType: " + EnumInfos.ToYamlString(model.WeaponType));
					break;
			}

			if (DbReader.ToInt(model.Buy, out intValue) && intValue != 0)
				builder.AppendLine($"    Buy: {intValue}");

			if (!DbReader.IsNullOrEmpty(model.Sell) && DbReader.ToInt(model.Sell, out intValue))
				builder.AppendLine($"    Sell: {intValue}");

			if (DbReader.ToInt(model.Weight, out intValue) && intValue != 0)
				builder.AppendLine($"    Weight: {intValue}");

			if (DbReader.ToInt(model.Attack, out intValue) && intValue != 0)
				builder.AppendLine($"    Attack: {intValue}");

			if (ProjectConfiguration.IsRenewal) {
				if (DbReader.ToInt(model.MagicAttack, out intValue) && intValue != 0)
					builder.AppendLine($"    MagicAttack: {intValue}");
			}

			if (DbReader.ToInt(model.Defense, out intValue) && intValue != 0)
				builder.AppendLine($"    Defense: {intValue}");

			if (DbReader.ToInt(model.Range, out intValue) && intValue != 0)
				builder.AppendLine($"    Range: {intValue}");

			if (DbReader.ToInt(model.Slots, out intValue) && intValue != 0)
				builder.AppendLine($"    Slots: {intValue}");

			WriteJobs(model, builder);
			WriteItemJobFlag(model, builder);

			if (model.Gender != GenderType.SEX_BOTH)
				builder.AppendLine("    Gender: " + EnumInfos.ToYamlString(model.Gender));

			WriteLocationsFlag(model, builder);

			if (DbReader.ToInt(model.WeaponLevel, out intValue) && intValue != 0)
				builder.AppendLine($"    WeaponLevel: {intValue}");

			if (DbReader.ToInt(model.ArmorLevel, out intValue) && intValue != 0)
				builder.AppendLine($"    ArmorLevel: {intValue}");

			if (DbReader.ToInt(model.EquipLevelMin, out intValue) && intValue != 0)
				builder.AppendLine($"    EquipLevelMin: {intValue}");

			if (DbReader.ToInt(model.EquipLevelMax, out intValue) && intValue != 0)
				builder.AppendLine($"    EquipLevelMax: {intValue}");

			if (model.Refineable)
				builder.AppendLine($"    Refineable: true");

			if (model.Gradable)
				builder.AppendLine($"    Gradable: true");

			if (DbReader.ToInt(model.View, out intValue) && intValue != 0)
				builder.AppendLine($"    View: {intValue}");

			if (!DbReader.IsNullOrEmpty(model.AliasName))
				builder.AppendLine($"    AliasName: " + DbUtilities.MobId2Name(model.AliasName, ItemDb));

			WriteFlagsFlag(model, builder);

			if (DbReader.ToInt(model.Delay, out intValue) && intValue != 0) {
				builder.AppendLine("    Delay:");
				builder.AppendLine("      Duration: " + intValue);

				if (!DbReader.IsNullOrEmpty(model.DelayStatus)) {
					builder.AppendLine("      Status: " + model.DelayStatus);
				}
			}

			if (DbReader.ToInt(model.StackAmount, out intValue) && intValue != 0) {
				builder.AppendLine("    Stack:");
				builder.AppendLine("      Amount: " + intValue);

				DbWriter.ExpandFlagToBool<ItemStackFlag>(builder, model.StackFlags.ToLong(), "", "    ");
			}

			flagValue = model.NoUseFlags.ToLong();
			if (flagValue != 0) {
				builder.AppendLine("    NoUse:");

				if (DbReader.ToInt(model.NoUseOverride, out intValue) && intValue != 100)
					builder.AppendLine($"      Override: {intValue}");

				DbWriter.ExpandFlagToBool<NoUseFlag>(builder, flagValue, "", "    ");
			}

			flagValue = model.TradeFlags.ToLong();
			if (flagValue != 0) {
				builder.AppendLine("    Trade:");

				if (DbReader.ToInt(model.TradeOverride, out intValue) && intValue != 100)
					builder.AppendLine($"      Override: {intValue}");

				DbWriter.ExpandFlagToBool<TradeFlag>(builder, flagValue, "", "    ");
			}

			if (!DbReader.IsNullOrEmpty(model.Script)) {
				builder.AppendLine("    Script: |");
				builder.AppendLine(DbWriter.ToYamlScript(model.Script, "      "));
			}

			if (!DbReader.IsNullOrEmpty(model.EquipScript)) {
				builder.AppendLine("    EquipScript: |");
				builder.AppendLine(DbWriter.ToYamlScript(model.EquipScript, "      "));
			}

			if (!DbReader.IsNullOrEmpty(model.UnEquipScript)) {
				builder.AppendLine("    UnEquipScript: |");
				builder.AppendLine(DbWriter.ToYamlScript(model.UnEquipScript, "      "));
			}
		}

		private void WriteFlagsFlag(Item model, StringBuilder builder) {
			var flag = model.Flags.ToFlag<ItemFlag>();

			if (flag == 0 && model.DropEffect == 0)
				return;

			builder.AppendLine("    Flags:");

			if (flag != 0)
				DbWriter.ExpandFlagToBool<ItemFlag>(builder, (long)flag, "", "    ");

			if (model.DropEffect != 0)
				builder.AppendLine("      DropEffect: " + EnumInfos.ToYamlString(model.DropEffect));
		}

		private void WriteLocationsFlag(Item model, StringBuilder builder) {
			EquipLocationFlag location = model.Locations.ToFlag<EquipLocationFlag>();

			if (location != 0) {
				builder.AppendLine("    Locations:");

				if (location.HasFlag(EquipLocationFlag.EQP_HEAD_LOW))
					builder.AppendLine("      Head_Low: true");
				if (location.HasFlag(EquipLocationFlag.EQP_HEAD_MID))
					builder.AppendLine("      Head_Mid: true");
				if (location.HasFlag(EquipLocationFlag.EQP_HEAD_TOP))
					builder.AppendLine("      Head_Top: true");
				if (location.HasFlag(EquipLocationFlag.EQP_SHOES))
					builder.AppendLine("      Shoes: true");
				if (location.HasFlag(EquipLocationFlag.EQP_Both_Hand))
					builder.AppendLine("      Both_Hand: true");
				else if (location.HasFlag(EquipLocationFlag.EQP_HAND_R))
					builder.AppendLine("      Right_Hand: true");
				else if (location.HasFlag(EquipLocationFlag.EQP_HAND_L))
					builder.AppendLine("      Left_Hand: true");

				if (location.HasFlag(EquipLocationFlag.EQP_ARMOR))
					builder.AppendLine("      Armor: true");
				if (location.HasFlag(EquipLocationFlag.EQP_GARMENT))
					builder.AppendLine("      Garment: true");
				if (location.HasFlag(EquipLocationFlag.EQP_ACC_RL))
					builder.AppendLine("      Both_Accessory: true");
				else if (location.HasFlag(EquipLocationFlag.EQP_ACC_R))
					builder.AppendLine("      Right_Accessory: true");
				else if (location.HasFlag(EquipLocationFlag.EQP_ACC_L))
					builder.AppendLine("      Left_Accessory: true");

				if (location.HasFlag(EquipLocationFlag.EQP_AMMO))
					builder.AppendLine("      Ammo: true");
				if (location.HasFlag(EquipLocationFlag.EQP_COSTUME_HEAD_TOP))
					builder.AppendLine("      Costume_Head_Top: true");
				if (location.HasFlag(EquipLocationFlag.EQP_COSTUME_HEAD_MID))
					builder.AppendLine("      Costume_Head_Mid: true");
				if (location.HasFlag(EquipLocationFlag.EQP_COSTUME_HEAD_LOW))
					builder.AppendLine("      Costume_Head_Low: true");
				if (location.HasFlag(EquipLocationFlag.EQP_COSTUME_GARMENT))
					builder.AppendLine("      Costume_Garment: true");
				if (location.HasFlag(EquipLocationFlag.EQP_SHADOW_ARMOR))
					builder.AppendLine("      Shadow_Armor: true");
				if (location.HasFlag(EquipLocationFlag.EQP_SHADOW_WEAPON))
					builder.AppendLine("      Shadow_Weapon: true");
				if (location.HasFlag(EquipLocationFlag.EQP_SHADOW_SHIELD))
					builder.AppendLine("      Shadow_Shield: true");
				if (location.HasFlag(EquipLocationFlag.EQP_SHADOW_SHOES))
					builder.AppendLine("      Shadow_Shoes: true");
				if (location.HasFlag(EquipLocationFlag.EQP_SHADOW_ACC_R))
					builder.AppendLine("      Shadow_Right_Accessory: true");
				if (location.HasFlag(EquipLocationFlag.EQP_SHADOW_ACC_L))
					builder.AppendLine("      Shadow_Left_Accessory: true");
			}
		}

		private void WriteItemJobFlag(Item model, StringBuilder builder) {
			ItemJobFlag itemJob = model.Classes.ToFlag<ItemJobFlag>();

			if (itemJob != ItemJobFlag.ITEMJ_ALL) {
				builder.AppendLine("    Classes:");

				if (itemJob == 0) {
					builder.AppendLine("      All: false");
				}
				else {
					if ((itemJob & ItemJobFlag.ITEMJ_ALL_THIRD) == ItemJobFlag.ITEMJ_ALL_THIRD) {
						builder.AppendLine("      All_Third: true");
						itemJob &= ~ItemJobFlag.ITEMJ_ALL_THIRD;
					}

					if ((itemJob & ItemJobFlag.ITEMJ_ALL_BABY) == ItemJobFlag.ITEMJ_ALL_BABY) {
						builder.AppendLine("      All_Baby: true");
						itemJob &= ~ItemJobFlag.ITEMJ_ALL_BABY;
					}

					if ((itemJob & ItemJobFlag.ITEMJ_ALL_UPPER) == ItemJobFlag.ITEMJ_ALL_UPPER) {
						builder.AppendLine("      All_Upper: true");
						itemJob &= ~ItemJobFlag.ITEMJ_ALL_UPPER;
					}

					if ((itemJob & ItemJobFlag.ITEMJ_NORMAL) != 0)
						builder.AppendLine("      Normal: true");
					if ((itemJob & ItemJobFlag.ITEMJ_UPPER) != 0)
						builder.AppendLine("      Upper: true");
					if ((itemJob & ItemJobFlag.ITEMJ_BABY) != 0)
						builder.AppendLine("      Baby: true");
					if ((itemJob & ItemJobFlag.ITEMJ_THIRD) != 0)
						builder.AppendLine("      Third: true");
					if ((itemJob & ItemJobFlag.ITEMJ_THIRD_UPPER) != 0)
						builder.AppendLine("      Third_Upper: true");
					if ((itemJob & ItemJobFlag.ITEMJ_THIRD_BABY) != 0)
						builder.AppendLine("      Third_Baby: true");
					if ((itemJob & ItemJobFlag.ITEMJ_FOURTH) != 0)
						builder.AppendLine("      Fourth: true");
				}
			}
		}

		public void WriteJobs(Item model, StringBuilder builder) {
			var jobValue = model.Jobs.ToUInt64();

			if ((Int64)jobValue != -1) {
				List<Job> jobs;
				bool add = true;

				builder.AppendLine("    Jobs:");

				if (jobValue == 0) {
					builder.AppendLine("      All: false");
					return;
				}
				else {
					if ((Int64)jobValue < 0) {
						builder.AppendLine("      All: true");
						jobs = JobOperations.GetJobs(~jobValue);
						add = false;
					}
					else {
						jobs = JobOperations.GetJobs(jobValue);
					}
				}

				foreach (var job in jobs) {
					builder.Append("      ");

					var enumInfo = EnumInfos.GetEnumBase<EAJs>((EAJs)job.MapId);
					builder.Append(enumInfo.YamlName);
					
					if (add)
						builder.AppendLine(": true");
					else
						builder.AppendLine(": false");
				}
			}
		}
	}
}
