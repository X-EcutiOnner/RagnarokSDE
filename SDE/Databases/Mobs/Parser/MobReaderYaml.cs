using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Mobs.Common;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using System;
using System.Linq;

namespace SDE.Databases.Mobs.Parser {
	public class MobReaderYaml : DatabaseReaderYaml<int> {
		public override string KeyField => "Id";

		public override void ReadEntry(DbLoadContext context, ParserObject mob) {
			int id = Int32.Parse(mob[KeyField]);

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

			foreach (var entry in mob.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "AegisName":
						model.AegisName = entry.ObjectValue;
						break;
					case "Name":
						model.Name = entry.ObjectValue;
						break;
					case "JapaneseName":
						model.JapaneseName = entry.ObjectValue;
						break;
					case "Level":
						model.Level = entry.ObjectValue;
						break;
					case "Hp":
						model.Hp = entry.ObjectValue;
						break;
					case "Sp":
						model.Sp = entry.ObjectValue;
						break;
					case "BaseExp":
						model.BaseExp = entry.ObjectValue;
						break;
					case "JobExp":
						model.JobExp = entry.ObjectValue;
						break;
					case "MvpExp":
						model.MvpExp = entry.ObjectValue;
						break;
					case "Attack":
						model.Attack = entry.ObjectValue;
						break;
					case "Attack2":
						model.Attack2 = entry.ObjectValue;
						break;
					case "Defense":
						model.Defense = entry.ObjectValue;
						break;
					case "MagicDefense":
						model.MagicDefense = entry.ObjectValue;
						break;
					case "Resistance":
						model.Resistance = entry.ObjectValue;
						break;
					case "MagicResistance":
						model.MagicResistance = entry.ObjectValue;
						break;
					case "Str":
						model.Str = entry.ObjectValue;
						break;
					case "Agi":
						model.Agi = entry.ObjectValue;
						break;
					case "Vit":
						model.Vit = entry.ObjectValue;
						break;
					case "Int":
						model.Int = entry.ObjectValue;
						break;
					case "Dex":
						model.Dex = entry.ObjectValue;
						break;
					case "Luk":
						model.Luk = entry.ObjectValue;
						break;
					case "AttackRange":
						model.AttackRange = entry.ObjectValue;
						break;
					case "SkillRange":
						model.SkillRange = entry.ObjectValue;
						break;
					case "ChaseRange":
						model.ChaseRange = entry.ObjectValue;
						break;
					case "Size":
						model.Size = DbReader.LoadEnum(entry, SizeType.Size_Small);
						break;
					case "Race":
						model.Race = DbReader.LoadEnum(entry, RaceType.RC_FORMLESS);
						break;
					case "RaceGroups":
						model.RaceGroups = DbReader.LoadFlag<Race2Flag>(entry.Value);
						break;
					case "Element":
						model.Element = DbReader.LoadEnum(entry, ElementType.ELE_NEUTRAL);
						break;
					case "ElementLevel":
						model.ElementLevel = (ElementLevelType)Int32.Parse(entry.ObjectValue);
						break;
					case "WalkSpeed":
						model.WalkSpeed = entry.ObjectValue;
						break;
					case "AttackDelay":
						model.AttackDelay = entry.ObjectValue;
						break;
					case "AttackMotion":
						model.AttackMotion = entry.ObjectValue;
						break;
					case "ClientAttackMotion":
						model.ClientAttackMotion = entry.ObjectValue;
						break;
					case "DamageMotion":
						model.DamageMotion = entry.ObjectValue;
						break;
					case "DamageTaken":
						model.DamageTaken = entry.ObjectValue;
						break;
					case "Title":
						model.Title = entry.ObjectValue;
						break;
					case "GroupId":
						model.GroupId = entry.ObjectValue;
						break;
					case "Class":
						model.Class = DbReader.LoadEnum(entry, ClassType.CLASS_NORMAL);
						break;
					case "MvpDrops":
						foreach (var list in entry.Value.OfType<ParserArray>()) {
							model.MvpDrops.Add(LoadItemDrop(list));
						}
						break;
					case "Drops":
						foreach (var list in entry.Value.OfType<ParserArray>()) {
							model.Drops.Add(LoadItemDrop(list));
						}
						break;
				}

				long mode = 0;

				if (mob["Ai"] != null) {
					MonsterType ai = 0;
					DbReader.LoadEnum(ref ai, mob["Ai"].ObjectValue);
					mode |= (long)ai;
				}

				mode |= Int64.Parse(DbReader.LoadFlag<ModeFlag>(mob["Modes"]));
				model.Modes = mode.ToString();

				if (model.JapaneseName == null)
					model.JapaneseName = model.Name;
			}

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, MobAttributes.Model, model, false);
			}
		}

		public ItemDrop LoadItemDrop(ParserObject list) {
			var listDrop = new ItemDrop();

			foreach (var drop in list.OfType<ParserKeyValue>()) {
				switch (drop.Key) {
					case "Item":
						listDrop.Item = CachedDbs.AegisNameItem.ToStringId(drop.ObjectValue);
						break;
					case "Rate":
						listDrop.Rate = drop.ObjectValue;
						break;
					case "StealProtected":
						listDrop.StealProtected = Boolean.Parse(drop.ObjectValue);
						break;
					case "RandomOptionGroup":
						listDrop.RandomOptionGroup = drop.ObjectValue;
						break;
					case "Index":
						listDrop.Index = drop.ObjectValue;
						break;
				}
			}

			return listDrop;
		}
	}
}
