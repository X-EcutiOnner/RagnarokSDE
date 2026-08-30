using SDE.Editor.Database;
using SDE.Editor.Parsers.Libconfig;
using SDE.Editor.Parsers.Yaml;
using System;
using System.Linq;
using System.Text;

namespace SDE.Databases.Generic.Parser {
	public abstract class DatabaseWriterYaml : DatabaseWriter {
		public abstract string KeyField { get; }

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			base.Writer(context, db);

			try {
				var lines = new YamlParser(context.OldPath, ParserMode.Write, KeyField);

				if (lines.Output == null)
					return;

				lines.Remove(db, KeyToYamlKey);

				foreach (ReadableTuple tuple in db.Table.FastItems.Where(p => !p.Normal).OrderBy(p => p.Key)) {
					StringBuilder builder = new StringBuilder();
					WriteEntry(builder, tuple);
					lines.Write(KeyToYamlKey(tuple.Key), builder.ToString().Trim('\r', '\n'));
				}

				lines.WriteFile(context.FilePath);
			}
			catch (Exception err) {
				context.ReportException(err);
			}
		}

		public virtual string KeyToYamlKey(int key) {
			return key.ToString();
		}
	}
}
