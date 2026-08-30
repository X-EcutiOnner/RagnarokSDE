using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GRF.FileFormats.LubFormat;
using GRF.IO;
using GRF.Threading;
using Lua;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.ClientItems.Common;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Editor.Items;
using SDE.Editor.LuaTables;
using Utilities;
using Utilities.Extension;
using Utilities.Services;

namespace SDE.Editor.Validation {
	public partial class DbValidationEngine {
		public void FindClientItemErrors(List<ValidationErrorView> errors) {
			_startTask(() => _findClientItemErrors(errors));
		}

		private void _findClientItemErrors(List<ValidationErrorView> errors) {
			var itemDb = _sde.GetMergedTable(DataSources.Item);
			var citemDb = _sde.GetMergedTable(DataSources.ClientItem);

			int current = 0;
			int totalEntries = citemDb.FastItems.Count;

			Dictionary<int, int> viewIdToWepClass = _getWeaponClasses();

			//Expansion_Weapon_IDs
			foreach (var citem in citemDb.FastItems) {
				var clientItem = citem.GetModel<ClientItem>();
				AProgress.IsCancelling(this);
				Progress = (float)current / totalEntries * 100f;
				current++;

				var sitem = itemDb.TryGetTuple(citem.Key);

				if (sitem == null)
					continue;

				var server = sitem.GetModel<Item>();
				var itemType = server.Type;

				var parameters = new ParameterHolder(clientItem);

				if (parameters != null) {
					foreach (var param in ParameterHolder.KnownItemParameters) {
						try {
							if (parameters.Values.ContainsKey(param)) {
								var value = parameters.Values[param];

								if (param == ParameterHolderKeys.Class && SdeAppConfiguration.VaCiClass) {
									var sViewId = server.View.ToInt();
									var location = (int)server.Locations.ToLong();

									int cViewId;

									if (server.Type == ItemType.IT_WEAPON && String.IsNullOrEmpty(server.View))
										sViewId = (int)server.WeaponType;

									switch (itemType) {
										case ItemType.IT_WEAPON:
											var twoHanded = (location & 34) == 34;
											var isWeaponLocation = (location & 2) != 0;

											if (!isWeaponLocation) {
												errors.Add(new CiError(ValidationErrors.CiItemClassUnknown, citem.Key, "ItemLocation: server Location requires the 'Weapon' flag.", DataSources.Item, this));
											}

											ItemGeneratorEngineHelper.WeaponTypeToViewId.TryGetValue(value, out cViewId);

											if (sViewId < 0) {
												errors.Add(new CiError(ValidationErrors.CiItemClassUnknown, citem.Key, "ItemDescription: server View ID cannot be below 0.", DataSources.Item, this));
												continue;
											}

											if (sViewId >= ItemGeneratorEngineHelper.WeaponTypes.Count) {
												errors.Add(new CiError(ValidationErrors.CiItemClassUnknown, citem.Key, "ItemDescription: unknown server View ID, custom weapon?", DataSources.Item, this));
												continue;
											}

											if (cViewId == 0) {
												errors.Add(new CiError(ValidationErrors.CiItemClassUnknown, citem.Key, "ItemDescription: found class '" + value + "', expected '" + ItemGeneratorEngineHelper.WeaponTypes[sViewId] + "'", DataSources.ClientItem, this));
											}
											else if (cViewId != sViewId) {
												if (twoHanded) {
													if (ItemGeneratorEngineHelper.TwoHandedWeapons.Contains(sViewId)) {
														errors.Add(new CiError(ValidationErrors.CiItemClassUnknown, citem.Key, "ItemDescription: found class '" + value + "', expected '" + ItemGeneratorEngineHelper.WeaponTypes[sViewId] + "'", DataSources.ClientItem, this));
													}
													else {
														errors.Add(new CiError(ValidationErrors.CiItemClassUnknown, citem.Key, "ItemDescription: found class '" + value + "', expected '" + ItemGeneratorEngineHelper.WeaponTypes[sViewId] + "'. The server View ID doesn't belong to a two-handed weapon class.", DataSources.ClientItem, this));
													}
												}
												else {
													if (!ItemGeneratorEngineHelper.TwoHandedWeapons.Contains(sViewId)) {
														errors.Add(new CiError(ValidationErrors.CiItemClassUnknown, citem.Key, "ItemDescription: found class '" + value + "', expected '" + ItemGeneratorEngineHelper.WeaponTypes[sViewId] + "'", DataSources.ClientItem, this));
													}
													else {
														errors.Add(new CiError(ValidationErrors.CiItemClassUnknown, citem.Key, "ItemDescription: found class '" + value + "', expected '" + ItemGeneratorEngineHelper.WeaponTypes[sViewId] + "'. The server View ID belongs to a two-handed weapon class.", DataSources.ClientItem, this));
													}
												}
											}

											break;
										default:

											break;
									}
								}
								else if (param == ParameterHolderKeys.Attack && SdeAppConfiguration.VaCiAttack) {
									int ival;
									if (Int32.TryParse(value, out ival)) {
										var sval = server.Attack.ToInt();
										var name = server.Name ?? "";

										if (name.EndsWith(" Box"))
											continue;

										if (ival != sval) {
											errors.Add(new CiError(ValidationErrors.CiAttack, citem.Key, "Attack: found '" + value + "', expected '" + sval + "'.", DataSources.ClientItem, this));
										}
									}
									else {
										errors.Add(new CiError(ValidationErrors.CiParseError, citem.Key, "Parse: failed to parse Attack field, found '" + value + "'.", DataSources.ClientItem, this));
									}
								}
								else if (param == ParameterHolderKeys.Defense && SdeAppConfiguration.VaCiDefense) {
									int ival;
									if (Int32.TryParse(value, out ival)) {
										var sval = server.Defense.ToInt();
										if (ival != sval) {
											errors.Add(new CiError(ValidationErrors.CiDefense, citem.Key, "Defense: found '" + value + "', expected '" + sval + "'.", DataSources.ClientItem, this));
										}
									}
									else {
										errors.Add(new CiError(ValidationErrors.CiParseError, citem.Key, "Parse: failed to parse Defense field, found '" + value + "'.", DataSources.ClientItem, this));
									}
								}
								else if (param == ParameterHolderKeys.Property && SdeAppConfiguration.VaCiProperty) {
									var script = server.Script ?? "";
									const string Bonus1 = "bonus bAtkEle";
									const string Bonus2 = "bonus bDefEle";

									if (script.Contains(Bonus1) || script.Contains(Bonus2)) {
										var bonusScript = script.Contains(Bonus1) ? Bonus1 : Bonus2;

										int start = script.IndexOf(bonusScript, StringComparison.Ordinal) + bonusScript.Length;
										int end = script.IndexOf(";", start, StringComparison.Ordinal);

										if (end < 0)
											end = script.Length;

										var bonus = script.Substring(start, end - start).Trim(',', ' ', ';');

										if (bonus.Length > 4) {
											var element = bonus.Substring(4);

											if (element != value) {
												if (value == "Shadow" && element == "Dark") {
												}
												else {
													errors.Add(new CiError(ValidationErrors.CiProperty, citem.Key, "Property: found '" + value + "', expected '" + element + "'.", DataSources.ClientItem, this));
												}
											}
										}
									}
									else {
										if (value != "Neutral") {
											errors.Add(new CiError(ValidationErrors.CiProperty, citem.Key, "Property: found '" + value + "', expected no element or Neutral.", DataSources.ClientItem, this));
										}
									}
								}
								else if (param == ParameterHolderKeys.RequiredLevel && SdeAppConfiguration.VaCiRequiredLevel) {
									int ival;
									if (Int32.TryParse(value, out ival)) {
										var sval = server.EquipLevelMin.ToInt();
										if (ival != sval) {
											errors.Add(new CiError(ValidationErrors.CiEquipLevel, citem.Key, "EquipLevel: found '" + value + "', expected '" + sval + "'.", DataSources.ClientItem, this));
										}
									}
									else {
										errors.Add(new CiError(ValidationErrors.CiParseError, citem.Key, "Parse: failed to parse EquipLevel field, found '" + value + "'.", DataSources.ClientItem, this));
									}
								}
								else if (param == ParameterHolderKeys.WeaponLevel && SdeAppConfiguration.VaCiWeaponLevel) {
									int ival;

									if (Int32.TryParse(value, out ival)) {
										var name = server.Name ?? "";
										var sval = server.WeaponLevel.ToInt();

										if (name.EndsWith(" Box"))
											continue;

										if (ival != sval) {
											errors.Add(new CiError(ValidationErrors.CiWeaponLevel, citem.Key, "WeaponLevel: found '" + value + "', expected '" + sval + "'.", DataSources.ClientItem, this));
										}
									}
									else {
										errors.Add(new CiError(ValidationErrors.CiParseError, citem.Key, "Parse: failed to parse WeaponLevel field, found '" + value + "'.", DataSources.ClientItem, this));
									}
								}
								else if (param == ParameterHolderKeys.Weight && SdeAppConfiguration.VaCiWeight) {
									int ival = (int)(FormatConverters.SingleConverter(value) * 10);

									var sval = server.Weight.ToInt();
									if (ival != sval) {
										errors.Add(new CiError(ValidationErrors.CiWeight, citem.Key, "Weight: found '" + ival + "', expected '" + sval + "'.", DataSources.ClientItem, this));
									}
								}
								else if ((param == ParameterHolderKeys.Location || param == ParameterHolderKeys.EquippedOn) && SdeAppConfiguration.VaCiLocation) {
									var name = server.Name ?? "";

									// Do not scan rental items
									if (name.EndsWith(" Box"))
										continue;

									string[] items = value.Split(',', '/', '-', '&').Select(p => p.Trim(' ')).ToArray();
									int ival = 0;

									foreach (string item in items) {
										if (item.ToLower() == "lower")
											ival |= 1;
										if (item.ToLower() == "mid" || item.ToLower() == "middle")
											ival |= 512;
										if (item.ToLower() == "upper")
											ival |= 256;
										if (item.ToLower() == "all slot")
											ival |= 1023;
									}

									var sval = (int)server.Locations.ToLong();

									if ((sval & 7168) != 0) {
										// It's a costume ;x!
										ival = 0;

										foreach (string item in items) {
											if (item.ToLower() == "lower")
												ival |= 4096;
											if (item.ToLower() == "mid" || item.ToLower() == "middle")
												ival |= 2048;
											if (item.ToLower() == "upper")
												ival |= 1024;
										}
									}

									if (ival != sval) {
										errors.Add(new CiError(ValidationErrors.CiLocation, citem.Key, "Location: found '" + ival + "', expected '" + sval + "'.", DataSources.ClientItem, this));
									}
								}
								else if (param == ParameterHolderKeys.CompoundOn && SdeAppConfiguration.VaCiCompoundOn) {
									var valueLower = value.ToLower();
									int location = 0;

									switch(valueLower) {
										case "weapon":
											location = 2;
											break;
										case "headgear":
											location = 769;
											break;
										case "armor":
											location = 16;
											break;
										case "shield":
											location = 32;
											break;
										case "garment":
											location = 4;
											break;
										case "accessory":
											location = 136;
											break;
										case "shoes":
										case "footwear":
										case "foot gear":
										case "footgear":
											location = 64;
											break;
										default:
											errors.Add(new CiError(ValidationErrors.CiParseError, citem.Key, "CompoundOn: found '" + value + "'.", DataSources.ClientItem, this));
											break;
									}

									var sval = (int)server.Locations.ToLong();

									if ((location & sval) != sval) {
										errors.Add(new CiError(ValidationErrors.CiCompoundOn, citem.Key, "CompoundOn: found '" + location + "', expected '" + sval + "'.", DataSources.ClientItem, this));
									}
								}
							}
						}
						catch {
							errors.Add(new CiError(ValidationErrors.Generic, citem.Key, "Failed to analyse property '" + param + "'.", DataSources.Item, this));
						}
					}
				}
				// End of parameters

				if (itemType == ItemType.IT_WEAPON && SdeAppConfiguration.VaCiItemRange) {
					var sVal = server.View.ToInt();

					if (sVal < 24 && sVal > 0) {
						var range = _ranges[sVal];
						var id = sitem.Key;

						bool found = false;

						for (int i = 0; i < range.Length; i += 2) {
							if (range[i] < 0) {
								errors.Add(new CiError(ValidationErrors.CiItemRange, citem.Key, "ItemRange: found weapon class '" + sVal + "', which does not have any ID range.", DataSources.Item, this));
								found = true;
								break;
							}

							if (range[i] <= id && id <= range[i + 1]) {
								found = true;
								break;
							}
						}

						if (!found) {
							string idRange = "";

							for (int i = 0; i < range.Length; i += 2) {
								if (i > 0)
									idRange += ", ";

								idRange += range[i] + "-" + range[i + 1];
							}

							errors.Add(new CiError(ValidationErrors.CiItemRange, citem.Key, "ItemRange: found weapon class '" + sVal + "', which has an ID range of [" + idRange + "].", DataSources.Item, this));
						}
					}
				}

				if (SdeAppConfiguration.VaCiNumberOfSlots) {
					var sVal = server.Slots.ToInt();
					var cVal = clientItem.NumberOfSlots.ToInt();

					if (sVal != cVal) {
						errors.Add(new CiError(ValidationErrors.CiNumOfSlots, citem.Key, "NumberOfSlots: found '" + cVal + "', expected '" + sVal + "'.", DataSources.ClientItem, this));
					}

					if (server.Type != ItemType.IT_ARMOR && server.Type != ItemType.IT_WEAPON) {
						if (sVal > 0)
							errors.Add(new CiError(ValidationErrors.CiNumOfSlots, citem.Key, "NumberOfSlots: found '" + sVal + "', but the server item type is neither an armor nor a weapon.", DataSources.Item, this));
					}
				}

				if (SdeAppConfiguration.VaCiViewId) {
					var sVal = server.View.ToInt();
					var cVal = clientItem.ClassNumber.ToInt();

					if (server.Type == ItemType.IT_WEAPON && String.IsNullOrEmpty(server.View))
						sVal = cVal;

					if (sVal != cVal) {
						var nVal = cVal;

						if (cVal > 24) {
							if (viewIdToWepClass.ContainsKey(cVal)) {
								nVal = viewIdToWepClass[cVal];
							}
						}

						if (sVal != nVal) {
							if (nVal != cVal) {
								// && nVal + 1 == sVal) {
							}
							else {
								bool showError = true;

								if (cVal == 0) {
									if ((citem.Key >= 18000 && citem.Key <= 18099) ||
										(citem.Key >= 13260 && citem.Key <= 13290)) {
										showError = false;
									}
								}

								if (showError)
									errors.Add(new CiError(ValidationErrors.CiViewId, citem.Key, "ClassNumber: found '" + cVal + "', class '" + nVal + "', expected '" + sVal + "'.", DataSources.ClientItem, this));
							}
						}
					}
				}

				if (SdeAppConfiguration.VaCiIsCard) {
					var sVal = itemType;
					var cVal = clientItem.IsCard;

					if ((sVal == ItemType.IT_CARD || cVal) && (sVal != ItemType.IT_CARD || !cVal)) {
						errors.Add(new CiError(ValidationErrors.CiCardType, citem.Key, "TypeMismatch: client item IsCard '" + cVal + "', server type '" + sVal + "'.", DataSources.ClientItem, this));
					}
				}

				if (SdeAppConfiguration.VaCiName) {
					var sname = server.Name ?? "";
					var cname = clientItem.IdentifiedDisplayName ?? "";

					if (sname != cname) {
						int distance = Methods.LevenshteinDistance(sname, cname);

						if (distance > 5) {
							errors.Add(new CiError(ValidationErrors.CiName, citem.Key, "NameMismatch: client name is '" + cname + "', server name is '" + sname + "', diff = " + distance + ".", DataSources.ClientItem, this));
						}
					}
				}
			}
		}

