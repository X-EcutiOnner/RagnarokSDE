using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using SDE.ApplicationConfiguration;
using SDE.Core.Avalon;
using SDE.View.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TokeiLibrary;

namespace SDE.View.Editors.ScriptEdit {
	public class ScriptAutoCompletion {
		private CompletionWindow _completionWindow;
		private TextEditor _textEditor;
		private List<(string Word, DataType DataType)> _keywords;
		private int _completionStartOffset;
		public bool IsOpened => _completionWindow != null;

		public ScriptAutoCompletion(TextEditor textEditor, List<(string Word, DataType DataType)> keywords) {
			_textEditor = textEditor;
			_keywords = keywords;

			_textEditor.PreviewKeyDown += _textEditor_PreviewKeyDown;
			_textEditor.TextArea.TextEntered += _textArea_TextEntered;
			_textEditor.TextArea.TextEntering += _textArea_TextEntering;
		}

		private void _completionWindow_Changed(object sender, EventArgs e) {
			UpdateFilter();
		}

		private void _textEditor_PreviewKeyDown(object sender, KeyEventArgs e) {
			try {
				switch (e.Key) {
					case Key.Escape:
						CloseWindow();
						break;
					case Key.Back:
					case Key.Delete:
						UpdateFilter();
						break;
				}
			}
			catch { }
		}

		private void _textArea_TextEntering(object sender, TextCompositionEventArgs e) {
			try {
				if (e.Text.Length == 0)
					return;

				//if (_completionWindow != null) {
				//	if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_') {
				//		if (e.Text[0] != '\t') {
				//			// The match must be exact
				//
				//			string word = AvalonLoader.GetWholeWord(_textEditor.TextArea.Document, _textEditor);
				//
				//			if (_li.SelectedItem == null || !_li.SelectedItem.ToString().StartsWith(word ?? "", StringComparison.OrdinalIgnoreCase)) {
				//				_completionWindow.Close();
				//				return;
				//			}
				//		}
				//
				//		_completionWindow.CompletionList.RequestInsertion(e);
				//	}
				//}
				//
				//if (_completionWindow != null) {
				//	if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_' && e.Text[0] != ' ' && e.Text[0] != '(' && e.Text[0] != ')') {
				//		string word = AvalonLoader.GetWholeWordAdv(_textEditor.TextArea.Document, _textEditor);
				//
				//		var strategy = new RegexSearchStrategy(new Regex(word), true);
				//
				//		if ((e.Text[0] != '\t' || e.Text[0] != '\n') && strategy.FindAll(_textEditor.Document, 0, _textEditor.Document.TextLength).Count() > 1) {
				//			_completionWindow.Close();
				//			return;
				//		}
				//
				//		string line = GetCurrentTextLine(_textEditor.TextArea.Caret.Line);
				//
				//		if (line.IndexOf('#') > -1) {
				//			_completionWindow.Close();
				//			return;
				//		}
				//
				//		_completionWindow.CompletionList.RequestInsertion(e);
				//	}
				//	else if (e.Text[0] == ' ') {
				//		_completionWindow.Close();
				//	}
				//}
				//
				//if (_completionWindow == null) {
				//	if (e.Text[0] == '\n') {
				//		int currentLine = _textEditor.TextArea.Caret.Line;
				//
				//		DocumentLine docLine = GetCurrentDocumentLine(currentLine);
				//		string line = GetCurrentTextLine(currentLine);
				//		int currentIndent = LineHelper.GetIndent(line);
				//
				//		if (line.EndsWith(":") && _textEditor.CaretOffset >= docLine.EndOffset) {
				//			currentIndent++;
				//		}
				//
				//		if (_textEditor.LineCount == currentLine) {
				//			_textEditor.Document.Insert(_textEditor.Document.TextLength, "\n" + LineHelper.GenerateIndent(currentIndent));
				//			_textEditor.CaretOffset = _textEditor.Text.Length;
				//		}
				//		else {
				//			var position = _textEditor.CaretOffset;
				//			_textEditor.Document.Insert(_textEditor.CaretOffset, "\n" + LineHelper.GenerateIndent(currentIndent));
				//			_textEditor.CaretOffset = position + ("\n" + LineHelper.GenerateIndent(currentIndent)).Length;
				//		}
				//
				//		_textEditor.TextArea.Caret.BringCaretToView();
				//		e.Handled = true;
				//	}
				//}
			}
			catch { }
		}

