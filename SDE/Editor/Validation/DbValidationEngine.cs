using System;
using System.Collections.Generic;
using System.Linq;
using ErrorManager;
using GRF.Core;
using GRF.Threading;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using SDE.Editor.Engines.PreviewEngine;
using Utilities.Extension;

namespace SDE.Editor.Validation {
	public partial class DbValidationEngine : IProgress {
		private readonly ProjectManager _sde;
		private PreviewHelper _helper;

		public DbValidationEngine(ProjectManager sde) {
			_sde = sde;
			Grf = new GrfHolder();
		}

		private void _validateMobDb(MergedTable db, List<ValidationErrorView> errors) {
			foreach (var tuple in db.FastItems) {
				var model = tuple.GetModel<Mob>();

				if (SdeAppConfiguration.DbValidMaxItemDbId >= 0) {
					if (tuple.Key <= 1000 || tuple.Key > SdeAppConfiguration.DbValidMaxMobDbId) {
						errors.Add(new TableError(ValidationErrors.TbMobId, tuple.Key,
							String.Format("Invalid monster ID {0}, allowed values {1} < ID <= {2} (MAX_MOB_DB).",
								tuple.Key, 1000, SdeAppConfiguration.DbValidMaxMobDbId), DataSources.Mob, this));
					}
				}

				if (_pcCheckId((uint)tuple.Key)) {
					errors.Add(new TableError(ValidationErrors.TbReservedId, tuple.Key,
						String.Format("Invalid monster ID {0}, reserved for player classes.",
							tuple.Key), DataSources.Mob, this));
				}

				if (tuple.Key >= (SdeAppConfiguration.DbValidMaxMobDbId - 999) && tuple.Key < SdeAppConfiguration.DbValidMaxMobDbId) {
					errors.Add(new TableError(ValidationErrors.TbInvalidRange, tuple.Key,
						String.Format("Invalid monster ID {0}. Range {1}-{2} is reserved for player clones. Please increase MAX_MOB_DB ({3}).",
							tuple.Key, SdeAppConfiguration.DbValidMaxMobDbId - 999, SdeAppConfiguration.DbValidMaxMobDbId - 1, SdeAppConfiguration.DbValidMaxMobDbId), DataSources.Mob, this));
				}

				var level = DbReader.ToInt(model.Level);
				var minAtk = DbReader.ToInt(model.Attack);
				var maxAtk = DbReader.ToInt(model.Attack2);
				var def = DbReader.ToInt(model.Defense);
				var mdef = DbReader.ToInt(model.MagicDefense);
				//var baseExp = tuple.GetIntNoThrow(ServerMobAttributes.Lv);
				//var jobExp = tuple.GetIntNoThrow(ServerMobAttributes.Lv);

				_capValue(level, "level", 1, 0xffff, tuple, errors);
				_capValue(minAtk, "minAtk", 0, 0xffff, tuple, errors);
				_capValue(maxAtk, "maxAtk", 0, 0xffff, tuple, errors);

				var isRenewal = DbPathLocator.GetIsRenewal();

				_capValue(def, "def", isRenewal ? -32768 : -128, isRenewal ? 32767 : 127, tuple, errors);
				_capValue(mdef, "mdef", isRenewal ? -32768 : -128, isRenewal ? 32767 : 127, tuple, errors);

				_capValue(DbReader.ToInt(model.Str), nameof(Mob.Str), 0, 0xffff, tuple, errors);
				_capValue(DbReader.ToInt(model.Agi), nameof(Mob.Agi), 0, 0xffff, tuple, errors);
				_capValue(DbReader.ToInt(model.Vit), nameof(Mob.Vit), 0, 0xffff, tuple, errors);
				_capValue(DbReader.ToInt(model.Int), nameof(Mob.Int), 0, 0xffff, tuple, errors);
				_capValue(DbReader.ToInt(model.Dex), nameof(Mob.Dex), 0, 0xffff, tuple, errors);
				_capValue(DbReader.ToInt(model.Luk), nameof(Mob.Luk), 0, 0xffff, tuple, errors);

				var defEle = (int)model.Element % 10;
				var eleLevel = (int)model.ElementLevel / 20;

				if (defEle >= SdeAppConfiguration.DbValidMaxMobDbElement) {
					errors.Add(new TableError(ValidationErrors.TbElementType, tuple.Key,
						String.Format("Invalid element type {0} for monster ID {1} (max = {2}).",
							defEle, tuple.Key, SdeAppConfiguration.DbValidMaxMobDbElement - 1), DataSources.Mob, this));
				}

				if (eleLevel < 1 || eleLevel > 4) {
					errors.Add(new TableError(ValidationErrors.TbElementLevel, tuple.Key,
						String.Format("Invalid element level {0} for monster ID {1}, must be in range 1-4.",
							eleLevel, tuple.Key), DataSources.Mob, this));
				}

				var adelay = DbReader.ToInt(model.AttackDelay);
				var amotion = DbReader.ToInt(model.AttackMotion);
				var hp = (long)DbReader.ToInt(model.Hp);
				var mexp = (long)DbReader.ToInt(model.MvpExp);

				_capValue(adelay, "aDelay", 0, 4000, tuple, errors);
				_capValue(amotion, "aMotion", 0, 2000, tuple, errors);
				_capValue(hp, "HP", 0, Int32.MaxValue, tuple, errors);
				_capValue(mexp, "MExp", 0, Int32.MaxValue, tuple, errors);
			}
		}

