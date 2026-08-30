using SDE.Core;
using SDE.Editor.Database;
using SDE.Editor.Parsers;
using SDE.Editor.Parsers.Yaml;
using System;
using System.Linq;

namespace SDE.Databases.Generic.Parser {
	public abstract class DatabaseReaderYaml<TKey> : DatabaseReader {
		public abstract string KeyField { get; }

		public override void Loader(DbLoadContext context, BaseDatabase db) {
			base.Loader(context, db);

			var parser = GetParser(context, db);

			if (parser.Output == null || ((ParserArray)parser.Output).Objects.Count == 0)
				return;

			var body = parser.Output["copy_paste"] ?? parser.Output["Body"];

			if (body != null) {
				var lines = (parser.Output["copy_paste"] ?? parser.Output["Body"]).ToList();
				int current = 0;
				int total = lines.Count;

				foreach (var entry in lines) {
					if (!ParseEntry(context, db, entry))
						return;

					db?.Progress?.SetTierProgress(++current / (float)total);
				}
			}

			DbReader.ParseImports(context, db, parser, Loader);
		}

		public virtual YamlParser GetParser(DbLoadContext context, BaseDatabase db) {
			return new YamlParser(context.FilePath);
		}

		public virtual bool ParseEntry(DbLoadContext context, BaseDatabase db, ParserObject entry) {
			try {
				ReadEntry(context, entry);
			}
			catch (Exception err) {
				if (entry[KeyField] == null) {
					//if (!context.ReportIdException("#", entry.Line)) return false;
					if (!context.ReportIdException(new FileParserException(context.FilePath, entry.Line, err.Message, err), "#")) return false;
				}
				//else if (!context.ReportIdException(entry[KeyField], entry.Line)) return false;
				else if (!context.ReportIdException(new FileParserException(context.FilePath, entry.Line, err.Message, err), entry[KeyField])) return false;
			}

			return true;
		}

		public virtual void ReadEntry(DbLoadContext context, ParserObject entry) {
			throw new NotImplementedException();
		}
	}
}
