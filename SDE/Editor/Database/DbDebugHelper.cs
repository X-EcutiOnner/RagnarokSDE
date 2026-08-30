using SDE.Databases;

namespace SDE.Editor.Database {
	public static class DbDebugHelper {
		#region Delegates
		public delegate void DbEventHandler(object sender, DataSource source, string subFile, BaseDatabase db);
		public delegate void DbUpdateEventHandler(object sender, string message);
		public delegate void DbWriteUpdateEventHandler(object sender, DataSource source, string subFile, BaseDatabase db, string message);
		#endregion

		public static event DbEventHandler Saved;
		public static event DbEventHandler Loaded;
		public static event DbEventHandler Cleared;
		public static event DbUpdateEventHandler Update;
		public static event DbWriteUpdateEventHandler Update2;
		public static event DbEventHandler ExceptionThrown;
		public static event DbEventHandler StoppedLoading;
		public static event DbWriteUpdateEventHandler WriteStatusUpdate;

		public static void OnSaved(DataSource source, string subfile, BaseDatabase db) => Saved?.Invoke(null, source, subfile, db);
		public static void OnLoaded(DataSource source, string subfile, BaseDatabase db) => Loaded?.Invoke(null, source, subfile, db);
		public static void OnUpdate(string message) => Update?.Invoke(null, message);
		public static void OnUpdate(DataSource source, string subfile, string message) => Update2?.Invoke(null, source, subfile, null, message);
		public static void OnCleared(DataSource source, string subfile, BaseDatabase db) => Cleared?.Invoke(null, source, subfile, db);
		public static void OnWriteStatusUpdate(DataSource source, string subfile, BaseDatabase db, string message) => WriteStatusUpdate?.Invoke(null, source, subfile, db, message);
		public static void OnStoppedLoading(DataSource source, string subfile, BaseDatabase db) => StoppedLoading?.Invoke(null, source, subfile, db);
		public static void OnExceptionThrown(DataSource source, string subfile, BaseDatabase db) => ExceptionThrown?.Invoke(null, source, subfile, db);

		public static void DetachEvents() {
			Saved = null;
			Loaded = null;
			Cleared = null;
			Update = null;
			Update2 = null;
			ExceptionThrown = null;
			StoppedLoading = null;
			WriteStatusUpdate = null;
		}
	}
}