using System;
using System.Windows;
using System.Windows.Input;
using SDE.Databases;
using TokeiLibrary.WPF.Styles;

namespace SDE.View.Editors {
	[Flags]
	public enum DropEditFlag {
		StealProtected = 1,
		RandGroup = 2,
	}

	/// <summary>
	/// Interaction logic for NewMvpDrop.xaml
	/// </summary>
	public partial class DropEditDialog : TkWindow {
		private readonly string _dropChance;
		private readonly string _id;
		private readonly DataSource _source;

		public DropEditDialog(string id, string dropChance, DataSource source, bool selectId = false, DropEditFlag flag = 0) : base("Item edit", "cde.ico", SizeToContent.Height, ResizeMode.NoResize) {
			_id = id;
			_dropChance = dropChance;
			_source = source;

			InitializeComponent();

			_tbChance.Text = _dropChance;
			_tbId.Text = _id;

			PreviewKeyDown += _dropEdit_PreviewKeyDown;

			Loaded += delegate {
				if (selectId) {
					_tbId.SelectAll();
					_tbId.Focus();
				}
				else {
					_tbChance.SelectAll();
					_tbChance.Focus();
				}

				if ((flag & DropEditFlag.StealProtected) == DropEditFlag.StealProtected) {
					_tbDStealProtected.Visibility = Visibility.Visible;
					_tbStealProtected.Visibility = Visibility.Visible;
				}

				if ((flag & DropEditFlag.RandGroup) == DropEditFlag.RandGroup) {
					_tbDRandGroup.Visibility = Visibility.Visible;
					_tbRandGroup.Visibility = Visibility.Visible;
				}
			};

			if (source != null) {
				_buttonQuery.Click += _buttonQuery_Click;
			}
			else {
				_buttonQuery.Visibility = Visibility.Collapsed;
			}
		}

		public string Id => _tbId.Text;
		public string DropChance => _tbChance.Text;
		public bool StealProtected => _tbStealProtected.IsChecked.Value;

		public string RandGroup => _tbRandGroup.Text;

		private void _buttonQuery_Click(object sender, RoutedEventArgs e) {
			var dialog = new SelectTupleDialog(SdeEditor.Project.GetMergedTable(_source), _source, _tbId.Text);
			dialog.Owner = this;

			if (dialog.ShowDialog() == true) {
				_tbId.Text = dialog.Id;
			}
		}

		private void _dropEdit_PreviewKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				DialogResult = true;
				e.Handled = true;
				Close();
			}
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();

			if (e.Key == Key.Enter) {
				DialogResult = true;
				e.Handled = true;
				Close();
			}
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
