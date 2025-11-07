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
    private record GridCache(float[] ColumnWidths, float[] RowHeights, List<PdfLayoutInfo> ChildMeasures, IReadOnlyList<RowDefinition> RowDefs);
    private record GridArrangeCache(PdfRect FinalRect, List<GridArrangeInfo> ArrangedCells);
    private record ChildMeasureInfo(IPdfGridCellInfo Child, PdfLayoutInfo Measure);

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfGridData grid)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfGridData)} or is null.");

        var contentAvailableWidth = availableRect.Width - (float)grid.GetMargin.HorizontalThickness - (float)grid.GetPadding.HorizontalThickness;
        if (grid.GetWidthRequest.HasValue)
        {
            contentAvailableWidth = (float)grid.GetWidthRequest.Value - (float)grid.GetPadding.HorizontalThickness;
        }

        var (columnWidths, rowHeights, childMeasures, rowDefs) = await MeasureGridContent(grid, context, new SKSize(contentAvailableWidth, float.PositiveInfinity));

        float finalContentWidth = columnWidths.Sum() + (float)grid.GetColumnSpacing * Math.Max(0, columnWidths.Length - 1);
        float finalContentHeight = rowHeights.Sum() + (float)grid.GetRowSpacing * Math.Max(0, rowHeights.Length - 1);

        float boxWidth = finalContentWidth + (float)grid.GetPadding.HorizontalThickness;
        float boxHeight = finalContentHeight + (float)grid.GetPadding.VerticalThickness;

        context.LayoutState[grid] = new GridCache(columnWidths, rowHeights, childMeasures, rowDefs);

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
            await MeasureAsync(context, new SKRect(0, 0, finalRect.Width, float.PositiveInfinity));
            if (!context.LayoutState.TryGetValue(grid, out state) || state is not GridCache)
            {
                context.Logger.LogError("Grid measure cache not found before arranging and could not be recreated.");
                return new PdfLayoutInfo(grid, finalRect.Width, finalRect.Height, finalRect);
            }
            measureCache = (GridCache)state;
        }

        var finalRowHeights = measureCache.RowHeights;
        var availableContentHeight = finalRect.Height - (float)grid.GetPadding.VerticalThickness - (float)grid.GetMargin.VerticalThickness;

        if (grid.GetVerticalOptions == LayoutAlignment.Fill && availableContentHeight > measureCache.RowHeights.Sum())
        {
            var zippedChildInfo = grid.GetChildren.Cast<PdfElementData>()
                .Zip(measureCache.ChildMeasures, (c, m) => new ChildMeasureInfo(c, m))
                .ToList();

            var autoRowHeights = new float[measureCache.RowDefs.Count];
            for (int i = 0; i < measureCache.RowDefs.Count; i++)
            {
                if (measureCache.RowDefs[i].Height.IsAuto)
                {
                    autoRowHeights[i] = measureCache.RowHeights[i];
                }
            }

            finalRowHeights = CalculateRowHeights(
                measureCache.RowDefs,
                availableContentHeight,
                (float)grid.GetRowSpacing,
                autoRowHeights,
                zippedChildInfo
            );
        }

        var (rowsForThisPage, remainingChildren) = DetermineRowsForCurrentPage(grid, finalRowHeights, availableContentHeight);

        if (rowsForThisPage.Count == 0 && grid.GetChildren.Any())
        {
            return new PdfLayoutInfo(grid, finalRect.Width, 0, PdfRect.Empty, grid);
        }

        var contentBox = new PdfRect(
            finalRect.X + (float)grid.GetMargin.Left + (float)grid.GetPadding.Left,
            finalRect.Y + (float)grid.GetMargin.Top + (float)grid.GetPadding.Top,
            finalRect.Width - (float)grid.GetMargin.HorizontalThickness - (float)grid.GetPadding.HorizontalThickness,
            finalRect.Height - (float)grid.GetMargin.VerticalThickness - (float)grid.GetPadding.VerticalThickness
        );

        var childrenForThisPage = grid.GetChildren.Cast<PdfElementData>().Where(c => !remainingChildren.Contains(c)).ToList();
        var arrangedCells = await ArrangeChildrenInRows(grid, context, measureCache.ColumnWidths, finalRowHeights, measureCache.ChildMeasures, childrenForThisPage, contentBox);

        float arrangedContentHeight = rowsForThisPage.Count != 0 ? rowsForThisPage.Select(r => finalRowHeights[r]).Sum() + (float)grid.GetRowSpacing * Math.Max(0, rowsForThisPage.Count - 1) : 0;
        float finalHeightForThisPage = arrangedContentHeight + (float)grid.GetPadding.VerticalThickness + (float)grid.GetMargin.VerticalThickness;
        var arrangedRect = new PdfRect(finalRect.X, finalRect.Y, finalRect.Width, finalHeightForThisPage);

        context.LayoutState[grid] = new GridArrangeCache(arrangedRect, arrangedCells);

        PdfGridData? continuationGrid = remainingChildren.Count != 0 ? new PdfGridData(remainingChildren, grid) : null;

        return new PdfLayoutInfo(grid, finalRect.Width, finalHeightForThisPage, arrangedRect, continuationGrid);
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

    private async Task<(float[] columnWidths, float[] rowHeights, List<PdfLayoutInfo> childMeasures, IReadOnlyList<RowDefinition> rowDefs)> MeasureGridContent(PdfGridData grid, PdfGenerationContext context, SKSize availableContentSize)
    {
        var children = grid.GetChildren.Cast<PdfElementData>().ToList();
        var numCols = grid.GetColumnDefinitions.Any() ? grid.GetColumnDefinitions.Count : children.Count != 0 ? children.Max(c => c.GridColumn + c.GridColumnSpan) : 1;
        var numRows = grid.GetRowDefinitions.Any() ? grid.GetRowDefinitions.Count : children.Count != 0 ? children.Max(c => c.GridRow + c.GridRowSpan) : 1;

        var colDefs = grid.GetColumnDefinitions.Count == numCols ? [.. grid.GetColumnDefinitions] : Enumerable.Repeat(new ColumnDefinition(GridLength.Star), numCols).ToList();
        var rowDefs = grid.GetRowDefinitions.Count == numRows ? grid.GetRowDefinitions.ToList() : [.. Enumerable.Repeat(new RowDefinition(GridLength.Auto), numRows)];

        var columnWidths = await CalculateColumnWidthsAsync(grid, context, availableContentSize.Width, colDefs, numCols);

        var childMeasures = new List<PdfLayoutInfo>();
        var rowAutoHeights = new float[numRows];
        foreach (var child in children)
        {
            var cellInfo = (IPdfGridCellInfo)child;
            if (cellInfo.Row >= numRows || cellInfo.Column >= numCols) continue;

            var childContext = context with { Element = child };
            var renderer = context.RendererFactory.GetRenderer(child);

            var colSpan = Math.Min(cellInfo.ColumnSpan, numCols - cellInfo.Column);
            var childAvailableWidth = Enumerable.Range(cellInfo.Column, colSpan).Sum(c => columnWidths[c]) + (float)grid.GetColumnSpacing * (colSpan - 1);

            var measure = await renderer.MeasureAsync(childContext, new SKRect(0, 0, childAvailableWidth, float.PositiveInfinity));
            childMeasures.Add(measure);

            if (cellInfo.RowSpan == 1 && rowDefs[cellInfo.Row].Height.IsAuto)
            {
                rowAutoHeights[cellInfo.Row] = Math.Max(rowAutoHeights[cellInfo.Row], measure.Height);
            }
        }

        var zippedChildInfo = children.Zip(childMeasures, (c, m) => new ChildMeasureInfo((IPdfGridCellInfo)c, m)).ToList();
        var rowHeights = CalculateRowHeights(rowDefs, float.PositiveInfinity, (float)grid.GetRowSpacing, rowAutoHeights, zippedChildInfo);

        return (columnWidths, rowHeights, childMeasures, rowDefs);
    }

    private async Task<float[]> CalculateColumnWidthsAsync(PdfGridData grid, PdfGenerationContext context, float totalAvailableWidth, IReadOnlyList<ColumnDefinition> colDefs, int numCols)
    {
        var columnWidths = new float[numCols];
        var children = grid.GetChildren.Cast<PdfElementData>().ToList();

        bool isWidthConstrained = !float.IsPositiveInfinity(totalAvailableWidth);

        bool treatStarAsAuto = grid.GetHorizontalOptions is not LayoutAlignment.Fill && !grid.GetWidthRequest.HasValue;

        var effectiveColDefs = new List<ColumnDefinition>();
        foreach (var cd in colDefs)
        {
            if (treatStarAsAuto && cd.Width.IsStar)
            {
                effectiveColDefs.Add(new ColumnDefinition(GridLength.Auto));
            }
            else
            {
                effectiveColDefs.Add(cd);
            }
        }

        float availableWidthForStars = isWidthConstrained ? totalAvailableWidth - (float)grid.GetColumnSpacing * Math.Max(0, numCols - 1) : 0;

        for (int i = 0; i < numCols; i++)
        {
            if (effectiveColDefs[i].Width.IsAbsolute)
            {
                columnWidths[i] = (float)effectiveColDefs[i].Width.Value;
                if (isWidthConstrained) availableWidthForStars -= columnWidths[i];
            }
        }

        var autoColumns = Enumerable.Range(0, numCols).Where(i => effectiveColDefs[i].Width.IsAuto).ToList();
        if (autoColumns.Count != 0)
        {
            var childMeasures = new Dictionary<object, float>();
            foreach (var child in children)
            {
                var info = (IPdfGridCellInfo)child;
                bool spansOnlyAutoOrAbsolute = true;
                for (int i = 0; i < info.ColumnSpan; i++)
                {
                    if (info.Column + i < numCols && effectiveColDefs[info.Column + i].Width.IsStar)
                    {
                        spansOnlyAutoOrAbsolute = false;
                        break;
                    }
                }

                if (spansOnlyAutoOrAbsolute)
                {
                    var renderer = context.RendererFactory.GetRenderer(child);
                    var childContext = context with { Element = child };
                    var measure = await renderer.MeasureAsync(childContext, new SKRect(0, 0, float.PositiveInfinity, float.PositiveInfinity));
                    childMeasures[child] = measure.Width;
                }
            }

            foreach (int i in autoColumns)
            {
                float maxAutoWidth = 0;
                foreach (var child in children)
                {
                    var info = (IPdfGridCellInfo)child;
                    if (info.Column <= i && (info.Column + info.ColumnSpan) > i && childMeasures.TryGetValue(child, out float desiredWidth))
                    {
                        float knownWidthInSpan = 0;
                        int autoColsInSpan = 0;
                        for (int j = 0; j < info.ColumnSpan; j++)
                        {
                            int colIndex = info.Column + j;
                            if (effectiveColDefs[colIndex].Width.IsAbsolute) knownWidthInSpan += columnWidths[colIndex];
                            else if (effectiveColDefs[colIndex].Width.IsAuto) autoColsInSpan++;
                        }
                        if (autoColsInSpan > 0)
                        {
                            maxAutoWidth = Math.Max(maxAutoWidth, (desiredWidth - knownWidthInSpan) / autoColsInSpan);
                        }
                    }
                }
                columnWidths[i] = maxAutoWidth;
                if (isWidthConstrained) availableWidthForStars -= columnWidths[i];
            }
        }

        if (isWidthConstrained && availableWidthForStars > 0)
        {
            float totalStarValue = effectiveColDefs.Where(cd => cd.Width.IsStar).Sum(cd => (float)cd.Width.Value);
            if (totalStarValue > 0)
            {
                float starUnit = availableWidthForStars / totalStarValue;
                for (int i = 0; i < numCols; i++)
                {
                    if (effectiveColDefs[i].Width.IsStar)
                    {
                        columnWidths[i] = (float)effectiveColDefs[i].Width.Value * starUnit;
                    }
                }
            }
        }

        return columnWidths;
    }

    private float[] CalculateRowHeights(IReadOnlyList<RowDefinition> rowDefs, float totalAvailableHeight, float spacing, float[] autoSizes, List<ChildMeasureInfo> childInfos)
    {
        var result = new float[rowDefs.Count];
        bool isHeightConstrained = !float.IsPositiveInfinity(totalAvailableHeight);

        float availableHeightForStars = isHeightConstrained ? totalAvailableHeight - spacing * Math.Max(0, result.Length - 1) : 0;

        for (int i = 0; i < result.Length; i++)
        {
            var def = rowDefs[i];
            if (def.Height.IsAbsolute)
            {
                result[i] = (float)def.Height.Value;
                if (isHeightConstrained) availableHeightForStars -= result[i];
            }
            else if (def.Height.IsAuto)
            {
                result[i] = autoSizes.Length > i ? autoSizes[i] : 0;
                if (isHeightConstrained) availableHeightForStars -= result[i];
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
                    if (autoRows.Count > 0)
                    {
                        var extraPerAutoRow = deficit / autoRows.Count;
                        foreach (var r in autoRows)
                        {
                            result[r] += extraPerAutoRow;
                            if (isHeightConstrained) availableHeightForStars -= extraPerAutoRow;
                        }
                    }
                }
            }
        }

        if (isHeightConstrained && availableHeightForStars > 0)
        {
            float totalStar = rowDefs.Where(d => d.Height.IsStar).Sum(d => (float)d.Height.Value);
            if (totalStar > 0)
            {
                float starUnit = availableHeightForStars / totalStar;
                for (int i = 0; i < result.Length; i++)
                {
                    if (rowDefs[i].Height.IsStar)
                    {
                        result[i] = (float)rowDefs[i].Height.Value * starUnit;
                    }
                }
            }
        }

        return result;
    }

    private (List<int> rowsToArrange, List<PdfElementData> remainingChildren) DetermineRowsForCurrentPage(PdfGridData grid, float[] rowHeights, float availableHeight)
    {
        var rowsToArrange = new List<int>();
        var allChildren = grid.GetChildren.Cast<PdfElementData>().ToList();
        float currentHeight = 0;

        for (int i = 0; i < rowHeights.Length; i++)
        {
            if (rowsToArrange.Contains(i)) continue;

            var childrenTouchingRow = allChildren.Where(c => c.GridRow <= i && (c.GridRow + c.GridRowSpan) > i).ToList();
            if (childrenTouchingRow.Count == 0)
            {
                if (currentHeight + rowHeights[i] <= availableHeight)
                {
                    rowsToArrange.Add(i);
                    currentHeight += rowHeights[i] + (rowsToArrange.Count != 0 ? (float)grid.GetRowSpacing : 0);
                }
                continue;
            }

            var blockStartRow = childrenTouchingRow.Min(c => c.GridRow);
            var blockEndRow = childrenTouchingRow.Max(c => c.GridRow + c.GridRowSpan - 1);

            float blockHeight = 0;
            for (int j = blockStartRow; j <= blockEndRow; j++)
            {
                blockHeight += rowHeights[j];
            }
            blockHeight += (float)grid.GetRowSpacing * (blockEndRow - blockStartRow);

            float heightWithBlock = (rowsToArrange.Count == 0) ? blockHeight : currentHeight + (float)grid.GetRowSpacing + blockHeight;

            if (heightWithBlock <= availableHeight || rowsToArrange.Count == 0)
            {
                for (int j = blockStartRow; j <= blockEndRow; j++)
                {
                    if (!rowsToArrange.Contains(j)) rowsToArrange.Add(j);
                }

                currentHeight = 0;
                for (int r = 0; r < rowsToArrange.Count; r++)
                {
                    currentHeight += rowHeights[rowsToArrange[r]];
                    if (r < rowsToArrange.Count - 1) currentHeight += (float)grid.GetRowSpacing;
                }
                i = blockEndRow;
            }
            else
            {
                break;
            }
        }

        var remainingChildren = allChildren.Where(c => !rowsToArrange.Contains(c.GridRow)).ToList();
        return (rowsToArrange, remainingChildren);
    }

    private async Task<List<GridArrangeInfo>> ArrangeChildrenInRows(PdfGridData grid, PdfGenerationContext context, float[] columnWidths, float[] rowHeights, List<PdfLayoutInfo> childMeasures, List<PdfElementData> childrenToArrange, PdfRect contentBox)
    {
        var arrangedCells = new List<GridArrangeInfo>();
        var rowOffsets = new float[rowHeights.Length];
        for (int i = 1; i < rowHeights.Length; i++)
        {
            rowOffsets[i] = rowOffsets[i - 1] + rowHeights[i - 1] + (float)grid.GetRowSpacing;
        }

        var colOffsets = new float[columnWidths.Length];
        for (int i = 1; i < columnWidths.Length; i++)
        {
            colOffsets[i] = colOffsets[i - 1] + columnWidths[i - 1] + (float)grid.GetColumnSpacing;
        }

        foreach (var child in childrenToArrange)
        {
            var cellInfo = (IPdfGridCellInfo)child;
            var measure = childMeasures.First(m => m.Element == child);
            var renderer = context.RendererFactory.GetRenderer(child);

            var colSpan = Math.Min(cellInfo.ColumnSpan, columnWidths.Length - cellInfo.Column);
            var rowSpan = Math.Min(cellInfo.RowSpan, rowHeights.Length - cellInfo.Row);

            var cellWidth = Enumerable.Range(cellInfo.Column, colSpan).Sum(c => columnWidths[c]) + (float)grid.GetColumnSpacing * (colSpan - 1);
            var cellHeight = Enumerable.Range(cellInfo.Row, rowSpan).Sum(r => rowHeights[r]) + (float)grid.GetRowSpacing * (rowSpan - 1);

            var slotWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? cellWidth : measure.Width;
            var slotHeight = child.GetVerticalOptions == LayoutAlignment.Fill ? cellHeight : measure.Height;

            var offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (cellWidth - slotWidth) / 2f,
                LayoutAlignment.End => cellWidth - slotWidth,
                _ => 0 
            };
            var offsetY = child.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (cellHeight - slotHeight) / 2f,
                LayoutAlignment.End => cellHeight - slotHeight,
                _ => 0 
            };

            var childRect = new PdfRect(
                contentBox.X + colOffsets[cellInfo.Column] + offsetX,
                contentBox.Y + rowOffsets[cellInfo.Row] + offsetY,
                Math.Max(0, slotWidth),
                Math.Max(0, slotHeight)
            );

            var childContext = context with { Element = child };
            var arrangedChild = await renderer.ArrangeAsync(childRect, childContext);
            arrangedCells.Add(new GridArrangeInfo(childRect, arrangedChild));
        }

        return arrangedCells;
    }
}
