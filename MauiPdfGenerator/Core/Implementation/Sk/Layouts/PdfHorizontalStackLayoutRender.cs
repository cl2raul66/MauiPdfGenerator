using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfHorizontalStackLayoutRender : IElementRenderer
{
    public async Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfHorizontalStackLayout hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayout)} or is null.");

        float totalWidth = 0;
        float maxHeight = 0;
        var childMeasures = new List<LayoutInfo>();

        foreach (var child in hsl.GetChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };
            var measure = await renderer.MeasureAsync(childContext, SKRect.Create(0, 0, float.PositiveInfinity, float.PositiveInfinity));
            childMeasures.Add(measure);
            totalWidth += measure.Width;
            maxHeight = Math.Max(maxHeight, measure.Height);
        }

        if (hsl.GetChildren.Count > 1)
        {
            totalWidth += hsl.GetSpacing * (hsl.GetChildren.Count - 1);
        }

        totalWidth += (float)hsl.GetPadding.HorizontalThickness + (float)hsl.GetMargin.HorizontalThickness;
        maxHeight += (float)hsl.GetPadding.VerticalThickness + (float)hsl.GetMargin.VerticalThickness;

        context.LayoutState[hsl] = childMeasures;

        return new LayoutInfo(hsl, totalWidth, maxHeight);
    }

    public async Task RenderAsync(SKCanvas canvas, SKRect renderRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalStackLayout hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayout)} or is null.");

        if (!context.LayoutState.TryGetValue(hsl, out var state) || state is not List<LayoutInfo> childMeasures)
        {
            context.Logger.LogError("HorizontalStackLayout state was not calculated before rendering.");
            return;
        }

        float contentWidth = childMeasures.Sum(m => m.Width) + (hsl.GetChildren.Count > 1 ? hsl.GetSpacing * (hsl.GetChildren.Count - 1) : 0);
        float contentHeight = childMeasures.Any() ? childMeasures.Max(m => m.Height) : 0;

        if (hsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            var bgRect = SKRect.Create(
                renderRect.Left + (float)hsl.GetMargin.Left,
                renderRect.Top + (float)hsl.GetMargin.Top,
                contentWidth + (float)hsl.GetPadding.HorizontalThickness,
                contentHeight + (float)hsl.GetPadding.VerticalThickness);
            canvas.DrawRect(bgRect, bgPaint);
        }

        float x = renderRect.Left + (float)hsl.GetMargin.Left + (float)hsl.GetPadding.Left;

        for (int i = 0; i < hsl.GetChildren.Count; i++)
        {
            var child = (PdfElement)hsl.GetChildren[i];
            var measure = childMeasures[i];
            var renderer = context.RendererFactory.GetRenderer(child);

            float offsetY = child.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (contentHeight - measure.Height) / 2f,
                LayoutAlignment.End => contentHeight - measure.Height,
                _ => 0f
            };

            float y = renderRect.Top + (float)hsl.GetMargin.Top + (float)hsl.GetPadding.Top + offsetY;
            var childRect = SKRect.Create(x, y, measure.Width, measure.Height);
            var childContext = context with { Element = child };
            await renderer.RenderAsync(canvas, childRect, childContext);

            x += measure.Width;
            if (i < hsl.GetChildren.Count - 1)
            {
                x += hsl.GetSpacing;
            }
        }
    }
}
