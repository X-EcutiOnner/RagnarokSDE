using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Database.Commands;
using SDE.Databases.ClientItems.Common;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Databases.Mobs.Features;
using SDE.Databases.Pets.Features;
using SDE.Editor.Database;
using Utilities;
using Utilities.Extension;
using Utilities.Services;

namespace SDE.Editor.Items {
	public class ItemGeneratorEngineHelper {
		public static readonly List<string> WeaponTypes = new List<string> {
			"None",
			"Dagger",
			"Sword",
			"Two-handed Sword",
			"Spear",
			"Two-handed Spear", // 5
			"Axe",
			"Two-handed Axe",
			"Mace",
			"Two-handed Mace",
			"Rod", // 10
			"Bow",
			"Knuckle",
			"Instrument",
			"Whip",
			"Book", // 15
			"Katar",
			"Pistol",
			"Rifle",
			"Gatling Gun",
			"Shotgun", // 20
			"Grenade Launcher",
			"Huuma",
			"Two-handed Staff",
			"Last",
			"Sword", // 25
			"Sword",
			"Axe",
			"Sword",
			"Axe",
			"Sword Axe",
		};

		public static readonly HashSet<int> TwoHandedWeapons = new HashSet<int> {
			3, 5, 7, 9, 11, 16, 17, 18, 19, 20, 21, 22, 23
		};

