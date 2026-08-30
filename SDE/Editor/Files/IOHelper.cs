using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ErrorManager;
using GRF.IO;
using SDE.ApplicationConfiguration;
using SDE.Core;
using SDE.Editor.Engines;
using SDE.View;

namespace SDE.Editor.Files {
	public static class IOHelper {
		public static void WriteAllText(string path, string content) {
			Encoding encoding = SdeAppConfiguration.EncodingServer;

			if (encoding.CodePage == Encoding.UTF8.CodePage)
				encoding = Extensions.Utf8NoBom;

			File.WriteAllText(path, content, encoding);
		}

		public static bool SameFile(string file1, string file2) {
			try {
				return new FileInfo(file1).LastWriteTimeUtc.Ticks == new FileInfo(file2).LastWriteTimeUtc.Ticks;
			}
			catch {
				return false;
			}
		}

		public static void SetData(string file, byte[] data) {
			var savePath = SdeEditor.MetaGrf.FindTkPath(file);
			
			if (savePath == null || savePath.IsContainer) {
				try {
					string outputPath = GrfPath.Combine(ProjectConfiguration.DatabaseDbPath, "client", file);
					GrfPath.CreateDirectoryFromFile(outputPath);
					File.WriteAllBytes(outputPath, data);
					ErrorHandler.HandleException("Cannot save data to the destination path: " + file + "\r\nSaving inside a GRF is no longer allowed.­\r\nThe file has been saved to '" + outputPath + "' instead, please update your paths in the settings accordingly.");
				}
				catch {
					throw new Exception("Cannot save data to the destination path: " + file + "\r\nSaving inside a GRF is no longer allowed.");
				}

				return;
			}

			if (File.Exists(savePath.FilePath)) {
				File.WriteAllBytes(savePath.FilePath, data);
			}
		}
	}
}