		private readonly Dictionary<int, int[]> _ranges = new Dictionary<int, int[]> {
			{ 1, new[] { 1200, 1249, 13000, 13099 } },
			{ 2, new[] { 1100, 1115, 1119, 1149, 13400, 13499 } },
			{ 3, new[] { 1116, 1118, 1150, 1199, 21000, 21999 } },
			{ 4, new[] { 1400, 1409, 1413, 1449 } },
			{ 5, new[] { 1410, 1412, 1450, 1471, 1474, 1499 } },
			{ 6, new[] { 1300, 1313, 1316, 1349 } },
			{ 7, new[] { 1314, 1315, 1350, 1399 } },
			{ 8, new[] { 1500, 1549, 1599, 1599, 16000, 16999 } },
			{ 9, new[] { -1 } },
			{ 10, new[] { 1600, 1699 } },
			{ 11, new[] { 1700, 1749, 18100, 18499 } },
			{ 12, new[] { 1800, 1899 } },
			{ 13, new[] { 1900, 1949 } },
			{ 14, new[] { 1950, 1999 } },
			{ 15, new[] { 1550, 1599 } },
			{ 16, new[] { 1250, 1299 } },
			{ 17, new[] { 13100, 13149 } },
			{ 18, new[] { 13150, 13199 } },
			{ 19, new[] { 13150, 13199 } },
			{ 20, new[] { 13150, 13199 } },
			{ 21, new[] { 13150, 13199 } },
			{ 22, new[] { 13300, 13399 } },
			{ 23, new[] { 1472, 1473, 2000, 2099 } },
		};

