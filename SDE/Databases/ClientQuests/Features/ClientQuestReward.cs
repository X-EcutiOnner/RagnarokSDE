using System;

namespace SDE.Databases.ClientQuests.Features {
	public class ClientQuestReward : ICloneable {
		public string Item;
		public string Count = "1";

		public object Clone() {
			return MemberwiseClone();
		}
	}
}
