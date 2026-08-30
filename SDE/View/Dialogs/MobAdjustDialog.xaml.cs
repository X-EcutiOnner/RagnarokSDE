using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Database;
using ErrorManager;
using GrfToWpfBridge;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using SDE.View.Editors;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities.Extension;

namespace SDE.View.Dialogs {
	public struct MobStatData {
		public int Level;
		public int Str;
		public int Agi;
		public int Vit;
		public int Int;
		public int Dex;
		public int Luk;
	}

	/// <summary>
	/// Interaction logic for ItemDescriptionDialog.xaml
	/// </summary>
	public partial class MobAdjustDialog : TkWindow {
		private readonly SdeEditor _editor;
		private BaseDatabase _mobDb;
		private bool _eventsDisabled = true;
		private ReadableTuple _mobTuple;
		private Func<double, double>[] _formulas = new Func<double, double>[6];
		private Mob _model;
		private bool _bind1 = false;
		private bool _bind2 = false;
		private MobStatData _srcStats;
		private MobStatData _dstStats;

		public event RoutedEventHandler Apply;
		public void OnApply(RoutedEventArgs e) => Apply?.Invoke(this, e);

		public MobAdjustDialog() : base("Mob stats edit", "properties.png") {
			InitializeComponent();
		}

		public MobAdjustDialog(SdeEditor editor) : base("Mob stats edit", "properties.png") {
			_editor = editor;
			InitializeComponent();

			Binder.Bind(_cbOldFormula, () => SdeAppConfiguration.MobAdjustOldFormula, () => AdjustMobStats());
			WpfUtilities.AddMouseInOutUnderline(_cbOldFormula);

			_gpRate.Minimum = 0;
			_gpRate.Maximum = Limit;
			_gpRate.ValueChanged += _gpRate_ValueChanged;
			_editor.SelectionChanged += _editor_SelectionChanged;

			_initializeFormulas();

			OnTupleSelected();
		}

		public ReadableTuple GetSelectedMobTuple() {
			DbTab tab = _editor.FindTopmostTab();

			if (tab == null || tab.Database.Source != DataSources.Mob && tab.Database.Source != DataSources.MobImport) {
				return null;
			}

			return tab.SelectedItem;
		}

		private void _initializeFormulas() {
			_formulas[0] = v => 0.0012 * v * v + 0.6902 * v;
			_formulas[1] = v => 0.0009 * v * v + 0.5517 * v;
			_formulas[2] = v => 0.0009 * v * v + 0.4902 * v;
			_formulas[3] = v => 0.0016 * v * v + 0.4162 * v;
			_formulas[4] = v => 0.0000 + 1.3349 * v;
			_formulas[5] = v => 7.9715 * Math.Pow(Math.E, 0.013 * v);

			for (int i = 0; i < 6; i++)
				_mult[i] = 1.1;

			_mult[1] = 0.6; // Agi
			_mult[5] = 0.8; // Luk
		}

		private void _editor_SelectionChanged(object sender, TabItem olditem, TabItem newitem) {
			OnTupleSelected();
		}

		public string Output { get; set; }
		private DbTab _tab;
		public ReadableTuple Item { get; private set; }
		public bool? Result { get; set; }

		private void OnTupleSelected() {
			try {
				var tuple = GetSelectedMobTuple();

				if (tuple == null)
					return;

				_tab = _editor.FindTopmostTab();
				_mobDb = _tab.Database;
				_mobTuple = _mobDb.Table.GetTuple(tuple.Key);
				_model = _mobTuple.GetModel<Mob>();

				if (_tab.Database.Source == DataSources.Mob && !_bind1) {
					_bind1 = true;
					WeakEventManager<ListView, SelectionChangedEventArgs>.AddHandler(_tab.ListView, nameof(ListView.SelectionChanged), _listView_SelectionChanged);
				}
				if (_tab.Database.Source == DataSources.MobImport && !_bind2) {
					_bind2 = true;
					WeakEventManager<ListView, SelectionChangedEventArgs>.AddHandler(_tab.ListView, nameof(ListView.SelectionChanged), _listView_SelectionChanged);
				}

				SetSourceStats(tuple.GetModel<Mob>());
				SetEditorValues(_srcStats);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			OnTupleSelected();
		}

		private void _gpRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (_eventsDisabled) return;

			try {
				_eventsDisabled = true;
				_dstStats.Level = (int)Math.Round(_gpRate.Value, MidpointRounding.AwayFromZero);
				AdjustMobStats();
			}
			finally {
				_eventsDisabled = false;
			}
		}

