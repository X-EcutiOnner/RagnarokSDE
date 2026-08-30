using SDE.Databases.Achievements.Common;
using SDE.Databases.Achievements.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using System;
using System.Linq;

namespace SDE.Databases.Achievements.Parser {
	public class AchvReaderYaml : DatabaseReaderYaml<int> {
		public override string KeyField => "Id";

		public override void ReadEntry(DbLoadContext context, ParserObject achv) {
			int id = Int32.Parse(achv[KeyField]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Achv>();
			Achv previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (Achv)model.Clone();
				model.Targets.Clear();
				model.Dependents.Clear();
			}

			foreach (var entry in achv.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "Group":
						model.Group = DbReader.LoadEnum(entry.ObjectValue, AchvGroupType.AG_NONE);
						break;
					case "Name":
						model.Name = entry.ObjectValue;
						break;
					case "Targets":
						foreach (var target in entry.Value) {
							model.Targets.Add(LoadTarget(target));
						}

						// Check if target IDs match their order. This is almost always the case
						// and thus makes the property somewhat useless for most people.
						// It's only useful when overwriting an existing target.
						model.Targets = model.Targets.OrderBy(p => Int32.Parse(p.Id)).ToList();

						for (int i = 0; i < model.Targets.Count; i++) {
							if (Int32.Parse(model.Targets[i].Id) == i) {
								model.Targets[i].Id = "";
							}
						}
						break;
					case "Condition":
						model.Condition = entry.ObjectValue.Trim(' ');
						break;
					case "Map":
						model.Map = entry.ObjectValue;
						break;
					case "Dependents":
						foreach (var dependent in entry.Value) {
							model.Dependents.Add(LoadDependent(dependent));
						}
						break;
					case "Rewards":
						foreach (var reward in entry.Value.OfType<ParserKeyValue>()) {
							switch (reward.Key) {
								case "Item":
									model.RewardItem = CachedDbs.AegisNameItem.ToStringId(reward.ObjectValue);
									break;
								case "Amount":
									model.RewardAmount = reward.ObjectValue;
									break;
								case "Script":
									model.RewardScript = reward.ObjectValue.Trim(' ', '\t');
									break;
								case "TitleId":
									model.RewardTitleId = reward.ObjectValue;
									break;
							}
						}
						break;
					case "Score":
						model.Score = entry.ObjectValue;
						break;
				}
			}

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, AchvAttributes.Model, model, false);
			}
		}

		public AchvTarget LoadTarget(ParserObject parser) {
			AchvTarget entry = new AchvTarget();

			foreach (var parserEntry in parser.OfType<ParserKeyValue>()) {
				switch (parserEntry.Key) {
					case "Id":
						entry.Id = parserEntry.ObjectValue;
						break;
					case "Mob":
						entry.Mob = CachedDbs.AegisNameMob.ToStringId(parserEntry.ObjectValue);
						break;
					case "Count":
						entry.Count = parserEntry.ObjectValue;
						break;
				}
			}

			return entry;
		}

		public AchvDependent LoadDependent(ParserObject parser) {
			AchvDependent entry = new AchvDependent();
			var keyValue = (ParserKeyValue)parser;

			entry.Id = keyValue.Key;
			entry.Active = Boolean.Parse(keyValue.Value);

			return entry;
		}
	}
}
