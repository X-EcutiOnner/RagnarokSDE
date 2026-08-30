using System;
using System.IO;
using System.Linq;
using ErrorManager;
using GRF.IO;
using GRF.GrfSystem;
using SDE.ApplicationConfiguration;
using SDE.Editor.Generic.Parsers.Generic;
using SDE.View;
using Utilities;
using Utilities.Extension;
using SDE.Databases;
using SDE.Editor.Files;

namespace SDE.Editor.Database {
	public static class DbPathLocator {
		private static readonly TkDictionary<string, string> _storedFiles = new TkDictionary<string, string>();
		private static readonly TkDictionary<string, DateTime> _lastModified = new TkDictionary<string, DateTime>();

		public static void StoreFile(string path) {
			if (path == null)
				return;

			if (File.Exists(path)) {
				string temp = TemporaryFilesManager.GetTemporaryFilePath("sdb_store_{0:0000}.dat");
				_storedFiles[path] = temp;
				_lastModified[path] = new FileInfo(temp).LastWriteTime;
				File.Copy(path, temp);
			}
			else {
				_lastModified[path] = default;
				_storedFiles[path] = null;
			}
		}

		public static void ClearStoredFiles() {
			_storedFiles.Clear();
			_lastModified.Clear();
		}

		public static string GetStoredFile(string path) {
			if (path == null)
				return null;

			return _storedFiles[path];
		}

		public static DateTime GetLastModifiedTime(string path) {
			if (path == null)
				return default;

			return _lastModified[path];
		}

		public static void SetLastModifiedTime(string path, DateTime time) {
			if (path == null)
				return;

			_lastModified[path] = time;
		}

		public static bool GenericErrorHandler(ref int numError, object item) {
			DbIOErrorHandler.Handle(StackTraceException.GetStrackTraceException(), "Failed to read an item.", item.ToString());
			numError--;
			if (numError < -10) {
				DbIOErrorHandler.FailedToReadTooManyItems();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Goes up in the parent folder until the path is found.
		/// </summary>
		/// <param name="db">The sub path to find.</param>
		/// <returns>The path found.</returns>
		public static string DetectPathAll(string db) {
			try {
				string path = ProjectConfiguration.DatabasePath;

				while (path != null) {
					if (File.Exists(GrfPath.Combine(path, db))) {
						return GrfPath.Combine(path, db);
					}

					path = GrfPath.GetDirectoryName(path);
				}

				return null;
			}
			catch {
				return null;
			}
		}

		private static string _getInParentPath(string fileInput) {
			string path = GrfPath.GetDirectoryName(ProjectConfiguration.DatabasePath);
			string[] files = fileInput.GetExtension() == null ? new string[] { fileInput + ".yml", fileInput + ".txt", fileInput + ".conf" } : new string[] { fileInput };
			return files.Select(file => GrfPath.CombineUrl(path, file)).FirstOrDefault(File.Exists);
		}

		private static string _getInCurrentPath(string fileInput) {
			string path = ProjectConfiguration.DatabasePath;
			string[] files = fileInput.GetExtension() == null ? new string[] { fileInput + ".yml", fileInput + ".txt", fileInput + ".conf" } : new string[] { fileInput };
			return files.Select(file => GrfPath.CombineUrl(path, file)).FirstOrDefault(File.Exists);
		}

		public static string DetectPath(string toString) {
			if (File.Exists(toString))
				return toString;

			string path = _getInCurrentPath(toString);

			if (path != null)
				return path;

			path = _getInParentPath(toString);
			return path;
		}

		public static TkPath DetectPath(DataSource source) {
			if (source == null)
				return null;

			if (source.IsClientSide && source.ClientSidePath != null) {
				string dest = source.ClientSidePath();

				if (!string.IsNullOrEmpty(dest)) {
					var tkpath = SdeEditor.MetaGrf.FindTkPath(dest);

					if (tkpath == null)
						return null;

					return tkpath;
				}
			}

			var dbPath = ProjectConfiguration.DatabaseDbPath;
			var subPath = Path.GetFileName(ProjectConfiguration.DatabasePath);

			foreach (var path in source.Paths) {
				if (File.Exists(path))
					return path;

				var fullPath = GrfPath.Combine(dbPath, path.Replace("{DBPATH}", subPath));

				if (File.Exists(fullPath))
					return fullPath;
			}

			return null;
		}

		/// <summary>
		/// Gets the type of the file based on the path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>The file type</returns>
		public static FileType GetFileType(string path) {
			switch (path.GetExtension()) {
				case ".conf": return FileType.Conf;
				case ".yml": return FileType.Yaml;
				case ".lua": return FileType.Lua;
				case ".lub": return FileType.Lua;
				default: return FileType.Txt;
			}
		}

		/// <summary>
		/// Determines if the current server is renewal or not.
		/// </summary>
		/// <returns></returns>
		public static bool GetIsRenewal() {
			string path = DetectPath(DataSources.Item)?.FilePath;
			string parent = Path.GetDirectoryName(path);

			if (parent != null && parent.EndsWith("pre-re"))
				return false;
			return true;
		}
	}
}