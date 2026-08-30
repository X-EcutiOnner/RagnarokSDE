using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using System;
using System.Linq;

namespace SDE.Databases.Items.Parser {
	public class ItemReaderYaml : DatabaseReaderYaml<int> {
		public override string KeyField => "Id";

		public override void ReadEntry(DbLoadContext context, ParserObject item) {
			int id = Int32.Parse(item[KeyField]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			var model = tuple.GetModel<Item>();
			Item previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (Item)model.Clone();
			}

			// Some propertie must be read first
			model.Type = DbReader.LoadEnum(item["Type"], ItemType.IT_ETC);

			foreach (var entry in item.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "AegisName":
						model.AegisName = entry.ObjectValue;
						break;
					case "Name":
						model.Name = entry.ObjectValue;
						break;
					case "SubType":
						switch (model.Type) {
							case ItemType.IT_AMMO:
								model.AmmoType = DbReader.LoadEnum<AmmoType>(entry.Value, 0);
								break;
							case ItemType.IT_CARD:
								model.CardType = DbReader.LoadEnum<CardType>(entry.Value, 0);
								break;
							case ItemType.IT_WEAPON:
								model.WeaponType = DbReader.LoadEnum<WeaponType>(entry.Value, 0);
								break;
						}
						break;
					case "Buy":
						model.Buy = entry.ObjectValue;
						break;
					case "Sell":
						model.Sell = entry.ObjectValue;
						break;
					case "Weight":
						model.Weight = entry.ObjectValue;
						break;
					case "Attack":
						model.Attack = entry.ObjectValue;
						break;
					case "MagicAttack":
						model.MagicAttack = entry.ObjectValue;
						break;
					case "Defense":
						model.Defense = entry.ObjectValue;
						break;
					case "Range":
						model.Range = entry.ObjectValue;
						break;
					case "Slots":
						model.Slots = entry.ObjectValue;
						break;
					case "Jobs":
						model.Jobs = ReadJobs(entry.Value);
						break;
					case "Classes":
						model.Classes = DbReader.LoadFlag<ItemJobFlag>(entry.Value);
						break;
					case "Gender":
						model.Gender = DbReader.LoadEnum(entry.Value, GenderType.SEX_BOTH);
						break;
					case "Locations":
						model.Locations = DbReader.LoadFlag<EquipLocationFlag>(entry.Value);
						break;
					case "WeaponLevel":
						model.WeaponLevel = entry.ObjectValue;
						break;
					case "ArmorLevel":
						model.ArmorLevel = entry.ObjectValue;
						break;
					case "EquipLevelMin":
						model.EquipLevelMin = entry.ObjectValue;
						break;
					case "EquipLevelMax":
						model.EquipLevelMax = entry.ObjectValue;
						break;
					case "Refineable":
						model.Refineable = Boolean.Parse(entry.ObjectValue);
						break;
					case "Gradable":
						model.Gradable = Boolean.Parse(entry.ObjectValue);
						break;
					case "View":
						model.View = entry.ObjectValue;
						break;
					case "AliasName":
						model.AliasName = CachedDbs.AegisNameItem.ToStringId(entry.ObjectValue);
						break;
					case "Flags":
						model.Flags = DbReader.LoadFlag<ItemFlag>(entry.Value);
						model.DropEffect = model.DropEffect = DbReader.LoadEnum(entry.Value["DropEffect"], DropEffectType.DROPEFFECT_NONE);
						break;
					case "Delay":
						foreach (var delayEntry in entry.Value.OfType<ParserKeyValue>()) {
							switch (delayEntry.Key) {
								case "Duration":
									model.Delay = delayEntry.ObjectValue;
									break;
								case "Status":
									model.DelayStatus = delayEntry.ObjectValue;
									break;
							}
						}
						break;
					case "Stack":
						var stack = entry.Value;

						model.StackAmount = stack["Amount"] ?? "";
						model.StackFlags = DbReader.LoadFlag<ItemStackFlag>(stack);
						break;
					case "NoUse":
						var noUse = entry.Value;

						model.NoUseOverride = noUse["Override"] ?? "100";
						model.NoUseFlags = DbReader.LoadFlag<NoUseFlag>(noUse);
						break;
					case "Trade":
						var trade = entry.Value;

						model.TradeOverride = trade["Override"] ?? "100";
						model.TradeFlags = DbReader.LoadFlag<TradeFlag>(trade);
						break;
					case "Script":
						model.Script = entry.ObjectValue;
						break;
					case "EquipScript":
						model.EquipScript = entry.ObjectValue;
						break;
					case "UnEquipScript":
						model.UnEquipScript = entry.ObjectValue;
						break;
				}
			}

			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, ItemAttributes.Model, model, false);
			}
		}

		public string ReadJobs(ParserObject parser) {
			var all = parser["All"];
			UInt64 flag = 0;

			if (all != null) {
				if (Boolean.Parse(all.ObjectValue))
					flag = UInt64.MaxValue;
			}

			foreach (ParserKeyValue entry in parser) {
				if (entry.Key == "All")
					continue;

				bool active = Boolean.Parse(entry.ObjectValue);

				EAJs v = default;

				if (!DbReader.LoadEnum(ref v, entry.Key)) {
					continue;
				}

				// Strip all job modifiers
				MAPIDs mapid = ((MAPIDs)v & MAPIDs.MAPID_FIRSTMASK);

				// Add back 2nd class tree
				if (((MAPIDs)v & (MAPIDs)JOBLs.JOBL_2_1) == (MAPIDs)JOBLs.JOBL_2_1)
					mapid |= (MAPIDs)JOBLs.JOBL_2_1;
				else if (((MAPIDs)v & (MAPIDs)JOBLs.JOBL_2_2) == (MAPIDs)JOBLs.JOBL_2_2)
					mapid |= (MAPIDs)JOBLs.JOBL_2_2;

				// Convert MAPID to SDE's internal flag ID.
				if (active)
					flag |= Job.MapId2Job[mapid].JobSdeUid;
				else
					flag &= ~Job.MapId2Job[mapid].JobSdeUid;
			}

			return "0x" + flag.ToString("X16");
		}
	}
}
