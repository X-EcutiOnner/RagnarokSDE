using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Database;
using ErrorManager;
using GRF.Core.GrfWriters;
using GRF.Core.GroupedGrf;
using GRF.Threading;
using GrfToWpfBridge.Application;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.ClientItems.Parser;
using SDE.Editor.Backups;
using SDE.Editor.Database.Commands;
using SDE.View;
using TokeiLibrary;
using Utilities;
using Utilities.CommandLine;
using Utilities.Extension;

namespace SDE.Editor.Database {
	/// <summary>
	/// This class is responsible to load and save all the databases.
	/// </summary>
	public class ProjectManager {
		#region Delegates
		public delegate void ClientDatabaseEventHandler(object sender);
		#endregion

		private static ConfigAsker _configAsker;

		private readonly Dictionary<DataSource, BaseDatabase> _databases = new Dictionary<DataSource, BaseDatabase>();
		protected MultiGrfReader _metaGrf;

		public ProjectManager(MultiGrfReader metaGrf) {
			_metaGrf = metaGrf;
			Commands = new CommandsHolder();
		}

		public MultiGrfReader MetaGrf => _metaGrf;
		public CommandsHolder Commands { get; private set; }

		/// <summary>
		/// Gets the config asker of the currently loaded database.
		/// These values will get destroyed upon reloading.
		/// </summary>
		public static ConfigAsker ConfigAsker {
			get { return _configAsker ?? (_configAsker = new ConfigAsker(SdeAppConfiguration.ConfigAsker.ConfigFile.Replace("config", "db_config"))); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the database is modified.
		/// </summary>
		public bool IsModified {
			get { return Commands.IsModified; }
			set {
				if (value == false) {
					foreach (var db in _databases) {
						db.Value.ClearCommands();
					}
				}
			}
		}

		/// <summary>
		/// Gets the dictionary of tables.
		/// </summary>
		public Dictionary<DataSource, BaseDatabase> AllTables => _databases;

		public event ClientDatabaseEventHandler Reloaded;
		public event ClientDatabaseEventHandler PreviewReloaded;
		public event ClientDatabaseEventHandler Modified;

		public virtual void OnModified() => Modified?.Invoke(this);
		public virtual void OnPreviewReloaded() => PreviewReloaded?.Invoke(this);
		public virtual void OnReloaded() => Reloaded?.Invoke(this);

		/// <summary>
		/// Gets the abstract database.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="source">The name of the database to get.</param>
		/// <returns></returns>
		public BaseDatabase GetDb(DataSource source) {
			return _databases[source];
		}

		/// <summary>
		/// Gets the abstract database.
		/// </summary>
		/// <param name="source">The name of the database to get.</param>
		/// <returns></returns>
		public BaseDatabase TryGetDb(DataSource source) {
			if (_databases.ContainsKey(source))
				return _databases[source];
			return null;
		}

		/// <summary>
		/// Gets the table.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="source">The name of the database to get.</param>
		/// <returns></returns>
		public Table<int, ReadableTuple> GetTable(DataSource source) {
			return _databases[source].Table;
		}

		private Dictionary<DataSource, object> _mergedTables = new Dictionary<DataSource, object>();

		/// <summary>
		/// Gets the merged table for the database source.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="source">The name.</param>
		/// <returns></returns>
		public MergedTable GetMergedTable(DataSource source) {
			if (_mergedTables.TryGetValue(source, out var table))
				return (MergedTable)table;

			var table1 = _databases[source].Table;
			var mergedTable = new MergedTable(table1.AttributeList);
			mergedTable.AddTable(table1);

			if (source.ImportTable != null) {
				mergedTable.AddTable(_databases[source.ImportTable].Table);
			}

			_mergedTables[source] = mergedTable;
			return mergedTable;
		}

		/// <summary>
		/// Reloads the database.
		/// </summary>
		public void Reload() {
			Reload(SdeEditor.Instance);
		}

		/// <summary>
		/// Reloads the database.
		/// </summary>
		/// <param name="progress">The progress object.</param>
		public void Reload(IProgress progress) {
			DbDebugHelper.OnUpdate("Reloading database...");
			OnPreviewReloaded();
			_mergedTables.Clear();
			_mergedTables.Clear();

			ProjectConfiguration.IsRenewal = DbPathLocator.GetIsRenewal();
			TieredProgress tierProgress = new TieredProgress(progress);

			try {
				Commands.ClearCommands();
				DbPathLocator.ClearStoredFiles();
				ResetAllSettings();

				var dbs = _databases.Values.ToList();

				for (int i = 0; i < dbs.Count; i++) {
					dbs[i].Clear();
					DbDebugHelper.OnCleared(dbs[i].Source, null, dbs[i]);
				}

				dbs = dbs.Where(p => !p.DoNotLoadInEditor).ToList();
				float totalCount = dbs.Count;

				DbDebugHelper.OnUpdate("All database tables have been cleared.");

				tierProgress.AddTiers(dbs.Count);
				float previousProgress = -1;

				for (int i = 0; i < dbs.Count; i++) {
					var db = dbs[i];

					CLHelper.CStart(i);

					try {
						db.Progress = tierProgress;
						db.LoadDb();
					}
					finally {
						db.Progress.CompleteTier();
						db.Progress = null;
					}

					previousProgress = progress.Progress;

					CLHelper.CStopAndDisplay(db.Source.DisplayName, i);
					AProgress.IsCancelling(progress);
				}

				ClearCommands();
			}
			finally {
				DbDebugHelper.OnUpdate("Database reloaded...");
			}

			OnReloaded();
			SdeEditor.Instance.Dispatch(p => p.OnSelectionChanged());
		}

		/// <summary>
		/// Saves the database.
		/// </summary>
		public void Save() {
			Save(SdeEditor.Instance._asyncOperation, SdeEditor.Instance);
		}

		/// <summary>
		/// Saves the database.
		/// </summary>
		/// <param name="ap">The progress object.</param>
		/// <param name="progress"> </param>
		public virtual void Save(AsyncOperation ap, IProgress progress) {
			string dbPath = ProjectConfiguration.DatabaseDbPath;
			string subPath = ProjectConfiguration.DatabaseSubDbPath;
			DbDebugHelper.OnUpdate("Saving tables.");

			MetaGrf.Clear();

			try {
				BackupManager.Instance.Start(ProjectConfiguration.DatabasePath);

				var dbs = _databases.Values.ToList();

				for (int i = 0; i < dbs.Count; i++) {
					var db = dbs[i];
					db.WriteDb(dbPath, subPath);

					if (progress != null)
						progress.Progress = AProgress.LimitProgress((i + 1f) / dbs.Count * 100f);
				}

				foreach (var db in dbs) {
					db.SaveCommandIndex();
				}

				Commands.SaveCommandIndex();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
			finally {
				if (ap != null && progress != null)
					progress.Progress = ap.ProgressBar.GetIntermediateState("Backup manager");

				BackupManager.Instance.Stop();
				DbDebugHelper.OnUpdate("Finished saving tables.");
			}
		}

		/// <summary>
		/// Exports the database.
		/// </summary>
		/// <param name="dbPath">The db path.</param>
		/// <param name="subPath">The sub path.</param>
		/// <param name="serverType">Type of the server.</param>
		/// <param name="fileType">The file type.</param>
		public void ExportDatabase(string dbPath, string subPath, ServerType serverType, FileType fileType) {
			foreach (var db in _databases) {
				db.Value.WriteDb(dbPath, subPath, fileType);
			}
		}

		/// <summary>
		/// Clears the commands of all the tables.
		/// </summary>
		public void ClearCommands() {
			foreach (var db in _databases) {
				db.Value.ClearCommands();
				db.Value.SaveCommandIndex();
			}
		}

		/// <summary>
		/// Adds a database object to this database.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="source">The name of the database.</param>
		/// <param name="db">The db.</param>
		public void AddDb(DataSource source, BaseDatabase db) {
			_databases[source] = db;

			if (db.DoNotLoadInEditor)
				return;

			int commandIndex = db.Table.Commands.CommandIndex;

			// The commands executed on a database are only stored, they are not executed.
			// The database holder (this class) will execute them.
			db.Table.Commands.CommandExecuted += (r, s) => Commands.Store(new GenericDbCommand(db));
			db.Table.Commands.CommandIndexChanged += (r, s) => OnModified();
			db.Table.Commands.ModifiedStateChanged += (r, s) => {
				if (db.Table.Commands.StackStatus == Utilities.Commands.StackStatus.Clear) {
					Commands.RemoveCommands(db.Table.Commands.CommandsClearedCount);
				}
			};
		}

		public BaseTable LoadTable(string file, DataSource source) {
			DatabaseExceptions.ThrowIfTraceNotEnabled();

			// Special cases
			if (source == DataSources.ClientItem) {
				BaseDatabase db = GetDb(source).Copy();
				db.DummyInit();

				if (file.IsExtension(".lub", ".lua")) {
					var parser = new ClientItemReaderLua();
					parser.LoadFile(db, ProjectConfiguration.ClientItemInfo);
				}
				else {
					ClientItemTextFileParser parser;
					string fileName = Path.GetFileNameWithoutExtension(file);
					if (fileName.StartsWith("cardprefixnametable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.CardAffix;
					else if (fileName.StartsWith("cardpostfixnametable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.CardPostfix;
					else if (fileName.StartsWith("num2cardillustnametable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.CardIllustration;
					else if (fileName.StartsWith("idnum2itemdisplaynametable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.IdentifiedDisplayName;
					else if (fileName.StartsWith("num2itemdisplaynametable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.UnidentifiedDisplayName;
					else if (fileName.StartsWith("idnum2itemdesctable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.IdentifiedDescription;
					else if (fileName.StartsWith("num2itemdesctable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.UnidentifiedDescription;
					else if (fileName.StartsWith("idnum2itemresnametable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.IdentifiedResourceName;
					else if (fileName.StartsWith("num2itemresnametable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.UnidentifiedResourceName;
					else if (fileName.StartsWith("itemslotcounttable", StringComparison.OrdinalIgnoreCase)) parser = ClientItemTextFileParsers.NumberOfSlots;
					else
						throw new Exception("File name does not match with a known client resource file.");

					ClientItemReaderHelper.LoadDataFromSystem(db, parser, file);
				}

				return db.Table;
			}
			else {
				var db = TryGetDb(source);

				if (db == null)
					return null;

				var newDb = db.Copy();
				newDb.DummyInit();
				newDb.LoadDb();
				return newDb.Table;
			}
		}

		#region Temporary settings
		private static bool? _isRenewal;

		public bool IsRenewal {
			get {
				if (_isRenewal == null) {
					_isRenewal = DbPathLocator.GetIsRenewal();
				}

				return _isRenewal.Value;
			}
		}

		public static void ResetAllSettings() {
			ConfigAsker.DeleteKeys("");
			_isRenewal = null;
		}
		#endregion
	}
}