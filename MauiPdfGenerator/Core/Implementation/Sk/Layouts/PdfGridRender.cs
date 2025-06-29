using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfGridRender
{
    private readonly GridVirtualLayoutCalculator _layoutCalculator = new();

    public async Task<MeasureOutput> MeasureAsync(PdfGrid grid, PdfPageData pageDef, ElementsRender elementsRender, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var layoutResult = await _layoutCalculator.MeasureAsync(grid, pageDef,
            async (canvas, element, page, rect, y) =>
            {
                var measure = await elementsRender.Measure(element, page, rect, y, layoutState, fontRegistry);
                return new RenderOutput(measure.HeightRequired, measure.WidthRequired, measure.RemainingElement, measure.RequiresNewPage, measure.VisualHeight);
            });

        float totalWidth = layoutResult.ColumnWidths.Sum() + grid.GetSpacing * (layoutResult.ColumnWidths.Length > 0 ? layoutResult.ColumnWidths.Length - 1 : 0);
        float totalHeight = layoutResult.RowHeights.Sum() + grid.GetSpacing * (layoutResult.RowHeights.Length > 0 ? layoutResult.RowHeights.Length - 1 : 0);

        totalWidth += (float)grid.GetPadding.HorizontalThickness + (float)grid.GetMargin.HorizontalThickness;
        totalHeight += (float)grid.GetPadding.VerticalThickness + (float)grid.GetMargin.VerticalThickness;

        layoutState[grid] = layoutResult;

        return new MeasureOutput(totalHeight, totalHeight, totalWidth, [], null, false, 0, 0, 0, 0, null);
    }

    public async Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfGrid grid, PdfPageData pageDef, ElementsRender elementsRender, SKRect availableRect, float currentY, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        if (!layoutState.TryGetValue(grid, out var state) || state is not GridVirtualLayoutCalculator.GridLayoutResult layoutResult)
        {
            throw new InvalidOperationException("Grid layout state was not calculated before rendering.");
        }

        float[] colWidths = layoutResult.ColumnWidths;
        float[] rowHeights = layoutResult.RowHeights;
        float gridWidth = colWidths.Sum() + grid.GetSpacing * (colWidths.Length > 1 ? colWidths.Length - 1 : 0);
        float gridHeight = rowHeights.Sum() + grid.GetSpacing * (rowHeights.Length > 1 ? rowHeights.Length - 1 : 0);

        float left = availableRect.Left + (float)grid.GetMargin.Left + (float)grid.GetPadding.Left;
        float top = currentY + (float)grid.GetMargin.Top + (float)grid.GetPadding.Top;

        if (grid.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(grid.GetBackgroundColor), Style = SKPaintStyle.Fill };
            SKRect bgRect = SKRect.Create(left - (float)grid.GetPadding.Left, top - (float)grid.GetPadding.Top, gridWidth + (float)grid.GetPadding.HorizontalThickness, gridHeight + (float)grid.GetPadding.VerticalThickness);
            canvas.DrawRect(bgRect, bgPaint);
        }

        float y = top;
        for (int r = 0; r < rowHeights.Length; r++)
        {
            float x = left;
            for (int c = 0; c < colWidths.Length; c++)
            {
                var childrenInCell = grid.GetChildren.Where(e => e.GridRow == r && e.GridColumn == c).ToList();
                if (childrenInCell.Any())
                {
                    float cellWidth = colWidths[c];
                    float cellHeight = rowHeights[r];

                    foreach (var child in childrenInCell)
                    {
                        var measure = await elementsRender.Measure(child, pageDef, SKRect.Create(0, 0, cellWidth, cellHeight), 0, layoutState, fontRegistry);

                        float childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? cellWidth : measure.WidthRequired;
                        float childHeight = child.GetVerticalOptions == LayoutAlignment.Fill ? cellHeight : measure.VisualHeight;

                        float offsetX = child.GetHorizontalOptions switch
                        {
                            LayoutAlignment.Center => (cellWidth - childWidth) / 2f,
                            LayoutAlignment.End => cellWidth - childWidth,
                            _ => 0f
                        };

                        float offsetY = child.GetVerticalOptions switch
                        {
                            LayoutAlignment.Center => (cellHeight - childHeight) / 2f,
                            LayoutAlignment.End => cellHeight - childHeight,
                            _ => 0f
                        };

                        var childRect = SKRect.Create(x + offsetX, y + offsetY, childWidth, childHeight);
                        await elementsRender.Render(canvas, child, pageDef, childRect, childRect.Top, layoutState, fontRegistry);
                    }
                }
                x += colWidths[c] + grid.GetSpacing;
            }
            y += rowHeights[r] + grid.GetSpacing;
        }

        float totalHeight = gridHeight + (float)grid.GetPadding.VerticalThickness + (float)grid.GetMargin.VerticalThickness;
        float totalWidth = gridWidth + (float)grid.GetPadding.HorizontalThickness + (float)grid.GetMargin.HorizontalThickness;

        return new RenderOutput(totalHeight, totalWidth, null, false, totalHeight);
    }
}
