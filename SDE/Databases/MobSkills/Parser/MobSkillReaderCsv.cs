using SDE.Databases.Generic.Parser;
using SDE.Databases.MobSkills.Common;
using SDE.Databases.MobSkills.Features;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using System;

namespace SDE.Databases.MobSkills.Parser {
	public class MobSkillReaderCsv : DatabaseReaderCsv<int> {
		public override void ReadEntry(DbLoadContext context, string[] elements) {
			var table = context.AbsractDb.Table;
			int uid = table.GenerateUniqueId();
			table.EnsureExists(uid);

			var tuple = table.GetTuple(uid);
			var model = tuple.GetModel<MobSkill>();
			MobSkill previousModel = model;

			if (table.EnableEvents) {  // From clipboard
				model = (MobSkill)model.Clone();
			}

			ReadEntry(model, elements);

			// From clipboard
			if (table.EnableEvents && previousModel != null) {
				if (previousModel.Equals(model))
					return;

				table.Commands.Set(tuple, MobSkillAttributes.Model, model, false);
			}
			else {
				tuple.SetRawValue(MobSkillAttributes.FileKeyRef, TextFileHelper.LastLineRead2);
			}
		}

		public void ReadEntry(MobSkill model, string[] elements) {
			int eleIdx = 0;
			LoadField(ref model.MobId, elements, eleIdx++);
			LoadField(ref model.FriendlyDisplay, elements, eleIdx++);
			LoadFieldEnum(ref model.State, elements, eleIdx++);
			LoadField(ref model.SkillId, elements, eleIdx++);
			LoadField(ref model.SkillLv, elements, eleIdx++);
			LoadField(ref model.Rate, elements, eleIdx++);
			LoadField(ref model.CastTime, elements, eleIdx++);
			LoadField(ref model.Delay, elements, eleIdx++);
			LoadFieldBool(ref model.Cancelable, elements, eleIdx++);
			LoadFieldEnum(ref model.Target, elements, eleIdx++);
			LoadFieldEnum(ref model.Cond1, elements, eleIdx++);

			if (eleIdx < elements.Length) {
				if (Int32.TryParse(elements[eleIdx], out _) || elements[eleIdx] == "") {
					model.CValue = elements[eleIdx++];
				}
				else {
					model.CValue = DbReader.LoadEnum(elements[eleIdx++], MobSkillCond2Type.SC_ANYBAD);
				}
			}

			LoadField(ref model.Val1, elements, eleIdx++);
			LoadField(ref model.Val2, elements, eleIdx++);
			LoadField(ref model.Val3, elements, eleIdx++);
			LoadField(ref model.Val4, elements, eleIdx++);
			LoadField(ref model.Val5, elements, eleIdx++);
			LoadField(ref model.Emotion, elements, eleIdx++);
			LoadField(ref model.Chat, elements, eleIdx++);
		}
	}
}
