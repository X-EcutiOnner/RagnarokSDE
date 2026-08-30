using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Database;
using Database.Commands;
using GRF.Core.GrfWriters;
using GRF.GrfSystem;
using GRF.IO;
using SDE.Databases;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Generic.SearchDescriptors;
using SDE.Editor.Files;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using TokeiLibrary;
using Utilities.Extension;

namespace SDE.Editor.Database {
	/// <summary>
	/// This class holds a table and information regarding its various
	/// properties. It tells the database how to load the table, how 
	/// to write it and how to display its attributes.
	/// </summary>
	public abstract class BaseDatabase {
		private bool _isEnabled = true;

		protected BaseDatabase() {
		}

		public SearchDescriptor SearchDescriptor { get; protected set; }

		public DataSource Source { get; set; }
		public AttributeList AttributeList { get; set; }
		public bool IsModified => Table.Commands.IsModified;
		public BaseTable BaseTable => Table;
		public bool IsGenerateTab { get; protected set; } = true;
		public bool IsLoaded { get; protected set; }
		public bool UseUniqueId { get; set; }
		public bool DoNotLoadInEditor;
		public bool IsCustom { get; set; }
		public bool ThrowFileNotFoundException { get; protected set; } = true;
		public bool IsTabContentValid { get; protected set; }
		public ObservableDictionary<string, object> Attached { get; private set; } = new ObservableDictionary<string, object>();
		public TieredProgress Progress;
		public DatabaseParserProvider Parser { get; set; }
		public TabGenerator TabGenerator { get; protected set; } = new TabGenerator();
		public Table<int, ReadableTuple> Table { get; protected set; }
		public FileType ActiveFormat = FileType.Error;

		public event EventHandler TableLoaded;
		public event EventHandler TableModified;

		public bool IsEnabled {
			get { return _isEnabled; }
			set {
				bool hasChanged = _isEnabled != value;
				_isEnabled = value;
				if (hasChanged) {
					OnIsEnabledChanged(_isEnabled);
				}
			}
		}

		public event Action<object, bool> IsEnabledChanged;

		public virtual void OnIsEnabledChanged(bool state) => IsEnabledChanged?.Invoke(this, state);

		public void DummyInit() {
			if (Table == null)
				Table = new Table<int, ReadableTuple>(AttributeList, UseUniqueId);
		}

		public void SaveCommandIndex() {
			Table.Commands.SaveCommandIndex();
		}

		public void Init() {
			if (Table == null)
				Table = new Table<int, ReadableTuple>(AttributeList, UseUniqueId);

			SdeEditor.Project.AddDb(Source, this);
		}

		public void LoadDb() {
			IsLoaded = false;
			Table.EnableEvents = false;
			Table.EnableRawEvents = false;
			Table.ResetUniqueId();

			_loadDb();

			IsLoaded = true;
			Table.EnableEvents = true;

			TableLoaded?.Invoke(this, null);
			Table.Commands.CommandIndexChanged -= TableModified_CommandIndexChanged;
			Table.Commands.CommandIndexChanged += TableModified_CommandIndexChanged;
		}

		private void TableModified_CommandIndexChanged(object sender, ITableCommand<int, ReadableTuple> command) {
			TableModified?.Invoke(this, null);
		}

		public void LoadFromClipboard(string content) {
			bool fileExists;

			try {
				fileExists = File.Exists(content);
			}
			catch {
				fileExists = false;
			}

			string path;

			if (fileExists) {
				path = content;
			}
			else {
				path = TemporaryFilesManager.GetTemporaryFilePath("clipboard_{0:000}");
				File.WriteAllText(path, content);
			}

			DbLoadContext context = new DbLoadContext(this);
			context.FilePath = path;
			context.IsClipboard = true;

			string text = File.ReadAllText(path);
			OnLoadFromClipboard(context, text, path, this);
			Attached["FromUserRawInput"] = true;
			Table.EnableRawEvents = true;
			OnLoadDataFromClipboard(context, text, path, this);
		}

		public virtual void OnLoadDataFromClipboard(DbLoadContext context, string text, string path, BaseDatabase db) {
			Parser.Read(context, db);
		}

		public virtual void OnLoadFromClipboard(DbLoadContext context, string text, string path, BaseDatabase db) {
			if (text.StartsWith("{") || text.Contains("(\r\n\t") || text.Contains("(\n\t") || path.IsExtension(".conf"))
				context.FileType = FileType.Conf;
			else if (text.StartsWith("  - "))
				context.FileType = FileType.Yaml;
			else if (text.Contains("] = {"))
				context.FileType = FileType.Lua;
			else
				context.FileType = FileType.Txt;
		}

		public void ClearCommands() {
			Table.Commands.ClearCommands();
		}

		public void Clear() {
			if (DoNotLoadInEditor)
				return;

			Table.Clear();
			Attached.Clear();
			IsTabContentValid = false;
			IsLoaded = false;
			IsEnabled = true;
		}

		public DbTab GenerateTab(ProjectManager sdb, TabControl control, BaseDatabase baseDb) {
			return TabGenerator.GenerateTab(sdb, control, baseDb);
		}

		public virtual void WriteDb(string dbPath, string subPath, FileType fileType = FileType.Detect) {
			if (Parser == null || IsGenerateTab == false)
				return;

			DbSaveContext context = new DbSaveContext(this);
			DatabaseWriter writer;

			if (!context.TryDetectFileType(Source))
				return;

			writer = Parser.GetWriter(context.FileType);

			if (!writer.SplitDatabaseFiles) {
				if (context.PrepareWrite(fileType: fileType) != SaveContextState.Valid)
					return;
			}

			writer.Writer(context, this);
		}

		protected virtual void _loadDb() {
			DbLoadContext context = new DbLoadContext(this);

			if (Parser == null)
				return;

			DatabaseReader reader;

			if (!context.TryDetectFileType(Source))
				return;

			reader = Parser.GetReader(context.FileType);

			if (!reader.SplitDatabaseFiles) {
				if (!context.PrepareRead(Source))
					return;
			}

			reader.Loader(context, this);
		}

		public BaseDatabase Copy() {
			DummyDb dummy = new DummyDb();

			dummy.Source = Source;
			dummy.AttributeList = AttributeList;
			dummy.Parser = Parser;

			return dummy;
		}

		public T GetAttacked<T>(string property) {
			if (Attached[property] == null)
				return default;

			return (T)Attached[property];
		}

		public void DirectCopy(DbSaveContext context) {
			try {
				if (context.OldPath != context.FilePath) {
					var storeCompareList = Attached["StoreCompare"] as List<string>;

					if (storeCompareList != null) {
						foreach (var path in storeCompareList) {
							var oldPath = DbPathLocator.GetStoredFile(path);

							if (!IOHelper.SameFile(oldPath, path)) {
								// Test their modified date
								GrfPath.Delete(path);
								File.Copy(oldPath, path);
							}
						}
					}
					else if (!IOHelper.SameFile(context.OldPath, context.FilePath)) {
						// Test their modified date
						GrfPath.Delete(context.FilePath);
						File.Copy(context.OldPath, context.FilePath);
					}
				}
			}
			catch (Exception err) {
				context.ReportException(err);
			}
		}
	}
}