		private void _capValue(int value, string nameValue, int min, int max, ReadableTuple tuple, List<ValidationErrorView> errors) {
			if (value > max) {
				errors.Add(new TableError(ValidationErrors.TbCapValue, tuple.Key,
					String.Format("Invalid {0} for {1}, found {3}. Value cannot be above {2}.", nameValue, tuple.Key, max, value),
					DataSources.Mob, this));
			}

			if (value < min) {
				errors.Add(new TableError(ValidationErrors.TbCapValue, tuple.Key,
					String.Format("Invalid {0} for {1}, found {3}. Value cannot be below {2}.", nameValue, tuple.Key, min, value),
					DataSources.Mob, this));
			}
		}

		private void _capValue(long value, string nameValue, long min, long max, ReadableTuple tuple, List<ValidationErrorView> errors) {
			if (value > max) {
				errors.Add(new TableError(ValidationErrors.TbCapValue, tuple.Key,
					String.Format("Invalid {0} for {1}, found {3}. Value cannot be above {2}.", nameValue, tuple.Key, max, value),
					DataSources.Mob, this));
			}

			if (value < min) {
				errors.Add(new TableError(ValidationErrors.TbCapValue, tuple.Key,
					String.Format("Invalid {0} for {1}, found {3}. Value cannot be below {2}.", nameValue, tuple.Key, min, value),
					DataSources.Mob, this));
			}
		}