		internal void ProcessText(TextCompositionEventArgs e) {
			if (!IsOpened) {
				string line = GetTextLine(_textEditor.TextArea.Caret.Line);

				// For IronPython, check if writing within a '#' comment
				foreach (var c in new char[] { '#', '/'}) {
					int commentIndex = line.IndexOf(c);

					if (commentIndex > -1) {
						int caretIndexInLine = _textEditor.TextArea.Caret.Column - 1;

						if (caretIndexInLine > commentIndex) {
							return;
						}
					}
				}
			}

			if (e.Text.Length == 1) {
				switch (e.Text) {
					//case " ":
					//case ",":
					//case "{":
					//case "}":
					//case "[":
					//case "]":
					//	FilterCheck(e, false);
					//	break;
					case "\t":
						FilterCheck(e, true);
						break;
					default:
						if (char.IsLetter(e.Text[0]) || e.Text[0] == '_') {
							if (!IsOpened) {
								ShowCompletion();
								return;
							}
						}
						break;
				}
			}

			if (!string.IsNullOrEmpty(e.Text) && (char.IsLetterOrDigit(e.Text[0]) || e.Text[0] == '_')) {
				UpdateFilter();
			}
		}

		private void _textArea_TextEntered(object sender, TextCompositionEventArgs e) {
			ProcessText(e);


			//if (e.Text.Length > 0 && (char.IsLetter(e.Text[0]) || e.Text[0] == '_')) {
			//	string line = GetCurrentTextLine(_textEditor.TextArea.Caret.Line);
			//
			//	int hashIndex = line.IndexOf('#');
			//
			//	if (hashIndex > -1) {
			//		int caretIndexInLine = _textEditor.TextArea.Caret.Column - 1;
			//
			//		if (caretIndexInLine >= hashIndex) {
			//			_completionWindow?.Close();
			//			return;
			//		}
			//	}
			//
			//	int commentIndex = line.IndexOf("//");
			//
			//	if (commentIndex > -1) {
			//		int caretIndexInLine = _textEditor.TextArea.Caret.Column - 1;
			//
			//		if (caretIndexInLine >= hashIndex) {
			//			_completionWindow?.Close();
			//			return;
			//		}
			//	}
			//
			//	UpdateFilter();
			//}
		}

		internal string GetTextLine(int line) {
			return _textEditor.Document.GetText(_textEditor.Document.GetLineByNumber(line)).TrimEnd(' ', '\r');
		}

		internal DocumentLine GetDocumentLine(int line) {
			return _textEditor.Document.GetLineByNumber(line);
		}

		internal void UpdateFilter() {
			if (!SdeAppConfiguration.IronPythonAutocomplete) {
				CloseWindow();
				return;
			}

			_textEditor.Dispatcher.BeginInvoke((Action)_updateFilter);
		}

		private void _updateFilter() {
			if (!IsOpened)
				return;

			string filter = GetCurrentFilter();
			
			_completionWindow.CompletionList.SelectItem(filter);

			if (_completionWindow.CompletionList.ListBox.Items.Count == 0) {
				_completionWindow.Visibility = Visibility.Hidden;

				if (_completionWindow.ToolTipCompletion != null)
					_completionWindow.ToolTipCompletion.Visibility = Visibility.Hidden;
			}
			else {
				_completionWindow.Visibility = Visibility.Visible;

				if (_completionWindow.ToolTipCompletion != null)
					_completionWindow.ToolTipCompletion.Visibility = Visibility.Visible;
			}
		}

		private bool _compiling = false;

