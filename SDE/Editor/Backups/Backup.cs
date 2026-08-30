using System;
using GRF.Core;
using GRF.IO;
using Utilities;

namespace SDE.Editor.Backups {
	public class Backup {
		public Backup() {
		}

		public Backup(string backup) {
			if (backup == null) throw new ArgumentNullException("backup");

			BackupTimestamp = backup;
			Entry = BackupManager.Instance.Grf.FileTable[GrfPath.Combine(BackupTimestamp, BackupManager.InfoName)];
			Metadata = new BackupMetadata(new ReadonlyConfigAsker(Entry.GetDecompressedData()));
		}

		public string BackupTimestamp { get; private set; }
		public FileEntry Entry { get; private set; }
		public BackupMetadata Metadata { get; private set; }
	}
}