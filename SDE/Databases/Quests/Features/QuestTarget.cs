using SDE.Core;
using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Quests.Features {
	public class QuestTarget : ICloneable {
		public string Mob;
		public string Count;
		public string Id;
		public RaceType Race = RaceType.RC_ALL;
		public SizeType Size = SizeType.SZ_ALL;
		public ElementType Element = ElementType.ELE_ALL;
		public string MinLevel;
		public string MaxLevel;
		public string Location;
		public string MapName;
		public List<MapMobTarget> MapMobTargets = new List<MapMobTarget>();

		public object Clone() {
			return new QuestTarget {
				Mob = Mob,
				Count = Count,
				Id = Id,
				Race = Race,
				Size = Size,
				Element = Element,
				MinLevel = MinLevel,
				MaxLevel = MaxLevel,
				Location = Location,
				MapName = MapName,
				MapMobTargets = MapMobTargets.Select(x => (MapMobTarget)x.Clone()).ToList()
			};
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<QuestTarget>.Equals(this, (QuestTarget)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<QuestTarget>.GetHashCode(this);
		}
	}
}
