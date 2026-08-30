using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Database;
using ErrorManager;
using GRF.GrfSystem;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using Utilities.Controls;
using Utilities.Services;

namespace SDE.Editor.IronPython {
	public class ScriptInterpreter {
		private readonly ScriptEngine _mEngine = Python.CreateEngine();
		private DbTab _tab;
		private ScriptScope _mScope;
		private ObservableList<ReadableTuple> _selected;
		private BaseTable _selectedDb;
		private bool _selectionChanged;

		public string Execute(DbTab tab, string code) {
			MemoryStream stream = new MemoryStream();
			string output = "";
			_tab = tab;
			ScriptHost host = new ScriptHost();

			try {
				TableHelper.EnableTupleTrace = true;

				if (_tab == null)
					throw new Exception("No database tab selected.");

				_selected = new ObservableList<ReadableTuple>();

				foreach (var tuple in _tab.ListView.SelectedItems.OfType<ReadableTuple>().OrderBy(p => p)) {
					_selected.Add(tuple);
				}

				_selectionChanged = false;

				_selected.CollectionChanged += delegate { _selectionChanged = true; };

				_mEngine.Runtime.IO.SetOutput(stream, EncodingService.DisplayEncoding);
				_mEngine.Runtime.IO.SetErrorOutput(stream, EncodingService.DisplayEncoding);

				_mScope = _mEngine.CreateScope();

				var database = SdeEditor.Project;
				List<BaseDatabase> dbs = new List<BaseDatabase>();

				foreach (var source in DataSources.AllSources) {
					var db = database.TryGetDb(source);

					if (db != null) {
						dbs.Add(db);
						TableHelper.Tables.Add(db.Table);
						_mScope.SetVariable(source.UidName.ToLower().Replace(" ", "_"), db.Table);
					}
				}

				_mScope.SetVariable("DataSources", DynamicHelpers.GetPythonTypeFromType(typeof(DataSources)));
				_mScope.SetVariable("item_db_m", database.GetMergedTable(DataSources.Item));
				_mScope.SetVariable("mob_db_m", database.GetMergedTable(DataSources.Mob));
				_mScope.SetVariable("mob_skill_db_m", database.GetMergedTable(DataSources.MobSkill));
				_mScope.SetVariable("quest_db_m", database.GetMergedTable(DataSources.Quest));
				_mScope.SetVariable("selection", _selected);
				_mScope.SetVariable("database", database);
				_mScope.SetVariable("script", host);
				_mScope.SetVariable("exit", new Action(host.exit));
				_mScope.SetVariable("input", new Func<string, string, string>(host.input));
				_mScope.SetVariable("input2", new Func<string, string, string, string>(host.input));
				_mScope.SetVariable("show", new Action<string>(host.show));
				_mScope.SetVariable("show2", new Action<string, object[]>(host.show));
				_mScope.SetVariable("confirm", new Func<string, bool>(host.confirm));
				_mScope.SetVariable("throw", new Action<string>(host.@throw));
				_mScope.SetVariable("format", new Func<string, object[], string>(host.format));
				_mScope.SetVariable("trim", new Func<string, string>(host.trim));
				_mScope.SetVariable("int", new Func<object, int>(host.@int));
				_mScope.SetVariable("hex", new Func<object, string>(host.hex));

				_selectedDb = _tab.Database.Table;
				_mScope.SetVariable("selected_db", _selectedDb);

				string temp = TemporaryFilesManager.GetTemporaryFilePath("python_script_{0:0000}.py");

				byte[] file = File.ReadAllBytes(code);
				Encoding encoding = EncodingService.DetectEncoding(file);

				using (StreamWriter writer = new StreamWriter(File.Create(temp), encoding))
				using (StreamReader reader = new StreamReader(code)) {
					writer.WriteLine("#!/usr/bin/env python");
					writer.WriteLine("# -*- coding: {0} -*- ", encoding.CodePage);

					while (!reader.EndOfStream) {
						string line = reader.ReadLine();

						if (line == null) continue;

						if (line.Contains("Flags.")) {
							line = Regex.Replace(line, @"Flags\.(\w+)", "Flags[\"$1\"]");
						}

						writer.WriteLine(EncodingService.FromAnyTo(line, encoding));
					}
				}

				ScriptSource scriptSource = _mEngine.CreateScriptSourceFromFile(temp);

				foreach (var db in dbs) {
					DbBegin(db);
				}

				try {
					try {
						scriptSource.Execute(_mScope);
					}
					catch (OperationCanceledException) {
					}

					if (stream.Position > 0) {
						stream.Seek(0, SeekOrigin.Begin);
						byte[] data = new byte[stream.Length];
						stream.Read(data, 0, data.Length);

						output = EncodingService.DisplayEncoding.GetString(data);
						Clipboard.SetDataObject(EncodingService.DisplayEncoding.GetString(data));
					}
				}
				catch {
					foreach (var db in dbs) {
						db.Table.Commands.CancelEdit();
					}

					throw;
				}
				finally {
					TableHelper.EnableTupleTrace = false;
					TableHelper.Tables.Clear();

					foreach (var db in dbs) {
						DbEnd(db);
					}

					stream.Close();
					_tab.Filter();
					_tab.Update();
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
			finally {
				TableHelper.EnableTupleTrace = false;
				TableHelper.Tables.Clear();
			}

			return output;
		}

		private void DbBegin(BaseDatabase p) {
			p.Table.Commands.BeginNoDelay(_ => {
				p.Table.OnTableUpdated();

				if (_selectionChanged && p.Table == _selectedDb) {
					_tab.SelectItems(_selected.ToList(), focus: true);
				}
			});
		}

		private void DbEnd(BaseDatabase p) {
			int cmdCount = p.Table.Commands.CommandIndex;
			p.Table.Commands.End();

			if (cmdCount == p.Table.Commands.CommandIndex) {
				if (_selectionChanged && p.Table == _selectedDb) {
					_tab.SelectItems(_selected.ToList(), focus: true);
				}
			}
		}
	}
}