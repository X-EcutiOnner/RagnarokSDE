using SDE.Databases.ClientItems.Features;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.View;
using System.Text;
using Utilities.Extension;

namespace SDE.Databases.ClientItems.Parser {
	public sealed class ClientItemTextFileParsers {
		public static ClientItemTextFileParser CardIllustration = new CardIllustrationParser();
		public static ClientItemTextFileParser CardAffix = new CardAffixParser();
		public static ClientItemTextFileParser CardPostfix = new CardPostfixParser();
		public static ClientItemTextFileParser NumberOfSlots = new NumberOfSlotsParser();
		public static ClientItemTextFileParser IdentifiedResourceName = new IdentifiedResourceNameParser();
		public static ClientItemTextFileParser UnidentifiedResourceName = new UnidentifiedResourceNameParser();
		public static ClientItemTextFileParser IdentifiedDescription = new IdentifiedDescriptionParser();
		public static ClientItemTextFileParser UnidentifiedDescription = new UnidentifiedDescriptionParser();
		public static ClientItemTextFileParser IdentifiedDisplayName = new IdentifiedDisplayNameParser();
		public static ClientItemTextFileParser UnidentifiedDisplayName = new UnidentifiedDisplayNameParser();
	}

	public abstract class ClientItemTextFileParser : ClientTextFileParser<ClientItem> {
		public override DataSource Source => DataSources.ClientItem;
	}

	public class CardIllustrationParser : ClientItemTextFileParser {
		public CardIllustrationParser() {
			GetFilename = () => ProjectConfiguration.ClientCardIllustration;
			AllowMultiLine = true;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.Illustration = elements[1];
			model.IsCard = true;
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (!model.IsCard)
				return false;

			AddNewLineIfNotContinuous(pItemId, itemId, b);

			b.AppendLine(itemId + "#" + model.Illustration + "#");
			return true;
		}
	}

	public class CardAffixParser : ClientItemTextFileParser {
		public CardAffixParser() {
			GetFilename = () => ProjectConfiguration.ClientCardAffixes;
			AllowMultiLine = true;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.Affix = elements[1];
			model.IsCard = true;
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (!model.IsCard)
				return false;

			AddNewLineIfNotContinuous(pItemId, itemId, b);

			b.AppendLine(itemId + "#" + model.Affix + "#");
			return true;
		}
	}

	public class CardPostfixParser : ClientItemTextFileParser {
		public CardPostfixParser() {
			GetFilename = () => ProjectConfiguration.ClientCardPostfixes;
			AllowMultiLine = true;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.IsPostfix = true;
			model.IsCard = true;
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (!model.IsPostfix)
				return false;

			b.AppendLine(itemId + "#");
			return true;
		}
	}

	public class NumberOfSlotsParser : ClientItemTextFileParser {
		private MergedTable _itemDb;

		public NumberOfSlotsParser() {
			GetFilename = () => ProjectConfiguration.ClientItemSlotCount;
			AllowMultiLine = true;
			_itemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);
		}

		public override void Read(ClientItem model, string[] elements) {
			model.NumberOfSlots = elements[1];
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (string.IsNullOrEmpty(model.NumberOfSlots))
				return false;

			int slots = model.NumberOfSlots.ToInt();

			if (slots == 0) {
				var tuple = _itemDb.TryGetTuple(itemId);

				if (tuple != null) {
					var itemModel = tuple.GetModel<Item>();

					if (itemModel.Type != ItemType.IT_WEAPON &&
						itemModel.Type != ItemType.IT_ARMOR) {
						return false;
					}
				}
			}

			b.AppendLine(itemId + "#" + DbReader.ToInt(model.NumberOfSlots) + "#");
			return true;
		}
	}

	public class IdentifiedResourceNameParser : ClientItemTextFileParser {
		public IdentifiedResourceNameParser() {
			GetFilename = () => ProjectConfiguration.ClientItemIdentifiedResourceName;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.IdentifiedResourceName = elements[1];
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (string.IsNullOrEmpty(model.IdentifiedResourceName))
				return false;

			b.AppendLine(itemId + "#" + model.IdentifiedResourceName + "#");
			return true;
		}
	}

	public class UnidentifiedResourceNameParser : ClientItemTextFileParser {
		public UnidentifiedResourceNameParser() {
			GetFilename = () => ProjectConfiguration.ClientItemUnidentifiedResourceName;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.UnidentifiedResourceName = elements[1];
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (string.IsNullOrEmpty(model.UnidentifiedResourceName))
				return false;

			b.AppendLine(itemId + "#" + model.UnidentifiedResourceName + "#");
			return true;
		}
	}

	public class IdentifiedDescriptionParser : ClientItemTextFileParser {
		public IdentifiedDescriptionParser() {
			GetFilename = () => ProjectConfiguration.ClientItemIdentifiedDescription;
			AllowMultiLine = true;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.IdentifiedDescription = elements[1].Trim('\r', '\n');
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (string.IsNullOrEmpty(model.IdentifiedDescription))
				return false;

			b.AppendLine(itemId + "#\r\n" + model.IdentifiedDescription + "\r\n#");
			return true;
		}
	}

	public class UnidentifiedDescriptionParser : ClientItemTextFileParser {
		public UnidentifiedDescriptionParser() {
			GetFilename = () => ProjectConfiguration.ClientItemUnidentifiedDescription;
			AllowMultiLine = true;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.UnidentifiedDescription = elements[1].Trim('\r', '\n');
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (string.IsNullOrEmpty(model.UnidentifiedDescription))
				return false;

			b.AppendLine(itemId + "#\r\n" + model.UnidentifiedDescription + "\r\n#");
			return true;
		}
	}

	public class IdentifiedDisplayNameParser : ClientItemTextFileParser {
		public IdentifiedDisplayNameParser() {
			GetFilename = () => ProjectConfiguration.ClientItemIdentifiedName;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.IdentifiedDisplayName = elements[1];
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (string.IsNullOrEmpty(model.IdentifiedDisplayName))
				return false;

			b.AppendLine(itemId + "#" + model.IdentifiedDisplayName.Replace(' ', '_') + "#");
			return true;
		}
	}

	public class UnidentifiedDisplayNameParser : ClientItemTextFileParser {
		public UnidentifiedDisplayNameParser() {
			GetFilename = () => ProjectConfiguration.ClientItemUnidentifiedName;
		}

		public override void Read(ClientItem model, string[] elements) {
			model.UnidentifiedDisplayName = elements[1];
		}

		public override bool Write(ClientItem model, StringBuilder b, int pItemId, int itemId) {
			if (string.IsNullOrEmpty(model.UnidentifiedDisplayName))
				return false;

			b.AppendLine(itemId + "#" + model.UnidentifiedDisplayName.Replace(' ', '_') + "#");
			return true;
		}
	}
}
