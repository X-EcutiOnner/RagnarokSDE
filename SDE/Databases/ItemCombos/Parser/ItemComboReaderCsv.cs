using SDE.Databases.Generic.Parser;
using SDE.Databases.ItemCombos.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.ItemCombos.Parser {
	public class ItemComboReaderCsv : DatabaseReaderCsv<int> {
		private Dictionary<long, int> _comboId2Tuple = new Dictionary<long, int>();

		public override void Loader(DbLoadContext context, BaseDatabase db) {
			_comboId2Tuple.Clear();

			if (db != null && db.Table.EnableRawEvents) {
				foreach (var tuple in db.Table.FastItems) {
					var model = tuple.GetModel<ItemCombo>();
					_comboId2Tuple[model.ToUniqueId()] = tuple.Key;
				}
			}

			base.Loader(context, db);
		}

		public override void ReadEntry(DbLoadContext context, string[] elements) {
			var table = context.AbsractDb.Table;

			if (table.EnableEvents) {  // From clipboard
				var model = ParseModel(new ItemCombo(), elements);
				var modelUid = model.ToUniqueId();

				if (_comboId2Tuple.TryGetValue(modelUid, out int key)) {
					var previousModel = table.GetTuple(key).GetModel<ItemCombo>();
					
					if (!previousModel.Equals(model))
						table.SetRaw(key, ItemComboAttributes.Model, model);
				}
				else {
					table.SetRaw(table.GenerateUniqueId(), ItemComboAttributes.Model, model);
				}
			}
			else {
				// Normal read
				var model = ParseModel(new ItemCombo(), elements);
				var modelUid = model.ToUniqueId();

				if (_comboId2Tuple.TryGetValue(modelUid, out int key)) {
					table.SetRaw(key, ItemComboAttributes.Model, model);
				}
				else {
					int uid = table.GenerateUniqueId();
					table.SetRaw(uid, ItemComboAttributes.Model, model);
					table.SetRaw(uid, ItemComboAttributes.FileKeyRef, TextFileHelper.LastLineRead2);
				}
			}
		}

		private ItemCombo ParseModel(ItemCombo model, string[] elements) {
			int eleIdx = 0;
			model.NameIds.AddRange(elements[eleIdx++].Split(':').Select(p => new NameId(p)).ToList());
			model.Script = DbReader.FromScript(elements[eleIdx++]);

			for (int i = model.NameIds.Count; i < ItemCombo.MaxNameIdCount; i++) {
				model.NameIds.Add(new NameId());
			}

			return model;
		}
	}
}
