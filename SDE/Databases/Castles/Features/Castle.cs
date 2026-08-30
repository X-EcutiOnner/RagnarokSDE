using SDE.Databases.Castles.Common;
using System;

namespace SDE.Databases.Castles.Features {
	public class Castle : ICloneable {
		public string Map;
		public string Name;
		public string Npc;
		public CastleType Type = CastleType.WOE_FIRST_EDITION;
		public string ClientId;
		public bool WarpEnabled;
		public string WarpX;
		public string WarpY;
		public string WarpCost = "100";
		public string WarpCostSiege = "100000";

		public object Clone() {
			return MemberwiseClone();
		}
	}
}