		public void ShowCompletion() {
			if (_compiling)
				return;

			ISegment wordSegment = AvalonLoader.GetWholeWordSegmentAdv(_textEditor.TextArea.Document, _textEditor);
			string word = _textEditor.Document.GetText(wordSegment);

			if (_textEditor.CaretOffset - 2 > 0 && _textEditor.Document.GetCharAt(_textEditor.CaretOffset - 2) == '\\' && _textEditor.Document.GetCharAt(_textEditor.CaretOffset - 1) == 't')
				return;

			_compiling = true;

			Task.Run(() => {
				try {
					var keywords = _keywords.Where(p => p.Word.IndexOf(word, StringComparison.OrdinalIgnoreCase) != -1).OrderBy(p => p).ToList();

					if (keywords.Count == 0)
						return;

					_textEditor.Dispatch(delegate {
						if (IsOpened) {
							CloseWindow();
						}

						//_completionStartOffset = _textEditor.CaretOffset;
						_completionStartOffset = wordSegment.Offset;
						_completionWindow = new CompletionWindow(_textEditor.TextArea);
						_completionWindow.AllowsTransparency = true;
						_completionWindow.CompletionList.ListBox.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);

						var completionData = new List<ICompletionData>();

						foreach (var keyword in keywords) {
							var data = new MyCompletionData(keyword.Word, _textEditor, keyword.DataType);
							completionData.Add(data);
							_completionWindow.CompletionList.CompletionData.Add(data);
						}

						_completionWindow.Show();
						_completionWindow.Closed += delegate {
							_completionWindow = null;
						};

						_completionWindow.CompletionList.SelectedItem = completionData.FirstOrDefault(p => string.Compare(p.Text, word, StringComparison.OrdinalIgnoreCase) >= 0);
					});
				}
				catch { }
				finally {
					_compiling = false;
				}
			});
		}

		private void FilterCheck(TextCompositionEventArgs e, bool eatKey) {
			if (IsOpened) {
				if (_completionWindow.CompletionList.SelectedItem is ICompletionData item) {
					if (!eatKey && e != null)
						((MyCompletionData)item).AddKey(e.Text);

					item.Complete(
						_textEditor.TextArea,
						new SimpleSegment(_completionWindow.StartOffset, _completionWindow.EndOffset - _completionWindow.StartOffset),
						new EventArgs());
				}

				_completionWindow.Close();

				if (eatKey && e != null)
					e.Handled = true;

				return;
			}

			return;
		}

		public string GetCurrentFilter() {
			int caret = _textEditor.CaretOffset;
			if (caret < _completionStartOffset)
				return "";

			return _textEditor.Document.GetText(
				_completionStartOffset,
				caret - _completionStartOffset);
		}

		internal void CloseWindow() {
			if (_completionWindow != null)
				_completionWindow.Changed -= _completionWindow_Changed;

			_completionWindow?.Close();
		}
	}

	#region Nested type: MyCompletionData

	public class MyCompletionData : ICompletionData {
		private readonly TextEditor _editor;
		private readonly DataType _type;
		private string _toAdd;

		public MyCompletionData(string text, TextEditor editor, DataType type) {
			_editor = editor;
			Text = text;
			Priority = 1;
			_type = type;
		}

		#region ICompletionData Members

		public ImageSource Image {
			get {
				switch (_type) {
					case DataType.IronPythonConstant:
						return ApplicationManager.PreloadResourceImage("file_imf.png");
					case DataType.IronPythonFunction:
						return ApplicationManager.PreloadResourceImage("properties.png");
					case DataType.ScriptFunction:
						return ApplicationManager.PreloadResourceImage("file_imf.png");
					case DataType.ScriptConstant:
						return ApplicationManager.PreloadResourceImage("properties.png");
					case DataType.ScriptSkill:
						return ApplicationManager.PreloadResourceImage("sword.png");
				}
				return null;
			}
		}

		public string Text { get; private set; }

		// Use this property if you want to show a fancy UIElement in the list.
		public object Content {
			get { return Text; }
		}

		public object Description {
			get { return null; }
		}

		public double Priority { get; private set; }

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs) {
			ISegment seg = AvalonLoader.GetWholeWordSegmentAdv(textArea.Document, _editor);
			textArea.Document.Replace(seg, Text + _toAdd ?? "");
		}

		internal void AddKey(string text) {
			_toAdd = text;
		}

		#endregion
	}

	#endregion
}
