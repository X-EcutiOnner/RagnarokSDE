using System;
using System.Collections.Generic;
using System.Linq;
using ErrorManager;
using GRF.Core;
using GRF.IO;
using GRF.GrfSystem;
using GRF.Threading;

namespace SDE.Editor.Backups {
	public class BackupThread : PausableThread {
		private long _backupUid;
		private readonly Dictionary<long, EditableBackupMetadata> _pendingBackups = new Dictionary<long, EditableBackupMetadata>();

		public bool IsCrashed { get; set; }

		public void AddNewBackup(EditableBackupMetadata backup) {
			_pendingBackups[backup.Uid] = backup;
			Resume();
		}

		public void Start() {
			GrfThread.Start(_start);
		}

		public void _start() {
			try {
				while (true) {
					Pause();

					var keys = _pendingBackups.Keys.ToList();

					foreach (var key in keys.Where(p => p > _backupUid).OrderBy(p => p)) {
						string grfPath = GrfPath.Combine(Settings.TempPath, "backup_" + _backupUid + ".grf");

						using (GrfHolder grf = new GrfHolder(grfPath, GrfLoadOptions.New)) {
							foreach (var entry in _pendingBackups[key].SystemPaths2GrfPaths) {
								grf.Commands.AddFile(entry.Value, entry.Key);
							}

							grf.Save();
							grf.Reload();

							// Currently saving another file, stop before breaking anything
							if (BackupManager.Instance.IsStarted)
								break;

							// Save to primary backup file
							BackupManager.Instance.Grf.Merge(grf);
							BackupManager.Instance.Grf.Reload();
						}

						GrfPath.Delete(grfPath);

						// Deletes unused files, it's not necessary but it can pile up quickly
						foreach (var file in _pendingBackups[key].SystemPaths2GrfPaths.Keys) {
							GrfPath.Delete(file);
						}

						_backupUid = key;
					}

					// Remove old backups
					if (!BackupManager.Instance.IsStarted) {
						List<string> paths = BackupManager.Instance.GetBackupFiles().OrderBy(long.Parse).ToList();

						// Only delete if it's worth it.
						if (paths.Count > BackupManager.MaximumNumberOfBackups + 15) {
							while (paths.Count > BackupManager.MaximumNumberOfBackups) {
								BackupManager.Instance.Grf.Commands.RemoveFolder(paths[0]);
								paths.RemoveAt(0);
							}

							BackupManager.Instance.Grf.Save();
						}

						// The GRF must always be closed
						BackupManager.Instance.Grf.Close();
					}
				}
			}
			catch (Exception err) {
				IsCrashed = true;
				ErrorHandler.HandleException(err);
				ErrorHandler.HandleException("The backup engine has failed to save your files. It will be disabled until you reload the application.", ErrorLevel.NotSpecified);
			}
		}
	}
}