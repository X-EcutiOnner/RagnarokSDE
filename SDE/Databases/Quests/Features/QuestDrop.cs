using SDE.Core;
using System;

namespace SDE.Databases.Quests.Features {
	public class QuestDrop : ICloneable {
		public string Mob = "0";
		public string Item;
		public string Count = "1";
		public string Rate;

		public object Clone() {
			return new QuestDrop {
				Mob = Mob,
				Item = Item,
				Count = Count,
				Rate = Rate
			};
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<QuestDrop>.Equals(this, (QuestDrop)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<QuestDrop>.GetHashCode(this);
		}
	}
}
