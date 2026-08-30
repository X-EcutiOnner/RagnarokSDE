using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ErrorManager;
using GRF.FileFormats.LubFormat;
using GRF.IO;
using GRF.GrfSystem;
using GrfToWpfBridge.Application;
using ICSharpCode.AvalonEdit.Document;
using SDE.ApplicationConfiguration;
using SDE.Core.Avalon;
using SDE.Editor.Engines;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles;
using Utilities;
using Utilities.Services;
using SDE.Databases.Items.Features;
using SDE.Databases.ClientItems.Features;
using SDE.Databases.ItemCombos.Features;
using SDE.Databases.Skills.Features;
using SDE.Databases.Mobs.Features;
using SDE.Databases.MobSkills.Features;
using SDE.Databases.Quests.Features;
using SDE.Databases.ClientQuests.Features;
using SDE.Databases.Achievements.Features;
using SDE.Databases.ClientAchievements.Features;
using SDE.Databases.Pets.Features;
using SDE.Databases.Castles.Features;
using System.Reflection;
using SDE.View.Editors.ScriptEdit;
using SDE.View.Editors;
using SDE.View;

namespace SDE.Editor.IronPython {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class IronPythonDialog : TkWindow {
		private readonly SdeEditor _editor;
		private WpfRecentFiles _rcm;
		private GridLength _oldHeight = default(GridLength);
		private ScriptAutoCompletion _autoCompletion;

		static IronPythonDialog() {
			var list = PythonEditorList.IronPythonConstants;
			var types = new Type[] { typeof(Item), typeof(ClientItem), typeof(ItemCombo), typeof(Skill), typeof(Mob), typeof(MobSkill), typeof(Quest), typeof(ClientQuest), typeof(Achv), typeof(ClientAchv), typeof(Pet), typeof(Castle) };
			
			foreach (var type in types) {
				_addType(list, type);
			}

			PythonEditorList.IronPythonTables = PythonEditorList.IronPythonTables.Distinct().ToList();
		}

		private static void _addType(HashSet<string> list, Type type) {
			var objectTree = TypeTreeHelper.GetObjectTree(type);

			foreach (var entry in objectTree.FieldsOrMembers) {
				if (entry.Value.Member is FieldInfo fi) {
					list.Add(entry.Value.Member.Name);

					if (entry.Value.IsCollection) {
						_addType(list, (entry.Value.Member as FieldInfo).FieldType.GetGenericArguments()[0]);
					}
				}
			}
		}

		public IronPythonDialog(SdeEditor editor)
			: base("IronPython Script Engine...", "dos.png", SizeToContent.Manual, ResizeMode.CanResize) {
			_editor = editor;

			InitializeComponent();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = WpfUtilities.TopWindow;

			_loadUi();
		}

		private void _loadUi() {
			_rcm = new WpfRecentFiles(SdeAppConfiguration.ConfigAsker, 6, _miLoadRecent, "Server database editor - IronPython recent files");
			_rcm.FileClicked += _rcm_FileClicked;
			
			GrfToWpfBridge.Binder.Bind(_textEditor, () => SdeAppConfiguration.IronPythonScript);
			GrfToWpfBridge.Binder.Bind(_miAutocomplete, () => SdeAppConfiguration.IronPythonAutocomplete);
			
			AvalonLoader.Load(_textEditor);
			AvalonLoader.SetSyntax(_textEditor, "IronPython");

			_autoCompletion = new ScriptAutoCompletion(_textEditor, 
				PythonEditorList.IronPythonTables.Select(p => (p, DataType.IronPythonFunction)).Concat(
				PythonEditorList.IronPythonConstants.Select(p => (p, DataType.IronPythonConstant))).ToList());

			_textEditor.TextArea.TextEntering += _textArea_TextEntering;
			_rowConsole.Height = new GridLength(0);
			_buttonCloseConsole.Margin = new Thickness(0, 5, SystemParameters.HorizontalScrollBarButtonWidth + 2, 0);
			_textEditor.Drop += _textEditor_Drop;

			ApplicationShortcut.Link(SdeCommands.PythonRun, _miRun, this);
			ApplicationShortcut.Link(SdeCommands.PythonRun2, _miRun, this);
			ApplicationShortcut.Link(SdeCommands.PythonNew, _miNew, this);
			ApplicationShortcut.Link(SdeCommands.PythonOpen, _miOpen, this);
			ApplicationShortcut.Link(SdeCommands.PythonSave, _miSave, this);
		}

