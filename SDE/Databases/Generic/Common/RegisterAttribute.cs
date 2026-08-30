using System;

namespace SDE.Databases.Generic.Common {
	[AttributeUsage(AttributeTargets.Enum)]
	public class RegisterAttribute : Attribute {
		public Type TargetClass { get; }

		public RegisterAttribute(Type targetClass) {
			TargetClass = targetClass;
		}
	}
}
