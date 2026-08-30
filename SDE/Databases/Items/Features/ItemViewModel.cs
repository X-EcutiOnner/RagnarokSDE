using Database;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Databases.Items.Common;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TokeiLibrary.WPF;
using Utilities;

namespace SDE.Databases.Items.Features {
	public class ItemViewModel : BaseModelView<Item> {
		public int MaximumDrops = -1;
		public int MaximumTargets = -1;

		public RangeObservableCollection<MobDropViewModel> MobDrops { get; } = new RangeObservableCollection<MobDropViewModel>();

		public bool IsLocked { get; set; }

		public ItemViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, Item model) {
			if (IsLocked)
				return;

			Model = model;
			Tuple = tuple;

			OnMobDropsListUpdated();
			OnPropertyChanged("");

			ClearErrors();
			ValidateAegisName();
		}

		public string Id {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple?.Key.ToString() == value)
					return;

				if (Int32.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Id));
			}
		}

		public string AegisName { get => Model?.AegisName; set { ExecuteCommand(value); ValidateAegisName(); } }
		public string Name { get => Model?.Name; set => ExecuteCommand(value); }
		public EnumInfoBase Type { get => EnumInfos.GetEnumBase(Model?.Type); set { ExecuteCommand((ItemType)value.Value); OnPropertyChanged(nameof(HasSubType)); OnPropertyChanged(nameof(HasNoSubType)); OnPropertyChanged(nameof(HasNoGear)); } }
		public EnumInfoBase AmmoType { get => EnumInfos.GetEnumBase(Model?.AmmoType); set => ExecuteCommand((AmmoType)value.Value); }
		public EnumInfoBase CardType { get => EnumInfos.GetEnumBase(Model?.CardType); set => ExecuteCommand((CardType)value.Value); }
		public EnumInfoBase WeaponType { get => EnumInfos.GetEnumBase(Model?.WeaponType); set => ExecuteCommand((WeaponType)value.Value); }
		public string Buy { get => Model?.Buy; set { ExecuteCommand(value); OnPropertyChanged(nameof(BuyPreview)); OnPropertyChanged(nameof(SellPreview)); } }
		public string Sell { get => Model?.Sell; set { ExecuteCommand(value); OnPropertyChanged(nameof(BuyPreview)); OnPropertyChanged(nameof(SellPreview)); } }
		public string Weight { get => Model?.Weight; set => ExecuteCommand(value); }
		public string Attack { get => Model?.Attack; set => ExecuteCommand(value); }
		public string MagicAttack { get => Model?.MagicAttack; set => ExecuteCommand(value); }
		public string Defense { get => Model?.Defense; set => ExecuteCommand(value); }
		public string Range { get => Model?.Range; set => ExecuteCommand(value); }
		public string Slots { get => Model?.Slots; set => ExecuteCommand(value); }
		public string Jobs { get => Model?.Jobs; set => ExecuteCommand(value); }
		public string Classes { get => Model?.Classes; set => ExecuteCommand(value); }
		public EnumInfoBase Gender { get => EnumInfos.GetEnumBase(Model?.Gender); set => ExecuteCommand((GenderType)value.Value); }
		public string Locations { get => Model?.Locations; set => ExecuteCommand(value); }
		public string WeaponLevel { get => Model?.WeaponLevel; set => ExecuteCommand(value); }
		public string ArmorLevel { get => Model?.ArmorLevel; set => ExecuteCommand(value); }
		public string EquipLevelMin { get => Model?.EquipLevelMin; set => ExecuteCommand(value); }
		public string EquipLevelMax { get => Model?.EquipLevelMax; set => ExecuteCommand(value); }
		public bool Refineable { get => Model == null ? false : Model.Refineable; set => ExecuteCommand(value); }
		public bool Gradable { get => Model == null ? false : Model.Gradable; set => ExecuteCommand(value); }
		public string View { get => Model?.View; set => ExecuteCommand(value); }
		public string AliasName { get => Model?.AliasName; set => ExecuteCommand(value); }
		public string Flags { get => Model?.Flags; set => ExecuteCommand(value); }
		public EnumInfoBase DropEffect { get => EnumInfos.GetEnumBase(Model?.DropEffect); set => ExecuteCommand((DropEffectType)value.Value); }
		public string Delay { get => Model?.Delay; set => ExecuteCommand(value); }
		public string DelayStatus { get => Model?.DelayStatus; set => ExecuteCommand(value); }
		public string StackAmount { get => Model?.StackAmount; set => ExecuteCommand(value); }
		public string StackFlags { get => Model?.StackFlags; set => ExecuteCommand(value); }
		public string NoUseOverride { get => Model?.NoUseOverride; set => ExecuteCommand(value); }
		public string NoUseFlags { get => Model?.NoUseFlags; set => ExecuteCommand(value); }
		public string TradeOverride { get => Model?.TradeOverride; set => ExecuteCommand(value); }
		public string TradeFlags { get => Model?.TradeFlags; set => ExecuteCommand(value); }
		public string Script { get => Model?.Script; set => ExecuteCommand(value); }
		public string EquipScript { get => Model?.EquipScript; set => ExecuteCommand(value); }
		public string UnEquipScript { get => Model?.UnEquipScript; set => ExecuteCommand(value); }
		public bool HasSubType {
			get {
				if (Model == null)
					return true;
				
				switch (Model?.Type) {
					case ItemType.IT_AMMO:
					case ItemType.IT_CARD:
					case ItemType.IT_WEAPON:
						return true;
				}

				return false;
			}
		}
		public bool HasNoSubType => !HasSubType;
		public bool HasNoGear {
			get {
				if (Model == null)
					return true;
				
				switch (Model?.Type) {
					case ItemType.IT_ARMOR:
					case ItemType.IT_WEAPON:
						return false;
				}

				return true;
			}
		}
		public string SellPreview {
			get {
				if (!String.IsNullOrEmpty(Model?.Sell))
					return "";
				Int64.TryParse(Model?.Buy ?? "", out Int64 value);
				return (value / 2).ToString();
			}
		}
		public string BuyPreview {
			get {
				if (!String.IsNullOrEmpty(Model?.Buy))
					return "";
				Int64.TryParse(Model?.Sell ?? "", out Int64 value);
				return (value * 2).ToString();
			}
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}

		#region Validation
		private MergedTable _itemDb;

		public async void ValidateAegisName() {
			ClearErrors(nameof(AegisName));

			if (_itemDb == null)
				_itemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);

			if (Tuple == null)
				return;

			try {
				var current = AegisName;

				if (String.IsNullOrEmpty(current)) {
					AddError(nameof(AegisName), "AegisName cannot be empty.");
				}

				List<ReadableTuple> results = await Task.Run(delegate {
					return _itemDb.FastItems.Where(p => String.Compare(p.GetModel<Item>().AegisName, current, true) == 0).Where(p => p.Key != Tuple.Key).ToList();
				});

				if (results.Count > 0) {
					AddError(nameof(AegisName), "Another item(s) already uses this AegisName:\r\n" + Methods.Aggregate(results.Select(p => p.Key + " - " + p.GetModel<Item>().Name).ToList(), "\r\n"));
				}
			}
			catch (Exception err) {
				AddError(nameof(AegisName), "Generic error:\r\n" + err.Message);
			}
		}
		#endregion

		#region MvpDrops list
		public async void OnMobDropsListUpdated() {
			var viewModels = new List<MobDropViewModel>();

			if (Tuple == null) {
				MobDrops.ClearAndAddRange(new List<MobDropViewModel>());
				return;
			}

			await Task.Run(delegate {
				Table<int, ReadableTuple> mobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);
				var itemId = Tuple.Key.ToString();

				foreach (var mob in mobDb.FastItems) {
					var model = mob.GetModel<Mob>();

					foreach (var drop in model.Drops) {
						if (drop.Item == itemId) {
							viewModels.Add(new MobDropViewModel(this, new MobDrop() { Mob = mob.Key, ItemDrop = drop }));
						}
					}

					foreach (var drop in model.MvpDrops) {
						if (drop.Item == itemId) {
							viewModels.Add(new MobDropViewModel(this, new MobDrop() { Mob = mob.Key, ItemDrop = drop, IsMvp = true }));
						}
					}
				}
			});

			MobDrops.ClearAndAddRange(viewModels);
		}
		#endregion
	}
}
