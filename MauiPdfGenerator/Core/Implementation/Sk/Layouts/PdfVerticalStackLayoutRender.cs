using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfVerticalStackLayoutRender : IElementRenderer
{
    public async Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfVerticalStackLayout vsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfVerticalStackLayout)} or is null.");

        float totalHeight = 0;
        float maxWidth = 0;
        var childMeasures = new List<LayoutInfo>();

        // El ancho disponible para los hijos es el ancho del VSL menos su propio padding.
        var childAvailableWidth = availableRect.Width - (float)vsl.GetPadding.HorizontalThickness;

        foreach (var child in vsl.GetChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };
            // Pasamos el ancho restringido a los hijos.
            var measure = await renderer.MeasureAsync(childContext, SKRect.Create(0, 0, childAvailableWidth, float.PositiveInfinity));
            childMeasures.Add(measure);
            totalHeight += measure.Height;
            maxWidth = Math.Max(maxWidth, measure.Width);
        }

        if (vsl.GetChildren.Count > 1)
        {
            totalHeight += vsl.GetSpacing * (vsl.GetChildren.Count - 1);
        }

        float boxWidth = maxWidth + (float)vsl.GetPadding.HorizontalThickness;
        float boxHeight = totalHeight + (float)vsl.GetPadding.VerticalThickness;

        context.LayoutState[vsl] = childMeasures;

        var totalWidth = boxWidth + (float)vsl.GetMargin.HorizontalThickness;
        var totalHeightWithMargin = boxHeight + (float)vsl.GetMargin.VerticalThickness;

        return new LayoutInfo(vsl, totalWidth, totalHeightWithMargin);
    }

    public async Task RenderAsync(SKCanvas canvas, SKRect renderRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfVerticalStackLayout vsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfVerticalStackLayout)} or is null.");

        if (!context.LayoutState.TryGetValue(vsl, out var state) || state is not List<LayoutInfo> childMeasures)
        {
            context.Logger.LogError("VerticalStackLayout state was not calculated before rendering.");
            return;
        }

        var elementBox = new SKRect(
            renderRect.Left + (float)vsl.GetMargin.Left,
            renderRect.Top + (float)vsl.GetMargin.Top,
            renderRect.Right - (float)vsl.GetMargin.Right,
            renderRect.Bottom - (float)vsl.GetMargin.Bottom
        );

        if (vsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBox, bgPaint);
        }

        float contentWidth = elementBox.Width - (float)vsl.GetPadding.HorizontalThickness;
        float currentY = elementBox.Top + (float)vsl.GetPadding.Top;

        for (int i = 0; i < vsl.GetChildren.Count; i++)
        {
            var child = (PdfElement)vsl.GetChildren[i];
            var measure = childMeasures[i];
            var renderer = context.RendererFactory.GetRenderer(child);

            float childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? contentWidth : measure.Width;

            float offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentWidth - childWidth) / 2f,
                LayoutAlignment.End => contentWidth - childWidth,
                _ => 0f
            };

            float x = elementBox.Left + (float)vsl.GetPadding.Left + offsetX;
            var childRect = SKRect.Create(x, currentY, childWidth, measure.Height);
            var childContext = context with { Element = child };

            await renderer.RenderAsync(canvas, childRect, childContext);

            currentY += measure.Height;
            if (i < vsl.GetChildren.Count - 1)
            {
                currentY += vsl.GetSpacing;
            }
        }
    }
}
