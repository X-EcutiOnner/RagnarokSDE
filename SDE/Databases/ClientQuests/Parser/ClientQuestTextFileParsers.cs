using SDE.Databases.ClientQuests.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor;
using System.Text;

namespace SDE.Databases.ClientQuests.Parser {
	public sealed class ClientQuestTextFileParsers {
		public static ClientQuestTextFileParser Quest = new TextQuestParser();
	}

	public abstract class ClientQuestTextFileParser : ClientTextFileParser<ClientQuest> {
		public override DataSource Source => DataSources.ClientQuest;
	}

	public class TextQuestParser : ClientQuestTextFileParser {
		public TextQuestParser() {
			GetFilename = () => ProjectConfiguration.ClientQuest;
			AllowMultiLine = true;
		}

		public override void Read(ClientQuest model, string[] elements) {
			model.Title = elements[1];
			model.SG = elements[2];
			model.QUE = elements[3];
			model.Description = elements[4];
			model.Summary = elements[5];
		}

		public override bool Write(ClientQuest model, StringBuilder b, int pItemId, int itemId) {
			b.AppendLine(itemId + "#" + model.Title + "#" + model.SG + "#" + model.QUE + "#\r\n" + model.Description + "#\r\n" + model.Summary + "#");

			if (pItemId != -2)
				b.AppendLine();

			return true;
		}
	}
}
