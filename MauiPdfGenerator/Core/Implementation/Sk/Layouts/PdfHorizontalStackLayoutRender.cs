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

        var childMeasures = new List<LayoutInfo>();
        foreach (var child in hsl.GetChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };
            // El HSL simplemente pasa las restricciones de su padre a sus hijos.
            var measure = await renderer.MeasureAsync(childContext, availableRect);
            childMeasures.Add(measure);
        }

        // El ancho del contenido es la suma de las "huellas" totales de los hijos.
        float contentWidth = childMeasures.Sum(m => m.Width);
        if (childMeasures.Count > 1)
        {
            contentWidth += hsl.GetSpacing * (childMeasures.Count - 1);
        }
        // La altura es la del hijo con la "huella" más alta.
        float contentHeight = childMeasures.Any() ? childMeasures.Max(m => m.Height) : 0;

        float boxWidth = contentWidth + (float)hsl.GetPadding.HorizontalThickness;
        float boxHeight = contentHeight + (float)hsl.GetPadding.VerticalThickness;

        context.LayoutState[hsl] = childMeasures;

        var totalWidth = boxWidth + (float)hsl.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)hsl.GetMargin.VerticalThickness;

        return new LayoutInfo(hsl, totalWidth, totalHeight);
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

        var elementBox = new SKRect(
            renderRect.Left + (float)hsl.GetMargin.Left,
            renderRect.Top + (float)hsl.GetMargin.Top,
            renderRect.Right - (float)hsl.GetMargin.Right,
            renderRect.Bottom - (float)hsl.GetMargin.Bottom
        );

        canvas.Save();
        canvas.ClipRect(elementBox);

        if (hsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBox, bgPaint);
        }

        float contentHeight = elementBox.Height - (float)hsl.GetPadding.VerticalThickness;
        float currentX = elementBox.Left + (float)hsl.GetPadding.Left;

        for (int i = 0; i < childMeasures.Count; i++)
        {
            var measure = childMeasures[i];
            var child = (PdfElement)measure.Element;
            var renderer = context.RendererFactory.GetRenderer(child);

            float offsetY = child.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (contentHeight - measure.Height) / 2f,
                LayoutAlignment.End => contentHeight - measure.Height,
                _ => 0f
            };

            float y = elementBox.Top + (float)hsl.GetPadding.Top + offsetY;

            var childRect = SKRect.Create(currentX, y, measure.Width, measure.Height);
            var childContext = context with { Element = child };

            await renderer.RenderAsync(canvas, childRect, childContext);

            currentX += measure.Width;
            if (i < childMeasures.Count - 1)
            {
                currentX += hsl.GetSpacing;
            }
        }

        canvas.Restore();
    }
}
