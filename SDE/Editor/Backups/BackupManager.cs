using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GRF.Core;
using GRF.Core.GroupedGrf;
using GRF.IO;
using GRF.GrfSystem;
using SDE.ApplicationConfiguration;
using Utilities;
using Utilities.Extension;
using SDE.View;
using SDE.Editor.Files;

namespace SDE.Editor.Backups {
	public sealed class BackupManager {
		public const int MaximumNumberOfBackups = 50;
		public const string InfoName = "restore.inf";
		private static readonly BackupManager _instance = new BackupManager();

		private GrfHolder _grf;
		public bool IsStarted { get; set; }
		private readonly BackupThread _backupThread = new BackupThread();

		private EditableBackupMetadata _backupInfo;

		/// <summary>
		/// Initializes the <see cref="BackupManager"/> class.
		/// </summary>
		static BackupManager() {
			Instance.Init();
		}

		private BackupManager() {
		}

		private string _grfPath {
			get { return GrfPath.Combine(SdeAppConfiguration.ProgramDataPath, "_backups.grf"); }
		}

		public GrfHolder Grf {
			get {
				EnsuresGrfOpened();
				return _grf;
			}
		}

		public static BackupManager Instance => _instance;

		/// <summary>
		/// Initializes the backup feature and its thread to accept backup requests.
		/// </summary>
		public void Init() {
			_grf = new GrfHolder(_grfPath, GrfLoadOptions.OpenOrNew);
			_grf.Close();
			_backupThread.Start();
		}

		/// <summary>
		/// Starts the a backup and allows for files to be added. The backup will not be stored until Stop() is called.
		/// </summary>
		/// <param name="dbPath">The database path.</param>
		public void Start(string dbPath) {
			if (!SdeAppConfiguration.BackupsManagerState || _backupThread.IsCrashed) return;
			if (dbPath == null) throw new ArgumentNullException("dbPath");

			// Creates a new backup info, this is where all logic will be stored.
			_backupInfo = new EditableBackupMetadata(GrfPath.GetDirectoryName(dbPath));

			EnsuresGrfOpened();

			// Store the metadata file (ConfigAsker/ini) locally, as a temporary file.
			string systemFile = TemporaryFilesManager.GetTemporaryFilePath("backup_local_copy_{0:0000}");
			File.WriteAllBytes(systemFile, _backupInfo.GetData());

			// Link metadatafile to the backup itself.
			_backupInfo.AddFileLink(systemFile, InfoName);

			IsStarted = true;
		}

		/// <summary>
		/// Ensures the backup GRF file is opened and ready.
		/// </summary>
		public void EnsuresGrfOpened() {
			if (!_grf.IsOpened)
				_grf.Open(_grfPath, GrfLoadOptions.OpenOrNew);
		}

		/// <summary>
		/// Removes a backup.
		/// </summary>
		/// <param name="backup">The backup path.</param>
		/// <param name="delayed">Save after removing the backup or not.</param>
		/// <exception cref="ArgumentNullException">backup</exception>
		public void RemoveBackup(string backup, bool delayed) {
			if (backup == null) throw new ArgumentNullException("backup");

			EnsuresGrfOpened();
			_grf.Commands.RemoveFolder(backup);

			if (!delayed) {
				_grf.Save();
				_grf.Close();
			}
		}

		/// <summary>
		/// Removes backups.
		/// </summary>
		/// <param name="backups">The backup paths.</param>
		/// <exception cref="ArgumentNullException">backups</exception>
		public void RemoveBackup(string[] backups) {
			if (backups == null) throw new ArgumentNullException("backups");

			EnsuresGrfOpened();

			_grf.Commands.RemoveFolders(backups);
			_grf.Save();
			_grf.Close();
		}