		public static readonly Dictionary<string, int> WeaponTypeToViewId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
			{ "None", 0 },
			{ "Dagger", 1 },
			{ "Sword", 2 },
			{ "One-handed Sword", 2 },
			{ "One handed Sword", 2 },
			{ "1-handed Sword", 2 },
			{ "Two-handed Sword", 3 },
			{ "Two handed Sword", 3 },
			{ "2-handed Sword", 3 },
			{ "Spear", 4 },
			{ "One-handed Spear", 4 },
			{ "One handed Spear", 4 },
			{ "1-handed Spear", 4 },
			{ "Two-handed Spear", 5 },
			{ "Two handed Spear", 5 },
			{ "2-handed Spear", 5 },
			{ "Axe", 6 },
			{ "One-handed Axe", 6 },
			{ "One handed Axe", 6 },
			{ "1-handed Axe", 6 },
			{ "Two-handed Axe", 7 },
			{ "Two handed Axe", 7 },
			{ "2-handed Axe", 7 },
			{ "Mace", 8 },
			{ "Two-handed Mace", 9 },
			{ "Two handed Mace", 9 },
			{ "2-handed Mace", 9 },
			{ "Rod", 10 },
			{ "Staff", 10 },
			{ "One-handed Staff", 10 },
			{ "One handed Staff", 10 },
			{ "1-handed Staff", 10 },
			{ "Bow", 11 },
			{ "Knuckle", 12 },
			{ "Claw", 12 },
			{ "Instrument", 13 },
			{ "Musical Instrument", 13 },
			{ "Whip", 14 },
			{ "Book", 15 },
			{ "Katar", 16 },
			{ "Pistol", 17 },
			{ "Revolver", 17 },
			{ "Rifle", 18 },
			{ "Gatling Gun", 19 },
			{ "Shotgun", 20 },
			{ "Grenade Launcher", 21 },
			{ "Huuma", 22 },
			{ "Huuma Shuriken", 22 },
			{ "Two-handed Staff", 23 },
			{ "Two handed Staff", 23 },
			{ "2-handed Staff", 23 },
			{ "Last", 24 },
		};
	}

	public class ItemGeneratorEngine<TKey> {
		private ReadableTuple _item;

		private bool _emptyFill(string fieldValue) {
			if (ProjectConfiguration.AutocompleteFillOnlyEmptyFields) {
				return String.IsNullOrEmpty(fieldValue);
			}

			return true;
		}

		public GroupCommand<TKey, ReadableTuple> Generate(ReadableTuple item, ReadableTuple tupleSource, BaseDatabase mobDb1, BaseDatabase mobDb2, BaseDatabase pet1, BaseDatabase pet2) {
			_item = item;

			var client = item.GetModel<ClientItem>();
			var server = tupleSource.GetModel<Item>();
			var description = new ParameterHolder(client).Values[ParameterHolderKeys.Description] ?? "";
			description = ParameterHolder.ClearDescription(description);

			ParameterHolder holder = new ParameterHolder();
			GroupCommand<TKey, ReadableTuple> commands = GroupCommand<TKey, ReadableTuple>.Make();

			int numSlotC = DbReader.ToInt(client.NumberOfSlots);
			int numSlotS = DbReader.ToInt(server.Slots);

			if (ProjectConfiguration.AutocompleteViewId) {
				int viewIdC = DbReader.ToInt(client.ClassNumber);
				int viewIdS = DbReader.ToInt(server.View);

				if (viewIdC != viewIdS) {
					commands.Add(new ModelCommand<TKey, ReadableTuple, string>(item, () => client.NumberOfSlots, v => client.NumberOfSlots = v, viewIdS.ToString(), nameof(ClientItem.ClassNumber)));
				}
			}

			if (ProjectConfiguration.AutocompleteNumberOfSlot) {
				if (numSlotC != numSlotS) {
					commands.Add(new ModelCommand<TKey, ReadableTuple, string>(item, () => client.NumberOfSlots, v => client.NumberOfSlots = v, numSlotS.ToString(), nameof(ClientItem.NumberOfSlots)));
				}
			}

			if (server.Type != ItemType.IT_CARD) {
				if (client.IsCard)
					commands.Add(new ModelCommand<TKey, ReadableTuple, bool>(item, () => client.IsCard, v => client.IsCard = v, false, nameof(ClientItem.IsCard)));
			}

			string idDisplayName = server.Name ?? "";
			if (_emptyFill(client.IdentifiedDisplayName) && ProjectConfiguration.AutocompleteIdDisplayName && _isNotNullDifferent(idDisplayName, client.IdentifiedDisplayName)) {
				commands.Add(new ModelCommand<TKey, ReadableTuple, string>(item, () => client.IdentifiedDisplayName, v => client.IdentifiedDisplayName = v, idDisplayName, nameof(ClientItem.IdentifiedDisplayName)));
			}

			ItemType itemType = server.Type;

			// Weight:
			//holder = item.GetValue<ParameterHolder>(ClientItemAttributes.Parameters);
			//
			//switch (itemType) {
			//	case ItemType.IT_WEAPON:
			//	case ItemType.IT_AMMO:
			//	case ItemType.IT_ARMOR:
			//	case ItemType.IT_CARD:
			//	case ItemType.IT_PETEGG:
			//	case ItemType.IT_PETARMOR:
			//	case ItemType.IT_USABLE:
			//	case ItemType.IT_ETC:
			//	case ItemType.IT_HEALING:
			//	case ItemType.IT_SHADOWGEAR:
			//	case ItemType.IT_DELAYCONSUME:
			//	case ItemType.IT_CASH:
			//		_autoAddWeight(tupleSource, holder);
			//		break;
			//}

			int equipLevel = server.EquipLevelMin.ToInt();

			if (server.EquipLevelMin.ToInt() > server.EquipLevelMax.ToInt()) {
				equipLevel = server.EquipLevelMin.ToInt();
			}

			string stringValue;

			switch(itemType) {
				case ItemType.IT_WEAPON:
					string type = _findWeaponType(server) ?? "Weapon";
					holder.Values[ParameterHolderKeys.Class] = type;

					string unidentifiedResourceName = EncodingService.FromAnyToDisplayEncoding(_findWeaponUnidentifiedResource(server) ?? "");
					if (ProjectConfiguration.AutocompleteUnResourceName && 
						_emptyFill(client.UnidentifiedResourceName) &&
						_isNotNullDifferent(unidentifiedResourceName, client.UnidentifiedResourceName)) {
						AddCommand(commands,
							() => client.UnidentifiedResourceName,
							v => client.UnidentifiedResourceName = v,
							nameof(ClientItem.UnidentifiedResourceName),
							unidentifiedResourceName);
					}

					stringValue = client.IdentifiedResourceName;
					if (ProjectConfiguration.AutocompleteIdResourceName &&
						String.IsNullOrEmpty(stringValue) &&
						_isNotNullDifferent(unidentifiedResourceName, client.IdentifiedResourceName)) {
						AddCommand(commands,
							() => client.IdentifiedResourceName,
							v => client.IdentifiedResourceName = v,
							nameof(ClientItem.IdentifiedResourceName),
							stringValue);
					}

					stringValue = client.UnidentifiedDisplayName;
					if (ProjectConfiguration.AutocompleteUnDisplayName &&
						String.IsNullOrEmpty(stringValue) &&
						_isNotNullDifferent(stringValue, client.UnidentifiedDisplayName)) {
						AddCommand(commands,
							() => client.UnidentifiedDisplayName,
							v => client.UnidentifiedDisplayName = v,
							nameof(ClientItem.UnidentifiedDisplayName),
							stringValue);
					}

					if (!server.Refineable) {
						if (!description.Contains("Impossible to refine") &&
						    !description.Contains("Cannot be upgraded") &&
						    !description.ToLower().Contains("rental item")) {
							description += "\r\nImpossible to refine this item.";
						}
					}
					else if (server.Refineable) {
						if (description.Contains("Impossible to refine")) {
							description = description.Replace("Impossible to refine this item.", "").Trim('\r', '\n');
						}
					}

					_autoAdd(server.Attack, ParameterHolderKeys.Attack, holder);
					_autoAddWeight(server, holder);
					_autoAdd(server.WeaponLevel, ParameterHolderKeys.WeaponLevel, holder);
					_autoAddJob(server, holder, equipLevel);
					_autoAddElement(server, holder);
					break;
				case ItemType.IT_AMMO:
					type = _findAmmoType(server.Jobs.ToUInt64()) ?? "Ammunition";
					holder.Values[ParameterHolderKeys.Class] = type;

					_autoAdd(server.Attack, ParameterHolderKeys.Attack, holder, -1);
					_autoAddWeight(server, holder);
					_autoAddElement(server, holder);
					break;
				case ItemType.IT_ARMOR:
					long location = server.Locations.ToLong();
					type = _findArmorType(location) ?? "Armor";
					holder.Values[ParameterHolderKeys.Class] = type;

					unidentifiedResourceName = EncodingService.FromAnyToDisplayEncoding(_findArmorUnidentifiedResource(server, client) ?? "");
					if (ProjectConfiguration.AutocompleteUnResourceName &&
						_emptyFill(client.UnidentifiedResourceName) &&
						_isNotNullDifferent(unidentifiedResourceName, client.UnidentifiedResourceName)) {
						AddCommand(commands,
							() => client.UnidentifiedResourceName,
							v => client.UnidentifiedResourceName = v,
							nameof(ClientItem.UnidentifiedResourceName),
							unidentifiedResourceName);
					}
					else {
						unidentifiedResourceName = client.UnidentifiedResourceName;
					}

					string identifiedResourceName = client.IdentifiedResourceName;
					if (ProjectConfiguration.AutocompleteIdResourceName &&
						String.IsNullOrEmpty(identifiedResourceName) &&
						_isNotNullDifferent(identifiedResourceName, client.IdentifiedResourceName)) {
						AddCommand(commands,
							() => client.UnidentifiedResourceName,
							v => client.UnidentifiedResourceName = v,
							nameof(ClientItem.UnidentifiedResourceName),
							unidentifiedResourceName);
					}

					string unDisplayName = _findArmorUnidentifiedDisplayName(unidentifiedResourceName);
					if (ProjectConfiguration.AutocompleteUnDisplayName &&
						_emptyFill(client.UnidentifiedDisplayName) &&
						_isNotNullDifferent(identifiedResourceName, client.UnidentifiedDisplayName)) {
						AddCommand(commands,
							() => client.UnidentifiedDisplayName,
							v => client.UnidentifiedDisplayName = v,
							nameof(ClientItem.UnidentifiedDisplayName),
							unDisplayName);
					}

					if ((server.Locations.ToLong() & 374) != 0) {
						if (!server.Refineable) {
							if (!description.Contains("Impossible to refine")) {
								description += "\r\nImpossible to refine this item.";
							}
						}

						if (server.Refineable) {
							if (description.Contains("Impossible to refine")) {
								description = description.Replace("Impossible to refine this item.", "").Trim('\r', '\n');
							}
						}
					}

					_autoAdd(server.Defense, ParameterHolderKeys.Defense, holder);
					_autoAddEquippedOn(ParameterHolderKeys.Location, server, holder);
					_autoAddWeight(server, holder);
					_autoAddJob(server, holder, equipLevel);
					break;
				case ItemType.IT_CARD:
					holder.Values[ParameterHolderKeys.Class] = "Card";
					_autoAddCompound(ParameterHolderKeys.CompoundOn, server, holder);
					_autoAdd(equipLevel.ToString(), ParameterHolderKeys.RequiredLevel, holder, 1);
					_autoAddWeight(server, holder);

					if (!client.IsCard)
						AddCommand(commands,
							() => client.IsCard,
							v => client.IsCard = v,
							nameof(ClientItem.IsCard),
							true);

					if (String.IsNullOrEmpty(client.Illustration))
						AddCommand(commands,
							() => client.Illustration,
							v => client.Illustration = v,
							nameof(ClientItem.Illustration),
							"sorry");

					if (String.IsNullOrEmpty(client.Affix))
						AddCommand(commands,
							() => client.Affix,
							v => client.Affix = v,
							nameof(ClientItem.Affix),
							server.Name ?? "");

					const string CardResource = "ÀÌ¸§¾ø´ÂÄ«µå";

					unDisplayName = server.Name ?? "";
					if (ProjectConfiguration.AutocompleteUnDisplayName &&
						_emptyFill(unDisplayName) &&
						_isNotNullDifferent(unDisplayName, client.UnidentifiedDisplayName)) {
						AddCommand(commands,
							() => client.UnidentifiedDisplayName,
							v => client.UnidentifiedDisplayName = v,
							nameof(ClientItem.UnidentifiedDisplayName),
							unDisplayName);
					}

					unidentifiedResourceName = EncodingService.FromAnyToDisplayEncoding(CardResource);
					if (ProjectConfiguration.AutocompleteUnResourceName &&
						_emptyFill(unidentifiedResourceName) &&
						_isNotNullDifferent(unidentifiedResourceName, client.UnidentifiedResourceName)) {
						AddCommand(commands,
							() => client.UnidentifiedResourceName,
							v => client.UnidentifiedResourceName = v,
							nameof(ClientItem.UnidentifiedResourceName),
							unidentifiedResourceName);
					}

					if (ProjectConfiguration.AutocompleteIdResourceName &&
						_emptyFill(unidentifiedResourceName) &&
						_isNotNullDifferent(unidentifiedResourceName, client.IdentifiedResourceName)) {
						AddCommand(commands,
							() => client.IdentifiedResourceName,
							v => client.IdentifiedResourceName = v,
							nameof(ClientItem.IdentifiedResourceName),
							unidentifiedResourceName);
					}
					break;
				case ItemType.IT_PETEGG:
					holder.Values[ParameterHolderKeys.Class] = "Monster Egg";
					_autoAddWeight(server, holder);
					break;
				case ItemType.IT_PETARMOR:
					holder.Values[ParameterHolderKeys.Class] = "Cute Pet Armor";
					_autoAddWeight(server, holder);

					int id = item.GetKey<int>();
					string idString = id.ToString();

					List<ReadableTuple> tuples = pet1.Table.Tuples.Where(p => p.Value.GetModel<Pet>().EquipItem == idString).Select(p => p.Value).Concat(
						pet2.Table.Tuples.Where(p => p.Value.GetModel<Pet>().EquipItem == idString).Select(p => p.Value)
						).ToList();

					if (tuples.Count > 0) {
						// Try to retrieve the names
						List<string> values = new List<string>();

						foreach (ReadableTuple tuple in tuples) {
							var pid = tuple.GetKey<int>();

							var pTuple = mobDb2.Table.TryGetTuple(pid) ?? mobDb1.Table.TryGetTuple(pid);

							if (pTuple != null) {
								values.Add(pTuple.GetModel<Mob>().Name);
							}
						}

						if (values.Count > 0)
							holder.Values[ParameterHolderKeys.ApplicablePet] = String.Join(", ", values.ToArray());
					}
					break;
				case ItemType.IT_USABLE:
					_autoAddPet(server, holder);
					_autoAddWeight(server, holder);
					_autoAddJobIfRestricted(server, holder);
					break;
				case ItemType.IT_ETC:
				case ItemType.IT_HEALING:
				case ItemType.IT_SHADOWGEAR:
				case ItemType.IT_DELAYCONSUME:
				case ItemType.IT_CASH:
					_autoAddWeight(server, holder);
					_autoAddJobIfRestricted(server, holder);
					break;
			}

			_autoAdd(equipLevel.ToString(), ParameterHolderKeys.RequiredLevel, holder, 1);

			holder.Values[ParameterHolderKeys.Description] = description == "" ? ProjectConfiguration.AutocompleteDescNotSet : description;

			var idDescription = holder.GenerateDescription();

			if (ProjectConfiguration.AutocompleteIdDescription) {
				if (idDescription != client.IdentifiedDescription)
					AddCommand(commands, 
						() => client.IdentifiedDescription, 
						v => client.IdentifiedDescription = v, 
						nameof(ClientItem.IdentifiedDescription), 
						idDescription);
			}

			var unDescription = client.UnidentifiedDescription;

			// unidentified
			switch(server.Type) {
				case ItemType.IT_AMMO:
				case ItemType.IT_ETC:
				case ItemType.IT_HEALING:
				case ItemType.IT_PETEGG:
				case ItemType.IT_USABLE:
				case ItemType.IT_DELAYCONSUME:
				case ItemType.IT_CASH:
					if (ProjectConfiguration.AutocompleteUnDescription && unDescription != idDescription)
						AddCommand(commands,
							() => client.UnidentifiedDescription,
							v => client.UnidentifiedDescription = v,
							nameof(ClientItem.UnidentifiedDescription),
							idDescription);

					string unDisplayName = server.Name ?? "";
					if (ProjectConfiguration.AutocompleteUnDisplayName &&
						_emptyFill(client.UnidentifiedDisplayName) &&
						_isNotNullDifferent(unDisplayName, client.UnidentifiedDisplayName)) {
						AddCommand(commands,
							() => client.UnidentifiedDisplayName,
							v => client.UnidentifiedDisplayName = v,
							nameof(ClientItem.UnidentifiedDisplayName),
							unDisplayName);
					}

					string unidentifiedResourceName = client.IdentifiedResourceName;
					if (String.IsNullOrEmpty(unidentifiedResourceName)) {
						unidentifiedResourceName = EncodingService.FromAnyToDisplayEncoding("Á¶°¢ÄÉÀÌÅ©"); // Cake
					}

					if (ProjectConfiguration.AutocompleteUnResourceName &&
						_emptyFill(client.UnidentifiedResourceName) && 
						_isNotNullDifferent(unidentifiedResourceName, client.UnidentifiedResourceName)) {
						AddCommand(commands,
							() => client.UnidentifiedResourceName,
							v => client.UnidentifiedResourceName = v,
							nameof(ClientItem.UnidentifiedResourceName),
							unidentifiedResourceName);
					}

					if (ProjectConfiguration.AutocompleteIdResourceName &&
						_emptyFill(client.IdentifiedResourceName) && 
						_isNotNullDifferent(unidentifiedResourceName, client.IdentifiedResourceName)) {
						AddCommand(commands,
							() => client.IdentifiedResourceName,
							v => client.IdentifiedResourceName = v,
							nameof(ClientItem.IdentifiedResourceName),
							unidentifiedResourceName);
					}
					break;
				case ItemType.IT_CARD:
					if (ProjectConfiguration.AutocompleteUnDescription && unDescription != idDescription)
						AddCommand(commands,
							() => client.UnidentifiedDescription,
							v => client.UnidentifiedDescription = v,
							nameof(ClientItem.UnidentifiedDescription),
							idDescription);
					break;
				default:
					if (ProjectConfiguration.AutocompleteUnDescription && unDescription != ProjectConfiguration.AutocompleteUnDescriptionFormat)
						AddCommand(commands,
							() => client.UnidentifiedDescription,
							v => client.UnidentifiedDescription = v,
							nameof(ClientItem.UnidentifiedDescription),
							ProjectConfiguration.AutocompleteUnDescriptionFormat);
					break;
			}

			if (commands.Commands.Count == 0)
				return null;

			return commands;
		}

		public void AddCommand<TFieldValue>(GroupCommand<TKey, ReadableTuple> commands, Func<TFieldValue> getter, Action<TFieldValue> setter, string fieldName, TFieldValue newValue) {
			commands.Add(new ModelCommand<TKey, ReadableTuple, TFieldValue>(_item, getter, setter, newValue, fieldName));
		}

		private readonly TkDictionary<string, string> _armorUnDisplayName = new TkDictionary<string, string> {
			{ "¸Ó¸®¶ì", "Hairband" },
			{ "Ä¸", "Hat" },
			{ "ÇÞ", "Hat" },
			{ "Çï¸§", "Helm" },
			{ "¸®º»", "Ribbon" },
			{ "½º¸¶ÀÏ", "Mask" },
			{ "±Û·¡½º", "Glasses" },
			{ "¿ìµç¸ÞÀÏ", "Armor" },
			{ "ÈÄµå", "Garment" },
			{ "ÀÌ¾î¸µ", "Earring" },
			{ "¸µ", "Accessory" },
			{ "±Û·¯ºê", "Glove" },
			{ "³×Å¬¸®½º", "Necklace" },
			{ "ÄÚÆ°¼ÅÃ÷", "Clothing" },
			{ "»÷µé", "Shoes" },
			{ "±×¸®ºê", "Greaves" },
			{ "ºÎÃ÷", "Boots" },
			{ "¸ÓÇÃ·¯", "Muffler" }
		};

		private string _findArmorUnidentifiedDisplayName(string value) {
			string ansi = EncodingService.GetAnsiString(value);
			return _armorUnDisplayName[ansi];
		}

		private bool _isNotNullDifferent(string newValue, string oldValue) {
			if (string.IsNullOrEmpty(newValue)) return false;
			return newValue != oldValue;
		}

		private bool _overridableString(string value, params string[] strings) {
			return strings.Any(s => value.IndexOf(s, StringComparison.OrdinalIgnoreCase) > -1);
		}

		private string _findArmorUnidentifiedResource(Item server, ClientItem client) {
			List<Job> jobs = JobOperations.GetJobs(server.Jobs.ToUInt64());
			int location = (int)server.Locations.ToLong();

			if (_is(location, 32)) {
				return "°¡µå";
			}

			if (_is(location, 1, 256, 512, 1024, 2048, 4096)) {
				if (_overridableString(client.UnidentifiedResourceName,
					"¸Ó¸®¶ì", "±Û·¡½º", "Çï¸§", "Ä¸", "½º¸¶ÀÏ")) return null;

				if (location == 513)
					return "½º¸¶ÀÏ";

				if (location == 512)
					return "±Û·¡½º";

				if (_is(location, 256)) {
					if (jobs.All(p => p.BaseJob == Job.Swordman))
						return "Çï¸§";
				}

				return "Ä¸";
			}

			if (_is(location, 16)) {
				if (_overridableString(server.Name ?? "", "Armor"))
					return "¿ìµç¸ÞÀÏ";

				if (_overridableString(server.Name ?? "", "Clothe", "Robe", " Suit", "Coat"))
					return "ÄÚÆ°¼ÅÃ÷";

				if (jobs.Contains(Job.Novice) || jobs.Any(p => p.BaseJob == Job.Acolyte))
					return "ÄÚÆ°¼ÅÃ÷";

				int weight = server.Weight.ToInt();

				if (weight >= 1000)
					return "¿ìµç¸ÞÀÏ";

				return "ÄÚÆ°¼ÅÃ÷";
			}

			if (_is(location, 64))
				return "»÷µé";

			if (_is(location, 4))
				return "ÈÄµå";

			if (_is(location, 8, 128))
				return "¸µ";
			return null;
		}

		private void _autoAddPet(Item model, ParameterHolder holder) {
			var script = model.Script ?? "";
			const string Bonus = "pet ";

			if (script.Contains(Bonus)) {
				int start = script.IndexOf(Bonus, StringComparison.Ordinal) + Bonus.Length;
				int end = script.IndexOf(";", start, StringComparison.Ordinal);

				if (end < 0)
					end = script.Length;

				var bonus = script.Substring(start, end - start).Trim(',', ' ', ';');

				if (bonus.Length > 0)
					holder.Values[ParameterHolderKeys.Class] = "Taming Item";
			}
		}

		private void _autoAddEquippedOn(ParameterHolderKeys key, Item model, ParameterHolder holder) {
			var location = (int)model.Locations.ToLong();

			if (_is(location, 1, 256, 512)) {
				List<string> values = new List<string>();

				if (_is(location, 256)) values.Add("Upper");
				if (_is(location, 512)) values.Add("Mid");
				if (_is(location, 1)) values.Add("Lower");

				holder.Values[key] = string.Join(", ", values.ToArray());
			}

			if (_is(location, 1024, 2048, 4096)) {
				List<string> values = new List<string>();

				if (_is(location, 1024)) values.Add("Upper");
				if (_is(location, 2048)) values.Add("Mid");
				if (_is(location, 4096)) values.Add("Lower");

				holder.Values[key] = string.Join(", ", values.ToArray());
			}
		}

		private void _autoAddCompound(ParameterHolderKeys para, Item model, ParameterHolder holder) {
			int val = (int)model.Locations.ToLong();

			if (_is(val, 2)) {
				holder.Values[para] = "Weapon";
			}
			else if (_is(val, 32)) {
				holder.Values[para] = "Shield";
			}
			else if (_is(val, 8, 128)) {
				if (val == 8) {
					holder.Values[para] = "Accessory (Right)";
				}
				else if (val == 128) {
					holder.Values[para] = "Accessory (Left)";
				}
				else {
					holder.Values[para] = "Accessory";
				}
			}
			else if (_is(val, 16)) {
				holder.Values[para] = "Armor";
			}
			else if (_is(val, 64)) {
				holder.Values[para] = "Footgear";
			}
			else if (_is(val, 4)) {
				holder.Values[para] = "Garment";
			}
			else if (_is(val, 1, 256, 512)) {
				holder.Values[para] = "Headgear";
			}
		}

		private bool _is(int val, int to) {
			return (val & to) == to;
		}

		private bool _is(int val, params int[] to) {
			return to.Any(t => _is(val, t));
		}

		private string _findAmmoType(UInt64 jobHex) {
			if ((jobHex & Job.Ninja.JobSdeUid) != 0)
				return "Throwing Weapon";

			if ((jobHex & Job.Gunslinger.JobSdeUid) != 0)
				return "Bullet";

			if ((jobHex & (Job.Archer.JobSdeUid | Job.Hunter.JobSdeUid | Job.BardDancer.JobSdeUid)) != 0)
				return "Arrow";

			if ((jobHex & Job.Alchemist.JobSdeUid) != 0)
				return "Shell";

			if ((jobHex & Job.Assassin.JobSdeUid) != 0)
				return "Throwing Dagger";

			return null;
		}

		private string _findArmorType(long l) {
			int location = (int)l;

			if (_is(location, 32))
				return "Shield";

			if (_is(location, 1, 256, 512))
				return "Headgear";

			if (_is(location, 1024, 2048, 4096))
				return "Costume";

			if (_is(location, 16))
				return "Armor";

			if (_is(location, 64))
				return "Footgear";

			if (_is(location, 4))
				return "Garment";

			if (_is(location, 8, 128)) {
				if (location == 8) {
					return "Accessory (Right)";
				}
				
				if (location == 128) {
					return "Accessory (Left)";
				}

				return "Accessory";
			}

			return null;
		}

		private void _autoAddElement(Item model, ParameterHolder holder) {
			var script = model.Script ?? "";
			const string Bonus = "bonus bAtkEle";

			if (script.Contains(Bonus)) {
				int start = script.IndexOf(Bonus, StringComparison.Ordinal) + Bonus.Length;
				int end = script.IndexOf(";", start, StringComparison.Ordinal);

				if (end < 0)
					end = script.Length;

				var bonus = script.Substring(start, end - start).Trim(',', ' ', ';');

				if (bonus.Length > 4)
					holder.Values[ParameterHolderKeys.Property] = bonus.Substring(4);
			}
			else {
				if (ProjectConfiguration.AutocompleteNeutralProperty) {
					holder.Values[ParameterHolderKeys.Property] = "Neutral";
				}
			}
		}

		private void _autoAddWeight(Item model, ParameterHolder holder) {
			var val = model.Weight.ToInt();
			holder.Values[ParameterHolderKeys.Weight] = (val / 10f).ToString(CultureInfo.InvariantCulture).Replace(',', '.');
		}

		private void _autoAdd(string sVal, ParameterHolderKeys key, ParameterHolder holder, int min = 0) {
			int val = 0;

			if (!string.IsNullOrEmpty(sVal))
				Int32.TryParse(sVal, out val);

			if (val > min) {
				holder.Values[key] = val.ToString(CultureInfo.InvariantCulture);
			}
		}

		private void _autoAddJob(Item model, ParameterHolder holder, int equipLevel) {
			var val = model.Jobs.ToUInt64();
			string applicationJob = JobOperations.GetStringFormat(val, model.Classes.ToFlag<ItemJobFlag>(), model.Gender, equipLevel);

			holder.Values[ParameterHolderKeys.ApplicableJob] = applicationJob;
		}

		private void _autoAddJobIfRestricted(Item model, ParameterHolder holder) {
			var val = model.Jobs.ToUInt64();
			string applicationJob = JobOperations.GetStringFormat(val, model.Classes.ToFlag<ItemJobFlag>(), model.Gender, 0);

			if (String.CompareOrdinal(applicationJob, "Every Job") != 0) {
				holder.Values[ParameterHolderKeys.ApplicableJob] = applicationJob;
			}
		}

		private static readonly List<string> _undWeaponTypes = new List<string> {
			"None",
			"³ªÀÌÇÁ",
			"¼Òµå",
			"¹Ù½ºÅ¸µå¼Òµå",
			"Àðº§¸°",
			"Àðº§¸°", // 5
			"¾×½º",
			"¾×½º",
			"Å¬·´",
			"Å¬·´",
			"·Ôµå", // 10
			"º¸¿ì",
			"¹Ù±×³«",
			"¹ÙÀÌ¿Ã¸°",
			"·ÎÇÁ",
			"ºÏ", // 15
			"Ä«Å¸¸£",
			"½Ä½º½´ÅÍ",
			"¶óÀÌÇÃ",
			"µå¸®ÇÁÅÍ",
			"½Ì±Û¼¦°Ç", // 20
			"µð½ºÆ®·ÎÀÌ¾î",
			"Ç³¸¶_ÆíÀÍ",
			"·Ôµå",
			null,
			null, // 25
			null,
			null,
			null,
			null,
			null,
		};

		private string _findWeaponType(Item model) {
			var viewId = model.View.ToInt();

			switch (model.Type) {
				case ItemType.IT_WEAPON:
					var idType = (int)model.WeaponType;

					if (idType >= 0 && idType < ItemGeneratorEngineHelper.WeaponTypes.Count)
						return ItemGeneratorEngineHelper.WeaponTypes[(int)model.WeaponType];

					break;
			}

			return null;
		}

		private string _findWeaponUnidentifiedResource(Item model) {
			var viewId = model.View.ToInt();

			// Based on view id
			if (viewId > 0 && viewId < _undWeaponTypes.Count) {
				var res = _undWeaponTypes[viewId];
				if (res == null) return null;
				return EncodingService.FromAnyToDisplayEncoding(res);
			}

			return null;
		}
	}
}