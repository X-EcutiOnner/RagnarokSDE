using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.Parsers.Generic;
using SDE.Editor.Parsers;
using System;
using System.IO;
using Utilities.Extension;

namespace SDE.Databases.Items.Parser {
	public class ItemReaderCsv : DatabaseReaderCsv<int> {
		public override void Loader(DbLoadContext context, BaseDatabase db) {
			bool isImport = context.Source != DataSources.Item;
			
			LoadItem(db, isImport);
			LoadItemDelay(db, isImport);
			LoadItemBuyingStore(db, isImport);
			LoadItemFlag(db, isImport);
			LoadItemAvail(db, isImport);
			LoadItemNoUse(db, isImport);
			LoadItemStack(db, isImport);
			LoadItemTrade(db, isImport);
		}

		public void LoadItem(BaseDatabase db, bool isImport) => Load(db, DataSources.Item, isImport, ReadEntry);
		public void LoadItemDelay(BaseDatabase db, bool isImport) => Load(db, DataSources.ItemDelay, isImport, ReadEntryDelay);
		public void LoadItemBuyingStore(BaseDatabase db, bool isImport) => Load(db, DataSources.ItemsBuyingStore, isImport, ReadEntryBuyingStore);
		public void LoadItemFlag(BaseDatabase db, bool isImport) => Load(db, DataSources.ItemFlag, isImport, ReadEntryItemFlag);
		public void LoadItemAvail(BaseDatabase db, bool isImport) => Load(db, DataSources.ItemAvail, isImport, ReadEntryItemAvail);
		public void LoadItemNoUse(BaseDatabase db, bool isImport) => Load(db, DataSources.ItemNoUse, isImport, ReadEntryNoUse);
		public void LoadItemStack(BaseDatabase db, bool isImport) => Load(db, DataSources.ItemStack, isImport, ReadEntryItemStack);
		public void LoadItemTrade(BaseDatabase db, bool isImport) => Load(db, DataSources.ItemTrade, isImport, ReadEntryTrade);

		public void Load(BaseDatabase db, DataSource source, bool isImport, Action<DbLoadContext, string[]> readEntryFunc) {
			var context = new DbLoadContext(db);
			
			if (isImport)
				source = source.ImportTable;

			if (!context.PrepareRead(source)) return;

			if (!File.Exists(context.FilePath)) {
				if (db.ThrowFileNotFoundException)
					DbIOErrorHandler.FileNotFound(db.Source);

				return;
			}

			foreach (string[] elements in TextFileHelper.GetElementsByCommasAll(File.ReadAllBytes(context.FilePath))) {
				try {
					readEntryFunc(context, elements);
				}
				catch {
					if (elements.Length <= 0) {
						if (!context.ReportIdException("#")) return;
					}
					else if (!context.ReportIdException(elements[0])) return;
				}
			}
		}

		public void ReadEntryDelay(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			model.Delay = elements[1];

			if (elements.Length < 3)
				return;

			model.DelayStatus = elements[2].ReplaceFirst("SC_", "");
		}

		public void ReadEntryBuyingStore(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			model.Flags = ((long)(model.Flags.ToFlag<ItemFlag>() | ItemFlag.BuyingStore)).ToString();
		}

		public void ReadEntryItemFlag(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			var flags = elements[1].ToLong();
			var modelFlag = model.Flags.ToFlag<ItemFlag>();

			if ((flags & 1) != 0)
				modelFlag |= ItemFlag.DeadBranch;
			if ((flags & 2) != 0)
				modelFlag |= ItemFlag.Container;
			if ((flags & 4) != 0)
				modelFlag |= ItemFlag.UniqueId;
			if ((flags & 8) != 0)
				modelFlag |= ItemFlag.BindOnEquip;
			if ((flags & 16) != 0)
				modelFlag |= ItemFlag.DropAnnounce;
			if ((flags & 32) != 0)
				modelFlag |= ItemFlag.NoConsume;

			model.Flags = ((long)modelFlag).ToString();
		}

		public void ReadEntryItemAvail(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);
			model.AliasName = elements[1];
		}

		public void ReadEntryNoUse(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			var flag = elements[1].ToFlag<NoUseFlag>();
			model.NoUseFlags = ((long)(model.TradeFlags.ToFlag<NoUseFlag>() | flag)).ToString();

			LoadField(ref model.NoUseOverride, elements, 2);
		}

		public void ReadEntryItemStack(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			model.StackAmount = elements[1];

			if (elements.Length < 3)
				return;

			var flag = elements[2].ToFlag<ItemStackFlag>();
			model.StackFlags = ((long)(model.StackFlags.ToFlag<ItemStackFlag>() | flag)).ToString();
		}

		public void ReadEntryTrade(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);

			var flag = elements[1].ToFlag<TradeFlag>();
			model.TradeFlags = ((long)(model.TradeFlags.ToFlag<TradeFlag>() | flag)).ToString();

			LoadField(ref model.TradeOverride, elements, 2);
		}

		public override void ReadEntry(DbLoadContext context, string[] elements) {
			var model = SafeLoadModel(context, elements);
			
			if (elements.Length < 22)
				return;
			
			int eleIdx = 1;
			string e;
			model.AegisName = elements[eleIdx++];
			model.Name = elements[eleIdx++];
			model.Type = (ItemType)elements[eleIdx++].ToInt();
			model.Buy = elements[eleIdx++];
			model.Sell = elements[eleIdx++];
			model.Weight = elements[eleIdx++];

			var atkMatk = elements[eleIdx++].Split(':');
			model.Attack = atkMatk[0];
			model.MagicAttack = atkMatk.Length > 1 ? atkMatk[1] : "";
			model.Defense = elements[eleIdx++];
			model.Range = elements[eleIdx++];
			model.Slots = elements[eleIdx++];
			model.Jobs = elements[eleIdx++];

			int jobAthenaId = model.Jobs.ToInt();
			model.Jobs = "0x" + JobOperations.CsvFlag2SdeFlag(jobAthenaId).ToString("X16");
			model.Classes = elements[eleIdx++];
			e = elements[eleIdx++];
			if (String.IsNullOrEmpty(e))
				model.Gender = GenderType.SEX_BOTH;
			else
				model.Gender = (GenderType)e.ToInt();
			model.Locations = elements[eleIdx++];
			model.WeaponLevel = elements[eleIdx++];

			e = elements[eleIdx++];
			if (!String.IsNullOrEmpty(e)) {
				var equipLevelMinMax = e.Split(':');
				model.EquipLevelMin = equipLevelMinMax[0];
				model.EquipLevelMax = equipLevelMinMax.Length > 1 ? equipLevelMinMax[1] : "";
			}

			model.Refineable = elements[eleIdx++] == "1" ? true : false;
			model.View = elements[eleIdx++];
			model.Script = DbReader.FromScript(elements[eleIdx++]);
			model.EquipScript = DbReader.FromScript(elements[eleIdx++]);
			model.UnEquipScript = DbReader.FromScript(elements[eleIdx++]);
		}

		private Item SafeLoadModel(DbLoadContext context, string[] elements) {
			int id = int.Parse(elements[0]);

			var table = context.AbsractDb.Table;
			table.EnsureExists(id);
			var tuple = table.GetTuple(id);
			return tuple.GetModel<Item>();
		}
	}
}
