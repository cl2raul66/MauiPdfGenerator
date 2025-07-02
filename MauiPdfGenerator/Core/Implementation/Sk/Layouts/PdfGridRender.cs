using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfGridRender : IElementRenderer
{
    private readonly GridVirtualLayoutCalculator _layoutCalculator = new();

    public async Task<LayoutInfo> MeasureAsync(PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var grid = (PdfGrid)element;
        var layoutResult = await _layoutCalculator.MeasureAsync(grid, rendererFactory, pageDef, layoutState, fontRegistry);

        float totalWidth = layoutResult.ColumnWidths.Sum() + grid.GetSpacing * (layoutResult.ColumnWidths.Length > 0 ? layoutResult.ColumnWidths.Length - 1 : 0);
        float totalHeight = layoutResult.RowHeights.Sum() + grid.GetSpacing * (layoutResult.RowHeights.Length > 0 ? layoutResult.RowHeights.Length - 1 : 0);

        totalWidth += (float)grid.GetPadding.HorizontalThickness + (float)grid.GetMargin.HorizontalThickness;
        totalHeight += (float)grid.GetPadding.VerticalThickness + (float)grid.GetMargin.VerticalThickness;

        layoutState[grid] = layoutResult;

        return new LayoutInfo(grid, totalWidth, totalHeight);
    }

    public async Task RenderAsync(SKCanvas canvas, PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect renderRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var grid = (PdfGrid)element;
        if (!layoutState.TryGetValue(grid, out var state) || state is not GridVirtualLayoutCalculator.GridLayoutResult layoutResult)
        {
            throw new InvalidOperationException("Grid layout state was not calculated before rendering.");
        }

        float[] colWidths = layoutResult.ColumnWidths;
        float[] rowHeights = layoutResult.RowHeights;
        float gridContentWidth = colWidths.Sum() + grid.GetSpacing * (colWidths.Length > 1 ? colWidths.Length - 1 : 0);
        float gridContentHeight = rowHeights.Sum() + grid.GetSpacing * (rowHeights.Length > 1 ? rowHeights.Length - 1 : 0);

        float left = renderRect.Left + (float)grid.GetMargin.Left + (float)grid.GetPadding.Left;
        float top = renderRect.Top + (float)grid.GetMargin.Top + (float)grid.GetPadding.Top;

        if (grid.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(grid.GetBackgroundColor), Style = SKPaintStyle.Fill };
            SKRect bgRect = SKRect.Create(
                renderRect.Left + (float)grid.GetMargin.Left,
                renderRect.Top + (float)grid.GetMargin.Top,
                gridContentWidth + (float)grid.GetPadding.HorizontalThickness,
                gridContentHeight + (float)grid.GetPadding.VerticalThickness);
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
                        var childRenderer = rendererFactory.GetRenderer(child);
                        var measure = await childRenderer.MeasureAsync(child, rendererFactory, pageDef, SKRect.Create(0, 0, cellWidth, cellHeight), layoutState, fontRegistry);

                        float childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? cellWidth : measure.Width;
                        float childHeight = child.GetVerticalOptions == LayoutAlignment.Fill ? cellHeight : measure.Height;

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
                        await childRenderer.RenderAsync(canvas, child, rendererFactory, pageDef, childRect, layoutState, fontRegistry);
                    }
                }
                x += colWidths[c] + grid.GetSpacing;
            }
            y += rowHeights[r] + grid.GetSpacing;
        }
    }
}
