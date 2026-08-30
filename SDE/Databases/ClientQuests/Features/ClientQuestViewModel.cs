using Database.Commands;
using SDE.Editor.Generic.DbTabs;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TokeiLibrary.WPF;
using SDE.Databases.Achievements.Parser;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;

namespace SDE.Databases.ClientQuests.Features {
	public class ClientQuestViewModel : BaseModelView<ClientQuest> {
		public RangeObservableCollection<ClientQuestRewardViewModel> Rewards { get; } = new RangeObservableCollection<ClientQuestRewardViewModel>();

		private ClientQuestRewardViewModel _selectedReward;

		public bool IsLocked { get; set; }

		public ClientQuestViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, ClientQuest model) {
			if (IsLocked)
				return;

			Model = model;
			Tuple = tuple;

			_selectedReward = null;

			OnRewardsListUpdated();
			OnPropertyChanged("");
		}

		public string Id {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple.Key.ToString() == value)
					return;

				if (int.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Id));
			}
		}

		public string Title { get => Model?.Title; set => ExecuteCommand(value); }
		public string IconName { get => Model?.IconName; set => ExecuteCommand(value); }
		public string Summary { get => Model?.Summary; set => ExecuteCommand(value); }
		public string BgName { get => Model?.BgName; set => ExecuteCommand(value); }
		public string NpcSpr { get => Model?.NpcSpr; set => ExecuteCommand(value); }
		public string NpcNavi { get => Model?.NpcNavi; set => ExecuteCommand(value); }
		public string NpcPosX { get => Model?.NpcPosX; set => ExecuteCommand(value); }
		public string NpcPosY { get => Model?.NpcPosY; set => ExecuteCommand(value); }
		public string QuestInfo1 { get => Model?.QuestInfo1; set => ExecuteCommand(value); }
		public string QuestInfo2 { get => Model?.QuestInfo2; set => ExecuteCommand(value); }
		public string QuestInfo3 { get => Model?.QuestInfo3; set => ExecuteCommand(value); }
		public string Description { get => Model?.Description; set => ExecuteCommand(value); }
		public bool CoolTimeQuest { get => Model == null ? false : Model.CoolTimeQuest; set => ExecuteCommand(value); }
		public string RewardEXP { get => Model?.RewardEXP; set => ExecuteCommand(value); }
		public string RewardJEXP { get => Model?.RewardJEXP; set => ExecuteCommand(value); }
		public string SG { get => Model?.SG; set => ExecuteCommand(value); }
		public string QUE { get => Model?.QUE; set => ExecuteCommand(value); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}

		#region Rewards
		public ClientQuestRewardViewModel SelectedReward {
			get => _selectedReward;
			set {
				if (_selectedReward == value)
					return;

				_selectedReward = value;
				OnPropertyChanged(nameof(SelectedReward));
				OnPropertyChanged(nameof(IsRewardSelected));
			}
		}

		public bool IsRewardSelected => _selectedReward != null;
		
		public void OnRewardsListUpdated() {
			Rewards.ClearAndAddRange(Model == null ? new List<ClientQuestRewardViewModel>() : Model.Rewards.Select(p => new ClientQuestRewardViewModel(this, p)));
		}
		public void CopyRewards(List<ClientQuestReward> entries) => Copy<ClientQuestReward, ClientQuestWriterLua>(entries, (v, writer, b) => writer.WriteReward(b, v));

		public void ChangeRewards(List<ClientQuestReward> targets, ListCommandMode mode) {
			if (targets.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.Rewards, targets, mode);
				OnRewardsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}
		#endregion
	}
}
