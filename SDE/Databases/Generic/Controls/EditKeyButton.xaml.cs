using ErrorManager;
using SDE.View;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class EditKeyButton : UserControl {
		public EditKeyButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(EditKeyButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			var tab = SdeEditor.Instance.FindTopmostTab();

			if (tab == null) {
				ErrorHandler.HandleException("Not table selected, cannot modify the entry's key.");
				return;
			}

			if (tab.GetNewKey(out int key)) {
				SourceField = key.ToString();
			}
		}
	}
}
