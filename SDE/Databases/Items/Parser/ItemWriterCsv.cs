using GRF.IO;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.Editor.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilities;
using Utilities.Extension;

namespace SDE.Databases.Items.Parser {
	public class ItemWriterCsv : DatabaseWriterCsv {
		public override string KeyField => "Id";
		public override bool SplitDatabaseFiles => true;

		public override void Writer(DbSaveContext context, BaseDatabase db) {
			bool isImport = context.Source != DataSources.Item;

			WriteItem(db, isImport);
			WriteItemDelay(db, isImport);
			WriteItemBuyingStore(db, isImport);
			WriteItemFlag(db, isImport);
			WriteItemAvail(db, isImport);
			WriteItemNoUse(db, isImport);
			WriteItemStack(db, isImport);
			WriteItemTrade(db, isImport);
		}

		public void WriteItem(BaseDatabase db, bool isImport) => Write(db, DataSources.Item, isImport, WriteEntry);
		public void WriteItemDelay(BaseDatabase db, bool isImport) => Write(db, DataSources.ItemDelay, isImport, WriteEntryDelay);
		public void WriteItemBuyingStore(BaseDatabase db, bool isImport) => Write(db, DataSources.ItemsBuyingStore, isImport, WriteEntryBuyingStore);
		public void WriteItemFlag(BaseDatabase db, bool isImport) => Write(db, DataSources.ItemFlag, isImport, WriteEntryItemFlag);
		public void WriteItemAvail(BaseDatabase db, bool isImport) => Write(db, DataSources.ItemAvail, isImport, WriteEntryItemAvail);
		public void WriteItemNoUse(BaseDatabase db, bool isImport) => Write(db, DataSources.ItemNoUse, isImport, WriteEntryNoUse);
		public void WriteItemStack(BaseDatabase db, bool isImport) => Write(db, DataSources.ItemStack, isImport, WriteEntryItemStack);
		public void WriteItemTrade(BaseDatabase db, bool isImport) => Write(db, DataSources.ItemTrade, isImport, WriteEntryTrade);

		public void Write(BaseDatabase db, DataSource source, bool isImport, Func<ReadableTuple, string> writeEntryFunc) {
			var context = new DbSaveContext(db);

			if (isImport)
				source = source.ImportTable;

			// Create file if it doesn't exist
			TkPath path = DbPathLocator.DetectPath(source);
			string dbPath = ProjectConfiguration.DatabaseDbPath;
			string subPath = ProjectConfiguration.DatabaseSubDbPath;

			if (path == null || !path.IsFile) {
				var sourcePathCsv = source.Paths.First(p => p.IsExtension(".txt"));
				string filePath = GrfPath.Combine(dbPath, sourcePathCsv.Replace("{DBPATH}", subPath));
				File.WriteAllBytes(filePath, new byte[] { });
				DbPathLocator.StoreFile(filePath);
			}

			if (context.PrepareWrite(source) != SaveContextState.Valid) return;

			CsvWriter lines = new CsvWriter(context.OldPath, useUniqueId: db.Table.UseUniqueId, fileKeyRef: FileKeyRef);
			lines.Remove(db);

			foreach (ReadableTuple tuple in db.Table.FastItems.Where(p => !p.Normal).OrderBy(p => p.GetKey<int>())) {
			//foreach (ReadableTuple tuple in db.Table.FastItems.OrderBy(p => p.GetKey<int>())) {
				int key = tuple.GetKey<int>();

				string line = writeEntryFunc(tuple);

				if (line == null)
					lines.Delete(key);
				else
					lines.Write(key, line);
			}

			lines.WriteFile(context.FilePath);
		}

		public string WriteEntryDelay(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();

			if (model.Delay.ToInt() == 0)
				return null;

			var output = new List<string>();
			output.Add(tuple.Key.ToString());
			output.Add(model.Delay);

			if (!String.IsNullOrEmpty(model.DelayStatus)) {
				var status = model.DelayStatus.ToUpper();

				if (!status.StartsWith("SC_"))
					status = "SC_" + status;
				
				output.Add(status);
			}

			return string.Join(",", output) + "\t//" + model.AegisName;
		}

