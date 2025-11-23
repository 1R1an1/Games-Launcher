using System;
using System.Windows;
using System.Windows.Controls;

namespace Games_Launcher.Views
{
    public class GridWrapPanel : Panel
    {
        public int ItemsPerRow { get; set; } = 2;

        protected override Size MeasureOverride(Size availableSize)
        {
            double rowHeight = 0;
            double totalHeight = 0;
            double rowWidth = 0;
            double maxRowWidth = 0;

            int count = 0;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var size = child.DesiredSize;

                rowHeight = Math.Max(rowHeight, size.Height);
                rowWidth += size.Width;

                count++;

                if (count == ItemsPerRow)
                {
                    totalHeight += rowHeight;
                    maxRowWidth = Math.Max(maxRowWidth, rowWidth);

                    rowHeight = 0;
                    rowWidth = 0;
                    count = 0;
                }
            }

            if (count > 0)
            {
                totalHeight += rowHeight;
                maxRowWidth = Math.Max(maxRowWidth, rowWidth);
            }

            return new Size(maxRowWidth, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;
            double y = 0;

            double rowHeight = 0;

            int count = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (count == ItemsPerRow)
                {
                    // nueva fila
                    x = 0;
                    y += rowHeight;
                    rowHeight = 0;
                    count = 0;
                }

                var size = child.DesiredSize;

                child.Arrange(new Rect(new Point(x, y), size));

                x += size.Width;
                rowHeight = Math.Max(rowHeight, size.Height);

                count++;
            }

            return finalSize;
        }
    }
}
