using System;
using System.Collections.Generic;
using SDE.Databases;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Items.Common;
using SDE.Databases.Items.Features;
using SDE.Editor.Database;
using SDE.Editor.LuaTables;
using SDE.View;
using Utilities.Extension;

namespace SDE.Editor.Engines.PreviewEngine {
	public interface IViewIdPreview {
		int SuggestedAction { get; }
		bool CanRead(ReadableTuple tuple);
		void Read(ReadableTuple tuple, PreviewHelper helper, List<Job> jobs);
		string GetSpriteFromJob(ReadableTuple tuple, PreviewHelper helper);
	}

	public class HeadgearPreview : IViewIdPreview {
		#region IViewIdPreview Members
		public int SuggestedAction {
			get { return 33; }
		}

		public bool CanRead(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();
			return model.Type == ItemType.IT_ARMOR && (model.Locations.ToLong() & 7937) != 0;
		}

		public void Read(ReadableTuple tuple, PreviewHelper helper, List<Job> jobs) {
			helper.PreviewSprite = LuaHelper.GetSpriteFromViewId(tuple.GetModel<Item>().View.ToInt(), LuaHelper.ViewIdTypes.Headgear, tuple);

			if (String.IsNullOrEmpty(helper.PreviewSprite)) {
				helper.PreviewSprite = null;
				helper.SetError(PreviewHelper.ViewIdNotSet);
				return;
			}

			helper.SetJobs(jobs);
		}

		public string GetSpriteFromJob(ReadableTuple tuple, PreviewHelper helper) {
			if (helper.PreviewSprite == PreviewHelper.SpriteNone)
				return helper.PreviewSprite;

			return LuaHelper.GetSpriteFromJob(helper.Grf, helper.Job, helper, helper.PreviewSprite, LuaHelper.ViewIdTypes.Headgear) + ".act";
		}
		#endregion
	}

	public class ShieldPreview : IViewIdPreview {
		#region IViewIdPreview Members
		public int SuggestedAction {
			get { return 33; }
		}

		public bool CanRead(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();
			return model.Type == ItemType.IT_ARMOR && model.Locations.ToLong() == 32;
		}

		public void Read(ReadableTuple tuple, PreviewHelper helper, List<Job> jobs) {
			helper.PreviewSprite = LuaHelper.GetSpriteFromViewId(tuple.GetModel<Item>().View.ToInt(), LuaHelper.ViewIdTypes.Shield, tuple);

			if (helper.PreviewSprite == null) {
				helper.SetError(PreviewHelper.ViewIdNotSet);
				return;
			}

			helper.SetJobs(jobs);
		}

		public string GetSpriteFromJob(ReadableTuple tuple, PreviewHelper helper) {
			return LuaHelper.GetSpriteFromJob(helper.Grf, helper.Job, helper, helper.PreviewSprite, LuaHelper.ViewIdTypes.Shield) + ".act";
		}
		#endregion
	}

	public class WeaponPreview : IViewIdPreview {
		#region IViewIdPreview Members
		public int SuggestedAction {
			get { return 33; }
		}

		public bool CanRead(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();
			return model.Type == ItemType.IT_ARMOR;
		}

		public void Read(ReadableTuple tuple, PreviewHelper helper, List<Job> jobs) {
			// Try to get the ViewId from the client info table instead
			var itemModel = tuple.GetModel<Item>();
			int viewId = itemModel.View.ToInt();

			if (String.IsNullOrEmpty(itemModel.View)) {
				var cTuple = SdeEditor.Project.GetDb(DataSources.ClientItem).Table.TryGetTuple(tuple.Key);

				if (cTuple != null) {
					viewId = cTuple.GetModel<ClientItem>().ClassNumber.ToInt();
				}
			}
			
			helper.PreviewSprite = LuaHelper.GetSpriteFromViewId(viewId, LuaHelper.ViewIdTypes.Weapon, tuple);

			if (helper.PreviewSprite == null) {
				helper.SetError(PreviewHelper.ViewIdNotSet);
				return;
			}

			helper.SetJobs(jobs);
		}

		public string GetSpriteFromJob(ReadableTuple tuple, PreviewHelper helper) {
			return LuaHelper.GetSpriteFromJob(helper.Grf, helper.Job, helper, helper.PreviewSprite, LuaHelper.ViewIdTypes.Weapon) + ".act";
		}
		#endregion
	}

	public class GarmentPreview : IViewIdPreview {
		#region IViewIdPreview Members
		public int SuggestedAction {
			get { return 9; }
		}

		public bool CanRead(ReadableTuple tuple) {
			var model = tuple.GetModel<Item>();
			return model.Type == ItemType.IT_ARMOR && (model.Locations.ToLong() == 4 || model.Locations.ToLong() == 8192);
		}

		public void Read(ReadableTuple tuple, PreviewHelper helper, List<Job> jobs) {
			helper.PreviewSprite = LuaHelper.GetSpriteFromViewId(tuple.GetModel<Item>().View.ToInt(), LuaHelper.ViewIdTypes.Garment, tuple);

			if (helper.PreviewSprite == null) {
				helper.SetError(PreviewHelper.ViewIdNotSet);
				return;
			}

			helper.SetJobs(jobs);
		}

		public string GetSpriteFromJob(ReadableTuple tuple, PreviewHelper helper) {
			return LuaHelper.GetSpritePathFromJob(helper.Job, @"data\sprite\로브\" + helper.PreviewSprite + @"\" + helper.GenderString + "\\{0}_" + helper.GenderString, helper.Gender, helper.PreviewSprite) + ".act";
		}

		public string GetSprite2FromJob(PreviewHelper helper) {
			return $@"data\sprite\로브\{helper.PreviewSprite}\{helper.PreviewSprite}.spr".ToDisplayEncoding();
		}
		#endregion
	}

	public class NpcPreview : IViewIdPreview {
		#region IViewIdPreview Members
		public int SuggestedAction {
			get { return 4; }
		}

		public bool CanRead(ReadableTuple tuple) {
			return false;
		}

		public void Read(ReadableTuple tuple, PreviewHelper helper, List<Job> jobs) {
			helper.PreviewSprite = LuaHelper.GetSpriteFromViewId(helper.ViewId, LuaHelper.ViewIdTypes.Npc, tuple);

			if (helper.PreviewSprite == null) {
				helper.SetError(PreviewHelper.ViewIdNotSet);
				return;
			}

			helper.SetJobs(new List<Job>());
		}

		public string GetSpriteFromJob(ReadableTuple tuple, PreviewHelper helper) {
			var name = LuaHelper.GetSpriteFromJob(helper.Grf, null, helper, helper.PreviewSprite, LuaHelper.ViewIdTypes.Npc);
			if (name.EndsWith(".gr2"))
				return name;
			return name + ".act";
		}
		#endregion
	}

	public class NullPreview : IViewIdPreview {
		#region IViewIdPreview Members
		public int SuggestedAction {
			get { return 0; }
		}

		public bool CanRead(ReadableTuple tuple) {
			return true;
		}

		public void Read(ReadableTuple tuple, PreviewHelper helper, List<Job> jobs) {
			helper.SetError("Item type not supported.");
		}

		public string GetSpriteFromJob(ReadableTuple tuple, PreviewHelper helper) {
			return PreviewHelper.SpriteNone;
		}
		#endregion
	}
}