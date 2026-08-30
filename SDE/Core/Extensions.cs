using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GRF.Image;
using GRF.IO;
using ICSharpCode.AvalonEdit;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.Generic.Controls;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles.ListView;
using TokeiLibrary.WpfBugFix;
using Utilities.Extension;
using Utilities.Services;

namespace SDE.Core {
	public class FileParserException : Exception {
		public string File { get; set; }
		public int Line { get; set; }
		public string Reason { get; set; }

		public FileParserException(string file, int line, string reason) {
			File = file;
			Line = line;
			Reason = reason;
		}

		public FileParserException(string file, int line, string reason, Exception err) : base(reason, err) {
			File = file;
			Line = line;
			Reason = reason;
		}
	}

	public static class Extensions {
		private static readonly Dictionary<RangeListView, object> _defaultSearches = new Dictionary<RangeListView, object>();
		private static UTF8Encoding _utf8NoBom;

		internal static Encoding Utf8NoBom {
			get {
				if (_utf8NoBom == null) {
					UTF8Encoding noBom = new UTF8Encoding(false, true);
					Thread.MemoryBarrier();
					_utf8NoBom = noBom;
				}
				return _utf8NoBom;
			}
		}

		public static string ConvertEncoding(string line, Encoding source, Encoding destination, bool isUtf8) {
			//bool isUtf8 = Utf8Checker.IsUtf8(line, source);

			if (source.CodePage != 65001 && isUtf8) {
				string utf8 = Utf8NoBom.GetString(source.GetBytes(line));

				if (source.GetString(source.GetBytes(utf8)) != utf8) {
					return destination.GetString(Encoding.Convert(Utf8NoBom, destination, Utf8NoBom.GetBytes(line)));
				}

				return utf8;
			}

			if (source.CodePage != destination.CodePage) {
				if (isUtf8 || destination.CodePage == 65001) {
					return destination.GetString(Encoding.Convert(source, destination, source.GetBytes(line)));
				}
				// ??
				return destination.GetString(source.GetBytes(line));
			}

			return line;
		}

		public static DefaultComparer<T> BindDefaultSearch<T>(RangeListView lv, string id, bool enableAlphaNum = false) {
			if (!_defaultSearches.ContainsKey(lv)) {
				_defaultSearches[lv] = new DefaultComparer<T>(enableAlphaNum);
			}

			DefaultComparer<T> comparer = (DefaultComparer<T>)_defaultSearches[lv];
			lv.Dispatch(p => comparer.SetOrder(ListViewExtensions.GetLastGetSearchAccessor(lv) ?? id, ListViewExtensions.GetLastSortDirection(lv)));
			return comparer;
		}

		public static void InsertIntoList<T>(RangeListView lv, T item, IList<T> allItems) {
			if (!_defaultSearches.ContainsKey(lv)) {
				_defaultSearches[lv] = new DefaultComparer<T>();
			}

			DefaultComparer<T> comparer = (DefaultComparer<T>)_defaultSearches[lv];
			var index = allItems.ToList().BinarySearch(item, comparer);
			if (index < 0) index = ~index;
			allItems.Insert(index, item);
		}

		public static void SetMinimalSize(Window window) {
			window.Loaded += delegate {
				window.MinHeight = window.ActualHeight;
				window.MinWidth = window.ActualWidth;
			};
		}

		public static bool Equals<T>(T x, T y) {
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			return Object.Equals(x, y);
		}

		public static bool Equals<T>(List<T> x, List<T> y) {
			if (x.Count != y.Count) return false;
			for (int i = 0; i < x.Count; i++)
				if (!Equals(x[i], y[i]))
					return false;
			return true;
		}

		public static void CopyTo(this Stream stream, string path) {
			stream.CopyTo(path, 0);
		}

		public static void CopyTo(this Stream stream, string path, int silentIgnoredAttempts) {
			using (Stream dest = new FileStream(path, FileMode.Create, FileAccess.Write)) {
				stream.CopyTo(dest, 8 * 1024, silentIgnoredAttempts);
			}
		}

		public static void CopyTo(this Stream stream, Stream dest) {
			stream.CopyTo(dest, 8 * 1024, 0);
		}

		public static void CopyTo(this Stream stream, Stream dest, int bufferSize, int silentIgnoredAttempts) {
			if (stream.CanSeek) {
				stream.Seek(0, SeekOrigin.Begin);
			}

			while (true) {
				try {
					byte[] buffer = new byte[bufferSize];
					int len;
					while ((len = stream.Read(buffer, 0, buffer.Length)) > 0) {
						dest.Write(buffer, 0, len);
					}
					return;
				}
				catch {
					silentIgnoredAttempts--;

					if (silentIgnoredAttempts < 0) {
						throw;
					}
				}
			}
		}

