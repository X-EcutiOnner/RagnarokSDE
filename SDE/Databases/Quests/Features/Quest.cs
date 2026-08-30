using SDE.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Quests.Features {
	public class Quest : ICloneable {
		public string Title;
		public string TimeLimit;

		public List<QuestTarget> Targets = new List<QuestTarget>();
		public List<QuestDrop> Drops = new List<QuestDrop>();

		public object Clone() {
			return new Quest {
				Title = Title,
				TimeLimit = TimeLimit,
				Targets = Targets.Select(x => (QuestTarget)x.Clone()).ToList(),
				Drops = Drops.Select(x => (QuestDrop)x.Clone()).ToList()
			};
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<Quest>.Equals(this, (Quest)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<Quest>.GetHashCode(this);
		}
	}
}
