using System.Collections.Generic;
using SDE.Databases;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Editor.Database;
using SDE.Editor.LuaTables;
using Utilities.Extension;

namespace SDE.Editor.Validation {
	public static class TableRepairs {
		public static void WeaponViewIdRepair(ProjectManager sdb, BaseDatabase currentDb) {
			var itemDb = sdb.GetMergedTable(DataSources.Item);
			var itemDb1 = sdb.GetDb(DataSources.Item);
			var itemDb2 = sdb.GetDb(DataSources.ItemImport);

			string error;
			Dictionary<int, string> dico;

			if (LuaHelper.GetIdToSpriteTable(LuaHelper.ViewIdTypes.Weapon, out dico, out error)) {
			}

			foreach (var tuple in itemDb1.Table.FastItems) {
				var model = tuple.GetModel<Item>();

				if (model.Type == ItemType.IT_WEAPON) {
					int viewId = model.View.ToInt();
				}
			}
		}
	}

	public class WeaponViewIdRepair : IRepair {
		#region IRepair Members
		public string ImagePath {
			get { return "warning16.png"; }
			set { }
		}

		public string DisplayName { get; set; }

		public bool Show(BaseDatabase db) {
			return true;
		}

		public bool CanRepair(BaseDatabase db) {
			return true;
		}

		public bool Repair(BaseDatabase db) {
			return true;
		}
		#endregion
	}
}