		public static bool GetIntFromFloatValue(string text, out int ival) {
			float fval;

			bool hasPercentage = text.Contains("%");

			text = text.Replace("%", "").Trim(' ');

			if (!hasPercentage && Int32.TryParse(text, out ival)) {
				return true;
			}

			string tdot = text.Replace(",", ".");

			if (Single.TryParse(tdot, out fval)) {
				ival = (int)Math.Round((fval * 100), 0, MidpointRounding.AwayFromZero);
				return true;
			}

			string tcomma = text.Replace(".", ",");

			if (Single.TryParse(tcomma, out fval)) {
				ival = (int)Math.Round((fval * 100), 0, MidpointRounding.AwayFromZero);
				return true;
			}

			ival = 0;
			return false;
		}

		public static string ParseToTimeMs(string text) {
			long val;
			Int64.TryParse(text == "" ? "0" : text, out val);

			if (val == 0)
				return "0s";

			// There are no minutes
			if (val % 3600000 == 0) {
				val = val / 3600000; // Hours

				if (val > 24) {
					return String.Format("{0:0}d:{1:00}h", val / 24, val % 24);
				}

				return String.Format("{0:0}h", val);
			}

			// There are no seconds
			if (val % 60000 == 0) {
				val = val / 60000; // Minutes

				if (val > 1440) {
					return String.Format("{0:0}d:{1:00}h:{2:00}m", val / 1440, (val % 1440) / 60, val % 60);
				}

				if (val > 60) {
					return String.Format("{0:0}h:{1:00}m", val / 60, val % 60);
				}

				return String.Format("{0:0}m", val);
			}

			// There are no miliseconds
			if (val % 1000 == 0) {
				val = val / 1000; // Seconds

				if (val > 86400) {
					return String.Format("{0:0}d:{1:00}h:{2:00}m:{3:00}s", val / 86400, (val % 86400) / 3600, (val % 3600) / 60, val % 60);
				}

				if (val > 3600) {
					return String.Format("{0:0}h:{1:00}m:{2:00}s", val / 3600, (val % 3600) / 60, val % 60);
				}

				if (val > 60) {
					return String.Format("{0:0}m:{1:00}s", val / 60, val % 60);
				}

				return String.Format("{0:0}s", val);
			}

			if (val > 60000) {
				return String.Format("{0:0}m:{1:00}.{2:000}s", val / 60000, (val % 60000) / 1000, val % 1000);
			}

			return String.Format("{0:0}.{1:000}s", val / 1000, val % 1000);
		}

		public static string ParseToTimeSeconds(string text) {
			if (String.IsNullOrEmpty(text))
				return "0s";

			Int32.TryParse(String.IsNullOrEmpty(text) ? "0" : text, out int val);
			return ParseToTimeMs((val * 1000).ToString(CultureInfo.InvariantCulture));
		}

		public static string ParseBracket(string value, int index) {
			value = value.Trim('[', ']', '(', ')');
			string[] subs = value.Split(',');
			return subs[index].Trim(' ', '\t');
		}

		public static unsafe int strncasecmp(byte* ptr, string target, int length) {
			for (int i = 0; i < length; i++) {
				byte b1 = ptr[i];
				byte b2 = (byte)target[i];

				if (b1 >= 'a' && b1 <= 'z') b1 -= 32;
				if (b2 >= 'a' && b2 <= 'z') b2 -= 32;

				if (b1 != b2)
					return b1 - b2;

				if (b1 == 0) return 0;
			}

			return 0;
		}

		public static int SafeAtoi(string text) {
			if (text == null || text == "")
				return 0;

			int r = 0;

			for (int i = 0; i < text.Length; i++) {
				if (text[i] == ' ' || (text[i] >= '\t' && text[i] <= '\r')) {
					continue;
				}

				for (int j = i; j < text.Length; j++) {
					if (char.IsDigit(text[j])) {
						int digit = text[j] - '0';
						r = (r * 10) + digit;
						continue;
					}

					break;
				}

				break;
			}

			return r;
		}

		public static unsafe int atoi(byte* ptr) {
			if (ptr == null) return 0;

			while (*ptr == ' ' || (*ptr >= '\t' && *ptr <= '\r')) {
				ptr++;
			}

			int sign = 1;
			if (*ptr == '-') {
				sign = -1;
				ptr++;
			}
			else if (*ptr == '+') {
				ptr++;
			}

			int result = 0;
			while (*ptr >= '0' && *ptr <= '9') {
				int digit = *ptr - '0';

				result = (result * 10) + digit;
				ptr++;
			}

			return result * sign;
		}

