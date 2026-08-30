using SDE.Databases.Generic.Parser;
using SDE.Databases.ItemCombos.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using SDE.Editor.Parsers.Yaml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.ItemCombos.Parser {
	public class ItemComboReaderYaml : DatabaseReaderYaml<int> {
		public override string KeyField => "Combos";
		private Dictionary<long, int> _comboId2Tuple = new Dictionary<long, int>();

		public override void Loader(DbLoadContext context, BaseDatabase db) {
			if (db != null && db.Table.EnableRawEvents) {
				_comboId2Tuple.Clear();

				foreach (var tuple in db.Table.FastItems) {
					var model = tuple.GetModel<ItemCombo>();
					_comboId2Tuple[model.ToUniqueId()] = tuple.Key;
				}
			}

			base.Loader(context, db);
		}

		public override void ReadEntry(DbLoadContext context, ParserObject itemCombos) {
			var table = context.AbsractDb.Table;
			List<ItemCombo> combos = new List<ItemCombo>();

			foreach (var entry in itemCombos.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "Combos":
						foreach (var itemCombo in entry.Value) {
							ItemCombo combo = new ItemCombo();

							foreach (var item in itemCombo["Combo"]) {
								combo.NameIds.Add(CachedDbs.AegisNameItem.ToStringId(item.First().ObjectValue));
							}

							for (int i = combo.NameIds.Count; i < ItemCombo.MaxNameIdCount; i++) {
								combo.NameIds.Add(new NameId());
							}

							combos.Add(combo);
						}
						break;
					case "Script":
						foreach (var combo in combos) {
							combo.Script = entry.ObjectValue;
						}
						break;
					case "Clear":
						foreach (var combo in combos) {
							combo.Clear = Boolean.Parse(entry.ObjectValue);
						}
						break;
				}
			}

			foreach (var combo in combos) {
				// From clipboard, check if the combo already exists
				if (table.EnableRawEvents) {
					var uid = combo.ToUniqueId();

					if (_comboId2Tuple.TryGetValue(uid, out int key)) {
						var previousModel = table.GetTuple(key).GetModel<ItemCombo>();
						
						if (!previousModel.Equals(combo))
							table.SetRaw(key, ItemComboAttributes.Model, combo);
					}
					else {
						table.SetRaw(table.GenerateUniqueId(), ItemComboAttributes.Model, combo);
					}
				}
				else {
					table.SetRaw(table.GenerateUniqueId(), ItemComboAttributes.Model, combo);
				}
			}
		}
	}

	public class ItemComboIndexReaderYaml : ItemComboReaderYaml {
		public Dictionary<int, ParserObject> IndexedParser = new Dictionary<int, ParserObject>();
		public Dictionary<string, ParserObject> ScriptToParser = new Dictionary<string, ParserObject>();
		private int _uid = 1;
		private YamlParser _parser;

		public ItemComboIndexReaderYaml(YamlParser parser) {
			_parser = parser;
		}

		public override YamlParser GetParser(DbLoadContext context, BaseDatabase db) {
			return _parser;
		}

		public override bool ParseEntry(DbLoadContext context, BaseDatabase db, ParserObject entry) {
			try {
				ReadEntry(context, entry);
			}
			catch {
				// Ignore
			}

			return true;
		}

		public override void ReadEntry(DbLoadContext context, ParserObject itemCombos) {
			List<ItemCombo> combos = new List<ItemCombo>();

			foreach (var entry in itemCombos.OfType<ParserKeyValue>()) {
				switch (entry.Key) {
					case "Combos":
						foreach (var itemCombo in entry.Value) {
							IndexedParser[_uid++] = itemCombo["Combo"];
						}
						break;
					case "Script":
						ScriptToParser[entry.ObjectValue] = entry;
						break;
				}
			}
		}
	}
}
