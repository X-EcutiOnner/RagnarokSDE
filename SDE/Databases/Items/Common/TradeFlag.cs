using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDE.Databases.Items.Common {
	[Flags]
	[Register(typeof(TradeFlagInfo))]
	public enum TradeFlag : Int64 {
		NoDrop = 1 << 0,
		NoTrade = 1 << 1,
		TradePartner = 1 << 2,
		NoSell = 1 << 3,
		NoCart = 1 << 4,
		NoStorage = 1 << 5,
		NoGuildStorage = 1 << 6,
		NoMail = 1 << 7,
		NoAuction = 1 << 8,

		CharBound = NoDrop | NoTrade | NoStorage | NoGuildStorage | NoMail | NoAuction,
		AccountBound = NoDrop | NoTrade | NoCart | NoGuildStorage | NoMail | NoAuction,
		QuestBound = NoDrop | NoTrade | NoSell | NoCart | NoStorage | NoGuildStorage | NoMail | NoAuction,
	}

	public static class TradeFlagInfo {
		public const string Marker = "";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static TradeFlagInfo() {
			All.Add(new EnumInfoBase(TradeFlag.NoDrop, "No drop", Marker, "NoDrop") { ToolTip = "Item can't be droped" });
			All.Add(new EnumInfoBase(TradeFlag.NoTrade, "No trade", Marker, "NoTrade") { ToolTip = "Item can't be traded (nor vended)" });
			All.Add(new EnumInfoBase(TradeFlag.TradePartner, "Trade partner", Marker, "TradePartner") { ToolTip = "Wedded partner can override restriction 2." });
			All.Add(new EnumInfoBase(TradeFlag.NoSell, "No sell", Marker, "NoSell") { ToolTip = "Item can't be sold to npcs" });
			All.Add(new EnumInfoBase(TradeFlag.NoCart, "No cart", Marker, "NoCart") { ToolTip = "Item can't be placed in the cart" });
			All.Add(new EnumInfoBase(TradeFlag.NoStorage, "No storage", Marker, "NoStorage") { ToolTip = "Item can't be placed in the storage" });
			All.Add(new EnumInfoBase(TradeFlag.NoGuildStorage, "No guild storage", Marker, "NoGuildStorage") { ToolTip = "Item can't be placed in the guild storage" });
			All.Add(new EnumInfoBase(TradeFlag.NoMail, "No mail", Marker, "NoMail") { ToolTip = "Item can't be attached to mail" });
			All.Add(new EnumInfoBase(TradeFlag.NoAuction, "No auction", Marker, "NoAuction") { ToolTip = "Item can't be auctioned" });

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<TradeFlag>(All, TypeToInfo, Marker);
		}

		public static bool ProcessFlagToName(TradeFlag flag, StringBuilder builder) {
			if (flag.HasFlag(TradeFlag.TradePartner)) {

			}
			else if (flag.HasFlag(TradeFlag.CharBound)) {  // 483
				builder.Append("Char bound, ");

				if (flag.HasFlag(TradeFlag.NoSell))
					builder.Append("can't sell");
				else
					builder.Append("can sell");
				return true;
			}
			else if (flag.HasFlag(TradeFlag.AccountBound)) {   // 467
				builder.Append("Account bound, ");

				if (flag.HasFlag(TradeFlag.NoSell))
					builder.Append("can't sell");
				else
					builder.Append("can sell");
				return true;
			}
			else if (flag == TradeFlag.QuestBound) {   // 507
				builder.Append("Quest bound");
				return true;
			}

			return false;
		}
	}
}