using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfGridRender : IElementRenderer
{
    private readonly GridVirtualLayoutCalculator _layoutCalculator = new();

    private record GridRenderCache(GridVirtualLayoutCalculator.GridLayoutResult LayoutResult, Dictionary<object, LayoutInfo> ChildMeasures);

    public async Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfGrid grid)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfGrid)} or is null.");

        var (layoutResult, childMeasures) = await _layoutCalculator.MeasureAsync(grid, availableRect, context);

        float totalWidth = layoutResult.ColumnWidths.Sum() + grid.GetSpacing * (layoutResult.ColumnWidths.Length > 1 ? layoutResult.ColumnWidths.Length - 1 : 0);
        float totalHeight = layoutResult.RowHeights.Sum() + grid.GetSpacing * (layoutResult.RowHeights.Length > 1 ? layoutResult.RowHeights.Length - 1 : 0);

        totalWidth += (float)grid.GetPadding.HorizontalThickness + (float)grid.GetMargin.HorizontalThickness;
        totalHeight += (float)grid.GetPadding.VerticalThickness + (float)grid.GetMargin.VerticalThickness;

        context.LayoutState[grid] = new GridRenderCache(layoutResult, childMeasures);

        return new LayoutInfo(grid, totalWidth, totalHeight);
    }

    public async Task RenderAsync(SKCanvas canvas, SKRect renderRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfGrid grid)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfGrid)} or is null.");

        if (!context.LayoutState.TryGetValue(grid, out var state) || state is not GridRenderCache cache)
        {
            context.Logger.LogError("Grid layout state was not calculated before rendering. MeasureAsync must be called first.");
            return;
        }

        var layoutResult = cache.LayoutResult;
        var childMeasures = cache.ChildMeasures;

        float[] colWidths = layoutResult.ColumnWidths;
        float[] rowHeights = layoutResult.RowHeights;

        float left = renderRect.Left + (float)grid.GetMargin.Left + (float)grid.GetPadding.Left;
        float top = renderRect.Top + (float)grid.GetMargin.Top + (float)grid.GetPadding.Top;

        if (grid.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(grid.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(renderRect, bgPaint);
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
                        var childRenderer = context.RendererFactory.GetRenderer(child);

                        if (!childMeasures.TryGetValue(child, out var measure))
                        {
                            continue;
                        }

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
                        var childContext = context with { Element = child };
                        await childRenderer.RenderAsync(canvas, childRect, childContext);
                    }
                }
                x += colWidths[c] + grid.GetSpacing;
            }
            y += rowHeights[r] + grid.GetSpacing;
        }
    }
}
