using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SDE.ApplicationConfiguration;
using SDE.Core;
using SDE.Databases.Generic.Converters;
using SDE.Databases.Generic.Controls;
using SDE.Databases.Generic.Parser;
using SDE.Editor.Generic.Parsers.Generic;
using TokeiLibrary.WPF.Styles;
using SDE.Databases.Generic.Features;
using SDE.View.Editors.ItemCostEdit;
using SDE.View.Dialogs;

namespace SDE.View.Editors.ItemCostEdit {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class ItemCostDialog : TkWindow, IInputWindow {
		private ItemCostContainer _itemCostContainer;

		public ItemCostDialog(string text, int maxLevel) : base("Level edit", "cde.ico", SizeToContent.Height, ResizeMode.CanResize) {
			InitializeComponent();
			Extensions.SetMinimalSize(this);
			_itemCostContainer = new ItemCostContainer(text ?? "", maxLevel);
			DataContext = _itemCostContainer;

			int columnGroupCount = ((_itemCostContainer.Count - 1) / 10) + 1;
			columnGroupCount = columnGroupCount > 3 ? 3 : columnGroupCount;

			Width = 400 * columnGroupCount;

			for (int i = 0; i < columnGroupCount; i++) {
				_propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto) });
				_propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition());
				_propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto) });
				_propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto) });
				_propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
				_propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto) });
				_propertiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
			}

			for (int i = 0; i < 10; i++) {
				_propertiesGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(-1, GridUnitType.Auto) });
			}

			for (int i = 0; i < _itemCostContainer.Count; i++) {
				_addLabel("ID", i, 0);
				_addValidationBox(i, 1);
				_addSelectTuple(i, 2);
				_addLabel("Amount", i, 3);
				_addTextBox(i, 4, $"ItemCosts[{i}].Amount");
				_addLabel("Skill level", i, 5);
				_addTextBox(i, 6, $"ItemCosts[{i}].Level");

				_itemCostContainer.ItemCosts[i].PropertyChanged += (s, e) => OnValueChanged();
			}

			Core.Extensions.SetupZIndex(_propertiesGrid);
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		private void _addValidationBox(int index, int col) {
			var itemPreviewConverter = new ItemPreviewConverter();
			ValidationTextBox box = new ValidationTextBox();
			box.Height = 22;
			box.SetBinding(ValidationTextBox.TextProperty, $"ItemCosts[{index}].ItemId");
			box.SetBinding(ValidationTextBox.PreviewTextProperty, new Binding($"ItemCosts[{index}].ItemId") { Converter = itemPreviewConverter });

			Grid.SetRow(box, index % 10);
			Grid.SetColumn(box, col);

			_propertiesGrid.Children.Add(box);

			box.KeyDown += delegate {
				if (Keyboard.IsKeyDown(Key.Enter)) {
					if (!SdeAppConfiguration.UseIntegratedDialogsForLevels)
						DialogResult = true;

					Close();
				}
			};

			if (index == 0) {
				box.Loaded += delegate {
					Keyboard.Focus(box._tbData);
					box.SelectAll();
				};
			}
		}

		private TextBox _addTextBox(int index, int col, string binding) {
			TextBox box = new TextBox();
			box.Margin = new Thickness(3);
			box.Height = 22;
			box.VerticalContentAlignment = VerticalAlignment.Center;
			box.VerticalAlignment = VerticalAlignment.Center;
			box.SetBinding(TextBox.TextProperty, binding);

			Grid.SetRow(box, index % 10);
			Grid.SetColumn(box, col);

			_propertiesGrid.Children.Add(box);

			return box;
		}

		private SelectTupleButton _addSelectTuple(int index, int col) {
			SelectTupleButton selectTuple = new SelectTupleButton();
			selectTuple.SetBinding(SelectTupleButton.SourceFieldProperty, $"ItemCosts[{index}].ItemId");
			selectTuple.Width = 22;
			selectTuple.Height = 22;
			selectTuple.Margin = new Thickness(3);
			selectTuple.IsTabStop = false;
			selectTuple._button.IsTabStop = false;

			Grid.SetRow(selectTuple, index % 10);
			Grid.SetColumn(selectTuple, col);

			_propertiesGrid.Children.Add(selectTuple);

			return selectTuple;
		}

		private TextBlock _addLabel(string text, int index, int col) {
			TextBlock textBlock = new TextBlock();
			textBlock.Text = text;
			textBlock.Margin = new Thickness(3);
			textBlock.VerticalAlignment = VerticalAlignment.Center;
			//textBlock.HorizontalAlignment = HorizontalAlignment.Right;

			Grid.SetRow(textBlock, index % 10);
			Grid.SetColumn(textBlock, col);

			_propertiesGrid.Children.Add(textBlock);

			return textBlock;
		}

		public string Text => _itemCostContainer.GetCompactText();
		public Grid Footer => _footerGrid;
		public event Action ValueChanged;
		public void OnValueChanged() => ValueChanged?.Invoke();

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) => Close();
		
		private void _buttonOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = Text != "";
			Close();
		}
	}
}
