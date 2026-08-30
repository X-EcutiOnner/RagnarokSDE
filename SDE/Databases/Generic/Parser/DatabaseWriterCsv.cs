using Database;
using SDE.Editor.Database;
using SDE.Editor.Writers;
using System;
using System.Linq;
using System.Text;

namespace SDE.Databases.Generic.Parser {
	public abstract class DatabaseWriterCsv : DatabaseWriter {
		public abstract string KeyField { get; }
		public virtual DbAttribute FileKeyRef { get; } = null;

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			base.Writer(context, db);

			CsvWriter lines = new CsvWriter(context.OldPath, useUniqueId: db.Table.UseUniqueId, fileKeyRef: FileKeyRef);
			lines.Remove(db);

			//foreach (ReadableTuple tuple in db.Table.FastItems.OrderBy(p => p.Key)) {
			foreach (ReadableTuple tuple in db.Table.FastItems.Where(p => !p.Normal).OrderBy(p => p.Key)) {
				int key = tuple.GetKey<int>();
				string line = WriteEntry(tuple);

				if (FileKeyRef != null) {
					var keyRef = tuple.GetValue<string>(FileKeyRef);
					lines.Write(keyRef ?? line, line);
				}
				else {
					lines.Write(key, line);
				}
			}

			lines.WriteFile(context.FilePath);
		}

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			builder.AppendLine(WriteEntry(tuple));
		}

		public virtual string WriteEntry(ReadableTuple tuple) {
			throw new NotImplementedException();
		}
	}
}
