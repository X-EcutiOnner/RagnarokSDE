using SDE.Databases.Generic.Parser;
using SDE.Databases.ItemCombos.Features;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;
using Database.Commands;
using SDE.View;
using SDE.Editor.Database;
using SDE.Editor.Parsers.Libconfig;
using SDE.Editor.Parsers;
using SDE.Editor.Parsers.Yaml;
using SDE.Editor.Files;

namespace SDE.Databases.ItemCombos.Parser {
	public class ItemComboWriterYaml : DatabaseWriterYaml {
		public override string KeyField => "Combos";

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			context.MetaMobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
			context.MetaItemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);

			try {
				var lines = new YamlParser(context.OldPath, ParserMode.Write, KeyField);

				ItemComboIndexReaderYaml indexReader = new ItemComboIndexReaderYaml(lines);
				indexReader.Loader(null, null);

				Dictionary<int, ParserObject> entries = indexReader.IndexedParser;
				Dictionary<string, ParserObject> indexedScripts = indexReader.ScriptToParser;

				var allLines = lines.AllLines;

				foreach (GroupCommand<int, ReadableTuple> command in db.Table.Commands.GetUndoCommands().OfType<GroupCommand<int, ReadableTuple>>()) {
					foreach (DeleteTuple<int, ReadableTuple> deleteCommand in command.Commands.OfType<DeleteTuple<int, ReadableTuple>>()) {
						Delete(entries[deleteCommand.Key], allLines);
					}

					foreach (ChangeTupleKey<int, ReadableTuple> changeTupleKeyCommand in command.Commands.OfType<ChangeTupleKey<int, ReadableTuple>>()) {
						// If the key was changed, the old key must be removed
						Delete(entries[changeTupleKeyCommand.Key], allLines);
					}
				}

				foreach (ChangeTupleKey<int, ReadableTuple> command in db.Table.Commands.GetUndoCommands().OfType<ChangeTupleKey<int, ReadableTuple>>()) {
					// If the key was changed, the old key must be removed
					Delete(entries[command.Key], allLines);
				}

				Dictionary<string, List<ItemCombo>> modelsToAdd = new Dictionary<string, List<ItemCombo>>();

				foreach (ReadableTuple tuple in db.Table.FastItems.Where(p => !p.Normal).OrderBy(p => p.GetKey<int>())) {
					int key = tuple.GetKey<int>();
					var model = tuple.GetModel<ItemCombo>();

					if (tuple.Modified) {
						var parserEntry = entries[key];
						ClearLines(parserEntry, allLines);

						if (model.Script == parserEntry.Parent.Parent.Parent["Script"].ObjectValue) {
							StringBuilder builderEntry = new StringBuilder();
							WriteSubCombo(builderEntry, model);
							var entryData = builderEntry.ToString().Trim('\r', '\n');
							
							allLines[parserEntry.Line - 1] = entryData;
							continue;
						}
					}

					if (!modelsToAdd.TryGetValue(model.Script, out var list)) {
						list = new List<ItemCombo>();
						modelsToAdd[model.Script] = list;
					}

					list.Add(model);
				}

				Dictionary<string, List<ItemCombo>> remainingModelsToAdd = new Dictionary<string, List<ItemCombo>>();
				var body = lines.Output["Body"];

				foreach (var modelsToAddEntry in modelsToAdd) {
					var script = modelsToAddEntry.Key;
					var models = modelsToAddEntry.Value;

					// The entry already exists, mark it as added through insert line to sort them
					if (indexedScripts.TryGetValue(script, out var parserObject)) {
						StringBuilder builderEntry = new StringBuilder();

						foreach (var model in models) {
							WriteSubCombo(builderEntry, model);
						}

						var entryData = builderEntry.ToString().Trim('\r', '\n');
						var prevLine = allLines[parserObject.Line - 2];

						allLines[parserObject.Line - 2] = (prevLine != null ? prevLine + "\r\n" : "") + entryData;
					}
					else {
						StringBuilder builderEntry = new StringBuilder();

						builderEntry.AppendLine("  - Combos:");

						foreach (var model in models) {
							WriteSubCombo(builderEntry, model);
						}

						builderEntry.AppendLine("    Script: |");
						builderEntry.AppendLine(DbWriter.ToYamlScript(script, "      "));

						var entryData = builderEntry.ToString().Trim('\r', '\n');
						var prevLine = allLines[body.Line + body.Length - 2];

						allLines[body.Line + body.Length - 2] = (prevLine != null ? prevLine + "\r\n" : "") + entryData;
					}
				}

				StringBuilder builder = new StringBuilder();
				foreach (var line in allLines) {
					if (line != null)
						builder.AppendLine(line);
				}

				IOHelper.WriteAllText(context.FilePath, builder.ToString());
			}
			catch (Exception err) {
				context.ReportException(err);
			}
		}

		public class ItemComboAdd {
			public ItemCombo Model;
		}

		public void ClearLines(ParserObject parserObject, List<string> allLines) {
			for (int i = 0; i < parserObject.Length; i++) {
				allLines[i + parserObject.Line - 1] = null;
			}
		}

		public void Delete(ParserObject parserObject, List<string> allLines) {
			ClearLines(parserObject, allLines);

			parserObject.Removed = true;
			parserObject.Parent.Removed = true;

			if (parserObject.Parent.Parent.All(p => p.Removed)) {
				Delete(parserObject.Parent.Parent.Parent, allLines);

				// Mark all children as deleted as well
				_subDelete(parserObject.Parent.Parent.Parent);
			}
		}

		private void _subDelete(ParserObject obj) {
			obj.Removed = true;

			if (obj is ParserKeyValue keyValue) {
				foreach (var child in keyValue.Value) {
					_subDelete(child);
				}
			}
			else if (obj is ParserArrayBase) {
				foreach (var child in obj) {
					_subDelete(child);
				}
			}
		}

		public void WriteSubCombo(StringBuilder builder, ItemCombo itemCombo) {
			builder.AppendLine("      - Combo:");

			foreach (var nameId in itemCombo.NameIds) {
				if (string.IsNullOrEmpty(nameId.Item))
					continue;

				builder.AppendLine("          - " + DbUtilities.ItemId2AegisName(nameId, ItemDb) + "    # " + nameId);
			}
		}

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<ItemCombo>();

			WriteItemCombo(builder, model);
		}

		public void WriteItemCombo(StringBuilder builder, ItemCombo itemCombo) {
			builder.AppendLine("  - Combos:");
			builder.AppendLine("      - Combo:");
			
			foreach (var nameId in itemCombo.NameIds) {
				if (string.IsNullOrEmpty(nameId.Item))
					continue;

				builder.AppendLine("          - " + DbUtilities.ItemId2AegisName(nameId, ItemDb) + "    # " + nameId);
			}

			builder.AppendLine("    Script: |");
			builder.AppendLine(DbWriter.ToYamlScript(itemCombo.Script, "      "));
		}
	}
}