		public static long ParseRAthenaTimeToSeconds(string time) {
			unsafe {
				var bytes = Encoding.Default.GetBytes(time);
				int w = -1, d = -1, h = -1, mn = -1, s = -1;

				fixed (byte* modif_base = bytes) {
					byte* modif_p = modif_base;

					while (modif_p[0] != '\0') {
						int value = atoi(modif_p);

						if (modif_p[0] == '-' || modif_p[0] == '+')
							modif_p++;
						while (modif_p[0] >= '0' && modif_p[0] <= '9')
							modif_p++;
						if (strncasecmp(modif_p, "SUNDAY", 6) == 0) {
							w = 0;
							modif_p = modif_p + 6;
						}
						else if (strncasecmp(modif_p, "MONDAY", 6) == 0) {
							w = 1;
							modif_p = modif_p + 6;
						}
						else if (strncasecmp(modif_p, "TUESDAY", 7) == 0) {
							w = 2;
							modif_p = modif_p + 7;
						}
						else if (strncasecmp(modif_p, "WEDNESDAY", 9) == 0) {
							w = 3;
							modif_p = modif_p + 9;
						}
						else if (strncasecmp(modif_p, "THURSDAY", 8) == 0) {
							w = 4;
							modif_p = modif_p + 8;
						}
						else if (strncasecmp(modif_p, "FRIDAY", 6) == 0) {
							w = 5;
							modif_p = modif_p + 6;
						}
						else if (strncasecmp(modif_p, "SATURDAY", 8) == 0) {
							w = 6;
							modif_p = modif_p + 8;
						}
						else if (modif_p[0] == 's') {
							s = value;
							modif_p++;
						}
						else if (modif_p[0] == 'm' && modif_p[1] == 'n') {
							mn = value;
							modif_p = modif_p + 2;
						}
						else if (modif_p[0] == 'h') {
							h = value;
							modif_p++;
						}
						else if (modif_p[0] == 'd' || modif_p[0] == 'j') {
							d = value;
							modif_p++;
						}
						else if (modif_p[0] != '\0') {
							modif_p++;
						}
					}
				}

				if (h < 0 || h > 23 || mn > 59 || s > 59)   // hour is required
					return 0;

				return
					Math.Max(0, s) +
					Math.Max(0, mn) * 60 +
					Math.Max(0, h) * 3600 +
					Math.Max(0, d) * 86400 +
					Math.Max(0, w) * 604800;
			}
		}

		public static void RemoveUndoAndRedoEvents(FrameworkElement box, DbTab tab) {
			box.PreviewKeyDown += delegate (object sender, KeyEventArgs args) {
				if (SdeCommands.UndoGlobal.IsMatch()) {
					tab.Undo();
					args.Handled = true;
				}

				if (SdeCommands.RedoGlobal.IsMatch()) {
					tab.Redo();
					args.Handled = true;
				}

				if (SdeCommands.Undo.IsMatch()) {
					if (box is ValidationTextBox vBox) {
						box = vBox._tbData;
					}
					
					if (box is TextBox tBox) {
						if (!tBox.CanRedo && !tBox.CanUndo) {
							tab.Undo();
						}
						else if (tBox.CanUndo) {
							tBox.Undo();
						}
					}
					else if (box is TextEditor eBox) {
						if (!eBox.CanRedo && !eBox.CanUndo) {
							tab.Undo();
						}
						else if (eBox.CanUndo) {
							eBox.Undo();
						}
					}
					else {
						tab.Undo();
					}

					args.Handled = true;
				}

				if (SdeCommands.Redo.IsMatch()) {
					if (box is ValidationTextBox vBox) {
						box = vBox._tbData;
					}

					if (box is TextBox tBox) {
						if (!tBox.CanRedo && !tBox.CanRedo) {
							tab.Redo();
						}
						else if (tBox.CanRedo) {
							tBox.Redo();
						}
					}
					else if (box is TextEditor eBox) {
						if (!eBox.CanRedo && !eBox.CanRedo) {
							tab.Redo();
						}
						else if (eBox.CanRedo) {
							eBox.Redo();
						}
					}
					else {
						tab.Redo();
					}

					args.Handled = true;
				}
			};
		}

