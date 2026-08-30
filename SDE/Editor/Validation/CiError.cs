using SDE.Databases;

namespace SDE.Editor.Validation {
	public class CiError : ValidationErrorView {
		public CiError(ValidationErrors type, int itemId, string message, DataSource source, DbValidationEngine validationEngine) : base(type, itemId, message, source, validationEngine) {
		}
	}
}