		public float Limit = 400;
		private readonly double[] _mult = new double[6];
		private readonly int[] _results = new int[6];

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Result = false;
			Close();
		}

		public int CalculateStat(int srcStat, int i, double levelDiff) {
			double value;

			if (SdeAppConfiguration.MobAdjustOldFormula) {
				value = srcStat + (double)srcStat / _srcStats.Level * levelDiff * _mult[i];
			}
			else {
				var stat_diff = srcStat - _formulas[i](_srcStats.Level);
				value = _formulas[i](_dstStats.Level) + stat_diff;
			}

			if (value < 0)
				value = 0;

			return (int)Math.Round(value, MidpointRounding.AwayFromZero);
		}

		public void AdjustMobStats() {
			double diff = _dstStats.Level - _srcStats.Level;

			_dstStats.Str = CalculateStat(_srcStats.Str, 0, diff);
			_dstStats.Agi = CalculateStat(_srcStats.Agi, 0, diff);
			_dstStats.Vit = CalculateStat(_srcStats.Vit, 0, diff);
			_dstStats.Int = CalculateStat(_srcStats.Int, 0, diff);
			_dstStats.Dex = CalculateStat(_srcStats.Dex, 0, diff);
			_dstStats.Luk = CalculateStat(_srcStats.Luk, 0, diff);

			SetEditorValues(_dstStats);
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter)
				_buttonOk_Click(null, null);

			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			try {
				_mobDb.Table.Commands.Begin();
				_mobDb.Table.Commands.SetModelValue(_mobTuple, _model, nameof(Mob.Str), _dstStats.Str.ToString());
				_mobDb.Table.Commands.SetModelValue(_mobTuple, _model, nameof(Mob.Agi), _dstStats.Agi.ToString());
				_mobDb.Table.Commands.SetModelValue(_mobTuple, _model, nameof(Mob.Vit), _dstStats.Vit.ToString());
				_mobDb.Table.Commands.SetModelValue(_mobTuple, _model, nameof(Mob.Int), _dstStats.Int.ToString());
				_mobDb.Table.Commands.SetModelValue(_mobTuple, _model, nameof(Mob.Dex), _dstStats.Dex.ToString());
				_mobDb.Table.Commands.SetModelValue(_mobTuple, _model, nameof(Mob.Luk), _dstStats.Luk.ToString());
			}
			catch (Exception err) {
				_mobDb.Table.Commands.CancelEdit();
				ErrorHandler.HandleException(err);
			}
			finally {
				_mobDb.Table.Commands.End();
				_tab.Update();
			}
		}

		private void _tbImport_Click(object sender, RoutedEventArgs e) {
			Table<int, ReadableTuple> btable = SdeEditor.Project.GetMergedTable(DataSources.Mob);
			SelectTupleDialog select = new SelectTupleDialog(btable, DataSources.Mob, "");
			select.Owner = WpfUtilities.TopWindow;

			if (select.ShowDialog() == true) {
				ReadableTuple tuple = btable.GetTuple(select.Id.ToInt());

				// Set UI values to the loaded mob
				SetSourceStats(tuple.GetModel<Mob>());
				SetEditorValues(_srcStats);
			}
		}

		public void SetEditorValues(MobStatData stats) {
			try {
				_eventsDisabled = true;
				_gpRate.Value = stats.Level;
				_tbStr.Text = stats.Str.ToString();
				_tbAgi.Text = stats.Agi.ToString();
				_tbVit.Text = stats.Vit.ToString();
				_tbInt.Text = stats.Int.ToString();
				_tbDex.Text = stats.Dex.ToString();
				_tbLuk.Text = stats.Luk.ToString();
				_tbLevel.Text = stats.Level.ToString();
			}
			finally {
				_eventsDisabled = false;
			}
		}

		public void SetSourceStats(Mob model) {
			_srcStats.Level = DbReader.ToInt(model.Level);
			_srcStats.Str = DbReader.ToInt(model.Str);
			_srcStats.Agi = DbReader.ToInt(model.Agi);
			_srcStats.Vit = DbReader.ToInt(model.Vit);
			_srcStats.Int = DbReader.ToInt(model.Int);
			_srcStats.Dex = DbReader.ToInt(model.Dex);
			_srcStats.Luk = DbReader.ToInt(model.Luk);
			_dstStats = _srcStats;
		}
	}
}
