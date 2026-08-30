using ErrorManager;
using SDE.View;
using SDE.View.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public partial class LuaTableButton : UserControl {
		public LuaTableButton() {
			InitializeComponent();
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			try {
				var dialog = new LuaTableDialog(SdeEditor.Project);
				dialog.ShowDialog();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}
