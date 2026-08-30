using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Quests.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using System;
using System.Linq;

namespace SDE.Databases.Quests.Parser {
	public class QuestReaderYaml : DatabaseReaderYaml<int> {
		public override string KeyField => "Id";

		public override void ReadEntry(DbLoadContext context, ParserObject quest) {
			int id = Int32.Parse(quest[KeyField]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Quest>();
			Quest previousModel = model;

			if (table.EnableEvents) {	// From clipboard
				model = (Quest)model.Clone();
				model.Targets.Clear();
				model.Drops.Clear();
			}

			foreach (var entry in quest.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "Title":
						model.Title = entry.ObjectValue;
						break;
					case "TimeLimit":
						model.TimeLimit = entry.ObjectValue;
						break;
					case "Targets":
						foreach (var targetList in entry.Value.OfType<ParserArray>()) {
							model.Targets.Add(LoadQuestTarget(targetList));
						}
						break;
					case "Drops":
						foreach (var dropList in entry.Value.OfType<ParserArray>()) {
							model.Drops.Add(LoadQuestDrop(dropList));
						}
						break;
				}
			}

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, QuestAttributes.Model, model, false);
			}
		}

		public QuestDrop LoadQuestDrop(ParserObject dropList) {
			var questDrop = new QuestDrop();

			foreach (var drop in dropList.OfType<ParserKeyValue>()) {
				switch (drop.Key) {
					case "Mob":
						questDrop.Mob = CachedDbs.AegisNameMob.ToStringId(drop.ObjectValue);
						break;
					case "Item":
						questDrop.Item = CachedDbs.AegisNameItem.ToStringId(drop.ObjectValue);
						break;
					case "Count":
						questDrop.Count = drop.ObjectValue;
						break;
					case "Rate":
						questDrop.Rate = drop.ObjectValue;
						break;
				}
			}

			return questDrop;
		}

		//public QuestTarget LoadQuestTarget(ParserArray targetList) {
		public QuestTarget LoadQuestTarget(ParserObject targetList) {
			var questTarget = new QuestTarget();

			foreach (var target in targetList.OfType<ParserKeyValue>()) {
				switch (target.Key) {
					case "Mob":
						questTarget.Mob = CachedDbs.AegisNameMob.ToStringId(target.ObjectValue);
						break;
					case "Count":
						questTarget.Count = target.ObjectValue;
						break;
					case "Id":
						questTarget.Id = target.ObjectValue;
						break;
					case "Race":
						questTarget.Race = DbReader.LoadEnum(target.ObjectValue, RaceType.RC_ALL);
						break;
					case "Size":
						questTarget.Size = DbReader.LoadEnum(target.ObjectValue, SizeType.SZ_ALL);
						break;
					case "Element":
						questTarget.Element = DbReader.LoadEnum(target.ObjectValue, ElementType.ELE_ALL);
						break;
					case "MinLevel":
						questTarget.MinLevel = target.ObjectValue;
						break;
					case "MaxLevel":
						questTarget.MaxLevel = target.ObjectValue;
						break;
					case "Location":
						questTarget.Location = target.ObjectValue;
						break;
					case "MapName":
						questTarget.MapName = target.ObjectValue;
						break;
					case "MapMobTargets":
						var list = (ParserList)target.Value;

						foreach (ParserKeyValue entry in list) {
							questTarget.MapMobTargets.Add(LoadMapMobTarget(entry));
						}

						break;
				}
			}

			return questTarget;
		}

		public MapMobTarget LoadMapMobTarget(ParserKeyValue entry) {
			MapMobTarget mapMobTarget = new MapMobTarget();

			mapMobTarget.MobName = CachedDbs.AegisNameMob.ToStringId(entry.Key);
			mapMobTarget.Active = Boolean.Parse(entry.Value.ObjectValue);

			return mapMobTarget;
		}
	}
}
