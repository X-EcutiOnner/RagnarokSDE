using System;
using System.Text;

namespace SDE.Databases.Generic.Parser {
	public abstract class ClientTextFileParser<TModel> {
		public Func<string> GetFilename;
		public bool AllowMultiLine = false;
		public abstract DataSource Source { get; }

		public abstract bool Write(TModel model, StringBuilder b, int pItemId, int itemId);
		public abstract void Read(TModel model, string[] elements);

		public void AddNewLineIfNotContinuous(int previousItemId, int itemId, StringBuilder b) {
			if (previousItemId > -1 && previousItemId != itemId - 1)
				b.AppendLine();
		}
	}
}
