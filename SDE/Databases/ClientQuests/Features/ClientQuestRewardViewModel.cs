using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SDE.Databases.ClientQuests.Features {
	public class ClientQuestRewardViewModel : BaseModelView<ClientQuestReward> {
		private ClientQuestViewModel _vm;

		public ClientQuestRewardViewModel(ClientQuestViewModel viewModel, ClientQuestReward model) {
			_vm = viewModel;
			Model = model;
		}

		public Brush ForegroundBrush => ReadableTupleBrush.TextForeground;

		public string Item { get => Model?.Item; set => ExecuteCommand(value); }
		public string Count { get => Model?.Count; set => ExecuteCommand(value); }

		public string DisplayItemName => DbUtilities.ItemId2Name(Item);

		public BitmapSource DataImage {
			get => Core.Extensions.GetIconDataImage(Item);
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => _vm.IsLocked = v);
		}
	}
}
