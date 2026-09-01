using SDE.Editor.Generic.DbTabs;
using System.Windows.Controls;
using SDE.View.Dialogs;
using ErrorManager;
using System;
using SDE.Core.Avalon;
using ICSharpCode.AvalonEdit;
using TokeiLibrary.WPF;
using TokeiLibrary;
using System.Windows;
using System.Text;
using SDE.Databases.Items.Features;
using SDE.View;
using SDE.View.Editors.ScriptEdit;
using SDE.View.Editors;
using SDE.Databases.Generic.Features;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Database;

namespace SDE.Databases.ClientItems.Features {
	/// <summary>
	/// Interaction logic for ClientItemView.xaml
	/// </summary>
	public partial class ClientItemViewLua : UserControl, IDatabaseView {
		private ClientItemViewModel _viewModel;
		private DbTab _tab;
		private TextEditor _lastActiveTextEditor;
		private ItemDescriptionDialog _itemDescriptionDialog = new ItemDescriptionDialog();
		private Func<ReadableTuple, string> _update;
		private ScriptEditDialog _scriptEdit;

		public ClientItemViewLua() {
			InitializeComponent();

			AvalonLoader.Load(_tbIdDescription);
			AvalonLoader.Load(_tbUnDescription);

			_tbIdDescription.GotFocus += delegate { _lastActiveTextEditor = _tbIdDescription; };
			_tbUnDescription.GotFocus += delegate { _lastActiveTextEditor = _tbUnDescription; };

			_lastActiveTextEditor = _tbIdDescription;
			_tecc.Init(() => _lastActiveTextEditor);
		}

		public void Init(DbTab tab) {
			tab.UpdateAction = _updateAction;

			_viewModel = new ClientItemViewModel(tab);
			DataContext = _viewModel;

			_tab = tab;

			DisplayablePropertyHelper.SetTextBoxesUndo(_gridContainer, _tab);
			Core.Extensions.SetupZIndex(_gridContainer);
		}

		private void _updateAction(ReadableTuple tuple) {
			if (_viewModel.IsLocked)
				return;

			ClientItem model = tuple == null ? null : tuple.GetModel<ClientItem>();

			if (_itemDescriptionDialog != null && _itemDescriptionDialog.IsVisible)
				_itemDescriptionDialog.LoadItem(tuple.Key, model);

			if (_scriptEdit != null)
				_scriptEdit._textEditor.Text = DbWriter.AutoFormatScript(_update(tuple));

			if (tuple != null) {
				_viewModel.SetModel(tuple, model);
			}
			else {
				_viewModel.SetModel(null, null);
			}

			Core.Extensions.ClearUndos(_gridContainer);
		}

		private void _bLuaSettings_Click(object sender, RoutedEventArgs e) {
			try {
				var dialog = new LuaTableDialog(SdeEditor.Project);
				dialog.ShowDialog();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _buttonQuickEdit_Click(object sender, RoutedEventArgs e) {
			_itemDescriptionDialog.LoadItem(_viewModel.Tuple.Key, _viewModel.Model);
			WindowProvider.Show(_itemDescriptionDialog, (Control)sender, WpfUtilities.FindParentControl<Window>(_tab.Content as DependencyObject));

			_itemDescriptionDialog.Closed += delegate {
				if (_itemDescriptionDialog.Result == true && _itemDescriptionDialog.Item != null)
					_viewModel.IdentifiedDescription = _itemDescriptionDialog.Output;
				_itemDescriptionDialog = new ItemDescriptionDialog();
			};

			_itemDescriptionDialog.Apply += delegate {
				if (_itemDescriptionDialog.Result == true && _itemDescriptionDialog.Item != null)
					_viewModel.IdentifiedDescription = _itemDescriptionDialog.Output;
			};
		}

		private void _buttonItemBonus_Click(object sender, RoutedEventArgs e) {
			var itemScript = (Button)sender;

			itemScript.IsEnabled = false;

			var itemDb = SdeEditor.Project.GetMergedTable(DataSources.Item);
			var item = _tab.List.SelectedItem as ReadableTuple;

			_update = new Func<ReadableTuple, string>(tuple => {
				var output = new StringBuilder();

				if (tuple != null) {
					var entry = itemDb.TryGetTuple(tuple.Key);

					if (entry != null) {
						var model = entry.GetModel<Item>();

						output.AppendLine("-- Script --");
						output.AppendLine(model.Script ?? "");
						output.AppendLine("-- OnEquipScript --");
						output.AppendLine(model.EquipScript ?? "");
						output.AppendLine("-- OnUnequipScript --");
						output.AppendLine(model.UnEquipScript ?? "");
					}
					else {
						output.AppendLine("-- Not found in item_db_m --");
					}
				}
				else {
					output.AppendLine("-- No entry selected --");
				}

				return output.ToString();
			});

			_scriptEdit = new ScriptEditDialog(_update(item));
			_scriptEdit.Closed += delegate {
				itemScript.IsEnabled = true;
				_scriptEdit = null;
			};

			_scriptEdit._textEditor.IsReadOnly = true;
			_scriptEdit.DisableOk();
			_scriptEdit.Show();
			_scriptEdit.Owner = WpfUtilities.FindParentControl<Window>(_tab);
		}
	}
}
