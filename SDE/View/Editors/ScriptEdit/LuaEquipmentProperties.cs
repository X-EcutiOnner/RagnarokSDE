using Lua.Structure;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.ItemCombos.Features;
using SDE.Editor;
using SDE.View.Editors.ScriptEdit.Athena;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Services;

namespace SDE.View.Editors.ScriptEdit {
	public class AmmoStat {
		public int Element;
		public int Buy;

		public AmmoStat(LList list) {
			Element = int.Parse(((LStringValue)list.Variables[0]).Value);
			Buy = int.Parse(((LStringValue)list.Variables[1]).Value);
		}
	}

	public class ArmorStat {
		public int Defense;
		public int Str;
		public int Int;
		public int Vit;
		public int Dex;
		public int Agi;
		public int Luk;
		public int Unknown;
		public int Unknown2;
		public int Mdef;
		public int ArmorLevel;

		public ArmorStat(LList list) {
			int idx = 0;
			Defense = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Str = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Int = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Vit = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Dex = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Agi = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Luk = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			idx++;
			idx++;
			Mdef = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			ArmorLevel = int.Parse(((LStringValue)list.Variables[idx++]).Value);
		}
	}

	public class MWeaponStat {
		public int Element;
		public int WeaponSubType;
		public int Attack;
		public int MagicAttack;
		public int Str;
		public int Int;
		public int Vit;
		public int Dex;
		public int Agi;
		public int Luk;
		public int WeaponLevel;

		public MWeaponStat(LList list) {
			int idx = 0;
			Element = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			WeaponSubType = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Attack = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			MagicAttack = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Str = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Int = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Vit = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Dex = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Agi = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Luk = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			WeaponLevel = int.Parse(((LStringValue)list.Variables[idx++]).Value);
		}
	}

	public class RWeaponStat {
		public int WeaponSubType;
		public int Attack;
		public int Str;
		public int Int;
		public int Vit;
		public int Dex;
		public int Agi;
		public int Luk;
		public int WeaponLevel;

		public RWeaponStat(LList list) {
			int idx = 0;
			WeaponSubType = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Attack = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Str = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Int = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Vit = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Dex = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Agi = int.Parse(((LStringValue)list.Variables[idx++]).Value);
			Luk = int.Parse(((LStringValue)list.Variables[idx++]).Value);
		}
	}

	public static class LuaEquipmentProperties {
		private static bool _eventSet = false;
		private static LList _items;
		private static Dictionary<long, LList> _itemCombos = new Dictionary<long, LList>();

		/// <summary>
		/// Tries the get equipment properties entry for an item.
		/// </summary>
		/// <param name="itemId">The item id.</param>
		/// <returns>The EquipmentProperties entry for the specified item id.</returns>
		public static LList TryGetEquipmentEntry(int itemId) {
			CacheEquipmentProperties();

			var key = itemId.ToString();
			var entry = _items[key];

			if (!(entry is LList entryValue)) {
				return null;
			}

			return entryValue;
		}

		/// <summary>
		/// Tries the get equipment properties entry for an item combo.
		/// </summary>
		/// <param name="itemId">The item combo unique id.</param>
		/// <returns>The EquipmentProperties entry for the specified item combo id.</returns>
		public static LList TryGetEquipmentEntry(List<NameId> nameIds) {
			CacheEquipmentProperties();

			long uid = ItemCombo.ToUniqueId(nameIds);

			_itemCombos.TryGetValue(uid, out var lItemCombo);
			return lItemCombo;
		}

		private static void CacheEquipmentProperties() {
			if (!_eventSet) {
				SdeEditor.Project.Reloaded += _project_Reloaded;
				_eventSet = true;
			}

			if (_items == null) {
				var data = SdeEditor.Project.MetaGrf.GetData(ProjectConfiguration.ClientEquipmentProperties);

				if (data == null)
					throw new Exception("Failed to load file: '" + ProjectConfiguration.ClientEquipmentProperties + "'.");

				var equipment = new Lua.Parser(data).Parse(EncodingService.Ansi);

				var items = equipment["Item"] as LList;
				_items = items;

				var comboItems = equipment["Combiitem"] as LList;

				foreach (LKeyValue itemEntry in comboItems) {
					if (((LList)itemEntry.Value)["Item"] is LList itemComboList) {
						var uid = ItemCombo.ToUniqueId(itemComboList.Variables.Select(p => new NameId(((LStringValue)p).Value)).ToList());

						_itemCombos[uid] = itemEntry.Value as LList;
					}
				}
			}
		}

