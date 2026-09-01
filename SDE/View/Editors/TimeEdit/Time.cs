using SDE.Core;
using System.Collections.Generic;
using System.Text;

namespace SDE.View.Editors.TimeEdit {
	public enum WeekDay {
		Any = -1,
		Sunday,
		Monday,
		Tuesday,
		Wednesday,
		Thursday,
		Friday,
		Saturday,
	}

	public class Time {
		public static List<WeekDay> WeekDays { get; } = new List<WeekDay> { WeekDay.Any, WeekDay.Sunday, WeekDay.Monday, WeekDay.Tuesday, WeekDay.Wednesday, WeekDay.Thursday, WeekDay.Friday, WeekDay.Saturday };

		public bool Exact;
		public WeekDay Week = WeekDay.Any;
		public string Day = "0";
		public string Hour = "0";
		public string Minute = "0";
		public string Second = "0";
		public string Month = "0";
		public string Year = "0";

		private void Append(StringBuilder b, string text, string suffix) {
			if (!int.TryParse(text, out int value) || value <= 0)
				return;

			b.Append(value + suffix);
		}

		public string ToExactTime() {
			StringBuilder b = new StringBuilder();

			if (Week != WeekDay.Any) {
				b.Append(Week.ToString() + " ");
			}

			Append(b, Day, "d ");
			Append(b, Hour, "h ");
			Append(b, Minute, "mn ");
			Append(b, Second, "s ");

			if (b.Length == 0)
				return "";

			return b.ToString().TrimEnd(' ');
		}

		public string ToSolveTime() {
			StringBuilder b = new StringBuilder();

			b.Append("+");

			Append(b, Year, "y");
			Append(b, Month, "m");
			Append(b, Day, "d");
			Append(b, Hour, "h");
			Append(b, Minute, "mn");
			Append(b, Second, "s");

			if (b.Length == 1)
				return "";

			return b.ToString();
		}

		public override string ToString() {
			return Exact ? ToExactTime() : ToSolveTime();
		}

		public static Time FromExactTime(string time) {
			Time ret = new Time();
			ret.Exact = true;

			unsafe {
				var bytes = Encoding.Default.GetBytes(time);

				fixed (byte* modif_base = bytes) {
					byte* modif_p = modif_base;

					while (bytes.Length > 0 && modif_p[0] != '\0') {
						int value = Extensions.atoi(modif_p);

						if (modif_p[0] == '-' || modif_p[0] == '+')
							modif_p++;
						while (modif_p[0] >= '0' && modif_p[0] <= '9')
							modif_p++;
						if (Extensions.strncasecmp(modif_p, "SUNDAY", 6) == 0) {
							ret.Week = WeekDay.Sunday;
							modif_p = modif_p + 6;
						}
						else if (Extensions.strncasecmp(modif_p, "MONDAY", 6) == 0) {
							ret.Week = WeekDay.Monday;
							modif_p = modif_p + 6;
						}
						else if (Extensions.strncasecmp(modif_p, "TUESDAY", 7) == 0) {
							ret.Week = WeekDay.Tuesday;
							modif_p = modif_p + 7;
						}
						else if (Extensions.strncasecmp(modif_p, "WEDNESDAY", 9) == 0) {
							ret.Week = WeekDay.Wednesday;
							modif_p = modif_p + 9;
						}
						else if (Extensions.strncasecmp(modif_p, "THURSDAY", 8) == 0) {
							ret.Week = WeekDay.Thursday;
							modif_p = modif_p + 8;
						}
						else if (Extensions.strncasecmp(modif_p, "FRIDAY", 6) == 0) {
							ret.Week = WeekDay.Friday;
							modif_p = modif_p + 6;
						}
						else if (Extensions.strncasecmp(modif_p, "SATURDAY", 8) == 0) {
							ret.Week = WeekDay.Saturday;
							modif_p = modif_p + 8;
						}
						else if (modif_p[0] == 's') {
							ret.Second = value.ToString();
							modif_p++;
						}
						else if (modif_p[0] == 'm' && modif_p[1] == 'n') {
							ret.Minute = value.ToString();
							modif_p = modif_p + 2;
						}
						else if (modif_p[0] == 'h') {
							ret.Hour = value.ToString();
							modif_p++;
						}
						else if (modif_p[0] == 'd' || modif_p[0] == 'j') {
							ret.Day = value.ToString();
							modif_p++;
						}
						else if (modif_p[0] != '\0') {
							modif_p++;
						}
					}
				}

				return ret;
			}
		}

		public static Time FromSolveTime(string time) {
			Time ret = new Time();

			unsafe {
				var bytes = Encoding.Default.GetBytes(time);

				fixed (byte* modif_base = bytes) {
					byte* modif_p = modif_base;
					int s = 0;
					int mn = 0;
					int h = 0;
					int d = 0;
					int m = 0;
					int y = 0;

					while (bytes.Length > 0 && modif_p[0] != '\0') {
						int value = Extensions.atoi(modif_p);

						if (value == 0)
							modif_p++;
						else {
							if (modif_p[0] == '-' || modif_p[0] == '+')
								modif_p++;
							while (modif_p[0] >= '0' && modif_p[0] <= '9')
								modif_p++;
							if (modif_p[0] == 's') {
								s += value;
								modif_p++;
							}
							else if (modif_p[0] == 'n') {
								mn += value;
								modif_p++;
							}
							else if (modif_p[0] == 'm' && modif_p[1] == 'n') {
								mn += value;
								modif_p = modif_p + 2;
							}
							else if (modif_p[0] == 'h') {
								h += value;
								modif_p++;
							}
							else if (modif_p[0] == 'd' || modif_p[0] == 'j') {
								d += value;
								modif_p++;
							}
							else if (modif_p[0] == 'm') {
								m += value;
								modif_p++;
							}
							else if (modif_p[0] == 'y' || modif_p[0] == 'a') {
								y += value;
								modif_p++;
							}
							else if (modif_p[0] != '\0') {
								modif_p++;
							}
						}
					}

					ret.Second = s.ToString();
					ret.Minute = mn.ToString();
					ret.Hour = h.ToString();
					ret.Day = d.ToString();
					ret.Month = m.ToString();
					ret.Year = y.ToString();
				}
			}

			return ret;
		}

		public static Time Parse(string text) {
			if (text == null)
				text = "";

			if (text.Contains("+"))
				return FromSolveTime(text);
			else
				return FromExactTime(text);
		}
	}
}
