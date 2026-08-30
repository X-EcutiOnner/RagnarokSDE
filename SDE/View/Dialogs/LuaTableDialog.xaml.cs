using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GRF.Core.GroupedGrf;
using GrfToWpfBridge;
using SDE.Core;
using SDE.Databases;
using SDE.Editor;
using SDE.Editor.Database;
using SDE.Editor.LuaTables;
using SDE.View.Controls;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace SDE.View.Dialogs {
	/// <summary>
	/// Interaction logic for NewMvpDrop.xaml
	/// </summary>
	public partial class LuaTableDialog : TkWindow {
		private readonly MultiGrfReader _multiGrf;
		private readonly ProjectManager _sdb;
		private readonly string _oldPath1;
		private readonly string _oldPath2;

		public LuaTableDialog(ProjectManager sdb)
			: base("Lua settings", "settings.png", SizeToContent.Height, ResizeMode.NoResize) {
				_multiGrf = sdb.MetaGrf;
			InitializeComponent();

			Binder.Bind(_cbMobTables, () => ProjectConfiguration.SyncMobTables, _updateMobTables, true);
			Binder.Bind(_cbAccTables, () => ProjectConfiguration.HandleViewIds, _updateccTables, true);

			WpfUtilities.AddMouseInOutUnderline(_cbMobTables, _cbAccTables);
			_sdb = sdb;

			_oldPath1 = ProjectConfiguration.SyncMobId;
			_oldPath2 = ProjectConfiguration.SyncMobName;

			_spMob.Children.Add(new TextViewItem(null, new GetSetSetting(v => ProjectConfiguration.SyncMobId = v, () => ProjectConfiguration.SyncMobId), _multiGrf) { Description = "npcidentity.lub" });
			_spMob.Children.Add(new TextViewItem(null, new GetSetSetting(v => ProjectConfiguration.SyncMobName = v, () => ProjectConfiguration.SyncMobName), _multiGrf) { Description = "jobname.lub" });
			_spAcc.Children.Add(new TextViewItem(null, new GetSetSetting(v => ProjectConfiguration.SyncAccId = v, () => ProjectConfiguration.SyncAccId), _multiGrf) { Description = "accessoryid.lub" });
			_spAcc.Children.Add(new TextViewItem(null, new GetSetSetting(v => ProjectConfiguration.SyncAccName = v, () => ProjectConfiguration.SyncAccName), _multiGrf) { Description = "accname.lub" });

			_spMob.SizeChanged += _grid_SizeChanged;
			_spAcc.SizeChanged += _grid_SizeChanged;

			Extensions.SetMinimalSize(this);
			this.Owner = WpfUtilities.TopWindow;
		}

		private void _grid_SizeChanged(object sender, SizeChangedEventArgs e) {
			StackPanel panel = (StackPanel)sender;

			double width = panel.ActualWidth - 11;

			foreach (TextViewItem element in panel.Children) {
				element._grid.Width = width < 0 ? 0 : width;
			}
		}

		private void _updateMobTables() {
			_spMob.IsEnabled = ProjectConfiguration.SyncMobTables;
		}

		private void _updateccTables() {
			_spAcc.IsEnabled = ProjectConfiguration.HandleViewIds;
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			if (_oldPath1 != ProjectConfiguration.SyncMobId ||
				_oldPath2 != ProjectConfiguration.SyncMobName) {
				LuaHelper.ReloadJobTable((_sdb).GetDb(DataSources.Mob), true);
				LuaHelper.ReloadJobTable((_sdb).GetDb(DataSources.MobImport), true);
			}

			base.OnClosing(e);
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();

			if (e.Key == Key.Enter) {
				Close();
			}
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
