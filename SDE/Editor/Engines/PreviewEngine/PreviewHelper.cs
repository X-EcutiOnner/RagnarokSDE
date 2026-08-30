using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErrorManager;
using GRF.Core.GroupedGrf;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.SprFormat;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Items.Features;
using SDE.Editor.Database;
using SDE.View;
using SDE.View.Editors;
using TokeiLibrary;
using TokeiLibrary.WPF;
using TokeiLibrary.WpfBugFix;
using Utilities.Extension;
using Utilities.Services;
using static SDE.Databases.Generic.Controls.MobSpriteImage;

namespace SDE.Editor.Engines.PreviewEngine {
	public class PreviewHelper {
		public const string SpriteDefault = "default";
		public const string SpriteNone = "none";
		public const string ViewIdNotSet = "Lua error: view ID not associated.";
		public const string ViewIdIncrease = "Weapon: increase view ID";

		private readonly Act _bodyReferenceDefault;
		private readonly Act _emptyAct = new Act(new Spr());
		private readonly Border _gridSpriteMissing;
		private readonly Act _headReferenceDefault;
		private readonly RangeObservableCollection<Job> _jobs = new RangeObservableCollection<Job>();
		private readonly ListView _listView;
		private FrameRendererEditor _editor;
		private readonly List<IViewIdPreview> _previews = new List<IViewIdPreview>();
		private readonly TextBox _tbSpriteMissing;
		private IViewIdPreview _lastMatch;
		private ReadableTuple _lastTuple;
		private object _oldJob;
		private GenderType? _overrideGender;
		private MultiGrfReader _metaGrf;
		private int _viewId;
		private ViewIdActs _viewIdActs;

		public Act DefaultBodyReference => _bodyReferenceDefault;
		public Act DefaultHeadReference => _headReferenceDefault;

		public int ViewId {
			get { return _viewId; }
			set {
				_viewId = value;
				LastestViewId = value;
			}
		}

		public static int LastestViewId { get; set; }

		public PreviewHelper(RangeListView listView, BaseDatabase db) : this(listView, db, null, null, null, null) {

		}

		public PreviewHelper(RangeListView listView, BaseDatabase db, FrameRendererEditor editor, Border gridSpriteMissing, TextBox tbSpriteMissing, ViewIdActs viewIdActs) {
			_listView = listView;
			_editor = editor;
			_gridSpriteMissing = gridSpriteMissing;
			_tbSpriteMissing = tbSpriteMissing;
			_listView.ItemsSource = _jobs;
			_viewIdActs = viewIdActs;
			_metaGrf = SdeEditor.MetaGrf;

			Db = db;

			_headReferenceDefault = new Act(ApplicationManager.GetResource("ref_head.act"), new Spr(ApplicationManager.GetResource("ref_head.spr")));
			_bodyReferenceDefault = new Act(ApplicationManager.GetResource("ref_body.act"), new Spr(ApplicationManager.GetResource("ref_body.spr")));

			if (_viewIdActs != null) {
				_viewIdActs.Head = _headReferenceDefault;
				_viewIdActs.Body = _bodyReferenceDefault;
			}

			_listView.SelectionChanged += _jobChanged;
			_listView.PreviewMouseDown += _listView_PreviewMouseDown;
			_listView.PreviewMouseUp += _listView_PreviewMouseDown;

			_previews.Add(new HeadgearPreview());
			_previews.Add(new ShieldPreview());
			_previews.Add(new WeaponPreview());
			_previews.Add(new GarmentPreview());
			_previews.Add(new NpcPreview());
			_previews.Add(new NullPreview());

			for (int i = 0; i < 104; i++)
				_emptyAct.AddAction();
		}

		public string PreviewSprite { get; set; }
		public BaseDatabase Db { get; private set; }

		public Job Job {
			get { return _listView.SelectedItem as Job; }
		}

		public IEnumerable<Job> AllJobs {
			get { return _jobs; }
		}

		public Job PreferredJob { get; set; }
		protected bool KeepPreviousPreviewPosition { get; private set; }

		public MultiGrfReader Grf {
			get { return _metaGrf; }
		}

		public GenderType Gender {
			get {
				if (_overrideGender != null)
					return _overrideGender.Value;

				var gender = _lastTuple.GetModel<Item>().Gender;

				if (gender == GenderType.SEX_BOTH)
					return GenderType.SEX_MALE;
				return gender;
			}
		}

