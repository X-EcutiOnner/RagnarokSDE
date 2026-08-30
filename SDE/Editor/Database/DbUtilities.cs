using System;
using System.CodeDom;
using System.Collections.Generic;
using Database;
using SDE.Databases;
using SDE.Databases.Items.Features;
using SDE.Databases.Mobs;
using SDE.Databases.Mobs.Features;
using SDE.Databases.MobSkills;
using SDE.Databases.MobSkills.Features;
using SDE.Databases.Skills.Features;
using SDE.View;
using Utilities;

namespace SDE.Editor.Database {
	public class CachedDbAccessor {
		private bool _dirty;
		public Dictionary<string, int> _field2Id = new Dictionary<string, int>();
		public MergedTable MergedTable;
		public string FieldName { get; }
		public DataSource Source { get; }
		public Func<object, string> _accessor;

		public CachedDbAccessor(DataSource source, string fieldName) {
			Source = source;
			MergedTable = SdeEditor.Project.GetMergedTable(source);
			FieldName = fieldName;
			var modelAttribute = MergedTable.AttributeList[DbAttribute.DefaultModel.Index];
			_accessor = ReflectionOptimizer<string>.GetGetter(modelAttribute.DataType, fieldName);
		}

		public string ToStringId(string name) {
			return ToId(name).ToString();
		}

		public object ToId(string name) {
			CacheDatabase();

			if (Int32.TryParse(name, out int key))
				return key;

			if (_field2Id.TryGetValue(name, out key))
				return key;

			return name;
		}

		public int ToIntId(string name) {
			var res = ToId(name);

			if (res is int key)
				return key;

			throw new Exception("The associated entry was not found for '" + name + "' in '" + Source + "'.");
		}

		public void CacheDatabase() {
			if (!_dirty)
				return;

			_field2Id = new Dictionary<string, int>();

			foreach (var entry in MergedTable.FastItems) {
				_field2Id[_accessor(entry.GetModel())] = entry.Key;
			}

			_dirty = false;
		}

		public void Dirty() {
			_dirty = true;
		}
	}

	public static class CachedDbs {
		public static CachedDbAccessor AegisNameItem = new CachedDbAccessor(DataSources.Item, nameof(Item.AegisName));
		public static CachedDbAccessor AegisNameMob = new CachedDbAccessor(DataSources.Mob, nameof(Mob.AegisName));
		public static CachedDbAccessor SkillName = new CachedDbAccessor(DataSources.Skill, nameof(Skill.Name));
	}

	public static class DbUtilities {
		public static string MobId2Name(IntOrString id, Table<int, ReadableTuple> db = null) {
			if (!id.TryGetInt(out int parsedId, out string failResult)) return failResult;
			db = db ?? SdeEditor.Project.GetMergedTable(DataSources.Mob);
			return Id2ModelField<Mob>(db, MobAttributes.Model, parsedId, m => m.Name);
		}

		public static string ItemId2Name(IntOrString id, Table<int, ReadableTuple> db = null) {
			if (!id.TryGetInt(out int parsedId, out string failResult)) return failResult;
			db = db ?? SdeEditor.Project.GetMergedTable(DataSources.Item);
			return Id2ModelField<Item>(db, MobAttributes.Model, parsedId, m => m.Name);
		}

		public static string MobId2AegisName(IntOrString id, Table<int, ReadableTuple> db = null) {
			if (!id.TryGetInt(out int parsedId, out string failResult)) return failResult;
			db = db ?? SdeEditor.Project.GetMergedTable(DataSources.Mob);
			return Id2ModelField<Mob>(db, MobAttributes.Model, parsedId, m => m.AegisName);
		}

		public static string ItemId2AegisName(IntOrString id, Table<int, ReadableTuple> db = null) {
			if (!id.TryGetInt(out int parsedId, out string failResult)) return failResult;
			db = db ?? SdeEditor.Project.GetMergedTable(DataSources.Item);
			return Id2ModelField<Item>(db, MobAttributes.Model, parsedId, m => m.AegisName);
		}

		public static string SkillId2Description(IntOrString id, Table<int, ReadableTuple> db = null) {
			if (!id.TryGetInt(out int parsedId, out string failResult)) return failResult;
			db = db ?? SdeEditor.Project.GetMergedTable(DataSources.Skill);
			return Id2ModelField<Skill>(db, MobAttributes.Model, parsedId, m => m.Description);
		}

		public static string Id2ModelField<TModel>(Table<int, ReadableTuple> table, DbAttribute modelAttribute, int id, Func<TModel, string> getter) {
			var item = table.TryGetTuple(id);

			if (item == null)
				return id.ToString();

			return getter(item.GetValue<TModel>(modelAttribute));
		}

		public static string MobPreview(string mob) {
			if (string.IsNullOrEmpty(mob))
				return "";

			if (!int.TryParse(mob, out int value) || value <= 0)
				return "";

			MergedTable table = SdeEditor.Project.GetMergedTable(DataSources.Mob);
			ReadableTuple tuple = table.TryGetTuple(value);

			if (tuple == null)
				return "";

			return (tuple.GetModel<Mob>().Name ?? "") + " (" + value + ")";
		}

		public static string ItemPreview(string item) {
			if (string.IsNullOrEmpty(item))
				return "";

			if (!int.TryParse(item, out int value) || value <= 0)
				return "";

			MergedTable table = SdeEditor.Project.GetMergedTable(DataSources.Item);
			ReadableTuple tuple = table.TryGetTuple(value);

			if (tuple == null)
				return "";

			return (tuple.GetModel<Item>().Name ?? "") + " (" + value + ")";
		}

		public static bool MobSkillDirty = true;
		private static Dictionary<int, List<ReadableTuple>> _cacheMob2MobSkills = new Dictionary<int, List<ReadableTuple>>();
		public static Dictionary<int, List<ReadableTuple>> CacheMob2MobSkills() {
			if (MobSkillDirty) {
				var mobSkillsDb = SdeEditor.Project.GetMergedTable(DataSources.MobSkill);

				foreach (var tuple in mobSkillsDb.FastItems) {
					var model = tuple.GetRawValue<MobSkill>(MobSkillAttributes.Model);
					var mobId = model.IntMobId;

					if (!_cacheMob2MobSkills.TryGetValue(mobId, out var l)) {
						l = new List<ReadableTuple>();
						_cacheMob2MobSkills[mobId] = l;
					}

					l.Add(tuple);
				}

				MobSkillDirty = false;
			}

			return _cacheMob2MobSkills;
		}
	}

	public readonly struct IntOrString {
		private readonly int _intValue;
		private readonly string _stringValue;
		private readonly bool _isString;

		public IntOrString(int value) {
			_intValue = value;
			_stringValue = null;
			_isString = false;
		}

		public IntOrString(string value) {
			_intValue = 0;
			_stringValue = value;
			_isString = true;
		}

		public static implicit operator IntOrString(int? val) => new IntOrString(val == null ? 0 : val.Value);
		public static implicit operator IntOrString(int val) => new IntOrString(val);
		public static implicit operator IntOrString(string val) => new IntOrString(val);

		public bool TryGetInt(out int result, out string output) {
			if (!_isString) {
				result = _intValue;
				output = null;
				return true;
			}
			if (string.IsNullOrEmpty(_stringValue)) {
				result = 0;
				output = "";
				return false;
			}

			output = _stringValue;
			return int.TryParse(_stringValue, out result);
		}
	}
}