		private void _validateItemDb(MergedTable db, List<ValidationErrorView> errors) {
			HashSet<int> typeValues = Enum.GetValues(typeof(ItemType)).Cast<int>().ToList().ToHashSet();

			foreach (var tuple in db.FastItems) {
				var item = tuple.GetModel<Item>();

				try {
					if (!typeValues.Contains((int)item.Type)) {
						errors.Add(new TableError(ValidationErrors.TbItemType, tuple.Key, String.Format("Invalid item type {0} for item {1}.", item.Type, tuple.Key), DataSources.Item, this));
					}
				}
				catch {
					errors.Add(new TableError(ValidationErrors.TbItemType, tuple.Key, String.Format("Invalid item type {0} for item {1}.", "?", tuple.Key), DataSources.Item, this));
				}

				var buy = DbReader.ToInt(item.Buy);
				var sell = DbReader.ToInt(item.Sell);

				if (DbReader.IsNullOrEmpty(item.Buy) && DbReader.IsNullOrEmpty(item.Sell)) {
					buy = sell = 0;
				}
				else if (DbReader.IsNullOrEmpty(item.Buy)) {
					buy = 2 * sell;
				}
				else if (DbReader.IsNullOrEmpty(item.Sell)) {
					sell = buy / 2;
				}

				if (buy / 124.0 < sell / 75.0) {
					errors.Add(new TableError(ValidationErrors.TbZenyExploit, tuple.Key,
						String.Format("Buying/Selling [{0}/{1}] price of item {2} allows Zeny making exploit through buying/selling at discounted/overcharged prices.",
							buy, sell, tuple.Key), DataSources.Item, this));
				}

				var slot = item.Slots.ToInt();

				if (slot > SdeAppConfiguration.DbValidMaxSlotCount) {
					errors.Add(new TableError(ValidationErrors.TbMaxSlotCount, tuple.Key,
						String.Format("Item {0} specifies {1} slots, but the server only supports up to {2}.",
							tuple.Key, slot, SdeAppConfiguration.DbValidMaxSlotCount), DataSources.Item, this));
				}

				var equip = (int)item.Locations.ToLong();
				var type = item.Type;

				if (equip == 0 && _isEquip2(type)) {
					errors.Add(new TableError(ValidationErrors.TbEquipField, tuple.Key,
						String.Format("Item {0} is an equipment with no equip-field.",
							tuple.Key), DataSources.Item, this));
				}

				var trade = (int)item.TradeFlags.ToLong();

				if (trade > 0x1ff) {
					errors.Add(new TableError(ValidationErrors.TbTradeRestrict, tuple.Key,
						String.Format("Invalid trade restriction flag {0} for item {1}.",
							trade, tuple.Key), DataSources.Item, this));
				}

				var tradeOverride = item.TradeOverride.ToInt();

				if (tradeOverride <= 0 || tradeOverride > 100) {
					errors.Add(new TableError(ValidationErrors.TbTradeOverr, tuple.Key,
						String.Format("Invalid trade-override GM level {0} for item {1}.",
							tradeOverride, tuple.Key), DataSources.Item, this));
				}

				var nouse = (int)item.NoUseFlags.ToLong();

				if (nouse > 1) {
					errors.Add(new TableError(ValidationErrors.TbNoUseRestrict, tuple.Key,
						String.Format("Invalid nouse restriction flag {0} for item {1}.",
							nouse, tuple.Key), DataSources.Item, this));
				}

				var nouseOverride = item.NoUseOverride.ToInt();

				if (nouseOverride <= 0 || nouseOverride > 100) {
					errors.Add(new TableError(ValidationErrors.TbNoUseOverr, tuple.Key,
						String.Format("Invalid nouse-override GM level {0} for item {1}.",
							nouseOverride, tuple.Key), DataSources.Item, this));
				}

				int stackAmount = item.StackAmount.ToInt();

				if (stackAmount > 0 && !_isStackable2(type)) {
					errors.Add(new TableError(ValidationErrors.TbNoStack, tuple.Key,
						String.Format("Item {0} of type {1} is not stackable.",
							tuple.Key, type), DataSources.Item, this));
				}

				var viewId = item.View.ToInt();

				if ((viewId == 13 || viewId == 14) && type == ItemType.IT_WEAPON) {
					var gender = item.Gender;

					if (viewId == 13 && gender != GenderType.SEX_MALE) {
						errors.Add(new TableError(ValidationErrors.TbGender, tuple.Key,
							String.Format("Item {0}; musical instruments are always male-only, but the gender field allows females as well.",
								tuple.Key), DataSources.Item, this));
					}

					if (viewId == 14 && gender != 0) {
						errors.Add(new TableError(ValidationErrors.TbGender, tuple.Key,
							String.Format("Item {0}; whips are always female-only, but the gender field allows males as well.",
								tuple.Key), DataSources.Item, this));
					}
				}
			}
		}

		private static bool _isEquip2(ItemType type) {
			switch(type) {
				case ItemType.IT_AMMO:
				case ItemType.IT_ARMOR:
				case ItemType.IT_WEAPON:
					return true;
			}

			return false;
		}

		private static bool _isStackable2(ItemType type) {
			switch(type) {
				case ItemType.IT_PETEGG:
				case ItemType.IT_PETARMOR:
				case ItemType.IT_ARMOR:
				case ItemType.IT_WEAPON:
					return false;
			}

			return true;
		}

		private static bool _pcCheckId(uint id) {
			return id < 30
			       || (id >= 4001 && id <= 4052)
			       || (id >= 4054 && id <= 4087)
			       || (id >= 4096 && id <= 4112)
			       || (id >= 4190 && id <= 4191)
			       || (id >= 4211 && id <= 4212)
			       || (id >= 4215 && id < 4216);
		}

		public float Progress { get; set; }
		public bool IsCancelling { get; set; }
		public bool IsCancelled { get; set; }

		public GrfHolder Grf { get; set; }

		public void FindTableErrors(List<ValidationErrorView> errors) {
			_startTask(() => _findTableErrors(errors));
		}

		private void _startTask(Action action) {
			try {
				AProgress.Init(this);
				action();
			}
			catch (OperationCanceledException) {
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
			finally {
				AProgress.Finalize(this);
			}
		}

		private void _findTableErrors(List<ValidationErrorView> errors) {
			_validateItemDb(_sde.GetMergedTable(DataSources.Item), errors);
			_validateMobDb(_sde.GetMergedTable(DataSources.Mob), errors);
		}
	}
}