		private void _textEditor_Drop(object sender, DragEventArgs e) {
			try {
				if (e.Is(DataFormats.FileDrop)) {
					_textEditor.Document.Replace(0, _textEditor.Document.TextLength, File.ReadAllText(e.Get<string>(DataFormats.FileDrop)));
					_rcm.AddRecentFile(e.Get<string>(DataFormats.FileDrop));
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _rcm_FileClicked(string file) {
			try {
				_textEditor.Document.Replace(0, _textEditor.Document.TextLength, File.ReadAllText(file));
			}
			catch (Exception err) {
				_rcm.RemoveRecentFile(file);
				ErrorHandler.HandleException(err);
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
			string tempFile = TemporaryFilesManager.GetTemporaryFilePath("script_tmp_{0:0000}.py");

			try {
				if (_oldHeight != default(GridLength) && _rowConsole.Height.Value > 0) {
					_oldHeight = new GridLength(_rowConsole.Height.Value);
				}

				if (!SdeEditor.Project.AllTables.Any(p => p.Value.IsLoaded)) {
					throw new Exception("No databases loaded.");
				}

				File.WriteAllText(tempFile, _textEditor.Text);

				_tbOutput.Text = new ScriptInterpreter().Execute(_editor.FindTopmostTab(), tempFile);

				if (_tbOutput.Text != "") {
					_tbOutput.Visibility = Visibility.Visible;

					if (_oldHeight == default(GridLength)) {
						_oldHeight = new GridLength(150);
					}

					_rowConsole.Height = _oldHeight;
				}
				else {
					_button_ConsoleClose_Click(null, null);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
			finally {
				GrfPath.Delete(tempFile);
			}
		}

		private void _textArea_TextEntering(object sender, TextCompositionEventArgs e) {
			try {
				if (e.Text.Length > 0 && _autoCompletion.IsOpened) {
					if (e.Text[0] == '\n') {
						int currentLine = _textEditor.TextArea.Caret.Line;

						DocumentLine docLine = _autoCompletion.GetDocumentLine(currentLine);
						string line = _autoCompletion.GetTextLine(currentLine);
						int currentIndent = LineHelper.GetIndent(line);

						if (line.EndsWith(":") && _textEditor.CaretOffset >= docLine.EndOffset) {
							currentIndent++;
						}

						if (_textEditor.LineCount == currentLine) {
							_textEditor.Document.Insert(_textEditor.Document.TextLength, "\n" + LineHelper.GenerateIndent(currentIndent));
							_textEditor.CaretOffset = _textEditor.Text.Length;
						}
						else {
							var position = _textEditor.CaretOffset;
							_textEditor.Document.Insert(_textEditor.CaretOffset, "\n" + LineHelper.GenerateIndent(currentIndent));
							_textEditor.CaretOffset = position + ("\n" + LineHelper.GenerateIndent(currentIndent)).Length;
						}

						_textEditor.TextArea.Caret.BringCaretToView();
						e.Handled = true;
					}
				}
			}
			catch { }
		}

		private void _buttonSave_Click(object sender, RoutedEventArgs e) {
			try {
				string path = PathRequest.SaveFileCde("filter", "Python Files|*.py");

				if (path != null) {
					File.WriteAllText(path, _textEditor.Text);
					_rcm.AddRecentFile(path);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _miLoadClear_Click(object sender, RoutedEventArgs e) {
			try {
				_textEditor.Document.Remove(0, _textEditor.Document.TextLength);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _miLoadLoad_Click(object sender, RoutedEventArgs e) {
			try {
				string path = PathRequest.OpenFileCde("filter", "Python Files|*.py");

				if (path != null) {
					var text = File.ReadAllText(path);
					_textEditor.Document.Replace(0, _textEditor.Document.TextLength, text);
					_rcm.AddRecentFile(path);
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _miSample_Click(object sender, RoutedEventArgs e) {
			try {
				var text = EncodingService.DisplayEncoding.GetString(ApplicationManager.GetResource(((FrameworkElement)sender).Tag.ToString()));
				_textEditor.Document.Replace(0, _textEditor.Document.TextLength, text);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private void _button_ConsoleClose_Click(object sender, RoutedEventArgs e) {
			try {
				if (_rowConsole.Height.Value > 0)
					_oldHeight = new GridLength(_rowConsole.Height.Value);

				_tbOutput.Visibility = Visibility.Collapsed;
				_rowConsole.Height = new GridLength(0);
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}
