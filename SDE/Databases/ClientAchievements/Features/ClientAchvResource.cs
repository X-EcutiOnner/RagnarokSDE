using System;

namespace SDE.Databases.ClientAchievements.Features {
	public class ClientAchvResource : ICloneable {
		public string Id;
		public string Text;
		public string Count;
		public string Shortcut;

		public object Clone() {
			return MemberwiseClone();
		}
	}
}
