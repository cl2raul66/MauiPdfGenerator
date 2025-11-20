using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class HorizontalStackLayoutRenderer : IElementRenderer
{
    private record HorizontalLayoutCache(
        List<PdfLayoutInfo> ArrangedChildren,
        PdfRect RenderRect,
        PdfRect ClipRect
    );

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKSize availableSize)
    {
        if (context.Element is not PdfHorizontalStackLayoutData hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayoutData)} or is null.");

        var childMeasures = new List<PdfLayoutInfo>();

        float heightConstraintForChildren = hsl.GetHeightRequest.HasValue
            ? (float)hsl.GetHeightRequest.Value - (float)hsl.GetPadding.VerticalThickness
            : availableSize.Height - (float)hsl.GetMargin.VerticalThickness - (float)hsl.GetPadding.VerticalThickness;

        var childAvailableSize = new SKSize(float.PositiveInfinity, heightConstraintForChildren);

        foreach (var child in hsl.GetChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };
            var measure = await renderer.MeasureAsync(childContext, childAvailableSize);
            childMeasures.Add(measure);
        }

        float contentWidth = childMeasures.Sum(m => m.Width);
        if (childMeasures.Count > 1)
        {
            contentWidth += hsl.GetSpacing * (childMeasures.Count - 1);
        }
        float contentHeight = childMeasures.Count != 0 ? childMeasures.Max(m => m.Height) : 0;

        float boxWidth;
        if (hsl.GetWidthRequest.HasValue)
        {
            boxWidth = (float)hsl.GetWidthRequest.Value;
        }
        else if (hsl.GetHorizontalOptions is LayoutAlignment.Fill && !float.IsInfinity(availableSize.Width))
        {
            boxWidth = availableSize.Width - (float)hsl.GetMargin.HorizontalThickness;
        }
        else
        {
            boxWidth = contentWidth + (float)hsl.GetPadding.HorizontalThickness;
        }

        float boxHeight = hsl.GetHeightRequest.HasValue ? (float)hsl.GetHeightRequest.Value : contentHeight + (float)hsl.GetPadding.VerticalThickness;

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
            throw new InvalidOperationException("ArrangeAsync called without a prior successful MeasureAsync for HorizontalStackLayout. This indicates a bug in the layout orchestrator.");
        }

        var arrangedChildren = new List<PdfLayoutInfo>();

        float contentWidth = childMeasures.Sum(m => m.Width) + (childMeasures.Count > 1 ? hsl.GetSpacing * (childMeasures.Count - 1) : 0);
        float contentHeight = childMeasures.Count > 0 ? childMeasures.Max(m => m.Height) : 0;

        var clipRect = new PdfRect(
            finalRect.Left + (float)hsl.GetMargin.Left,
            finalRect.Top + (float)hsl.GetMargin.Top,
            finalRect.Width - (float)hsl.GetMargin.HorizontalThickness,
            finalRect.Height - (float)hsl.GetMargin.VerticalThickness
        );

        PdfRect renderRect;
        if (hsl.GetHorizontalOptions is LayoutAlignment.Fill)
        {
            renderRect = clipRect;
        }
        else
        {
            float selfWidth = contentWidth + (float)hsl.GetPadding.HorizontalThickness;
            float selfHeight = contentHeight + (float)hsl.GetPadding.VerticalThickness;

            float offsetX = hsl.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (clipRect.Width - selfWidth) / 2f,
                LayoutAlignment.End => clipRect.Width - selfWidth,
                _ => 0f
            };

            renderRect = new PdfRect(
                clipRect.X + offsetX,
                clipRect.Y,
                selfWidth,
                selfHeight
            );
        }

        float currentX = renderRect.Left + (float)hsl.GetPadding.Left;
        float availableContentHeight = renderRect.Height - (float)hsl.GetPadding.VerticalThickness;

        for (int i = 0; i < childMeasures.Count; i++)
        {
            var childMeasure = childMeasures[i];
            var child = (PdfElementData)childMeasure.Element;
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };

            var requiredSpacing = i > 0 ? hsl.GetSpacing : 0;

            // --- LÓGICA DE DECISIÓN DE TAMAÑO (BOX MODEL) ---

            // 1. Ancho (Eje Principal): En HSL es acumulativo, usamos lo medido (que ya incluye WidthRequest si existe).
            float finalChildWidth = childMeasure.Width;

            // 2. Alto (Eje Transversal): Aplicamos la jerarquía de restricciones.
            float finalChildHeight;
            if (child.GetHeightRequest.HasValue)
            {
                // Prioridad 1: HeightRequest explícito
                finalChildHeight = (float)child.GetHeightRequest.Value;
            }
            else if (child.GetVerticalOptions is LayoutAlignment.Fill)
            {
                // Prioridad 2: Fill
                finalChildHeight = availableContentHeight;
            }
            else
            {
                // Prioridad 3: Auto (Start/Center/End) -> Clamp al espacio disponible
                finalChildHeight = Math.Min(childMeasure.Height, availableContentHeight);
            }

            float offsetY = child.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (availableContentHeight - finalChildHeight) / 2f,
                LayoutAlignment.End => availableContentHeight - finalChildHeight,
                _ => 0f
            };
            float y = renderRect.Top + (float)hsl.GetPadding.Top + offsetY;

            var spaceAvailableForChild = (renderRect.Right - (float)hsl.GetPadding.Right) - (currentX + requiredSpacing);
            if (finalChildWidth > spaceAvailableForChild)
            {
                context.DiagnosticSink.Submit(new DiagnosticMessage(
                    DiagnosticSeverity.Warning,
                    DiagnosticCodes.LayoutOverflow,
                    $"Element '{child.GetType().Name}' with width {finalChildWidth} overflows the available space of {Math.Max(0, spaceAvailableForChild)} in the HorizontalStackLayout."
                ));
            }

            currentX += requiredSpacing;
            var childRect = new PdfRect(currentX, y, finalChildWidth, finalChildHeight);
            var arrangedChild = await renderer.ArrangeAsync(childRect, childContext);
            arrangedChildren.Add(arrangedChild);
            currentX += finalChildWidth;
        }

        context.LayoutState[hsl] = new HorizontalLayoutCache(arrangedChildren, renderRect, clipRect);

        return new PdfLayoutInfo(hsl, renderRect.Width, renderRect.Height, renderRect);
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

        var clipBox = new SKRect(cache.ClipRect.Left, cache.ClipRect.Top, cache.ClipRect.Right, cache.ClipRect.Bottom);
        var renderBox = new SKRect(cache.RenderRect.Left, cache.RenderRect.Top, cache.RenderRect.Right, cache.RenderRect.Bottom);

        canvas.Save();
        canvas.ClipRect(clipBox);

        if (hsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(renderBox, bgPaint);
        }

        foreach (var childInfo in cache.ArrangedChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(childInfo.Element);
            var childContext = context with { Element = (PdfElementData)childInfo.Element };
            await renderer.RenderAsync(canvas, childContext);
        }

        canvas.Restore();
    }

    public Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context)
    {
        return Task.CompletedTask;
    }
}
