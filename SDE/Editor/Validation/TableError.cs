using SDE.Databases;

namespace SDE.Editor.Validation {
	public class TableError : ValidationErrorView {
		public TableError(ValidationErrors type, int itemId, string message, DataSource source, DbValidationEngine validationEngine)
			: base(type, itemId, message, source, validationEngine) {
		}
	}
}