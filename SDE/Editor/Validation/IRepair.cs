using SDE.Editor.Database;

namespace SDE.Editor.Validation {
	public static class RepairHelper {
		#region Delegates
		public delegate bool RepairActionDelegate(BaseDatabase db);
		#endregion
	}

	public interface IRepair {
		string ImagePath { get; set; }
		string DisplayName { get; set; }
		bool Show(BaseDatabase db);
		bool CanRepair(BaseDatabase db);
		bool Repair(BaseDatabase db);
	}
}