		public string GenderString {
			get { return EncodingService.FromAnyToDisplayEncoding(Gender == GenderType.SEX_MALE ? "남" : "여"); }
		}

		private void _listView_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			try {
				ListViewItem item = _listView.GetObjectAtPoint<ListViewItem>(e.GetPosition(_listView));

				if (item != null)
					PreferredJob = item.Content as Job;
			}
			catch {
			}
		}

		public void SetJobs(List<Job> jobs) {
			_jobs.Clear();

			List<Job> j1 = jobs.Where(p => p.IsBaby).ToList();
			List<Job> j2 = jobs.Where(p => !p.IsBaby).ToList();

			_jobs.AddRange(j2);
			_jobs.AddRange(j1);
		}

		public void RemoveJobs() {
			_oldJob = _listView.SelectedItem;
			_jobs.Clear();
			ResetPreview();
		}

		public void ResetPreview() {
			_viewIdActs.Body = _bodyReferenceDefault;
		}

		public Act GetBodySprite(Job job, string gender = "남") {
			byte[] jobActionData;
			byte[] jobSpriteData;

			if (job.BaseJob == Job.Summoner) {
				jobActionData = Grf.GetData(EncodingService.FromAnyToDisplayEncoding(@"data\sprite\도람족\몸통\" + gender + "\\" + job.GetResource(Gender) + EncodingService.FromAnyToDisplayEncoding("_" + gender + ".act")));
				jobSpriteData = Grf.GetData(EncodingService.FromAnyToDisplayEncoding(@"data\sprite\도람족\몸통\" + gender + "\\" + job.GetResource(Gender) + EncodingService.FromAnyToDisplayEncoding("_" + gender + ".spr")));
			}
			else {
				jobActionData = Grf.GetData(EncodingService.FromAnyToDisplayEncoding(@"data\sprite\인간족\몸통\" + gender + "\\" + job.GetResource(Gender) + EncodingService.FromAnyToDisplayEncoding("_" + gender + ".act")));
				jobSpriteData = Grf.GetData(EncodingService.FromAnyToDisplayEncoding(@"data\sprite\인간족\몸통\" + gender + "\\" + job.GetResource(Gender) + EncodingService.FromAnyToDisplayEncoding("_" + gender + ".spr")));
			}

			if (jobActionData == null || jobSpriteData == null) {
				AddError("resource error: sprite for job '" + job.Name + "' not found.");
				return DefaultBodyReference;
			}

			return new Act(jobActionData, new Spr(jobSpriteData));
		}

		public List<string> TestItem(ReadableTuple tuple, MultiGrfReader grf, Type compare = null) {
			var result = new List<string>();
			_metaGrf = grf;
			_lastTuple = tuple;

			foreach (var preview in _previews) {
				if (preview.CanRead(tuple) && !(preview is NullPreview) && (compare == null || preview.GetType() == compare)) {
					var model = tuple.GetModel<Item>();
					var jobs = JobOperations.GetJobs(model.Jobs.ToUInt64(), model.Classes.ToFlag<ItemJobFlag>());
					preview.Read(tuple, this, jobs);

					_jobs.Clear();
					_jobs.AddRange(jobs);

					if (PreviewSprite == SpriteNone || PreviewSprite == null)
						return result;

					var gender = _lastTuple.GetModel<Item>().Gender;

					foreach (var job in jobs) {
						_listView.SelectedItem = job;

						if (_listView.SelectedItem == null) {
							continue;
						}

						if (gender == GenderType.SEX_BOTH || gender == GenderType.SEX_FEMALE) {
							_overrideGender = GenderType.SEX_FEMALE;

							var act = preview.GetSpriteFromJob(tuple, this);
							var spr = act.ReplaceExtension(".spr");

							result.Add(act);
							result.Add(spr);
						}

						if (gender == GenderType.SEX_BOTH || gender == GenderType.SEX_MALE) {
							_overrideGender = GenderType.SEX_MALE;

							var act = preview.GetSpriteFromJob(tuple, this);
							var spr = act.ReplaceExtension(".spr");

							result.Add(act);
							result.Add(spr);
						}

						_overrideGender = null;
					}
					break;
				}
			}

			return result;
		}

		public void Read(ReadableTuple tuple) {
			PreviewSprite = null;
			KeepPreviousPreviewPosition = true;
			RemoveJobs();
			RemoveError();
			List<Job> jobs;
			_lastTuple = tuple;
			_metaGrf = SdeEditor.MetaGrf;

			foreach (var preview in _previews) {
				if (preview.CanRead(tuple)) {
					if (_lastMatch != preview) {
						KeepPreviousPreviewPosition = false;
					}

					_lastMatch = preview;
					var model = tuple.GetModel<Item>();
					jobs = JobOperations.GetAllJobs(model.Jobs.ToUInt64(), model.Classes.ToFlag<ItemJobFlag>());
					preview.Read(tuple, this, jobs);
					break;
				}
			}

			if (_listView.Items.Count > 0) {
				_listView.SelectedItem = PreferredJob;

				if (_listView.SelectedItem == null) {
					if (_oldJob != null)
						_listView.SelectedItem = _oldJob;

					if (_listView.SelectedItem == null)
						_listView.SelectedIndex = 0;
				}
			}
			else {
				_updatePreview(SpriteDefault);
			}

			if (!KeepPreviousPreviewPosition) {
				_editor.IndexSelector.SelectedAction = _lastMatch.SuggestedAction;
			}
		}

		public void RemoveError() {
			_gridSpriteMissing.Visibility = Visibility.Collapsed;
			_tbSpriteMissing.Text = "";
		}

		public void SetError(string message) {
			if (_gridSpriteMissing == null) return;

			// The error needs to be removed to update the error again
			if (_gridSpriteMissing.Visibility != Visibility.Visible) {
				_gridSpriteMissing.Visibility = Visibility.Visible;
				_tbSpriteMissing.Text = message;
			}
		}

		public void AddError(string message) {
			_gridSpriteMissing.Visibility = Visibility.Visible;
			_tbSpriteMissing.Text += _tbSpriteMissing.Text == "" ? message : "\n" + message;
		}

		private void _jobChanged(object sender, SelectionChangedEventArgs e) {
			if (_gridSpriteMissing == null) return;
			Job job = _listView.SelectedItem as Job;
			if (job == null) return;
			RemoveError();

			try {
				_viewIdActs.Body = GetBodySprite(job, GenderString);
				_updatePreview(_lastMatch.GetSpriteFromJob(_lastTuple, this));
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _updatePreview(string sprite) {
			byte[] headActionData;
			byte[] headSpriteData;
			_editor.Act = _emptyAct;

			// Sprite has 3 states :
			// correct path - may not be found
			// null - do not update and show error
			// none - do not update and do not show error
			if (sprite == null) {
				AddError("Resource error: couldn't find the specified sprite.");
			}
			else if (sprite == SpriteDefault) {
				_viewIdActs.Body = DefaultBodyReference;
			}
			else if (sprite == SpriteNone) {
			}
			else {
				if (sprite.GetExtension() != null) {
					headActionData = Grf.GetData(sprite);
					headSpriteData = Grf.GetData(sprite.ReplaceExtension(".spr"));

					if (headSpriteData == null && _lastMatch is GarmentPreview garmentPreview) {
						headSpriteData = Grf.GetData(garmentPreview.GetSprite2FromJob(this));
					}

					if (headActionData != null && headSpriteData != null)
						_editor.Act = new Act(headActionData, new Spr(headSpriteData));

					if (headActionData == null || headSpriteData == null) {
						SetError(String.Format("Resource error: sprite(s) not found \n{0} - {1}\n{2} - {3}",
							sprite, headActionData == null ? "#MISSING" : "#FOUND", sprite.ReplaceExtension(".spr"), headSpriteData == null ? "#MISSING" : "#FOUND"));
					}
				}
			}

			_viewIdActs.Head.AnchoredTo = _viewIdActs.Body;
			_editor.Act.AnchoredTo = _viewIdActs.Head;

			_viewIdActs.IsGarment = false;
			_viewIdActs.Head.Commands.UndoAll();
			_viewIdActs.Body.Commands.UndoAll();
			_editor.Act?.Commands.UndoAll();

			if (Job != null && Job.Name.StartsWith("Baby ")) {
				_viewIdActs.Head.Commands.Backup(a => a.Magnify(0.75f, true));
				_viewIdActs.Body.Commands.Backup(a => a.Magnify(0.75f, true));
				_editor.Act?.Commands.Backup(a => a.Magnify(0.75f, true));
			}

			_editor.Act?.Safe();

			if (_lastMatch is GarmentPreview) {
				_viewIdActs.IsGarment = true;
			}

			_editor.OnActLoaded();
			_editor.IndexSelector.Init(_editor, _editor.PreferedLoadingAction, 0);
			_editor.FrameRenderer.Update();
		}
	}
}