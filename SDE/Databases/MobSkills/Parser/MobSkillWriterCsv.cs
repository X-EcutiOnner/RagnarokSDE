using Database;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.MobSkills.Common;
using SDE.Databases.MobSkills.Features;
using SDE.Editor.Database;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace SDE.Databases.MobSkills.Parser {
	public class MobSkillWriterCsv : DatabaseWriterCsv {
		public override string KeyField => "Id";
		public override DbAttribute FileKeyRef => MobSkillAttributes.FileKeyRef;

		public override string WriteEntry(ReadableTuple tuple) {
			if (tuple == null)
				return "";

			var model = tuple.GetModel<MobSkill>();

			return _writeEntry(model);
		}

		private string _writeEntry(MobSkill model) {
			var output = new List<string> {
				model.MobId,
				model.FriendlyDisplay,
				EnumInfos.ToYamlString(model.State),
				DbWriter.SetZeroDefault(model.SkillId),
				DbWriter.SetZeroDefault(model.SkillLv),
				DbWriter.SetZeroDefault(model.Rate),
				DbWriter.SetZeroDefault(model.CastTime),
				DbWriter.SetZeroDefault(model.Delay),
				model.Cancelable ? "yes" : "no",
				EnumInfos.ToYamlString(model.Target),
				EnumInfos.ToYamlString(model.Cond1),
				model.CValue is MobSkillCond2Type mobSkillCond2 ? EnumInfos.ToYamlString(mobSkillCond2) : DbWriter.SetZeroDefault(model.CValue as string),
				model.Val1,
				model.Val2,
				model.Val3,
				model.Val4,
				model.Val5,
				model.Emotion,
				model.Chat
			};

			return Methods.Aggregate(output, ",");
		}

		public void WriteEntry(StringBuilder builder, MobSkill model) {
			builder.AppendLine(_writeEntry(model));
		}
	}
}
