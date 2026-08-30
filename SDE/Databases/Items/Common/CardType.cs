using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Items.Common {
	[Register(typeof(CardTypeInfo))]
	public enum CardType {
		CARD_NORMAL = 0,
		CARD_ENCHANT,
		MAX_CARD_TYPE
	}

	public static class CardTypeInfo {
		public const string Marker = "CARD_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static CardTypeInfo() {
			All.Add(new EnumInfoBase(CardType.CARD_NORMAL, "Normal", Marker));
			All.Add(new EnumInfoBase(CardType.CARD_ENCHANT, "Enchant", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<CardType>(All, TypeToInfo, Marker);
		}
	}
}