		private static void _project_Reloaded(object sender) {
			_items = null;
			_itemCombos.Clear();
		}

		public static string CreateAthenaScript(ItemCombo combo) {
			var entry = TryGetEquipmentEntry(combo.NameIds);

			if (entry == null)
				return null;

			return CreateAthenaScript(entry);
		}

		public static string CreateAthenaScript(int itemId) {
			var entry = TryGetEquipmentEntry(itemId);
			
			if (entry == null)
				return null;

			return CreateAthenaScript(entry);
		}

		public static string CreateAthenaScript(LList equipmentEntry) {
			if (equipmentEntry == null)
				return null;

			var function = equipmentEntry["OnStartEquip"] as LFunction;
			var lStats = equipmentEntry["Stat"] as LList;

			if (function == null && lStats == null)
				return null;

			string script = "";

			if (function != null)
				script = _createItemScriptFromEquipment(function.Value);

			// Add stats
			if (lStats != null && equipmentEntry["Type"] is LStringValue lType) {
				var stats = GetScriptStats(lStats, lType);

				if (stats != "") {
					if (script != null && script.Length > 0)
						script = stats + " " + script;
					else
						script = stats + script;
				}
			}

			return DbWriter.AutoFormatScript(script);
		}

		public static string GetScriptStats(LList lStats, LStringValue lType) {
			StringBuilder b = new StringBuilder();
			var type = lType.Value;

			switch (type.ToLowerInvariant()) {
				case "ammo":
					var ammo = new AmmoStat(lStats);
					if (ammo.Element > 0)
						b.Append("bonus bAtkEle," + LuaAstToAthenaAst.ArgumentToType<ElementType>(ammo.Element, "Ele_") + ";");
					break;
				case "armor":
					var armor = new ArmorStat(lStats);
					if (armor.Str > 0) b.Append($"bonus bStr,{armor.Str};");
					if (armor.Int > 0) b.Append($"bonus bInt,{armor.Int};");
					if (armor.Vit > 0) b.Append($"bonus bVit,{armor.Vit};");
					if (armor.Dex > 0) b.Append($"bonus bDex,{armor.Dex};");
					if (armor.Agi > 0) b.Append($"bonus bAgi,{armor.Agi};");
					if (armor.Luk > 0) b.Append($"bonus bLuk,{armor.Luk};");
					if (armor.Mdef > 0) b.Append($"bonus bMdef,{armor.Mdef};");
					break;
				case "mweapon":
					var mweapon = new MWeaponStat(lStats);
					if (mweapon.Element > 0) b.Append("bonus bAtkEle," + LuaAstToAthenaAst.ArgumentToType<ElementType>(mweapon.Element, "Ele_") + ";");
					if (mweapon.Str > 0) b.Append($"bonus bStr,{mweapon.Str};");
					if (mweapon.Int > 0) b.Append($"bonus bInt,{mweapon.Int};");
					if (mweapon.Vit > 0) b.Append($"bonus bVit,{mweapon.Vit};");
					if (mweapon.Dex > 0) b.Append($"bonus bDex,{mweapon.Dex};");
					if (mweapon.Agi > 0) b.Append($"bonus bAgi,{mweapon.Agi};");
					if (mweapon.Luk > 0) b.Append($"bonus bLuk,{mweapon.Luk};");
					break;
				case "rweapon":
					var rweapon = new RWeaponStat(lStats);
					if (rweapon.Str > 0) b.Append($"bonus bStr,{rweapon.Str};");
					if (rweapon.Int > 0) b.Append($"bonus bInt,{rweapon.Int};");
					if (rweapon.Vit > 0) b.Append($"bonus bVit,{rweapon.Vit};");
					if (rweapon.Dex > 0) b.Append($"bonus bDex,{rweapon.Dex};");
					if (rweapon.Agi > 0) b.Append($"bonus bAgi,{rweapon.Agi};");
					if (rweapon.Luk > 0) b.Append($"bonus bLuk,{rweapon.Luk};");
					break;
			}

			return b.ToString();
		}

		private static string _createItemScriptFromEquipment(string script) {
			try {
				return AthenaAstWriter.ToScript(script);
			}
			catch {
				return null;
			}
		}
	}
}