		private Dictionary<int, int> _getWeaponClasses() {
			Dictionary<int, int> table = new Dictionary<int, int>();
			var accIdPath = ProjectConfiguration.SyncAccId;

			for (int i = 0; i <= 30; i++) {
				table[i] = i;
			}

			if (_sde.MetaGrf.GetData(ProjectConfiguration.SyncAccId) == null || _sde.MetaGrf.GetData(ProjectConfiguration.SyncAccName) == null) {
				return table;
			}

			var weaponPath = GrfPath.Combine(GrfPath.GetDirectoryName(accIdPath), "weapontable" + Path.GetExtension(accIdPath));
			var weaponData = _sde.MetaGrf.GetData(weaponPath);

			if (weaponData == null) {
				return table;
			}

			var weaponTable = new Parser(Lub.AutoDecompile(weaponData)).Parse(EncodingService.DisplayEncoding);
			var weaponIds = LuaHelper.GetLuaTable(weaponTable, "Weapon_IDs");
			var weaponExpansionNameTable = LuaHelper.GetLuaTable(weaponTable, "Expansion_Weapon_IDs");

			var ids = LuaHelper.SetIds(weaponIds, "Weapon_IDs");

			foreach (var id in ids) {
				if (id.Value == 24)
					continue;

				if (id.Value <= 30) {
					table[id.Value] = id.Value;
				}
				else {
					string sval;
					if (weaponExpansionNameTable.TryGetValue("[" + id.Key + "]", out sval)) {
						int ival;
						if (ids.TryGetValue(sval, out ival)) {
							table[id.Value] = ival;
						}
					}
				}
			}

			return table;
		}
	}
}