		public static List<T> FindChildren<T>(DependencyObject parent, List<T> children = null) where T : DependencyObject {
			if (children == null)
				children = new List<T>();

			if (parent == null)
				return null;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
				var child = VisualTreeHelper.GetChild(parent, i);

				if (child is T typed) {
					children.Add(typed);
				}

				if (child is Property property && property.Editor is T typed2) {
					children.Add(typed2);
				}

				FindChildren(child, children);
			}

			return children;
		}

		public static void SetupZIndex(DependencyObject parent) {
			int index = 0;
			SetupZIndex(parent, ref index);
		}

		public static void SetupZIndex(DependencyObject parent, ref int index, int multipler = 1) {
			if (parent == null)
				return;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
				var child = VisualTreeHelper.GetChild(parent, i);

				if (child is TextBox tb) {
					tb.TabIndex = index;
					index += multipler;

					tb.GotKeyboardFocus += delegate {
						if (Keyboard.IsKeyDown(Key.Tab))
							tb.SelectAll();
					};
				}
				else if (child is ValidationTextBox vtb2) {
					vtb2._tbData.TabIndex = index;
					index += multipler;

					vtb2.GotKeyboardFocus += delegate {
						if (Keyboard.IsKeyDown(Key.Tab))
							vtb2.SelectAll();
					};
				}
				else if (child is Property prop && prop.Editor is ValidationTextBox vtb) {
					vtb._tbData.TabIndex = index;
					index += multipler;

					vtb.GotKeyboardFocus += delegate {
						if (Keyboard.IsKeyDown(Key.Tab))
							vtb.SelectAll();
					};
				}
				else if (child is ComboBox cb) {
					cb.TabIndex = index;
					index += multipler;
				}

				SetupZIndex(child, ref index);
			}
		}

		public static BitmapSource GetIconDataImage(string name) {
			if (Int32.TryParse(name, out int intValue))
				return GetIconDataImage(intValue);

			return null;
		}

		public static BitmapSource GetIconDataImage(int id) {
			var clientTable = SdeEditor.Project.GetMergedTable(DataSources.ClientItem);

			try {
				var entry = clientTable.TryGetTuple(id);

				if (entry != null) {
					byte[] data = SdeEditor.MetaGrf.GetData(EncodingService.FromAnyToDisplayEncoding(@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\item\" + entry.GetModel<ClientItem>().IdentifiedResourceName.ExpandString() + ".bmp"));

					if (data != null) {
						GrfImage gimage = new GrfImage(data);
						gimage.MakePinkShadeTransparent();

						if (gimage.GrfImageType == GrfImageType.Bgr24) {
							gimage.Convert(GrfImageType.Bgra32);
						}

						return gimage.Cast<BitmapSource>();
					}
				}

				return null;
			}
			catch {
				return null;
			}
		}

		public static void ClearUndos(Panel grid) {
			FindChildren<ValidationTextBox>(grid).ForEach(p => p.ClearUndo());
		}

		public static object GetImage(string path, string file) {
			try {
				if (path == null || file == null)
					return null;

				byte[] data = SdeEditor.MetaGrf.GetDataBuffered(GrfPath.Combine(path.ToDisplayEncoding(), file.ExpandString().ToDisplayEncoding()));

				if (data != null) {
					GrfImage gimage = new GrfImage(data);
					gimage.MakePinkShadeTransparent();

					if (gimage.GrfImageType == GrfImageType.Bgr24) {
						gimage.Convert(GrfImageType.Bgra32);
					}

					var image = gimage.Cast<BitmapSource>();
					image.Freeze();
					return image;
				}

				return null;
			}
			catch {
				return null;
			}
		}

		// Magic code to count bit count, Hacker's Delight algorithm
		public static int PopCount(long value) {
			ulong x = (ulong)value;

			x -= (x >> 1) & 0x5555555555555555UL;
			x = (x & 0x3333333333333333UL) + ((x >> 2) & 0x3333333333333333UL);
			x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0FUL;

			return (int)((x * 0x0101010101010101UL) >> 56);
		}

		public static void FixDarkThemeListView(ListView listView) {
			bool applied = false;

			if (listView.IsLoaded) {
				_fixDarkThemeListView(listView);
				return;
			}

			listView.Loaded += delegate {
				if (applied) return;
				applied = true;

				_fixDarkThemeListView(listView);
			};
		}

		private static void _fixDarkThemeListView(ListView listView) {
			var border = WpfUtilities.FindChild<Border>(listView);

			if (border != null) {
				border.Background = Brushes.Transparent;
			}

			var sv = WpfUtilities.FindChild<ScrollViewer>(listView);
			sv?.SetResourceReference(ScrollViewer.BackgroundProperty, "UIThemeBackground2Brush");
		}
	}
}