		public string WriteEntryBuyingStore(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();

			if ((model.Flags.ToFlag<ItemFlag>() & ItemFlag.BuyingStore) == 0)
				return null;

			return tuple.Key.ToString() + "\t//" + model.AegisName;
		}

		public string WriteEntryItemFlag(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();
			var flag = model.Flags.ToFlag<ItemFlag>();

			if ((flag & (ItemFlag.DeadBranch | ItemFlag.Container | ItemFlag.UniqueId | ItemFlag.BindOnEquip | ItemFlag.DropAnnounce | ItemFlag.NoConsume)) == 0)
				return null;

			var longFlag = ((long)flag) >> 1;
			
			return tuple.Key.ToString() + "," + longFlag + " //" + model.AegisName;
		}

		public string WriteEntryItemAvail(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();

			if (String.IsNullOrEmpty(model.AliasName))
				return null;

			return tuple.Key.ToString() + "," + CachedDbs.AegisNameItem.ToStringId(model.AliasName) + "\t// " + model.AegisName + " > " + CachedDbs.AegisNameItem.ToStringId(model.AliasName);
		}

		public string WriteEntryNoUse(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();

			if (model.NoUseFlags.ToLong() == 0)
				return null;

			return tuple.Key.ToString() + "," + model.NoUseFlags.ToLong() + "," + model.NoUseOverride.ToInt() + "\t// " + model.AegisName;
		}

		public string WriteEntryItemStack(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();

			if (model.StackAmount.ToInt() == 0)
				return null;

			return tuple.Key.ToString() + "," + model.StackAmount.ToInt() + "," + model.StackFlags.ToLong() + "\t// " + model.Name;
		}

		public string WriteEntryTrade(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();

			if (model.TradeFlags.ToLong() == 0)
				return null;

			return tuple.Key.ToString() + "," + model.TradeFlags.ToLong() + "," + model.TradeOverride.ToInt() + "\t// " + model.AegisName;
		}

		public override string WriteEntry(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();
			int jobs = JobOperations.SdeFlag2CsvFlag(model.Jobs.ToULong());
			string gender = ((int)model.Gender).ToString();
			string refinable = model.Refineable ? "1" : "";

			switch (model.Type) {
				case ItemType.IT_CARD:
				case ItemType.IT_ETC:
					gender = "";
					break;
				case ItemType.IT_WEAPON:
				case ItemType.IT_ARMOR:
					if (refinable == "")
						refinable = "0";
					break;
			}

			var output = new string[] {
				tuple.Key.ToString(),
				DbWriter.SetEmptyDefault(model.AegisName),
				DbWriter.SetEmptyDefault(model.Name),
				((int)model.Type).ToString(),
				DbWriter.SetEmptyDefault(model.Buy),
				DbWriter.SetEmptyDefault(model.Sell),
				DbWriter.SetEmptyDefault(model.Weight),
				WriteSplitField(model.Attack, model.MagicAttack),
				DbWriter.SetEmptyDefault(model.Defense),
				DbWriter.SetEmptyDefault(model.Range),
				DbWriter.SetEmptyDefault(model.Slots),
				jobs == 0 ? "" : "0x" + jobs.ToString("X8"),
				DbWriter.SetEmptyDefault(model.Classes),
				gender,
				DbWriter.SetEmptyDefault(model.Locations),
				DbWriter.SetEmptyDefault(model.WeaponLevel),
				WriteSplitField(model.EquipLevelMin, model.EquipLevelMax),
				refinable,
				DbWriter.SetEmptyDefault(model.View),
				DbWriter.SetTextScript(model.Script),
				DbWriter.SetTextScript(model.EquipScript),
				DbWriter.SetTextScript(model.UnEquipScript)
			};

			return string.Join(",", output);
		}

		public string WriteSplitField(string fieldLeft, string fieldRight) {
			string output = "";

			if (!String.IsNullOrEmpty(fieldRight))
				output = ":" + fieldRight;
			if (!String.IsNullOrEmpty(fieldLeft))
				output = fieldLeft + output;
			if (output.StartsWith(":"))
				output = "0" + output;

			return output;
		}
	}
}
