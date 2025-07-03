using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class GridVirtualLayoutCalculator
{
    public record struct GridLayoutResult(float[] ColumnWidths, float[] RowHeights);
    private record CellInfo(object Element, int Row, int Column, int RowSpan, int ColSpan);

    public async Task<(GridLayoutResult, Dictionary<object, LayoutInfo>)> MeasureAsync(
        PdfGrid grid,
        SKRect availableRect,
        PdfGenerationContext context)
    {
        var colDefs = grid.ColumnDefinitionsList;
        var rowDefs = grid.RowDefinitionsList;
        int colCount = colDefs.Count;
        int rowCount = rowDefs.Count;

        var childMeasures = new Dictionary<object, LayoutInfo>();
        var cells = new List<CellInfo>();
        foreach (var child in grid.GetChildren)
        {
            cells.Add(new CellInfo(child, child.GridRow, child.GridColumn, child.GridRowSpan, child.GridColumnSpan));
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };
            var measure = await renderer.MeasureAsync(childContext, availableRect);
            childMeasures[child] = measure;
        }

        float[] colWidths = CalculateDimension(colDefs, childMeasures, cells, availableRect.Width, isColumn: true);
        float[] rowHeights = CalculateDimension(rowDefs, childMeasures, cells, availableRect.Height, isColumn: false);

        return (new GridLayoutResult(colWidths, rowHeights), childMeasures);
    }

    private float[] CalculateDimension(IReadOnlyList<PdfGridLength> definitions, Dictionary<object, LayoutInfo> childMeasures, List<CellInfo> cells, float availableSize, bool isColumn)
    {
        int count = definitions.Count;
        var result = new float[count];
        var measured = new bool[count];

        float totalAbsolute = 0;
        for (int i = 0; i < count; i++)
        {
            if (definitions[i].GridUnitType == GridUnitType.Absolute)
            {
                result[i] = (float)definitions[i].Value;
                measured[i] = true;
                totalAbsolute += result[i];
            }
        }

        float remainingForAutoAndStar = availableSize - totalAbsolute;

        for (int i = 0; i < count; i++)
        {
            if (definitions[i].GridUnitType == GridUnitType.Auto)
            {
                float maxChildSize = 0;
                var relevantCells = cells.Where(c => (isColumn ? c.Column : c.Row) == i && (isColumn ? c.ColSpan : c.RowSpan) == 1);
                foreach (var cell in relevantCells)
                {
                    if (childMeasures.TryGetValue(cell.Element, out var measure))
                    {
                        maxChildSize = Math.Max(maxChildSize, isColumn ? measure.Width : measure.Height);
                    }
                }
                result[i] = maxChildSize;
                measured[i] = true;
            }
        }

        float totalAuto = result.Where((_, i) => definitions[i].GridUnitType == GridUnitType.Auto).Sum();
        float totalStarValue = definitions.Where(d => d.GridUnitType == GridUnitType.Star).Sum(d => (float)d.Value);
        float availableForStar = remainingForAutoAndStar - totalAuto;

        if (totalStarValue > 0 && availableForStar > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (definitions[i].GridUnitType == GridUnitType.Star)
                {
                    result[i] = availableForStar * ((float)definitions[i].Value / totalStarValue);
                    measured[i] = true;
                }
            }
        }
        else
        {
            var starIndices = definitions.Select((d, i) => new { d, i }).Where(x => x.d.GridUnitType == GridUnitType.Star).ToList();
            if (starIndices.Any())
            {
                float sizePerStar = availableForStar > 0 ? availableForStar / starIndices.Count : 0;
                foreach (var item in starIndices)
                {
                    result[item.i] = sizePerStar;
                    measured[item.i] = true;
                }
            }
        }

        return result;
    }
}
