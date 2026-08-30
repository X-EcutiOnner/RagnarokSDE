using SDE.Databases.Generic.Parser;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using System.Collections.Generic;
using System.Text;
using Utilities;
using Utilities.Extension;

namespace SDE.Databases.Mobs.Parser {
	public class MobWriterCsv : DatabaseWriterCsv {
		public override string KeyField => "Id";

		public override string WriteEntry(ReadableTuple tuple) {
			if (tuple == null)
				return "";

			var model = tuple.GetModel<Mob>();

			var output = new List<string>();

			var attack1 = model.Attack;
			var attack2 = model.Attack2;

			if (!model.IsCsv) {
				int value = DbReader.ToInt(attack1);
				int level = DbReader.ToInt(model.Level);
				int str = DbReader.ToInt(model.Str);
				int minAttack = str + level + value * 80 / 100;
				int maxAttack = str + level + value * 120 / 100;

				attack1 = minAttack.ToString();
				attack2 = maxAttack.ToString();
			}

			output.Add(tuple.Key.ToString());
			output.Add(model.AegisName);
			output.Add(model.Name);
			output.Add(model.JapaneseName);
			output.Add(DbWriter.SetZeroDefault(model.Level));
			output.Add(DbWriter.SetZeroDefault(model.Hp));
			output.Add(DbWriter.SetZeroDefault(model.Sp));
			output.Add(DbWriter.SetZeroDefault(model.BaseExp));
			output.Add(DbWriter.SetZeroDefault(model.JobExp));
			output.Add(DbWriter.SetZeroDefault(model.AttackRange));
			output.Add(DbWriter.SetZeroDefault(attack1));
			output.Add(DbWriter.SetZeroDefault(attack2));
			output.Add(DbWriter.SetZeroDefault(model.Defense));
			output.Add(DbWriter.SetZeroDefault(model.MagicDefense));
			output.Add(DbWriter.SetZeroDefault(model.Str));
			output.Add(DbWriter.SetZeroDefault(model.Agi));
			output.Add(DbWriter.SetZeroDefault(model.Vit));
			output.Add(DbWriter.SetZeroDefault(model.Int));
			output.Add(DbWriter.SetZeroDefault(model.Dex));
			output.Add(DbWriter.SetZeroDefault(model.Luk));
			output.Add(DbWriter.SetZeroDefault(model.SkillRange));
			output.Add(DbWriter.SetZeroDefault(model.ChaseRange));
			output.Add(DbWriter.SetZeroDefault(((int)model.Size).ToString()));
			output.Add(DbWriter.SetZeroDefault(((int)model.Race).ToString()));
			output.Add(DbWriter.SetZeroDefault((20 * (int)model.ElementLevel + (int)model.Element).ToString()));
			output.Add(DbWriter.SetZeroDefault("0x" + model.Modes.ToLong().ToString("X")));
			output.Add(DbWriter.SetZeroDefault(model.WalkSpeed));
			output.Add(DbWriter.SetZeroDefault(model.AttackDelay));
			output.Add(DbWriter.SetZeroDefault(model.AttackMotion));
			output.Add(DbWriter.SetZeroDefault(model.DamageMotion));
			output.Add(DbWriter.SetZeroDefault(model.MvpExp));

			int intValue;

			for (int i = 0; i < 3; i++) {
				if (i < model.MvpDrops.Count) {
					var itemDrop = model.MvpDrops[i];

					if (DbReader.ToInt(itemDrop.Item, out intValue) && intValue > 0) {
						output.Add(intValue.ToString());
						output.Add(itemDrop.Rate);
						continue;
					}
				}

				output.Add("0");
				output.Add("0");
			}

			ItemDrop cardDrop = null;
			List<ItemDrop> drops = new List<ItemDrop>();

			for (int i = 0; i < 10; i++) {
				if (i < model.Drops.Count) {
					var itemDrop = model.Drops[i];

					if (itemDrop.StealProtected) {
						cardDrop = itemDrop;
						continue;
					}

					drops.Add(itemDrop);
					continue;
				}

				break;
			}

			for (int i = 0; i < 9; i++) {
				if (i < drops.Count) {
					var itemDrop = drops[i];

					if (itemDrop.StealProtected)
						continue;

					if (DbReader.ToInt(itemDrop.Item, out intValue) && intValue > 0) {
						output.Add(intValue.ToString());
						output.Add(itemDrop.Rate);
						continue;
					}
				}

				output.Add("0");
				output.Add("0");
			}

			if (cardDrop != null) {
				if (DbReader.ToInt(cardDrop.Item, out intValue) && intValue > 0) {
					output.Add(intValue.ToString());
					output.Add(cardDrop.Rate);
				}
			}
			else {
				output.Add("0");
				output.Add("0");
			}

			return Methods.Aggregate(output, ",");
		}

		public void WriteDrop(StringBuilder builder, Features.ItemDrop drop) {
			builder.Append($"{drop.Item},{drop.Rate},{drop.StealProtected},{drop.Index},");
		}
	}
}
