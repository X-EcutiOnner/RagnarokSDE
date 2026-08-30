
using System;
using System.Collections.Generic;

namespace SDE.Databases.Generic.SearchDescriptors {
	public class SearchField {
		public string DisplayName { get; set; }
		public bool IsActive { get; set; }
		public bool IsTuple { get; set; }
		public Type EnumType { get; set; }
		public Enum ActiveEnum { get; set; }
		public Func<object, object> Getter;
	}

	public class SearchDescriptor {
		public List<SearchField> Fields = new List<SearchField>();

		public void AddTuple(Func<object, object> getter, string displayName, bool isActive = false) {
			Fields.Add(new SearchField() { Getter = getter, DisplayName = displayName, IsActive = isActive, IsTuple = true });
		}

		public void Add(Func<object, object> getter, string displayName, bool isActive = false) {
			Fields.Add(new SearchField() { Getter = getter, DisplayName = displayName, IsActive = isActive });
		}

		public void Add<TEnum>(Func<object, object> getter, string displayName, bool isActive = false) where TEnum : struct, Enum {
			Fields.Add(new SearchField() { Getter = getter, DisplayName = displayName, IsActive = isActive, EnumType = typeof(TEnum) });
		}

		public void AddSpacer() {
			Fields.Add(null);
		}
	}
}
