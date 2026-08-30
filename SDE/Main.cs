using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SDE {
	public class GRFEditorMain {
		[STAThread]
		public static void Main(string[] args) {
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));

			var app = new App();
			app.StartupUri = new Uri("View\\SdeEditor.xaml", UriKind.Relative);
			app.Run();
		}

		public static byte[] Decompress(byte[] data) {
			using (MemoryStream memStream = new MemoryStream(data))
			using (GZipStream stream = new GZipStream(memStream, CompressionMode.Decompress)) {
				const int size = 4096;
				byte[] buffer = new byte[size];
				using (MemoryStream memory = new MemoryStream()) {
					int count;
					do {
						count = stream.Read(buffer, 0, size);
						if (count > 0) {
							memory.Write(buffer, 0, count);
						}
					}
					while (count > 0);
					return memory.ToArray();
				}
			}
		}

		public static byte[] Compress(byte[] data) {
			using (MemoryStream memory = new MemoryStream()) {
				using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true)) {
					gzip.Write(data, 0, data.Length);
				}
				return memory.ToArray();
			}
		}
	}
}
