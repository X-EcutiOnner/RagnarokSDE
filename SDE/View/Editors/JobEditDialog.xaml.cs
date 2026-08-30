using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GrfToWpfBridge;
using SDE.ApplicationConfiguration;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Items.Features;
using SDE.View.Dialogs;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities;
using Utilities.Extension;

namespace SDE.View.Editors {
	public class JobGridData {
		public delegate void ModifiedEventHandler();
		public event ModifiedEventHandler Modified;

		private Panel[] _grids;
		public List<CheckBox> Boxes = new List<CheckBox>();
		private Dictionary<Job, (int RowIndex, int ColIndex, CheckBox Box)> _jobPos = new Dictionary<Job, (int RowIndex, int ColIndex, CheckBox Box)>();

		public JobGridData(params Panel[] grids) {
			_grids = grids;
		}

		public void Add(int col, Job job) {
			var grid = _grids[col];

			CheckBox box = new CheckBox();
			box.Margin = new Thickness(3);
			grid.Children.Add(box);

			if (job == null) {
				box.Visibility = Visibility.Hidden;
				return;
			}

			box.Content = job == Job.BardDancer ? Methods.Aggregate(job.Names, ", ") : job.Name;
			box.Click += (s, e) => Modified?.Invoke();
			WpfUtilities.AddMouseInOutUnderline(box);
			Boxes.Add(box);

			_jobPos[job] = (grid.Children.Count - 1, col, box);
		}

		public void Restrict(ItemJobFlag upper, GenderType gender, int equipLevel = 0) {
			foreach (var entry in _jobPos) {
				var job = entry.Key;
				var box = (CheckBox)_grids[entry.Value.ColIndex].Children[entry.Value.RowIndex];

				if (box.Content is TextBlock tb) {
					tb.Text = Job.Get(job, upper, equipLevel).GetName(gender);
				}
				else {
					box.Content = Job.Get(job, upper, equipLevel).GetName(gender);
					WpfUtilities.AddMouseInOutUnderline(box);
				}

				if (SdeAppConfiguration.RestrictToAllowedJobs) {
					List<Job> jobs = Job.AllJobs.Where(p => p.SecondJob == job && p.CanUseItem(upper)).ToList();

					box.IsEnabled = jobs.Count != 0;

					if (jobs.Count == 0)
						box.IsChecked = false;
				}
			}
		}

		public void Select(UInt64 jobs, ItemJobFlag upper, int equipLevel) {
			foreach (var entry in _jobPos) {
				var job = entry.Key;
				var box = (CheckBox)_grids[entry.Value.ColIndex].Children[entry.Value.RowIndex];

				if ((job.JobSdeUid & jobs) != 0) {
					box.IsChecked = true;
				}
				else {
					box.IsChecked = false;
				}
			}
		}

