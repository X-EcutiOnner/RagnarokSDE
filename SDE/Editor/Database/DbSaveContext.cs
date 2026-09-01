using System;
using System.IO;
using System.Linq;
using GRF.IO;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Editor.Backups;
using SDE.Editor.Parsers;
using Utilities.Extension;

namespace SDE.Editor.Database {
	/// <summary>
	/// Class used to validate load or write operations.
	/// It loads parameters that will be used by the DbWriter methods
	/// and it also calls the backup engine. This object must always
	/// be called before proceeding to any load or write operations.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	public class DbSaveContext : DbContextBase {
		private readonly BaseDatabase _db;
		protected override BaseDatabase _bdb => _db;
		public BaseDatabase AbsractDb => _db;
		public bool DestinationRenewal { get; set; } = true;

		public DbSaveContext(BaseDatabase db) {
			_db = db;

			if (_db != null) {
				Source = _db.Source;

				if (Source.IsClientSide) {
					string path = DbPathLocator.DetectPath(Source)?.GetMostRelative();
					FileType format = FileType.Error;

					if (path.IsExtension(".lua", ".lub"))
						format = FileType.Lua;
					else if (path.IsExtension(".yml"))
						format = FileType.Yaml;
					else
						format = FileType.Txt;

					FileType = format;
				}
			}

			NumberOfErrors = MaximumNumberOfAllowedExceptions;
			TextFileHelper.LatestFile = null;
			TextFileHelper.LastReader = null;
		}

		public bool TryDetectFileType(DataSource source = null) {
			source = source ?? Source;
			FileType = FileType.Detect;
			string logicalPath = DbPathLocator.DetectPath(source)?.GetMostRelative();

			if (source.IsClientSide) {
				logicalPath = source.ClientSidePath();
			}

			if (!String.IsNullOrEmpty(logicalPath)) {
				if (logicalPath.IsExtension(".lua", ".lub"))
					FileType = FileType.Lua;
				else if (logicalPath.IsExtension(".txt"))
					FileType = FileType.Txt;
				else if (logicalPath.IsExtension(".yml"))
					FileType = FileType.Yaml;
				else if (logicalPath.IsExtension(".conf"))
					FileType = FileType.Conf;
			}

			if (FileType == FileType.Detect) {
				FileType = FileType.Error;
				DbDebugHelper.OnWriteStatusUpdate(source, FilePath, _db, "FileType couldn't be detected.");
				return false;
			}

			return true;
		}

		public SaveContextState PrepareWrite(DataSource source = null, FileType fileType = FileType.Detect, bool isModifiedCheck = false) {
			source = source ?? Source;
			string logicalPath = DbPathLocator.DetectPath(source)?.GetMostRelative();

			FileType = fileType;

			if ((fileType & FileType.Detect) == FileType.Detect) {
				if (!TryDetectFileType(source))
					return SaveContextState.InvalidFileType;
			}

			string ext = "." + FileType.ToString().ToLower();

			if (FileType == FileType.Yaml) {
				ext = ".yml";
			}

			IsRenewal = false;

			if (source.Paths.Count > 0) {
				FilePath = logicalPath;
			}

			TextFileHelper.LatestFile = FilePath;
			OldPath = DbPathLocator.GetStoredFile(logicalPath);

			if (OldPath == null || !File.Exists(OldPath)) {
				DbDebugHelper.OnWriteStatusUpdate(source, FilePath, _db, "Source path not found: '" + OldPath + "', cannot save this table.");
				return SaveContextState.OriginalFileNotFound;
			}

			if (!_db.IsEnabled) {
				DbDebugHelper.OnWriteStatusUpdate(source, FilePath, _db, "Table not enabled.");
				return SaveContextState.TableDisabled;
			}

			GrfPath.CreateDirectoryFromFile(FilePath);

			if (source.IsClientSide)
				BackupManager.Instance.BackupClient(source.ClientSidePath());
			else
				BackupManager.Instance.Backup(logicalPath);

			if (_db.Table.Commands.CommandIndex == -1 && logicalPath.IsExtension(FilePath.GetExtension())) {
				if (isModifiedCheck && _db.Table.Tuples.Values.Any(p => !p.Normal)) return SaveContextState.Valid;

				//// If we use the previous output, we should never overwrite the file
				//// because it will eat the previous modifications.
				//if (_db.UsePreviousOutput) {
				//	DbDebugHelper.OnWriteStatusUpdate(DbSource, FilePath, _db, "Output from master DB is more recent (will not be saved).");
				//	return false;
				//}

				if (SdeAppConfiguration.AlwaysOverwriteFiles) {
					_db.DirectCopy(this);
				}

				DbDebugHelper.OnWriteStatusUpdate(source, FilePath, _db, "Table not modified (will not be saved).");
				return SaveContextState.TableNotModified;
			}

			DbDebugHelper.OnWriteStatusUpdate(source, FilePath, _db, "The table is saving...");
			return SaveContextState.Valid;
		}

		public bool IsTableModified() {
			if (_db.Table.Commands.CommandIndex == -1) {
				if (_db.Table.Commands.GetRedoCommands() != null) {
					return true;
				}

				return false;
			}

			return true;
		}

		public void DoBackup(DataSource source) {
			source = source ?? Source;
			string logicalPath = DbPathLocator.DetectPath(source)?.GetMostRelative();

			if (source.IsClientSide)
				BackupManager.Instance.BackupClient(source.ClientSidePath());
			else
				BackupManager.Instance.Backup(logicalPath);
		}
	}
}
