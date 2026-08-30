using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public class Separator : TextBlock {
		public Separator() {
			MinHeight = 10;
			MaxHeight = 10;
			Margin = new System.Windows.Thickness(3);
		}
	}
}
