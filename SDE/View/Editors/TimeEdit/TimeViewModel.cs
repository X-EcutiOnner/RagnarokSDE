using System.ComponentModel;
using System.Windows;

namespace SDE.View.Editors.TimeEdit {
	public class TimeViewModel : INotifyPropertyChanged {
		public Time Model;

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public void SetModel(Time time) {
			Model = time;

			OnPropertyChanged("");
		}

		public bool Exact {
			get => Model.Exact;
			set {
				SetValue(ref Model.Exact, value, nameof(Exact));
				OnPropertyChanged(nameof(ExactVisibility));
				OnPropertyChanged(nameof(SolveVisibility));
			}
		}

		public Visibility ExactVisibility => Model.Exact ? Visibility.Visible : Visibility.Collapsed;
		public Visibility SolveVisibility => Model.Exact ? Visibility.Collapsed : Visibility.Visible;

		public WeekDay Week {
			get => Model.Week;
			set => SetValue(ref Model.Week, value, nameof(Week));
		}

		public string Day {
			get => Model.Day;
			set => SetValue(ref Model.Day, value, nameof(Day));
		}

		public string Hour {
			get => Model.Hour;
			set => SetValue(ref Model.Hour, value, nameof(Hour));
		}

		public string Minute {
			get => Model.Minute;
			set => SetValue(ref Model.Minute, value, nameof(Minute));
		}

		public string Second {
			get => Model.Second;
			set => SetValue(ref Model.Second, value, nameof(Second));
		}

		public string Month {
			get => Model.Month;
			set => SetValue(ref Model.Month, value, nameof(Month));
		}

		public string Year {
			get => Model.Year;
			set => SetValue(ref Model.Year, value, nameof(Year));
		}

		public string Output {
			get => Model.ToString();
		}

		private void SetValue<T>(ref T modelValue, T value, string propName) {
			modelValue = value;
			OnPropertyChanged(propName);
			OnPropertyChanged(nameof(Output));
		}
	}
}
