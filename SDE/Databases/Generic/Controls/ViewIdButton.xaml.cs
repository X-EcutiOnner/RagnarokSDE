using ErrorManager;
using SDE.Editor;
using SDE.View;
using SDE.View.Editors;
using System;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace SDE.Databases.Generic.Controls {
	public partial class ViewIdButton : UserControl {
		public ViewIdButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(ViewIdButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		public static readonly DependencyProperty MaxLevelProperty =
			DependencyProperty.Register(
				nameof(MaxLevel),
				typeof(string),
				typeof(ViewIdButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string MaxLevel {
			get => (string)GetValue(MaxLevelProperty);
			set => SetValue(MaxLevelProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			try {
				var dialog = new ViewIdPreviewDialog(SdeEditor.Instance, SdeEditor.Instance.FindTopmostTab());
				WindowProvider.Show(dialog, sender as Button, WpfUtilities.TopWindow);
				dialog.Closed += delegate {
					ViewIdPreviewDialog.IsOpened = false;
					var button = sender as Button;

					if (button != null) {
						button.IsEnabled = ProjectConfiguration.SynchronizeWithClientDatabases && !ViewIdPreviewDialog.IsOpened;
					}

					dialog = null;
				};
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}
