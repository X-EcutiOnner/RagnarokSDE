using SDE.Databases.Generic.Controls;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using System.Windows;

namespace SDE.Databases.Mobs.Controls {
	public partial class JapaneseNameButton : MultiApplyBase {
		public JapaneseNameButton() {
			InitializeComponent();
		}

		protected override string _getNewValue(ReadableTuple tuple, object oModel, string srcValue, string oldValue, string newValue) {
			var model = (Mob)oModel;
			return model.Name;
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			base.Execute();
		}
	}
}
