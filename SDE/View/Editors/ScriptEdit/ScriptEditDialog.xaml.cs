using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SDE.ApplicationConfiguration;
using SDE.Core.Avalon;
using SDE.Databases;
using SDE.Databases.Generic.Parser;
using SDE.Databases.ItemCombos.Features;
using SDE.Databases.Skills.Features;
using SDE.View.Dialogs;
using TokeiLibrary.WPF.Styles;

namespace SDE.View.Editors.ScriptEdit {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class ScriptEditDialog : TkWindow, IInputWindow {
		private ScriptAutoCompletion _autoCompletion;

		public ScriptEditDialog(string text) : base("Script edit", "cde.ico", SizeToContent.Manual, ResizeMode.CanResize) {
			InitializeComponent();

			AvalonLoader.Load(_textEditor);
			AvalonLoader.SetSyntax(_textEditor, "Script");

			string script = DbWriter.AutoFormatScript(text);

			var skills = new List<string>();
			var skill_db = SdeEditor.Project.GetMergedTable(DataSources.Skill);
			skill_db.FastItems.ForEach(p => skills.Add(p.GetModel<Skill>().Name ?? ""));

			_autoCompletion = new ScriptAutoCompletion(_textEditor, 
				ScriptEditorList.rAthenaScriptFunctions.Select(p => (p, DataType.ScriptFunction)).Concat(
				ScriptEditorList.rAthenaScriptConstants.Select(p => (p, DataType.ScriptConstant)).Concat(
				skills.Select(p => (p, DataType.ScriptSkill))
			)).ToList());

			_textEditor.Text = script;
			_textEditor.TextChanged += (e, a) => OnValueChanged();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;

			_textEditor.Loaded += delegate {
				_textEditor.Focus();
			};
		}

		public string Text => DbWriter.ScriptToSingleLineScript(_textEditor.Text);

		public Grid Footer { get { return _footerGrid; } }
		public event Action ValueChanged;

		public void OnValueChanged() {
			ValueChanged?.Invoke();
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				Close();
			}
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			if (!SdeAppConfiguration.UseIntegratedDialogsForScripts)
				DialogResult = true;
			Close();
		}

		public void DisableOk() {
			_buttonOk.Visibility = Visibility.Hidden;
		}

		private void _buttonFromEquipment_Click(object sender, RoutedEventArgs e) {
			// Find current item ID
			var tab = SdeEditor.Instance.FindTopmostTab();
			var tuple = tab.SelectedItem;

			if (tuple == null)
				return;

			string script;

			if (tab.Database.Source == DataSources.ItemCombo || tab.Database.Source == DataSources.ItemComboImport) {
				script = LuaEquipmentProperties.CreateAthenaScript(tuple.GetModel<ItemCombo>());
			}
			else {
				script = LuaEquipmentProperties.CreateAthenaScript(tuple.Key);
			}

			if (script == null)
				return;

			_textEditor.Document.Replace(0, _textEditor.Document.TextLength, script);
		}
	}

	public enum DataType {
		IronPythonFunction,
		IronPythonConstant,
		ScriptFunction,
		ScriptConstant,
		ScriptSkill,
	}
}
