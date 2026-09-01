using System;
using System.Collections.Generic;

namespace SDE.Editor.Validation {
	public class ValidationCommand {
		protected bool Equals(ValidationCommand other) {
			return string.Equals(CmdName, other.CmdName) && string.Equals(Icon, other.Icon);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ValidationCommand)obj);
		}

		public override int GetHashCode() {
			unchecked {
				return (CmdName != null ? CmdName.GetHashCode() : 0) * 397 ^ (Icon != null ? Icon.GetHashCode() : 0);
			}
		}

		public string DisplayName { get; set; }
		public string CmdName { get; set; }
		public string Icon { get; set; }
		public bool GroupCommand { get; set; }

		public bool Execute(ValidationErrorView error, List<ValidationErrorView> errors) {
			if (CanExecute == null) {
				return GroupCommand ? _executeGroup(error, errors) : _execute(error);
			}

			if (CanExecute(error)) {
				return GroupCommand ? _executeGroup(error, errors) : _execute(error);
			}

			return true;
		}

		public Func<ValidationErrorView, bool> _execute { get; set; }
		public Func<ValidationErrorView, List<ValidationErrorView>, bool> _executeGroup { get; set; }

		public Func<ValidationErrorView, bool> CanExecute { get; set; }
	}
}
