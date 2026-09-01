using ErrorManager;
using SDE.Editor.Database;
using SDE.Editor.Navigation;
using SDE.View;
using SDE.View.Editors;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary;

namespace SDE.Databases.Generic.Controls {
	public enum SelectTupleSource {
		Items,
		Mobs,
		Achievement,
		Skills,
		Emotes,
		AchievementIcons,
		Titles,
	}

	public partial class SelectTupleButton : UserControl {
		public SelectTupleButton() {
			InitializeComponent();
			
			_contextMenu.PlacementTarget = _button;
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(SelectTupleButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register(
				nameof(Source),
				typeof(SelectTupleSource),
				typeof(SelectTupleButton),
				new PropertyMetadata(SelectTupleSource.Items));

		public SelectTupleSource Source {
			get => (SelectTupleSource)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		public static readonly DependencyProperty SelectVisiblityProperty =
			DependencyProperty.Register(
				nameof(SelectVisiblity),
				typeof(Visibility),
				typeof(SelectTupleButton),
				new FrameworkPropertyMetadata(Visibility.Visible, OnSelectVisiblityPropertyChanged));

		public Visibility SelectVisiblity {
			get => (Visibility)GetValue(SelectVisiblityProperty);
			set => SetValue(SelectVisiblityProperty, value);
		}

		private static void OnSelectVisiblityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var control = (SelectTupleButton)d;

			control._miSelect.Visibility = (Visibility)e.NewValue;
		}

		private void _select_Click(object sender, RoutedEventArgs e) {
			if (Int32.TryParse(SourceField, out int value) && value > 0) {
				DataSource source = _getDataSource();
				TabNavigation.Select(source, value);
			}
		}

		private void _selectFromList_Click(object sender, RoutedEventArgs e) {
			try {
				DataSource source = _getDataSource();

				SelectTupleDialog select;

				MergedTable table = SdeEditor.Project.GetMergedTable(source);
				select = new SelectTupleDialog(table, source, SourceField);

				select.Owner = WpfUtilities.TopWindow;

				if (select.ShowDialog() == true) {
					SourceField = select.Id;
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			if (SelectVisiblity != Visibility.Visible) {
				_selectFromList_Click(null, null);
				return;
			}

			_miSelect.IsEnabled = Int32.TryParse(SourceField, out int value) && value > 0;

			try {
				string val = "Unknown";

				if (value > 0) {
					DataSource source = _getDataSource();
					MergedTable table = SdeEditor.Project.GetMergedTable(source);
					Database.Tuple tuple = table.TryGetTuple(value);

					if (tuple != null) {
						val = tuple.GetValue(table.AttributeList.Attributes.FirstOrDefault(p => p.IsDisplayAttribute) ?? table.AttributeList.Attributes[1]).ToString();
					}
				}

				_miSelect.Header = String.Format("Select '{0}'", val);
			}
			catch {
			}

			_button.ContextMenu.IsOpen = true;
		}

		private DataSource _getDataSource() {
			switch (Source) {
				case SelectTupleSource.Mobs:
					return DataSources.Mob;
				case SelectTupleSource.Items:
					return DataSources.Item;
				case SelectTupleSource.Achievement:
					return DataSources.Achievement;
				case SelectTupleSource.Skills:
					return DataSources.Skill;
				case SelectTupleSource.Emotes:
					return DataSources.Emote;
				case SelectTupleSource.AchievementIcons:
					return DataSources.AchievementIcon;
				case SelectTupleSource.Titles:
					return DataSources.Title;
				default:
					throw new ArgumentOutOfRangeException(nameof(Source));
			}
		}
	}
}
