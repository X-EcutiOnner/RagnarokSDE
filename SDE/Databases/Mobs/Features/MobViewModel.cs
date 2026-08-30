using Database;
using Database.Commands;
using ErrorManager;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Mobs.Parser;
using SDE.Databases.MobSkills;
using SDE.Databases.MobSkills.Features;
using SDE.Databases.MobSkills.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TokeiLibrary.WPF;
using Utilities;

namespace SDE.Databases.Mobs.Features {
	public class MobViewModel : BaseModelView<Mob> {
		public int MaximumDrops = -1;
		public int MaximumTargets = -1;

		public RangeObservableCollection<ItemDropViewModel> MvpDrops { get; } = new RangeObservableCollection<ItemDropViewModel>();
		public RangeObservableCollection<ItemDropViewModel> Drops { get; } = new RangeObservableCollection<ItemDropViewModel>();
		public RangeObservableCollection<MobSkillViewModel> MobSkills { get; } = new RangeObservableCollection<MobSkillViewModel>();

		public bool IsLocked { get; set; }

		public MobViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, Mob model) {
			if (IsLocked)
				return;

			Model = model;
			Tuple = tuple;

			OnMvpDropsListUpdated();
			OnDropsListUpdated();
			OnMobSkillsListUpdated();
			OnPropertyChanged("");
			
			ClearErrors();
			ValidateAegisName();
		}

		public string Id {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple?.Key.ToString() == value)
					return;

