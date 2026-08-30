using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;

namespace SDE.Databases.Mobs.Parser {
	public class MobReaderCsv : DatabaseReaderCsv<int> {
		public override void ReadEntry(DbLoadContext context, string[] elements) {
			int id = int.Parse(elements[0]);
			
			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Mob>();
			Mob previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (Mob)model.Clone();
				model.Drops.Clear();
				model.MvpDrops.Clear();
			}

			model.IsCsv = true;

			int eleIdx = 1;
			model.AegisName = elements[eleIdx++];
			model.Name = elements[eleIdx++];
			model.JapaneseName = elements[eleIdx++];
			model.Level = elements[eleIdx++];
			model.Hp = elements[eleIdx++];
			model.Sp = elements[eleIdx++];
			model.BaseExp = elements[eleIdx++];
			model.JobExp = elements[eleIdx++];
			model.AttackRange = elements[eleIdx++];
			model.Attack = elements[eleIdx++];
			model.Attack2 = elements[eleIdx++];
			model.Defense = elements[eleIdx++];
			model.MagicDefense = elements[eleIdx++];
			model.Str = elements[eleIdx++];
			model.Agi = elements[eleIdx++];
			model.Vit = elements[eleIdx++];
			model.Int = elements[eleIdx++];
			model.Dex = elements[eleIdx++];
			model.Luk = elements[eleIdx++];
			model.SkillRange = elements[eleIdx++];
			model.ChaseRange = elements[eleIdx++];
			model.Size = (SizeType)DbReader.ToInt(elements[eleIdx++]);
			model.Race = (RaceType)DbReader.ToInt(elements[eleIdx++]);
			var element = DbReader.ToInt(elements[eleIdx++]);
			model.Element = (ElementType)(element % 20);
			model.ElementLevel = (ElementLevelType)(element / 20);
			model.Modes = elements[eleIdx++];
			model.WalkSpeed = elements[eleIdx++];
			model.AttackDelay = elements[eleIdx++];
			model.AttackMotion = elements[eleIdx++];
			model.DamageMotion = elements[eleIdx++];
			model.MvpExp = elements[eleIdx++];

			for (int i = 0; i < 3; i++) {
				ItemDrop drop = new ItemDrop();

				drop.Item = elements[eleIdx++];
				drop.Rate = elements[eleIdx++];

				if (DbReader.ToInt(drop.Item) == 0)
					continue;

				model.MvpDrops.Add(drop);
			}

			for (int i = 0; i < 10; i++) {
				ItemDrop drop = new ItemDrop();

				drop.Item = elements[eleIdx++];
				drop.Rate = elements[eleIdx++];

				if (DbReader.ToInt(drop.Item) == 0)
					continue;

				if (i == 9)
					drop.StealProtected = true;

				model.Drops.Add(drop);
			}

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, MobAttributes.Model, model, false);
			}
		}
	}
}
