using SDE.Databases.Generic.Features;
using System;

namespace SDE.Editor.Backups {
	/// <summary>
	/// Backup view for list views
	/// </summary>
	public class BackupViewModel : BaseModelView<Backup> {
		public BackupViewModel(Backup model) {
			if (model == null) throw new ArgumentNullException("backup");

			Model = model;
			OnPropertyChanged("");
		}

		public string Date => DateTime.FromFileTime(DateInt).ToString("d/M/yyyy HH:mm:ss");
		public string DbPath => Model.Metadata.DestinationPath;
		public string BackupDate => Model.BackupTimestamp;
		public long DateInt => long.Parse(Model.BackupTimestamp);
	}
}