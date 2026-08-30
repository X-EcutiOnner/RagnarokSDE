using SDE.Databases.Generic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDE.Databases.Items.Common {
	[Flags]
	[Register(typeof(EquipLocationFlagInfo))]
	public enum EquipLocationFlag : Int64 {
		EQP_HEAD_LOW = 0x000001,
		EQP_HEAD_MID = 0x000200, // 512
		EQP_HEAD_TOP = 0x000100, // 256
		EQP_HAND_R = 0x000002, // 2
		EQP_HAND_L = 0x000020, // 32
		EQP_ARMOR = 0x000010, // 16
		EQP_SHOES = 0x000040, // 64
		EQP_GARMENT = 0x000004, // 4
		EQP_ACC_R = 0x000008, // 8
		EQP_ACC_L = 0x000080, // 128
		EQP_COSTUME_HEAD_TOP = 0x000400, // 1024
		EQP_COSTUME_HEAD_MID = 0x000800, // 2048
		EQP_COSTUME_HEAD_LOW = 0x001000, // 4096
		EQP_COSTUME_GARMENT = 0x002000, // 8192
										//EQP_COSTUME_FLOOR  = 0x004000, // 16384
		EQP_AMMO = 0x008000, // 32768
		EQP_SHADOW_ARMOR = 0x010000, // 65536
		EQP_SHADOW_WEAPON = 0x020000, // 131072
		EQP_SHADOW_SHIELD = 0x040000, // 262144
		EQP_SHADOW_SHOES = 0x080000, // 524288
		EQP_SHADOW_ACC_R = 0x100000, // 1048576
		EQP_SHADOW_ACC_L = 0x200000, // 2097152

		// Combined
		EQP_ACC_RL = EQP_ACC_R | EQP_ACC_L,
		EQP_SHADOW_ACC_RL = EQP_SHADOW_ACC_R | EQP_SHADOW_ACC_L,

		EQP_Both_Hand = EQP_HAND_R | EQP_HAND_L,
		EQP_Right_Hand = EQP_HAND_R,
		EQP_Left_Hand = EQP_HAND_L,
		EQP_Right_Accessory = EQP_ACC_R,
		EQP_Left_Accessory = EQP_ACC_L,
		EQP_Both_Accessory = EQP_ACC_RL,
		EQP_Shadow_Right_Accessory = EQP_SHADOW_ACC_R,
		EQP_Shadow_Left_Accessory = EQP_SHADOW_ACC_L,
	}

	public static class EquipLocationFlagInfo {
		public const string Marker = "EQP_";
		public static List<EnumInfoBase> All { get; } = new List<EnumInfoBase>();
		public static List<EnumInfoBase> AllVisibleOnly { get => All.Where(p => p.Visible).ToList(); }
		public static Dictionary<Enum, EnumInfoBase> TypeToInfo = new Dictionary<Enum, EnumInfoBase>();

		static EquipLocationFlagInfo() {
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_HEAD_TOP, "Top", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_HEAD_MID, "Middle", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_HEAD_LOW, "Lower", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_ARMOR, "Armor", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_HAND_R, "Weapon", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_HAND_L, "Shield", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_GARMENT, "Garment", Marker));

			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_SHOES, "Shoes", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_ACC_R, "Accessory right", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_ACC_L, "Accessory left", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_COSTUME_HEAD_TOP, "Costume top", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_COSTUME_HEAD_MID, "Costume middle", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_COSTUME_HEAD_LOW, "Costume lower", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_COSTUME_GARMENT, "Costume garment/robe", Marker));

			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_AMMO, "Ammo", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_SHADOW_ARMOR, "Shadow armor", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_SHADOW_WEAPON, "Shadow weapon", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_SHADOW_SHIELD, "Shadow shield", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_SHADOW_SHOES, "Shadow shoes", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_SHADOW_ACC_R, "Shadow accessory right (earring)", Marker));
			All.Add(new EnumInfoBase(EquipLocationFlag.EQP_SHADOW_ACC_L, "Shadow accessory left (pendant)", Marker));

			foreach (var info in All) {
				TypeToInfo[info.Value] = info;
			}

			EnumInfos.Add<EquipLocationFlag>(All, TypeToInfo, Marker);
		}

		public static bool ProcessFlagToName(EquipLocationFlag flag, StringBuilder builder) {
			long value = (long)flag;

			if ((flag & EquipLocationFlag.EQP_HEAD_TOP) == EquipLocationFlag.EQP_HEAD_TOP)
				builder.Append("Top, ");
			if ((flag & EquipLocationFlag.EQP_HEAD_MID) == EquipLocationFlag.EQP_HEAD_MID)
				builder.Append("Mid, ");
			if ((flag & EquipLocationFlag.EQP_HEAD_LOW) == EquipLocationFlag.EQP_HEAD_LOW)
				builder.Append("Low, ");
			if ((flag & EquipLocationFlag.EQP_SHOES) == EquipLocationFlag.EQP_SHOES)
				builder.Append("Shoes, ");

			if ((flag & EquipLocationFlag.EQP_Both_Hand) == EquipLocationFlag.EQP_Both_Hand) {
				builder.Append("Both Hand, ");
			}
			else {
				if ((flag & EquipLocationFlag.EQP_HAND_R) == EquipLocationFlag.EQP_HAND_R)
					builder.Append("Right Hand, ");
				if ((flag & EquipLocationFlag.EQP_HAND_L) == EquipLocationFlag.EQP_HAND_L)
					builder.Append("Left Hand, ");
			}

			if ((flag & EquipLocationFlag.EQP_ARMOR) == EquipLocationFlag.EQP_ARMOR)
				builder.Append("Armor, ");
			if ((flag & EquipLocationFlag.EQP_GARMENT) == EquipLocationFlag.EQP_GARMENT)
				builder.Append("Garment, ");

			if ((flag & EquipLocationFlag.EQP_Both_Accessory) == EquipLocationFlag.EQP_Both_Accessory) {
				builder.Append("Both Acc, ");
			}
			else {
				if ((flag & EquipLocationFlag.EQP_ACC_L) == EquipLocationFlag.EQP_ACC_L)
					builder.Append("L. Acc, ");
				if ((flag & EquipLocationFlag.EQP_ACC_R) == EquipLocationFlag.EQP_ACC_R)
					builder.Append("R. Acc, ");
			}

			if ((flag & EquipLocationFlag.EQP_AMMO) == EquipLocationFlag.EQP_AMMO)
				builder.Append("Ammo, ");
			if ((flag & EquipLocationFlag.EQP_COSTUME_HEAD_TOP) == EquipLocationFlag.EQP_COSTUME_HEAD_TOP)
				builder.Append("Costume Top, ");
			if ((flag & EquipLocationFlag.EQP_COSTUME_HEAD_MID) == EquipLocationFlag.EQP_COSTUME_HEAD_MID)
				builder.Append("Costume Mid, ");
			if ((flag & EquipLocationFlag.EQP_COSTUME_HEAD_LOW) == EquipLocationFlag.EQP_COSTUME_HEAD_LOW)
				builder.Append("Costume Low, ");
			if ((flag & EquipLocationFlag.EQP_COSTUME_GARMENT) == EquipLocationFlag.EQP_COSTUME_GARMENT)
				builder.Append("Costume Garment, ");
			if ((flag & EquipLocationFlag.EQP_SHADOW_ARMOR) == EquipLocationFlag.EQP_SHADOW_ARMOR)
				builder.Append("Shadow Armor, ");
			if ((flag & EquipLocationFlag.EQP_SHADOW_WEAPON) == EquipLocationFlag.EQP_SHADOW_WEAPON)
				builder.Append("Shadow Weapon, ");
			if ((flag & EquipLocationFlag.EQP_SHADOW_SHIELD) == EquipLocationFlag.EQP_SHADOW_SHIELD)
				builder.Append("Shadow Shield, ");
			if ((flag & EquipLocationFlag.EQP_SHADOW_SHOES) == EquipLocationFlag.EQP_SHADOW_SHOES)
				builder.Append("Shadow Shoes, ");
			if ((flag & EquipLocationFlag.EQP_Shadow_Right_Accessory) == EquipLocationFlag.EQP_Shadow_Right_Accessory) {
				builder.Append("Shadow Both Acc, ");
			}
			else {
				if ((flag & EquipLocationFlag.EQP_SHADOW_ACC_R) == EquipLocationFlag.EQP_SHADOW_ACC_R)
					builder.Append("Shadow R. Acc, ");
				if ((flag & EquipLocationFlag.EQP_SHADOW_ACC_L) == EquipLocationFlag.EQP_SHADOW_ACC_L)
					builder.Append("Shadow L. Acc, ");
			}

			return true;
		}
	}
}
