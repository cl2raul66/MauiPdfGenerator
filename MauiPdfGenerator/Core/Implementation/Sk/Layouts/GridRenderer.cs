using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Core.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class GridRenderer : IElementRenderer
{
    private record GridArrangeInfo(PdfRect CellRect, PdfLayoutInfo ArrangedChild);
    private record GridCache(float[] ColumnWidths, float[] RowHeights, List<PdfLayoutInfo> ChildMeasures);
    private record GridArrangeCache(PdfRect FinalRect, List<GridArrangeInfo> ArrangedCells);
    private record ChildMeasureInfo(IPdfGridCellInfo Child, PdfLayoutInfo Measure);

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfGridData grid)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfGridData)} or is null.");

        var availableSize = new SKSize(
            availableRect.Width - (float)grid.GetMargin.HorizontalThickness,
            availableRect.Height - (float)grid.GetMargin.VerticalThickness);

        var (columnWidths, rowHeights, childMeasures) = await MeasureGridContent(grid, context, availableSize);

        var totalContentWidth = columnWidths.Sum() + (float)grid.GetColumnSpacing * Math.Max(0, columnWidths.Length - 1);
        var totalContentHeight = rowHeights.Sum() + (float)grid.GetRowSpacing * Math.Max(0, rowHeights.Length - 1);

        var boxWidth = grid.GetWidthRequest.HasValue ? (float)grid.GetWidthRequest.Value : totalContentWidth + (float)grid.GetPadding.HorizontalThickness;
        var boxHeight = grid.GetHeightRequest.HasValue ? (float)grid.GetHeightRequest.Value : totalContentHeight + (float)grid.GetPadding.VerticalThickness;

        context.LayoutState[grid] = new GridCache(columnWidths, rowHeights, childMeasures);

        var totalWidth = boxWidth + (float)grid.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)grid.GetMargin.VerticalThickness;

        return new PdfLayoutInfo(grid, totalWidth, totalHeight);
    }

    public async Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfGridData grid)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfGridData)} or is null.");

        if (!context.LayoutState.TryGetValue(grid, out var state) || state is not GridCache measureCache)
        {
            context.Logger.LogError("Grid measure cache not found before arranging.");
            return new PdfLayoutInfo(grid, finalRect.Width, finalRect.Height, finalRect);
        }

        var elementBox = new PdfRect(
            finalRect.Left + (float)grid.GetMargin.Left,
            finalRect.Top + (float)grid.GetMargin.Top,
            finalRect.Width - (float)grid.GetMargin.HorizontalThickness,
            finalRect.Height - (float)grid.GetMargin.VerticalThickness
        );

        var contentBox = new PdfRect(
            elementBox.X + (float)grid.GetPadding.Left,
            elementBox.Y + (float)grid.GetPadding.Top,
            elementBox.Width - (float)grid.GetPadding.HorizontalThickness,
            elementBox.Height - (float)grid.GetPadding.VerticalThickness
        );

        var (rowsToArrange, remainingChildren) = DetermineRowsForCurrentPage(grid, measureCache.RowHeights, contentBox.Height);

        if (!rowsToArrange.Any() && remainingChildren.Count != 0)
        {
            return new PdfLayoutInfo(grid, finalRect.Width, 0, PdfRect.Empty, new PdfGridData(remainingChildren, grid));
        }

        var arrangedCells = await ArrangeChildrenInRows(grid, context, measureCache, rowsToArrange, contentBox);

        var heightForThisPage = rowsToArrange.Sum(r => measureCache.RowHeights[r]) + (float)grid.GetRowSpacing * Math.Max(0, rowsToArrange.Count - 1);
        var finalHeight = heightForThisPage + (float)grid.GetPadding.VerticalThickness + (float)grid.GetMargin.VerticalThickness;
        var arrangedRect = new PdfRect(finalRect.X, finalRect.Y, finalRect.Width, finalHeight);

        context.LayoutState[grid] = new GridArrangeCache(arrangedRect, arrangedCells);

        PdfGridData? continuationGrid = remainingChildren.Count != 0 ? new PdfGridData(remainingChildren, grid) : null;

        return new PdfLayoutInfo(grid, finalRect.Width, finalHeight, arrangedRect, continuationGrid);
    }

    public async Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfGridData grid)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfGridData)} or is null.");

        if (!context.LayoutState.TryGetValue(grid, out var state) || state is not GridArrangeCache arrangeCache)
        {
            context.Logger.LogError("Grid arrange cache not found before rendering.");
            return;
        }

        var elementBox = new SKRect(
            arrangeCache.FinalRect.Left + (float)grid.GetMargin.Left,
            arrangeCache.FinalRect.Top + (float)grid.GetMargin.Top,
            arrangeCache.FinalRect.Right - (float)grid.GetMargin.Right,
            arrangeCache.FinalRect.Bottom - (float)grid.GetMargin.Bottom
        );

        if (grid.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(grid.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBox, bgPaint);
        }

        foreach (var cellInfo in arrangeCache.ArrangedCells)
        {
            var child = (PdfElementData)cellInfo.ArrangedChild.Element;
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };

            canvas.Save();
            var childRect = cellInfo.ArrangedChild.FinalRect ?? cellInfo.CellRect;
            canvas.ClipRect(new SKRect(childRect.Left, childRect.Top, childRect.Right, childRect.Bottom));
            await renderer.RenderAsync(canvas, childContext);
            canvas.Restore();
        }
    }

    public Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context)
    {
        return Task.CompletedTask;
    }

    private async Task<(float[] columnWidths, float[] rowHeights, List<PdfLayoutInfo> childMeasures)> MeasureGridContent(PdfGridData grid, PdfGenerationContext context, SKSize availableSize)
    {
        var children = grid.GetChildren;
        var numCols = grid.GetColumnDefinitions.Any() ? grid.GetColumnDefinitions.Count : children.Any() ? children.Max(c => ((IPdfGridCellInfo)c).Column + ((IPdfGridCellInfo)c).ColumnSpan) : 1;
        var numRows = grid.GetRowDefinitions.Any() ? grid.GetRowDefinitions.Count : children.Any() ? children.Max(c => ((IPdfGridCellInfo)c).Row + ((IPdfGridCellInfo)c).RowSpan) : 1;

        var colDefs = grid.GetColumnDefinitions.Count == numCols ? grid.GetColumnDefinitions : Enumerable.Repeat(new ColumnDefinition(GridLength.Star), numCols).ToList();
        var rowDefs = grid.GetRowDefinitions.Count == numRows ? grid.GetRowDefinitions : Enumerable.Repeat(new RowDefinition(GridLength.Auto), numRows).ToList();

        var columnWidths = await CalculateColumnWidthsAsync(grid, context, availableSize.Width, colDefs, numCols);

        var childMeasures = new List<PdfLayoutInfo>();
        var rowMaxHeights = new float[numRows];
        foreach (var child in children)
        {
            var cellInfo = (IPdfGridCellInfo)child;
            var childContext = context with { Element = child };
            var renderer = context.RendererFactory.GetRenderer(child);

            var colSpan = Math.Min(cellInfo.ColumnSpan, numCols - cellInfo.Column);
            var childAvailableWidth = Enumerable.Range(cellInfo.Column, colSpan).Sum(c => columnWidths[c]) + (float)grid.GetColumnSpacing * (colSpan - 1);

            var measure = await renderer.MeasureAsync(childContext, new SKRect(0, 0, childAvailableWidth, float.PositiveInfinity));
            childMeasures.Add(measure);

            if (cellInfo.RowSpan == 1 && rowDefs[cellInfo.Row].Height.IsAuto)
            {
                rowMaxHeights[cellInfo.Row] = Math.Max(rowMaxHeights[cellInfo.Row], measure.Height);
            }
        }

        var zippedChildInfo = children.Zip(childMeasures, (c, m) => new ChildMeasureInfo((IPdfGridCellInfo)c, m)).ToList();
        var rowHeights = CalculateRowHeights(rowDefs, availableSize.Height, (float)grid.GetRowSpacing, rowMaxHeights, zippedChildInfo);

        return (columnWidths, rowHeights, childMeasures);
    }

    private async Task<float[]> CalculateColumnWidthsAsync(PdfGridData grid, PdfGenerationContext context, float totalAvailableWidth, IReadOnlyList<ColumnDefinition> colDefs, int numCols)
    {
        var columnWidths = new float[numCols];
        var children = grid.GetChildren;
        var childMeasures = new Dictionary<object, float>();

        totalAvailableWidth -= (float)grid.GetColumnSpacing * Math.Max(0, numCols - 1);

        // Pass 1: Absolute widths
        float assignedWidth = 0;
        for (int i = 0; i < numCols; i++)
        {
            if (colDefs[i].Width.IsAbsolute)
            {
                columnWidths[i] = (float)colDefs[i].Width.Value;
                assignedWidth += columnWidths[i];
            }
        }

        // Pass 2: Measure all children that span Auto columns to determine their desired width
        foreach (var child in children)
        {
            var info = (IPdfGridCellInfo)child;
            bool spansAuto = false;
            bool spansStar = false;
            for (int i = 0; i < info.ColumnSpan; i++)
            {
                int colIndex = info.Column + i;
                if (colIndex < numCols)
                {
                    if (colDefs[colIndex].Width.IsAuto) spansAuto = true;
                    if (colDefs[colIndex].Width.IsStar) spansStar = true;
                }
            }

            if (spansAuto && !spansStar)
            {
                var renderer = context.RendererFactory.GetRenderer(child);
                var childContext = context with { Element = child };
                var measure = await renderer.MeasureAsync(childContext, new SKRect(0, 0, float.PositiveInfinity, float.PositiveInfinity));
                childMeasures[child] = measure.Width;
            }
        }

        // Pass 3: Calculate Auto widths, iterating to solve dependencies
        bool changed;
        do
        {
            changed = false;
            for (int i = 0; i < numCols; i++)
            {
                if (colDefs[i].Width.IsAuto)
                {
                    float maxAutoWidth = 0;
                    foreach (var child in children)
                    {
                        var info = (IPdfGridCellInfo)child;
                        if (info.Column <= i && (info.Column + info.ColumnSpan) > i)
                        {
                            if (childMeasures.TryGetValue(child, out float desiredWidth))
                            {
                                float knownWidth = 0;
                                int autoColsInSpan = 0;
                                for (int j = 0; j < info.ColumnSpan; j++)
                                {
                                    int colIndex = info.Column + j;
                                    if (colDefs[colIndex].Width.IsAbsolute)
                                    {
                                        knownWidth += columnWidths[colIndex];
                                    }
                                    else if (colDefs[colIndex].Width.IsAuto)
                                    {
                                        autoColsInSpan++;
                                    }
                                }

                                if (autoColsInSpan > 0)
                                {
                                    float requiredWidth = (desiredWidth - knownWidth) / autoColsInSpan;
                                    if (requiredWidth > maxAutoWidth)
                                    {
                                        maxAutoWidth = requiredWidth;
                                    }
                                }
                            }
                        }
                    }

                    if (columnWidths[i] < maxAutoWidth)
                    {
                        columnWidths[i] = maxAutoWidth;
                        changed = true;
                    }
                }
            }
        } while (changed);

        assignedWidth += columnWidths.Where((w, i) => colDefs[i].Width.IsAuto).Sum();

        // Pass 4: Star widths
        float remainingForStar = totalAvailableWidth - assignedWidth;
        float totalStarValue = colDefs.Where(cd => cd.Width.IsStar).Sum(cd => (float)cd.Width.Value);

        if (remainingForStar > 0 && totalStarValue > 0)
        {
            float starUnit = remainingForStar / totalStarValue;
            for (int i = 0; i < numCols; i++)
            {
                if (colDefs[i].Width.IsStar)
                {
                    columnWidths[i] = (float)colDefs[i].Width.Value * starUnit;
                }
            }
        }

        return columnWidths;
    }

    private float[] CalculateRowHeights(IReadOnlyList<RowDefinition> rowDefs, float totalAvailableHeight, float spacing, float[] autoSizes, List<ChildMeasureInfo> childInfos)
    {
        var result = new float[rowDefs.Count];
        totalAvailableHeight -= spacing * Math.Max(0, result.Length - 1);
        float totalStar = rowDefs.Where(d => d.Height.IsStar).Sum(d => (float)d.Height.Value);
        float assigned = 0;

        for (int i = 0; i < result.Length; i++)
        {
            var def = rowDefs[i];
            if (def.Height.IsAbsolute)
            {
                result[i] = (float)def.Height.Value;
                assigned += result[i];
            }
            else if (def.Height.IsAuto)
            {
                result[i] = autoSizes[i];
                assigned += result[i];
            }
        }

        foreach (var info in childInfos)
        {
            if (info.Child.RowSpan > 1)
            {
                var rowSpan = Math.Min(info.Child.RowSpan, result.Length - info.Child.Row);
                var currentHeight = Enumerable.Range(info.Child.Row, rowSpan).Sum(r => result[r]) + spacing * (rowSpan - 1);
                var neededHeight = info.Measure.Height;
                if (neededHeight > currentHeight)
                {
                    var deficit = neededHeight - currentHeight;
                    var autoRows = Enumerable.Range(info.Child.Row, rowSpan).Where(r => rowDefs[r].Height.IsAuto).ToList();
                    if (autoRows.Count != 0)
                    {
                        var extraPerAutoRow = deficit / autoRows.Count;
                        foreach (var r in autoRows) result[r] += extraPerAutoRow;
                        assigned += deficit;
                    }
                }
            }
        }

        float remainingForStar = totalAvailableHeight - assigned;
        if (remainingForStar > 0 && totalStar > 0)
        {
            float starUnit = remainingForStar / totalStar;
            for (int i = 0; i < result.Length; i++)
            {
                if (rowDefs[i].Height.IsStar)
                {
                    result[i] = (float)rowDefs[i].Height.Value * starUnit;
                }
            }
        }
        return result;
    }

    private (List<int> rowsToArrange, List<PdfElementData> remainingChildren) DetermineRowsForCurrentPage(PdfGridData grid, float[] rowHeights, float availableHeight)
    {
        var rowsToArrange = new List<int>();
        var allChildren = grid.GetChildren;
        var remainingChildren = new List<PdfElementData>(allChildren);
        float currentHeight = 0;

        for (int i = 0; i < rowHeights.Length; i++)
        {
            if (rowsToArrange.Contains(i)) continue;

            var rowSpanGroups = allChildren
                .Where(c => ((IPdfGridCellInfo)c).Row <= i && (((IPdfGridCellInfo)c).Row + ((IPdfGridCellInfo)c).RowSpan) > i)
                .Select(c => new { StartRow = ((IPdfGridCellInfo)c).Row, Span = ((IPdfGridCellInfo)c).RowSpan })
                .ToList();

            var blockStartRow = rowSpanGroups.Count != 0 ? rowSpanGroups.Min(g => g.StartRow) : i;
            var blockEndRow = rowSpanGroups.Count != 0 ? rowSpanGroups.Max(g => g.StartRow + g.Span) - 1 : i;

            var blockHeight = Enumerable.Range(blockStartRow, blockEndRow - blockStartRow + 1).Sum(r => rowHeights[r]) + (float)grid.GetRowSpacing * (blockEndRow - blockStartRow);

            if (currentHeight + blockHeight <= availableHeight || rowsToArrange.Count == 0)
            {
                for (int j = blockStartRow; j <= blockEndRow; j++)
                {
                    if (!rowsToArrange.Contains(j))
                    {
                        rowsToArrange.Add(j);
                    }
                }
                currentHeight = rowsToArrange.Sum(r => rowHeights[r]) + (float)grid.GetRowSpacing * Math.Max(0, rowsToArrange.Count - 1);
                i = blockEndRow;
            }
            else
            {
                break;
            }
        }

        if (rowsToArrange.Any())
        {
            var maxRow = rowsToArrange.Max();
            remainingChildren.RemoveAll(c => (((IPdfGridCellInfo)c).Row + ((IPdfGridCellInfo)c).RowSpan - 1) <= maxRow);
        }
        else if (allChildren.Any())
        {
            remainingChildren = [.. allChildren];
        }

        return (rowsToArrange, remainingChildren);
    }

    private async Task<List<GridArrangeInfo>> ArrangeChildrenInRows(PdfGridData grid, PdfGenerationContext context, GridCache measureCache, List<int> rowsToArrange, PdfRect contentBox)
    {
        var arrangedCells = new List<GridArrangeInfo>();
        var childrenToArrange = grid.GetChildren.Where(c => rowsToArrange.Contains(((IPdfGridCellInfo)c).Row)).ToList();

        var rowOffsets = new float[measureCache.RowHeights.Length];
        for (int i = 1; i < measureCache.RowHeights.Length; i++)
        {
            rowOffsets[i] = rowOffsets[i - 1] + measureCache.RowHeights[i - 1] + (float)grid.GetRowSpacing;
        }

        var colOffsets = new float[measureCache.ColumnWidths.Length];
        for (int i = 1; i < measureCache.ColumnWidths.Length; i++)
        {
            colOffsets[i] = colOffsets[i - 1] + measureCache.ColumnWidths[i - 1] + (float)grid.GetColumnSpacing;
        }

        foreach (var child in childrenToArrange)
        {
            var cellInfo = (IPdfGridCellInfo)child;
            var measure = measureCache.ChildMeasures.First(m => m.Element == child);
            var renderer = context.RendererFactory.GetRenderer(child);

            var cellWidth = Enumerable.Range(cellInfo.Column, cellInfo.ColumnSpan).Sum(c => measureCache.ColumnWidths[c]) + (float)grid.GetColumnSpacing * (cellInfo.ColumnSpan - 1);
            var cellHeight = Enumerable.Range(cellInfo.Row, cellInfo.RowSpan).Sum(r => measureCache.RowHeights[r]) + (float)grid.GetRowSpacing * (cellInfo.RowSpan - 1);

            var childWidth = child.GetHorizontalOptions is LayoutAlignment.Fill ? cellWidth - (float)child.GetMargin.HorizontalThickness : measure.Width;
            var childHeight = child.GetVerticalOptions is LayoutAlignment.Fill ? cellHeight - (float)child.GetMargin.VerticalThickness : measure.Height;

            var offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (cellWidth - childWidth - (float)child.GetMargin.HorizontalThickness) / 2f,
                LayoutAlignment.End => cellWidth - childWidth - (float)child.GetMargin.Right,
                _ => (float)child.GetMargin.Left
            };
            var offsetY = child.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (cellHeight - childHeight - (float)child.GetMargin.VerticalThickness) / 2f,
                LayoutAlignment.End => cellHeight - childHeight - (float)child.GetMargin.Bottom,
                _ => (float)child.GetMargin.Top
            };

            var childRect = new PdfRect(
                contentBox.X + colOffsets[cellInfo.Column] + offsetX,
                contentBox.Y + rowOffsets[cellInfo.Row] + offsetY,
                childWidth,
                childHeight
            );

            var childContext = context with { Element = child };
            var arrangedChild = await renderer.ArrangeAsync(childRect, childContext);
            arrangedCells.Add(new GridArrangeInfo(childRect, arrangedChild));
        }

        return arrangedCells;
    }
}
