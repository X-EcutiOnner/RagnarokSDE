using SDE.Databases;
using SDE.Editor.Generic.Parsers.Generic;
using SDE.Editor.Parsers;
using Utilities;
using Utilities.Extension;

namespace SDE.Editor.Database {
	public class DbLoadContext : DbContextBase {
		private readonly BaseDatabase _db;
		protected override BaseDatabase _bdb => _db;
		public BaseDatabase AbsractDb => _db;

		public DbLoadContext(BaseDatabase db) {
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

		public bool PrepareRead(DataSource source) {
			Source = source;

			if (!TryDetectFileType(Source)) {
				return false;
			}

			TkPath path = DbPathLocator.DetectPath(source);

			if (path.IsFile) {
				FilePath = path.FilePath;
				DbPathLocator.StoreFile(FilePath);
				DbDebugHelper.OnLoaded(Source, FilePath, _db);
			}
			else {
				FilePath = path;
			}

			return true;
		}

		public bool TryDetectFileType(DataSource source = null) {
			source = source ?? Source;
			TkPath path = DbPathLocator.DetectPath(source);

			if (string.IsNullOrEmpty(path)) {
				if (_db.ThrowFileNotFoundException) {
					DbIOErrorHandler.FileNotFound(Source);
				}

				return false;
			}

			FileType = DbPathLocator.GetFileType(path.FileName);
			return true;
		}
	}
}
