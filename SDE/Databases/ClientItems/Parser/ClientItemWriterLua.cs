using ErrorManager;
using Lua.Structure;
using SDE.ApplicationConfiguration;
using SDE.Core;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor;
using SDE.Editor.Backups;
using SDE.Editor.Database;
using SDE.Editor.Files;
using SDE.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Services;

namespace SDE.Databases.ClientItems.Parser {
	public class ClientItemWriterLua : DatabaseWriterCsv {
		public override bool SplitDatabaseFiles => true;
		public override string KeyField => "Id";
		private SimpleBooleanFallback _detectEncodingFallback = new SimpleBooleanFallback();

		private Encoding _ansi;

		public ClientItemWriterLua() {
			_ansi = Encoding.GetEncoding(1252, _detectEncodingFallback, DecoderFallback.ReplacementFallback);
		}

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			Writer(db, null);
		}

		public void Writer(BaseDatabase db, string exportPath) {
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.CardIllustration);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.CardAffix);
			ClientTextWriterHelper<ClientItem>.SaveFileToSystem(exportPath, ClientItemTextFileParsers.CardPostfix);
			WriteClientInfoToSystem(db, ProjectConfiguration.ClientItemInfo, exportPath);
		}

		public void WriteClientInfoToSystem(BaseDatabase db, string filename) {
			WriteClientInfoToSystem(db, filename, null);
		}

		public void WriteClientInfoToSystem(BaseDatabase db, string filename, string output) {
			var project = SdeEditor.Project;

			if (output == null && project.MetaGrf.GetData(filename) == null) {
				Debug.Ignore(() => DbDebugHelper.OnWriteStatusUpdate(DataSources.ClientItem, filename, null, "ItemInfo table not saved."));
				return;
			}

			if (output == null)
				BackupManager.Instance.BackupClient(filename);

			var context = new DbSaveContext(db);
			if (!context.IsTableModified()) return;

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("tbl = {");

			List<ReadableTuple> tuples = project.GetDb(DataSources.ClientItem).Table.GetSortedItems().ToList();
			ReadableTuple tuple;

			for (int index = 0, count = tuples.Count; index < count; index++) {
				tuple = tuples[index];
				WriteEntry(builder, tuple);
			}

			if (builder.Length > 0 && builder[builder.Length - 1] == ',') {
				builder.Remove(builder.Length - 1, 1);
			}

			builder.AppendLine("}");
			builder.AppendLine();

			if (SdeAppConfiguration.DbWriterKeepOriginal) {
				var functionData = db.Attached[SdeStrings.LuaReaderFunctions] as List<LBaseType>;

				foreach (var function in functionData) {
					if (function is LKeyValue keyValue) {
						builder.AppendLine(keyValue.ToString());
					}
					else {
						builder.AppendLine(function.ToString());
					}
				}
			}
			else if (SdeAppConfiguration.DbWriterCompressData) {
				builder.AppendLine(ResourceString.Get("ItemInfoFunctionCompress"));
			}
			else {
				builder.AppendLine(ResourceString.Get("ItemInfoFunction"));
			}

			if (output == null) {
				IOHelper.SetData(filename, EncodingService.Ansi.GetBytes(builder.ToString()));
			}
			else {
				string copyPath = Path.Combine(output, Path.GetFileName(filename));

				try {
					File.WriteAllText(copyPath, builder.ToString(), EncodingService.Ansi);
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
			}

			Debug.Ignore(() => DbDebugHelper.OnWriteStatusUpdate(DataSources.ClientItem, project.MetaGrf.FindTkPath(filename), null, "Saving ItemInfo table."));
		}

		public override void WriteEntry(StringBuilder builder, ReadableTuple tuple) {
			if (tuple == null)
				return;

			var model = tuple.GetModel<ClientItem>();

			builder.Append("\t[");
			builder.Append(tuple.Key);
			builder.AppendLine("] = {");

			if (SdeAppConfiguration.DbWriterItemInfoUnDisplayName) {
				builder.Append("\t\tunidentifiedDisplayName = \"");
				AppendAnsiEscaped(builder, model.UnidentifiedDisplayName ?? "");
				builder.AppendLine("\",");
			}

			if (SdeAppConfiguration.DbWriterItemInfoUnResource) {
				builder.Append("\t\tunidentifiedResourceName = \"");
				AppendAnsiEscaped(builder, model.UnidentifiedResourceName ?? "");
				builder.AppendLine("\",");
			}

			if (SdeAppConfiguration.DbWriterItemInfoUnDescription) {
				builder.AppendLine("\t\tunidentifiedDescriptionName = {");
				AppendDescription(builder, model.UnidentifiedDescription ?? "");
				builder.AppendLine("\t\t},");
			}

			if (SdeAppConfiguration.DbWriterItemInfoIdDisplayName) {
				builder.Append("\t\tidentifiedDisplayName = \"");
				AppendAnsiEscaped(builder, model.IdentifiedDisplayName ?? "");
				builder.AppendLine("\",");
			}

			if (SdeAppConfiguration.DbWriterItemInfoIdResource) {
				builder.Append("\t\tidentifiedResourceName = \"");
				AppendAnsiEscaped(builder, model.IdentifiedResourceName ?? "");
				builder.AppendLine("\",");
			}

			if (SdeAppConfiguration.DbWriterItemInfoIdDescription) {
				builder.AppendLine("\t\tidentifiedDescriptionName = {");
				AppendDescription(builder, model.IdentifiedDescription ?? "");
				builder.AppendLine("\t\t},");
			}

			if (SdeAppConfiguration.DbWriterItemInfoSlotCount) {
				builder.Append("\t\tslotCount = ");
				builder.Append(DbReader.ToInt(model.NumberOfSlots));
				builder.AppendLine(",");
			}

			if (SdeAppConfiguration.DbWriterItemInfoClassNum) {
				builder.Append("\t\tClassNum = ");
				builder.Append(DbReader.ToInt(model.ClassNumber));

				if (SdeAppConfiguration.DbWriterItemInfoIsCostume)
					builder.AppendLine(",");
				else
					builder.AppendLine();
			}

			if (SdeAppConfiguration.DbWriterItemInfoIsCostume) {
				builder.Append("\t\tcostume = ");
				builder.AppendLine(DbWriter.ToBool(model.IsCostume));
			}

			builder.AppendLine("\t},");
		}

		public void AppendAnsiEscaped(StringBuilder b, string value) {
			if (!EncodingService.IsValid(value, _ansi, _detectEncodingFallback)) {
				value = EncodingService.ConvertTo(value, EncodingService.DisplayEncoding, _ansi);
			}

			if (value.Contains("\"")) {
				for (int i = 0; i < value.Length; i++) {
					if (value[i] == '\"') {
						b.Append("\\\"");
					}
					else if (value[i] == '\\' && i + 1 < value.Length && value[i + 1] == '\"') {
						i++;
						b.Append(value[i]);
						b.Append(value[i + 1]);
					}
					else {
						b.Append(value[i]);
					}
				}
			}
			else {
				b.Append(value);
			}
			
			//value.Escape(b, EscapeMode.KeepAsciiCode);
		}

		public void AppendDescription(StringBuilder builder, string value) {
			if (value.StartsWith("\r\n"))
				value = value.Remove(0, 2);

			if (value.EndsWith("\r\n"))
				value = value.Substring(0, value.Length - 2);

			string[] lines = value.Replace("\r\n", "\n").Split('\n');
			string line;

			if (lines.Length == 1 && lines[0] == "")
				return;

			for (int i = 0; i < lines.Length - 1; i++) {
				line = lines[i];
				builder.Append("\t\t\t\"");
				AppendAnsiEscaped(builder, line);
				builder.AppendLine("\",");
			}

			if (lines.Length > 0) {
				line = lines[lines.Length - 1];
				builder.Append("\t\t\t\"");
				AppendAnsiEscaped(builder, line);
				builder.AppendLine("\"");
			}
		}
	}
}
