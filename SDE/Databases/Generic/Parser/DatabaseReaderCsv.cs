using SDE.Editor.Database;
using SDE.Editor.Generic.Parsers.Generic;
using SDE.Editor.Parsers;
using System;
using System.IO;

namespace SDE.Databases.Generic.Parser {
	public class DatabaseReaderCsv<TKey> : DatabaseReader {
		public override void Loader(DbLoadContext context, BaseDatabase db) {
			base.Loader(context, db);

			if (!File.Exists(context.FilePath)) {
				if (db.ThrowFileNotFoundException)
					DbIOErrorHandler.FileNotFound(db.Source);
				
				return;
			}

			foreach (string[] elements in TextFileHelper.GetElementsByCommasAll(File.ReadAllBytes(context.FilePath))) {
				try {
					ReadEntry(context, elements);
				}
				catch {
					if (elements.Length <= 0) {
						if (!context.ReportIdException("#")) return;
					}
					else if (!context.ReportIdException(elements[0])) return;
				}

				//db?.Progress?.SetTierProgress(++current / (float)total);
			}
		}

		public virtual void ReadEntry(DbLoadContext context, string[] elements) {
			throw new NotImplementedException();
		}

		public void LoadField(ref string field, string[] elements, int index) {
			if (index < elements.Length)
				field = elements[index];
		}

		public void LoadFieldEnum<TEnum>(ref TEnum field, string[] elements, int index) where TEnum : struct, Enum {
			if (index < elements.Length) {
				DbReader.LoadEnum(ref field, elements[index]);
			}
		}

		public void LoadFieldBool(ref bool field, string[] elements, int index) {
			if (index < elements.Length) {
				var ele = elements[index].ToString().ToLowerInvariant();
				field = ele == "1" || ele == "true" || ele == "yes";
			}
		}
	}
}
