using SDE.Databases.Generic.Controls;
using SDE.Editor.Database;
using System.Windows;

namespace SDE.Databases.Items.Controls {
	public partial class NameButton : MultiApplyBase {
		public NameButton() {
			InitializeComponent();
		}

		protected override string _getNewValue(ReadableTuple tuple, object oModel, string srcValue, string oldValue, string newValue) {
			return srcValue?.Replace("_", " ").Trim(' ');
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			base.Execute();
		}
	}
}
