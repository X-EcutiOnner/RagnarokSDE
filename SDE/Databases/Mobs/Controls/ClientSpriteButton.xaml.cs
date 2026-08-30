using SDE.View;
using SDE.View.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TokeiLibrary;
using Utilities.Services;

namespace SDE.Databases.Mobs.Controls {
	public partial class ClientSpriteButton : UserControl {
		public ClientSpriteButton() {
			InitializeComponent();
		}

		public static readonly DependencyProperty SourceFieldProperty =
			DependencyProperty.Register(
				nameof(SourceField),
				typeof(string),
				typeof(ClientSpriteButton),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SourceField {
			get => (string)GetValue(SourceFieldProperty);
			set => SetValue(SourceFieldProperty, value);
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			MultiGrfExplorer dialog = new MultiGrfExplorer(SdeEditor.MetaGrf, EncodingService.FromAnyToDisplayEncoding(@"data\sprite\¸ó½ºÅÍ\"), ".spr", EncodingService.FromAnyToDisplayEncoding(SourceField));
			dialog.Owner = WpfUtilities.FindParentControl<Window>(_button);

			if (dialog.ShowDialog() == true) {
				SourceField = Path.GetFileNameWithoutExtension(dialog.SelectedPath.RelativePath);
			}
		}
	}
}