				if (Int32.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Id));
			}
		}

		public string AegisName { get => Model?.AegisName; set { ExecuteCommand(value); ValidateAegisName(); } }
		public string Name { get => Model?.Name; set => ExecuteCommand(value); }
		public string JapaneseName { get => Model?.JapaneseName; set { ExecuteCommand(value); OnPropertyChanged(nameof(JapaneseNamePreview)); } }
		public string Level { get => Model?.Level; set => ExecuteCommand(value); }
		public string Hp { get => Model?.Hp; set => ExecuteCommand(value); }
		public string Sp { get => Model?.Sp; set => ExecuteCommand(value); }
		public string BaseExp { get => Model?.BaseExp; set => ExecuteCommand(value); }
		public string JobExp { get => Model?.JobExp; set => ExecuteCommand(value); }
		public string MvpExp { get => Model?.MvpExp; set => ExecuteCommand(value); }
		public string Attack { get => Model?.Attack; set { ExecuteCommand(value); OnPropertyChanged(nameof(AttackPreview)); } }
		public string Attack2 { get => Model?.Attack2; set { ExecuteCommand(value); OnPropertyChanged(nameof(MagicAttackPreview)); } }
		public string Defense { get => Model?.Defense; set => ExecuteCommand(value); }
		public string MagicDefense { get => Model?.MagicDefense; set => ExecuteCommand(value); }
		public string Resistance { get => Model?.Resistance; set => ExecuteCommand(value); }
		public string MagicResistance { get => Model?.MagicResistance; set => ExecuteCommand(value); }
		public string Str { get => Model?.Str; set => ExecuteCommand(value); }
		public string Agi { get => Model?.Agi; set => ExecuteCommand(value); }
		public string Vit { get => Model?.Vit; set => ExecuteCommand(value); }
		public string Int { get => Model?.Int; set => ExecuteCommand(value); }
		public string Dex { get => Model?.Dex; set => ExecuteCommand(value); }
		public string Luk { get => Model?.Luk; set => ExecuteCommand(value); }
		public string AttackRange { get => Model?.AttackRange; set => ExecuteCommand(value); }
		public string SkillRange { get => Model?.SkillRange; set => ExecuteCommand(value); }
		public string ChaseRange { get => Model?.ChaseRange; set => ExecuteCommand(value); }
		public EnumInfoBase Size { get => EnumInfos.GetEnumBase(Model?.Size); set => ExecuteCommand((SizeType)value.Value); }
		public EnumInfoBase Race { get => EnumInfos.GetEnumBase(Model?.Race); set => ExecuteCommand((RaceType)value.Value); }
		public string RaceGroups { get => Model?.RaceGroups; set => ExecuteCommand(value); }
		public EnumInfoBase Element { get => EnumInfos.GetEnumBase(Model?.Element); set => ExecuteCommand((ElementType)value.Value); }
		public EnumInfoBase ElementLevel { get => EnumInfos.GetEnumBase(Model?.ElementLevel); set => ExecuteCommand((ElementLevelType)value.Value); }
		public string WalkSpeed { get => Model?.WalkSpeed; set => ExecuteCommand(value); }
		public string AttackDelay { get => Model?.AttackDelay; set => ExecuteCommand(value); }
		public string AttackMotion { get => Model?.AttackMotion; set => ExecuteCommand(value); }
		public string ClientAttackMotion { get => Model?.ClientAttackMotion; set => ExecuteCommand(value); }
		public string DamageMotion { get => Model?.DamageMotion; set => ExecuteCommand(value); }
		public string DamageTaken { get => Model?.DamageTaken; set => ExecuteCommand(value); }
		public string GroupId { get => Model?.GroupId; set => ExecuteCommand(value); }
		public string Title { get => Model?.Title; set => ExecuteCommand(value); }
		public string Ai { get => Model?.Ai; set => ExecuteCommand(value); }
		public EnumInfoBase Class { get => EnumInfos.GetEnumBase(Model?.Class); set => ExecuteCommand((Mobs.Common.ClassType)value.Value); }
		public string Modes { get => Model?.Modes; set => ExecuteCommand(value); }
		public string AliasSprite { get => Model?.AliasSprite; set => ExecuteCommand(value); }
		public string ClientSprite { get => Model?.ClientSprite; set => ExecuteCommand(value); }

		public string JapaneseNamePreview => Model == null || !String.IsNullOrEmpty(Model.JapaneseName) ? "" : Model.Name;
		public string AttackPreview {
			get {
				if (Model == null)
					return "";

				int value = DbReader.ToInt(Model?.Attack);
				// rAthena doesn't appear to take level and str into account
				// when calculating the attack...?
				int level = DbReader.ToInt(Model.Level);
				int str = DbReader.ToInt(Model.Str);
				int minAttack = str + level + value * 80 / 100;
				int maxAttack = str + level + value * 120 / 100;
				return $"{value} ({minAttack}~{maxAttack})";
			}
		}
		public string MagicAttackPreview {
			get {
				if (Model == null)
					return "";

				int value = DbReader.ToInt(Model?.Attack2);
				int level = DbReader.ToInt(Model.Level);
				int int_ = DbReader.ToInt(Model.Int);
				int minAttack = int_ + level + value * 70 / 100;
				int maxAttack = int_ + level + value * 130 / 100;

				return $"{value} ({minAttack}~{maxAttack})";
			}
		}
		public string WalkSpeedPreview {
			get {
				int value = DbReader.ToInt(Model?.WalkSpeed);

				if (value <= 0)
					return "";

				double speed = 1000.0 / value;
				var speedS = String.Format("{0:0.00}", speed).Replace(",", ".");
				return $"{value} - {speedS} c/s";
			}
		}
		public string ClientAttackMotionPreview {
			get {
				if (Model == null)
					return Model?.AttackMotion;

				if (String.IsNullOrEmpty(Model.ClientAttackMotion))
					return Model?.AttackMotion;

				return "";
			}
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}

		#region Validation
		private MergedTable _mobDb;

		private UpdateDispatcher _validateNameDispatcher = new UpdateDispatcher();

		public async void ValidateAegisName() {
			ClearErrors(nameof(AegisName));

			//var items = DbIOUtils.AegisMobName2List(AegisName);

			//if (!ProjectConfiguration.SynchronizeWithClientDatabases)
			//	return;

			if (_mobDb == null)
				_mobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);

			if (Tuple == null)
				return;

			try {
				//var jobTable = _tab.GetDb(ServerDbs.Mobs).Attached["jobtbl_T"] as Dictionary<string, string>;
				//
				//if (jobTable == null) return;

				var current = AegisName;

				if (String.IsNullOrEmpty(current)) {
					AddError(nameof(AegisName), "AegisName cannot be empty.");
				}

				int id = Tuple.Key;

				List<ReadableTuple> results = await Task.Run(delegate {
					return _mobDb.FastItems.Where(p => String.Compare(p.GetModel<Mob>().AegisName, current, true) == 0).Where(p => p.Key != Tuple.Key).ToList();
				});

				if (id != Tuple.Key)
					return;
				//var results = _mobDb.FastItems.Where(p => current.IndexOf(p.GetModel<Mob>().AegisName, StringComparison.OrdinalIgnoreCase) > -1).Where(p => p.Key != _tuple.Key).ToList();
				
				if (results.Count > 0) {
					AddError(nameof(AegisName), "Another item(s) already uses this AegisName:\r\n" + Methods.Aggregate(results.Select(p => p.Key + " - " + p.GetModel<Mob>().Name).ToList(), "\r\n"));
				}

				//var sid = _tuple.Key.ToString();
				//
				//foreach (var pair in jobTable.Where(p => p.Value == sid).ToList()) {
				//	jobTable.Remove(pair.Key);
				//}
			}
			catch (Exception err) {
				AddError(nameof(AegisName), "Generic error:\r\n" + err.Message);
			}
		}
		#endregion

		#region MvpDrops list
		public void ChangeMvpDrops(List<ItemDrop> drops, ListCommandMode mode) {
			if (drops.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.MvpDrops, drops, mode);
				OnMvpDropsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void OnMvpDropsListUpdated() {
			MvpDrops.ClearAndAddRange(Model == null ? new List<ItemDropViewModel>() : Model.MvpDrops.Select(p => new ItemDropViewModel(this, p, isMvp: true)));
		}

		public void CopyMvpDrops(List<ItemDrop> entries) => Copy<ItemDrop, MobWriterYaml>(entries, (v, writer, b) => writer.WriteItemDrop(b, v));
		#endregion

		#region Drops list
		public void ChangeDrops(List<ItemDrop> drops, ListCommandMode mode) {
			if (drops.Count == 0)
				return;

			try {
				IsLocked = true;
				Tab.Table.Commands.SetModelListValue(Tuple, () => Model.Drops, drops, mode);
				OnDropsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		public void OnDropsListUpdated() {
			Drops.ClearAndAddRange(Model == null ? new List<ItemDropViewModel>() : Model.Drops.Select(p => new ItemDropViewModel(this, p)));
		}

		public void CopyDrops(List<ItemDrop> entries) => Copy<ItemDrop, MobWriterYaml>(entries, (v, writer, b) => writer.WriteItemDrop(b, v));
		#endregion

		#region MobSkills list
		public void ChangeMobSkills(List<MobSkill> mobSkills, ListCommandMode mode) {
			if (mobSkills.Count == 0)
				return;

			Table<int, ReadableTuple> mobSkillsDb;
			var database = SdeEditor.Project;

			switch (mode) {
				case ListCommandMode.Remove:
					mobSkillsDb = database.GetMergedTable(DataSources.MobSkill);
					break;
				case ListCommandMode.Add:
					var isEnabled = database.GetDb(DataSources.MobSkillImport).IsEnabled;

					if (Tab.Database.Source == DataSources.Mob || !isEnabled)
						mobSkillsDb = database.GetTable(DataSources.MobSkill);
					else
						mobSkillsDb = database.GetTable(DataSources.MobSkillImport);
					break;
				default:
					throw new NotImplementedException();
			}

			try {
				IsLocked = true;

				mobSkillsDb.Commands.Begin();

				try {
					switch (mode) {
						case ListCommandMode.Remove:
							for (int i = 0; i < mobSkills.Count; i++) {
								var mobSkill = mobSkills[i];
								mobSkillsDb.Commands.Delete(MobSkills2Tuple[mobSkill].Key);
							}
							break;
						case ListCommandMode.Add:
							for (int i = 0; i < mobSkills.Count; i++) {
								var mobSkill = mobSkills[i];
								mobSkill.MobId = Id;
								
								var d = mobSkill.FriendlyDisplay.Split(new char[] { '@' }, 2);
								if (d.Length > 1)
									mobSkill.FriendlyDisplay = DbUtilities.MobId2Name(Id) + "@" + d[1];

								int uid = mobSkillsDb.GenerateUniqueId();
								ReadableTuple item = new ReadableTuple(uid, mobSkillsDb.AttributeList);
								item.Added = true;
								item.SetRawValue(1, mobSkill);

								mobSkillsDb.Commands.AddTuple(uid, item, false);
							}
							break;
					}
				}
				catch (Exception err) {
					ErrorHandler.HandleException(err);
				}
				finally {
					mobSkillsDb.Commands.End();
				}
				
				OnMobSkillsListUpdated();
			}
			finally {
				IsLocked = false;
			}
		}

		private List<ReadableTuple> _tuplesWithEvents = new List<ReadableTuple>();
		public Dictionary<MobSkill, ReadableTuple> MobSkills2Tuple = new Dictionary<MobSkill, ReadableTuple>();

		public void OnMobSkillsListUpdated() {
			List<MobSkillViewModel> viewModels = new List<MobSkillViewModel>();

			foreach (var tuple in _tuplesWithEvents) {
				tuple.PropertyChanged -= _mobSkillTuple_PropertyChanged;
			}

			_tuplesWithEvents.Clear();
			MobSkills2Tuple.Clear();

			if (Model != null) {
				var skillsDb = SdeEditor.Project.GetMergedTable(DataSources.Skill);
				var sid = Tuple.Key.ToString();

				var cache = DbUtilities.CacheMob2MobSkills();

				if (cache.TryGetValue(Tuple.Key, out var linkDict)) {
					foreach (var tuple in linkDict) {
						var model = tuple.GetRawValue<MobSkill>(MobSkillAttributes.Model);
						viewModels.Add(new MobSkillViewModel(null) { Model = model, Tuple = tuple });

						tuple.PropertyChanged += _mobSkillTuple_PropertyChanged;
						_tuplesWithEvents.Add(tuple);

						var skillTuple = skillsDb.TryGetTuple(model.IntSkillId);

						if (skillTuple != null) {
							skillTuple.PropertyChanged += _mobSkillTuple_PropertyChanged;
							_tuplesWithEvents.Add(skillTuple);
						}

						MobSkills2Tuple[model] = tuple;
					}
				}
			}

			MobSkills.ClearAndAddRange(viewModels);
		}

		private void _mobSkillTuple_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			OnMobSkillsListUpdated();
		}

		public void CopyMobSkills(List<MobSkill> entries) => Copy<MobSkill, MobSkillWriterCsv>(entries, (v, writer, b) => writer.WriteEntry(b, v));
		#endregion
	}
}