		/// <summary>
		/// Restores the specified backup.
		/// </summary>
		/// <param name="backup">The backup path.</param>
		/// <exception cref="ArgumentNullException">backup</exception>
		public void Restore(string backup) {
			if (backup == null) throw new ArgumentNullException("backup");

			EnsuresGrfOpened();

			BackupMetadata info = new BackupMetadata(new ReadonlyConfigAsker(_grf.FileTable[GrfPath.Combine(backup, InfoName)].GetDecompressedData()));

			if (!Directory.Exists(info.DestinationPath)) {
				Directory.CreateDirectory(info.DestinationPath);
			}

			foreach (FileEntry entry in _grf.FileTable.EntriesInDirectory(backup, SearchOption.AllDirectories)) {
				if (entry.RelativePath.EndsWith(InfoName))
					continue;

				entry.ExtractFromAbsolute(GrfPath.Combine(info.DestinationPath, entry.RelativePath.ReplaceFirst(backup + "\\", "")));
			}

			_grf.Close();
		}

		/// <summary>
		/// Ends the current backup process (from Start) and stores all the pending files into a single backup entry inside the GRF.
		/// </summary>
		public void Stop() {
			if (!SdeAppConfiguration.BackupsManagerState || _backupThread.IsCrashed) return;

			_backupThread.AddNewBackup(_backupInfo);
			IsStarted = false;
		}

		public void BackupClient(string file, byte[] data) {
			if (!SdeAppConfiguration.BackupsManagerState || !IsStarted || _backupThread.IsCrashed) return;
			if (file == null) throw new ArgumentNullException("file");
			if (data == null) return;

			try {
				string relativePath = GrfPath.Combine("client", Path.GetFileName(file));

				if (string.IsNullOrEmpty(relativePath)) {
					return;
				}

				EnsuresGrfOpened();

				string systemFile = TemporaryFilesManager.GetTemporaryFilePath("backup_local_copy_{0:0000}");
				File.WriteAllBytes(systemFile, data);

				_backupInfo.AddFileLink(systemFile, relativePath);
			}
			catch {
			}
		}

		public void BackupClient(string file) {
			if (!SdeAppConfiguration.BackupsManagerState || !IsStarted || _backupThread.IsCrashed) return;
			if (file == null) throw new ArgumentNullException("file");

			BackupClient(file, SdeEditor.MetaGrf.GetData(file));
		}

		public void Backup(string file) {
			if (!SdeAppConfiguration.BackupsManagerState || !IsStarted || _backupThread.IsCrashed) return;
			if (file == null) throw new ArgumentNullException("file");

			try {
				string relativePath = file.ReplaceFirst(GrfPath.GetDirectoryName(ProjectConfiguration.DatabasePath) + "\\", "");

				if (string.IsNullOrEmpty(relativePath)) {
					return;
				}

				EnsuresGrfOpened();

				string systemFile = TemporaryFilesManager.GetTemporaryFilePath("backup_local_copy_{0:0000}");
				File.Copy(file, systemFile);

				_backupInfo.AddFileLink(systemFile, relativePath);
			}
			catch {
			}
		}

		public List<string> GetBackupFiles() {
			EnsuresGrfOpened();

			return _grf.FileTable.Directories.Select(p => GrfPath.SplitDirectories(p)[0]).Distinct().ToList();
		}

		public List<Backup> GetBackups() {
			return GetBackupFiles().Select(p => new Backup(p)).ToList();
		}

		/// <summary>
		/// Exports the specified folder from the GRF.
		/// </summary>
		/// <param name="folder">The extraction folder path.</param>
		/// <param name="backup">The backup path.</param>
		/// <exception cref="ArgumentNullException">
		/// folder
		/// or
		/// backup
		/// </exception>
		public void Export(string folder, string backup) {
			if (folder == null) throw new ArgumentNullException("folder");
			if (backup == null) throw new ArgumentNullException("backup");

			EnsuresGrfOpened();

			foreach (FileEntry entry in _grf.FileTable.EntriesInDirectory(backup, SearchOption.AllDirectories)) {
				if (entry.RelativePath.EndsWith(InfoName))
					continue;

				entry.ExtractFromAbsolute(GrfPath.Combine(folder, entry.RelativePath.ReplaceFirst(backup + "\\", "")));
			}

			_grf.Close();
		}
	}
}