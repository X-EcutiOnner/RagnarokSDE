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
using SDE.Databases.Skills.Features;
using SDE.Editor.Generic.Parsers.Generic;
using SDE.View.Dialogs;
using SDE.View.Editors;
using SDE.View.Editors.ScriptEdit;
using TokeiLibrary.WPF.Styles;
using Utilities;

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

		public string Text {
			get {
				return Methods.Aggregate(_textEditor.Text.Split(new string[] {Environment.NewLine, "\n"}, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim(' ', '\t') + " ").ToList(), "").Trim(' ');
			}
		}

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
	}

	public enum DataType {
		IronPythonFunction,
		IronPythonConstant,
		ScriptFunction,
		ScriptConstant,
		ScriptSkill,
	}
}
