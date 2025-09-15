using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class HorizontalStackLayoutRenderer : IElementRenderer
{
    private record HorizontalLayoutCache(
        List<PdfLayoutInfo> ArrangedChildren,
        List<(PdfElementData Element, PdfRect Bounds)> OverflowedChildren,
        PdfRect FinalRect
    );

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfHorizontalStackLayoutData hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayoutData)} or is null.");

        var childMeasures = new List<PdfLayoutInfo>();

        var constrainedWidth = (float?)hsl.GetWidthRequest ?? availableRect.Width;
        var childAvailableRect = new SKRect(0, 0, constrainedWidth, availableRect.Height);

        foreach (var child in hsl.GetChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };
            var measure = await renderer.MeasureAsync(childContext, childAvailableRect);
            childMeasures.Add(measure);
        }

        float contentWidth = childMeasures.Sum(m => m.Width);
        if (childMeasures.Count > 1)
        {
            contentWidth += hsl.GetSpacing * (childMeasures.Count - 1);
        }
        float contentHeight = childMeasures.Any() ? childMeasures.Max(m => m.Height) : 0;

        float boxWidth = hsl.GetWidthRequest.HasValue
            ? (float)hsl.GetWidthRequest.Value
            : contentWidth + (float)hsl.GetPadding.HorizontalThickness;

        float boxHeight = contentHeight + (float)hsl.GetPadding.VerticalThickness;

        context.LayoutState[hsl] = childMeasures;

        var totalWidth = boxWidth + (float)hsl.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)hsl.GetMargin.VerticalThickness;

        return new PdfLayoutInfo(hsl, totalWidth, totalHeight);
    }

    public async Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalStackLayoutData hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayoutData)} or is null.");

        if (!context.LayoutState.TryGetValue(hsl, out var state) || state is not List<PdfLayoutInfo> childMeasures)
        {
            context.Logger.LogError("HorizontalStackLayout measure state was not found before arranging.");
            return new PdfLayoutInfo(hsl, finalRect.Width, finalRect.Height, finalRect);
        }

        var elementBox = new PdfRect(
            finalRect.Left + (float)hsl.GetMargin.Left,
            finalRect.Top + (float)hsl.GetMargin.Top,
            finalRect.Width - (float)hsl.GetMargin.HorizontalThickness,
            finalRect.Height - (float)hsl.GetMargin.VerticalThickness
        );

        float contentHeight = elementBox.Height - (float)hsl.GetPadding.VerticalThickness;
        float contentWidth = elementBox.Width - (float)hsl.GetPadding.HorizontalThickness;
        float currentX = elementBox.Left + (float)hsl.GetPadding.Left;

        var arrangedChildren = new List<PdfLayoutInfo>();
        var overflowedChildren = new List<(PdfElementData, PdfRect)>();

        for (int i = 0; i < childMeasures.Count; i++)
        {
            var measure = childMeasures[i];
            var child = (PdfElementData)measure.Element;

            var requiredSpacing = i > 0 ? hsl.GetSpacing : 0;
            var spaceAvailable = (elementBox.Left + contentWidth) - (currentX + requiredSpacing);

            if (measure.Width <= spaceAvailable)
            {
                currentX += requiredSpacing;
                var renderer = context.RendererFactory.GetRenderer(child);

                float childHeight = child.GetVerticalOptions == LayoutAlignment.Fill ? contentHeight : measure.Height;
                float offsetY = child.GetVerticalOptions switch
                {
                    LayoutAlignment.Center => (contentHeight - childHeight) / 2f,
                    LayoutAlignment.End => contentHeight - childHeight,
                    _ => 0f
                };

                float y = elementBox.Top + (float)hsl.GetPadding.Top + offsetY;
                var childRect = new PdfRect(currentX, y, measure.Width, childHeight);
                var childContext = context with { Element = child };

                var arrangedChild = await renderer.ArrangeAsync(childRect, childContext);
                arrangedChildren.Add(arrangedChild);

                currentX += measure.Width;
            }
            else
            {
                // Este hijo y todos los siguientes están desbordados.
                // Calculamos sus posiciones ideales para que el renderizador de error sepa dónde dibujarlos.
                currentX += requiredSpacing;
                for (int j = i; j < childMeasures.Count; j++)
                {
                    var overflowMeasure = childMeasures[j];
                    var overflowChild = (PdfElementData)overflowMeasure.Element;

                    float childHeight = overflowChild.GetVerticalOptions == LayoutAlignment.Fill ? contentHeight : overflowMeasure.Height;
                    float offsetY = overflowChild.GetVerticalOptions switch
                    {
                        LayoutAlignment.Center => (contentHeight - childHeight) / 2f,
                        LayoutAlignment.End => contentHeight - childHeight,
                        _ => 0f
                    };
                    float y = elementBox.Top + (float)hsl.GetPadding.Top + offsetY;

                    var overflowBounds = new PdfRect(currentX, y, overflowMeasure.Width, childHeight);
                    overflowedChildren.Add((overflowChild, overflowBounds));
                    currentX += overflowMeasure.Width + (j < childMeasures.Count - 1 ? hsl.GetSpacing : 0);
                }
                break;
            }
        }

        context.LayoutState[hsl] = new HorizontalLayoutCache(arrangedChildren, overflowedChildren, finalRect);
        return new PdfLayoutInfo(hsl, finalRect.Width, finalRect.Height, finalRect);
    }

    public async Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalStackLayoutData hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayoutData)} or is null.");

        if (!context.LayoutState.TryGetValue(hsl, out var state) || state is not HorizontalLayoutCache cache)
        {
            context.Logger.LogError("HorizontalStackLayout arranged state was not found before rendering.");
            return;
        }

        var elementBox = new SKRect(
            cache.FinalRect.Left + (float)hsl.GetMargin.Left,
            cache.FinalRect.Top + (float)hsl.GetMargin.Top,
            cache.FinalRect.Right - (float)hsl.GetMargin.Right,
            cache.FinalRect.Bottom - (float)hsl.GetMargin.Bottom
        );

        canvas.Save();
        canvas.ClipRect(elementBox);

        if (hsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBox, bgPaint);
        }

        foreach (var childInfo in cache.ArrangedChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(childInfo.Element);
            var childContext = context with { Element = (PdfElementData)childInfo.Element };
            await renderer.RenderAsync(canvas, childContext);
        }

        foreach (var (overflowChild, overflowBounds) in cache.OverflowedChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(overflowChild);
            var childContext = context with { Element = overflowChild };

            // --- LA CORRECCIÓN CRÍTICA ---
            // No pasamos el overflowBounds directamente.
            // El renderizador de error debe dibujar dentro de los límites del layout.
            // No necesitamos recortar aquí porque el ClipRect del canvas ya se encarga.
            await renderer.RenderOverflowAsync(canvas, overflowBounds, childContext);
        }

        canvas.Restore();
    }

    public Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context)
    {
        // Un layout desbordado simplemente no se dibuja.
        return Task.CompletedTask;
    }
}
