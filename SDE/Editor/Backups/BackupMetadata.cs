using GRF.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using Utilities;

namespace SDE.Editor.Backups {
	/// <summary>
	/// Used to load and save data related to a backup.
	/// </summary>
	public class BackupMetadata {
		protected readonly ConfigAsker _info;

		public BackupMetadata(ConfigAsker info) {
			if (info == null) throw new ArgumentNullException("info");

			_info = info;
		}

		/// <summary>
		/// Gets or sets the destination path.
		/// </summary>
		/// <value>
		/// The destination path.
		/// </value>
		public string DestinationPath {
			get { return _info["[Backup - Destination path]", null]; }
			set { _info["[Backup - Destination path]"] = value; }
		}

		/// <summary>
		/// Gets the config info as bytes.
		/// </summary>
		/// <returns></returns>
		public byte[] GetData() {
			return ((TextConfigAsker)_info).GetByteData();
		}
	}

	public class EditableBackupMetadata : BackupMetadata {
		public string TimeStampId { get; private set; }
		public long Uid { get; private set; }
		public Dictionary<string, string> SystemPaths2GrfPaths { get; } = new Dictionary<string, string>();

		public EditableBackupMetadata(string destinationPath) : base(new TextConfigAsker(new byte[] { })) {
			DestinationPath = destinationPath;
			Uid = DateTime.Now.ToFileTimeUtc();
			TimeStampId = Uid.ToString(CultureInfo.InvariantCulture);
		}

		public void AddFileLink(string file, string grfPath) {
			grfPath = GrfPath.CombineUrl(TimeStampId, grfPath);

			SystemPaths2GrfPaths[file] = grfPath;
		}
	}
}