		public UInt64 CalculateSdeJobId() {
			UInt64 r = 0;

			foreach (var entry in _jobPos) {
				if (entry.Value.Box.IsChecked == true) {
					r |= entry.Key.JobSdeUid;
				}
			}

			var maxCount = Core.Extensions.PopCount((long)JobGroups.EverySdeJobs);
			var count = Core.Extensions.PopCount((long)r);

			if (count > 0.5 * maxCount)
				return r | ~JobGroups.EverySdeJobs;

			return r;
		}
	}

	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class JobEditDialog : TkWindow, IInputWindow {
		private bool _selectionEventsEnabled = true;
		private int _equipLevel;
		private UInt64 _sdeJobId;
		private ItemJobFlag _upper = ItemJobFlag.ITEMJ_ALL;
		private GenderType _gender = GenderType.SEX_BOTH;
		private JobGridData _jobs;

		public JobEditDialog(Item model)
			: base("Job edit", "cde.ico", SizeToContent.Height, ResizeMode.CanResize) {
			InitializeComponent();
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			_upper = model.Classes.ToFlag<ItemJobFlag>();
			_gender = model.Gender;
			_equipLevel = model.EquipLevelMin.ToInt();
			_sdeJobId = model.Jobs.ToUInt64();

			_initializeJobGrid();
			_initializeOptions();

			_selectJobs(_sdeJobId);
			_optionsUpdate();
		}

		private void _initializeOptions() {
			Binder.Bind(_cbRestrictClasses, () => SdeAppConfiguration.RestrictToAllowedJobs, _optionsUpdate);

			_cbSelectAll.IsChecked = _jobs.Boxes.All(p => p.IsEnabled && p.IsChecked == true);
			_cbSelectAll.Click += (sender, e) => _selectAll(_cbSelectAll.IsChecked == true);

			WpfUtilities.AddMouseInOutUnderline(_cbRestrictClasses, _cbSelectAll);
		}

		private void _initializeJobGrid() {
			_jobs = new JobGridData(_panel0, _panel1, _panel2, _panel3, _panel4, _panel5);

			_jobs.Add(0, Job.Swordman);     _jobs.Add(1, Job.Knight);           _jobs.Add(2, Job.Crusader);
			_jobs.Add(0, Job.Mage);         _jobs.Add(1, Job.Wizard);           _jobs.Add(2, Job.Sage);
			_jobs.Add(0, Job.Archer);       _jobs.Add(1, Job.Hunter);           _jobs.Add(2, Job.BardDancer);
			_jobs.Add(0, Job.Acolyte);      _jobs.Add(1, Job.Priest);           _jobs.Add(2, Job.Monk);
			_jobs.Add(0, Job.Merchant);     _jobs.Add(1, Job.Blacksmith);       _jobs.Add(2, Job.Alchemist);
			_jobs.Add(0, Job.Thief);        _jobs.Add(1, Job.Assassin);         _jobs.Add(2, Job.Rogue);

			_jobs.Add(3, Job.Taekwon);      _jobs.Add(4, Job.StarGladiator);    _jobs.Add(5, Job.SoulLinker);
			_jobs.Add(3, Job.Novice);       _jobs.Add(4, Job.SuperNovice);		_jobs.Add(5, null);
			_jobs.Add(3, Job.Gunslinger);   _jobs.Add(4, Job.Rebellion);        _jobs.Add(5, null);
			_jobs.Add(3, Job.Ninja);        _jobs.Add(4, Job.KagerouOboro);     _jobs.Add(5, null);
			_jobs.Add(3, Job.Summoner);     _jobs.Add(4, Job.SpiritHandler);    _jobs.Add(5, null);
			_jobs.Add(3, Job.Druid);        _jobs.Add(4, Job.Karnos);           _jobs.Add(5, null);

			_jobs.Modified += _selectionUpdate;

			//if (DbPathLocator.DetectPath())
		}

		private void _selectJobs(UInt64 jobs) {
			_jobs.Select(jobs, _upper, _equipLevel);
		}

		private void _selectAll(bool v) {
			try {
				_selectionEventsEnabled = false;
				_jobs.Boxes.Where(p => p.IsEnabled).ToList().ForEach(p => p.IsChecked = v);
			}
			finally {
				_selectionEventsEnabled = true;
			}

			_selectionUpdate();
		}

		public string Text => "0x" + _sdeJobId.ToString("X16");
		public Grid Footer => _footerGrid;
		public event Action ValueChanged;

		public void OnValueChanged() => ValueChanged?.Invoke();

		private void _updateJobStringPreview() {
			_previewClass.Text = JobOperations.GetStringFormat(_sdeJobId, _upper, _gender, _equipLevel);
		}

		private void _optionsUpdate() {
			_jobs.Boxes.ForEach(p => p.IsEnabled = true);
			_jobs.Restrict(_upper, _gender, _equipLevel);
			_selectionUpdate();
		}

		private void _selectionUpdate() {
			if (!_selectionEventsEnabled) return;

			try {
				_selectionEventsEnabled = false;

				var oldJobId = _sdeJobId;
				_sdeJobId = _jobs.CalculateSdeJobId();

				_updateJobStringPreview();
				_cbSelectAll.IsChecked = _jobs.Boxes.Where(p => p.IsEnabled).All(p => p.IsChecked == true);
				
				if (oldJobId != _sdeJobId)
					OnValueChanged();
			}
			finally {
				_selectionEventsEnabled = true;
			}
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			if (!SdeAppConfiguration.UseIntegratedDialogsForJobs)
				DialogResult = true;
			Close();
		}
	}
}
