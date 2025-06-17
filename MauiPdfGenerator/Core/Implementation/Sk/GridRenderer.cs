using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class GridRenderer
{
    public async Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfGrid grid, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        var gridMargin = grid.GetMargin;
        float leftMargin = (float)gridMargin.Left;
        float rightMargin = (float)gridMargin.Right;
        SKRect gridMarginRect = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);

        if (grid.GetWidthRequest.HasValue)
        {
            gridMarginRect.Right = gridMarginRect.Left + (float)grid.GetWidthRequest.Value;
        }

        if (gridMarginRect.Width <= 0 || gridMarginRect.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }

        var arrangement = await MeasureAndArrangeGrid(grid, gridMarginRect, elementRenderer, pageDef);

        float totalHeight = arrangement.TotalHeight;
        float totalWidth = arrangement.TotalWidth;

        float finalLayoutHeight = (float)(grid.GetHeightRequest ?? (totalHeight + grid.GetPadding.VerticalThickness));

        if (finalLayoutHeight > gridMarginRect.Height && finalLayoutHeight > 0)
        {
            return new RenderOutput(0, 0, grid, true);
        }

        if (arrangement.ChildRects.Any())
        {
            float finalContainerWidth = (float)(grid.GetWidthRequest ?? totalWidth + grid.GetPadding.HorizontalThickness);

            float offsetX = grid.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (gridMarginRect.Width - finalContainerWidth) / 2,
                LayoutAlignment.End => gridMarginRect.Width - finalContainerWidth,
                _ => 0
            };
            float finalX = gridMarginRect.Left + offsetX;

            if (grid.GetBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(grid.GetBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(finalX, startY, finalContainerWidth, finalLayoutHeight);
                canvas.DrawRect(bgRect, bgPaint);
            }

            using var recorder = new SKPictureRecorder();
            using SKCanvas recordingCanvas = recorder.BeginRecording(new SKRect(0, 0, totalWidth, totalHeight));

            foreach (var kvp in arrangement.ChildRects)
            {
                await elementRenderer(recordingCanvas, kvp.Key, pageDef, kvp.Value, kvp.Value.Top);
            }

            using SKPicture picture = recorder.EndRecording();

            canvas.Save();
            canvas.Translate(finalX + (float)grid.GetPadding.Left, startY + (float)grid.GetPadding.Top);
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        return new RenderOutput(finalLayoutHeight, totalWidth, null, false, totalHeight);
    }

    private async Task<GridArrangement> MeasureAndArrangeGrid(PdfGrid grid, SKRect availableRect, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer, PdfPageData pageDef)
    {
        var arrangedChildren = PreArrangeChildren(grid.ChildrenList, grid.RowDefinitionsList.Count, grid.ColumnDefinitionsList.Count);

        var columnWidths = await CalculateColumnWidths(grid, availableRect.Width, arrangedChildren, elementRenderer, pageDef);
        var rowHeights = await CalculateRowHeights(grid, availableRect.Height, columnWidths, arrangedChildren, elementRenderer, pageDef);

        var childRects = new Dictionary<PdfElement, SKRect>();

        var columnPositions = new float[columnWidths.Length + 1];
        var rowPositions = new float[rowHeights.Length + 1];

        for (int i = 0; i < columnWidths.Length; i++)
            columnPositions[i + 1] = columnPositions[i] + columnWidths[i] + (i < columnWidths.Length - 1 ? (float)grid.GetSpacing : 0);

        for (int i = 0; i < rowHeights.Length; i++)
            rowPositions[i + 1] = rowPositions[i] + rowHeights[i] + (i < rowHeights.Length - 1 ? (float)grid.GetSpacing : 0);

        foreach (var child in arrangedChildren)
        {
            float x = columnPositions[child.GetColumn];
            float y = rowPositions[child.GetRow];

            float width = columnPositions[child.GetColumn + child.GetColumnSpan] - x - (child.GetColumnSpan > 1 ? (float)grid.GetSpacing : 0);
            float height = rowPositions[child.GetRow + child.GetRowSpan] - y - (child.GetRowSpan > 1 ? (float)grid.GetSpacing : 0);

            childRects[child.GetChild] = SKRect.Create(x, y, width, height);
        }

        float totalWidth = columnPositions.LastOrDefault();
        float totalHeight = rowPositions.LastOrDefault();

        return new GridArrangement(rowHeights, columnWidths, childRects, totalWidth, totalHeight);
    }

    private async Task<float[]> CalculateColumnWidths(PdfGrid grid, float availableWidth, List<GridCellChild> children, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer, PdfPageData pageDef)
    {
        var colDefs = grid.ColumnDefinitionsList;
        int numCols = colDefs.Count;
        var widths = new float[numCols];
        float totalAllocated = 0;
        float totalStar = 0;

        using var recorder = new SKPictureRecorder();
        using SKCanvas dummyCanvas = recorder.BeginRecording(SKRect.Empty);

        for (int i = 0; i < numCols; i++)
        {
            var colDef = colDefs[i];
            if (colDef.GridUnitType == GridUnitType.Absolute)
            {
                widths[i] = (float)colDef.Value;
                totalAllocated += widths[i];
            }
            else if (colDef.GridUnitType == GridUnitType.Star)
            {
                totalStar += (float)colDef.Value;
            }
        }
        totalAllocated += (float)Math.Max(0, numCols - 1) * (float)grid.GetSpacing;

        for (int i = 0; i < numCols; i++)
        {
            if (colDefs[i].GridUnitType == GridUnitType.Auto)
            {
                float maxWidth = 0;
                var childrenInCol = children.Where(c => c.GetColumn == i && c.GetColumnSpan == 1);
                foreach (var child in childrenInCol)
                {
                    var result = await elementRenderer(dummyCanvas, child.GetChild, pageDef, SKRect.Create(0, 0, float.MaxValue, float.MaxValue), 0);
                    maxWidth = Math.Max(maxWidth, result.WidthDrawnThisCall);
                }
                widths[i] = maxWidth;
                totalAllocated += widths[i];
            }
        }

        float remainingWidth = availableWidth - totalAllocated;
        if (remainingWidth > 0 && totalStar > 0)
        {
            float starUnit = remainingWidth / totalStar;
            for (int i = 0; i < numCols; i++)
            {
                if (colDefs[i].GridUnitType == GridUnitType.Star)
                {
                    widths[i] = (float)colDefs[i].Value * starUnit;
                }
            }
        }

        return widths;
    }

    private async Task<float[]> CalculateRowHeights(PdfGrid grid, float availableHeight, float[] columnWidths, List<GridCellChild> children, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer, PdfPageData pageDef)
    {
        var rowDefs = grid.RowDefinitionsList;
        int numRows = rowDefs.Count;
        var heights = new float[numRows];
        float totalAllocated = 0;
        float totalStar = 0;

        using var recorder = new SKPictureRecorder();
        using SKCanvas dummyCanvas = recorder.BeginRecording(SKRect.Empty);

        for (int i = 0; i < numRows; i++)
        {
            var rowDef = rowDefs[i];
            if (rowDef.GridUnitType == GridUnitType.Absolute)
            {
                heights[i] = (float)rowDef.Value;
                totalAllocated += heights[i];
            }
            else if (rowDef.GridUnitType == GridUnitType.Star)
            {
                totalStar += (float)rowDef.Value;
            }
        }
        totalAllocated += (float)Math.Max(0, numRows - 1) * (float)grid.GetSpacing;

        for (int i = 0; i < numRows; i++)
        {
            if (rowDefs[i].GridUnitType == GridUnitType.Auto)
            {
                float maxHeight = 0;
                var childrenInRow = children.Where(c => c.GetRow == i && c.GetRowSpan == 1);
                foreach (var child in childrenInRow)
                {
                    float childWidth = 0;
                    for (int j = 0; j < child.GetColumnSpan; j++)
                    {
                        childWidth += columnWidths[child.GetColumn + j];
                    }
                    childWidth += Math.Max(0, child.GetColumnSpan - 1) * (float)grid.GetSpacing;

                    var result = await elementRenderer(dummyCanvas, child.GetChild, pageDef, SKRect.Create(0, 0, childWidth, float.MaxValue), 0);
                    maxHeight = Math.Max(maxHeight, result.HeightDrawnThisCall);
                }
                heights[i] = maxHeight;
                totalAllocated += heights[i];
            }
        }

        float remainingHeight = availableHeight - totalAllocated;
        if (remainingHeight > 0 && totalStar > 0)
        {
            float starUnit = remainingHeight / totalStar;
            for (int i = 0; i < numRows; i++)
            {
                if (rowDefs[i].GridUnitType == GridUnitType.Star)
                {
                    heights[i] = (float)rowDefs[i].Value * starUnit;
                }
            }
        }

        return heights;
    }

    private List<GridCellChild> PreArrangeChildren(IReadOnlyList<GridCellChild> children, int numRows, int numCols)
    {
        if (numRows == 0 || numCols == 0) return [];

        var occupied = new bool[numRows, numCols];
        var arrangedChildren = new List<GridCellChild>();
        int currentRow = 0, currentCol = 0;

        foreach (var child in children)
        {
            var newChild = new GridCellChild(child.GetChild);
            newChild.Row(child.GetRow).Column(child.GetColumn).RowSpan(child.GetRowSpan).ColumnSpan(child.GetColumnSpan);

            if (child.GetRow == 0 && child.GetColumn == 0 && occupied[0, 0])
            {
                while (currentRow < numRows && occupied[currentRow, currentCol])
                {
                    currentCol++;
                    if (currentCol >= numCols)
                    {
                        currentCol = 0;
                        currentRow++;
                    }
                }
                newChild.Row(currentRow).Column(currentCol);
            }

            if (newChild.GetRow >= numRows || newChild.GetColumn >= numCols || newChild.GetRow + newChild.GetRowSpan > numRows || newChild.GetColumn + newChild.GetColumnSpan > numCols)
            {
                throw new InvalidOperationException($"Grid child goes out of bounds. Grid is {numRows}x{numCols}, child is at ({newChild.GetRow},{newChild.GetColumn}) with span ({newChild.GetRowSpan},{newChild.GetColumnSpan}).");
            }

            for (int r = 0; r < newChild.GetRowSpan; r++)
            {
                for (int c = 0; c < newChild.GetColumnSpan; c++)
                {
                    int targetRow = newChild.GetRow + r;
                    int targetCol = newChild.GetColumn + c;
                    if (occupied[targetRow, targetCol])
                    {
                        throw new InvalidOperationException($"Grid cells are already occupied at ({targetRow},{targetCol}).");
                    }
                    occupied[targetRow, targetCol] = true;
                }
            }
            arrangedChildren.Add(newChild);
        }
        return arrangedChildren;
    }

    private readonly record struct GridArrangement(float[] RowHeights, float[] ColumnWidths, Dictionary<PdfElement, SKRect> ChildRects, float TotalWidth, float TotalHeight);
}
