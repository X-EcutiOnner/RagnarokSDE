using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SDE.Databases.Generic.Controls {
	public class HorizontalGrid : Panel {
		public HorizontalGrid() {
			SetValue(Grid.IsSharedSizeScopeProperty, true);
		}

		public static readonly DependencyProperty ColumnCountProperty =
			DependencyProperty.Register(
				nameof(ColumnCount),
				typeof(int),
				typeof(HorizontalGrid),
				new FrameworkPropertyMetadata(1,
					FrameworkPropertyMetadataOptions.AffectsMeasure |
					FrameworkPropertyMetadataOptions.AffectsArrange));

		public int ColumnCount {
			get => (int)GetValue(ColumnCountProperty);
			set => SetValue(ColumnCountProperty, value);
		}

		public static readonly DependencyProperty AllLabelColumnSameSizeProperty =
			DependencyProperty.Register(
				nameof(AllLabelColumnSameSize),
				typeof(bool),
				typeof(HorizontalGrid),
				new FrameworkPropertyMetadata(false,
					FrameworkPropertyMetadataOptions.AffectsMeasure |
					FrameworkPropertyMetadataOptions.AffectsArrange));

		public bool AllLabelColumnSameSize {
			get => (bool)GetValue(AllLabelColumnSameSizeProperty);
			set => SetValue(AllLabelColumnSameSizeProperty, value);
		}

		public static readonly DependencyProperty SeparatorWidthProperty =
			DependencyProperty.Register(
				nameof(SeparatorWidth),
				typeof(double),
				typeof(HorizontalGrid),
				new FrameworkPropertyMetadata(10.0,
					FrameworkPropertyMetadataOptions.AffectsMeasure |
					FrameworkPropertyMetadataOptions.AffectsArrange));

		public double SeparatorWidth {
			get => (double)GetValue(SeparatorWidthProperty);
			set => SetValue(SeparatorWidthProperty, value);
		}

		private static readonly DependencyPropertyKey ColumnWidthsPropertyKey =
			DependencyProperty.RegisterReadOnly(
				nameof(ColumnWidths),
				typeof(IReadOnlyList<GridLength>),
				typeof(HorizontalGrid),
				new PropertyMetadata(new SafeReadOnlyList<GridLength>(Array.Empty<GridLength>(), GridLength.Auto)));

		public static readonly DependencyProperty ColumnWidthsProperty = ColumnWidthsPropertyKey.DependencyProperty;

		public IReadOnlyList<GridLength> ColumnWidths => (SafeReadOnlyList<GridLength>)GetValue(ColumnWidthsProperty);

		protected override Size MeasureOverride(Size availableSize) {
			int colCount = Math.Max(1, ColumnCount);
			double colSeparatorWidth = colCount > 1 ? SeparatorWidth : 0;
			double colSeparatorWidthTotal = colCount > 1 ? (colCount - 1) * colSeparatorWidth : 0;
			double constraintWidth = Math.Max(0, (availableSize.Width - colSeparatorWidthTotal) / colCount);

			double rowHeight = 0;
			int col = 0;
			double y = 0;

			foreach (UIElement child in InternalChildren) {
				if (child.Visibility == Visibility.Collapsed)
					continue;

				if (col == colCount) {
					col = 0;
					y += rowHeight;
					rowHeight = 0;
				}

				child.Measure(new Size(constraintWidth, double.PositiveInfinity));

				rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
				col++;
			}

			var finalSize = new Size(availableSize.Width, y + rowHeight);
			return finalSize;
		}

		protected override Size ArrangeOverride(Size finalSize) {
			int colCount = Math.Max(1, ColumnCount);

			double colSeparatorWidth = colCount > 1 ? SeparatorWidth : 0;
			double colSeparatorWidthTotal = colCount > 1 ? (colCount - 1) * colSeparatorWidth : 0;
			double colWidth = Math.Max(0, (finalSize.Width - colSeparatorWidthTotal) / colCount);

			//double colWidth = finalSize.Width / colCount;
			double x = 0;
			double y = 0;

			double rowHeight = 0;
			int col = 0;

			List<UIElement> rowChildren = new List<UIElement>();

			foreach (UIElement child in InternalChildren) {
				if (child.Visibility == Visibility.Collapsed)
					continue;

				if (col == colCount) {
					ArrangeRow(rowChildren, x, y, colWidth, rowHeight, colSeparatorWidth);

					col = 0;
					x = 0;
					y += rowHeight;
					rowHeight = 0;
					rowChildren.Clear();
				}

				if (child is Property property) {
					property.RootGrid.ColumnDefinitions[0].SharedSizeGroup = "Label" + (AllLabelColumnSameSize ? 0 : col);
				}

				rowChildren.Add(child);
				rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
				col++;
			}

			if (rowChildren.Count > 0) {
				ArrangeRow(rowChildren, x, y, colWidth, rowHeight, colSeparatorWidth);
				y += rowHeight;
			}

			UpdateColumnWidths(finalSize);
			return finalSize;
		}

		private void UpdateColumnWidths(Size finalSize) {
			int columnCount = ColumnCount;

			if (columnCount == 0)
				return;

			int colCount = Math.Max(1, columnCount);

			double colSeparatorWidth = colCount > 1 ? SeparatorWidth : 0;
			double colSeparatorWidthTotal = colCount > 1 ? (colCount - 1) * colSeparatorWidth : 0;
			double colWidth = Math.Max(0, (finalSize.Width - colSeparatorWidthTotal) / colCount);

			var widths = new double[2 + 3 * (columnCount - 1)];
			int columnIdx = 0;

			foreach (UIElement child in InternalChildren) {
				if (child is Property property) {
					if (widths[columnIdx * 3] == 0) {
						widths[columnIdx * 3] = property.RootGrid.ColumnDefinitions[0].ActualWidth;
					}
				}

				columnIdx = (++columnIdx) % columnCount;
			}

			for (int i = 1; i < columnCount; i++) {
				widths[3 * (i - 1) + 2] = SeparatorWidth;
			}

			for (int i = 0; i < columnCount; i++) {
				widths[3 * i + 1] = Math.Max(0, colWidth - widths[3 * i + 0]);
			}

			var newWidths = new SafeReadOnlyList<GridLength>(widths.Select(p => new GridLength(p)).ToList(), GridLength.Auto);

			if (!newWidths.SequenceEqual(ColumnWidths))
				SetValue(ColumnWidthsPropertyKey, newWidths);
		}

		private void ArrangeRow(List<UIElement> children, double startX, double y, double colWidth, double rowHeight, double seperatorWidth) {
			double currentX = 0; // Reset to 0 since we step row by row relative to the panel start
			foreach (var child in children) {
				// FIX: Give the child the full rowHeight so it has room to use VerticalAlignment
				child.Arrange(new Rect(currentX, y, colWidth, rowHeight));
				currentX += seperatorWidth;
				currentX += colWidth;
			}
		}
	}

	public sealed class SafeReadOnlyList<T> : IReadOnlyList<T> {
		private readonly IReadOnlyList<T> _list;
		private readonly T _defValue;

		public SafeReadOnlyList(IReadOnlyList<T> list, T defValue) {
			_list = list;
			_defValue = defValue;
		}

		public T this[int index] {
			get {
				if ((uint)index >= (uint)_list.Count)
					return _defValue;

				return _list[index];
			}
		}

		public int Count => _list.Count;

		public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
