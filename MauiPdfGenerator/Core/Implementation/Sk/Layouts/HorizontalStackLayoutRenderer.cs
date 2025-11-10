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
        List<(PdfElementData Element, PdfRect Bounds)> OverflowedChildren,
        PdfRect FinalRect
    );

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKSize availableSize)
    {
        if (context.Element is not PdfHorizontalStackLayoutData hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayoutData)} or is null.");

        var childMeasures = new List<PdfLayoutInfo>();
        var childAvailableSize = new SKSize(float.PositiveInfinity, availableSize.Height - (float)hsl.GetMargin.VerticalThickness - (float)hsl.GetPadding.VerticalThickness);

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

        var measure = await MeasureAsync(context, new SKSize(finalRect.Width, finalRect.Height));
        if (measure.Height > finalRect.Height)
        {
            return new PdfLayoutInfo(hsl, finalRect.Width, 0, PdfRect.Empty, hsl);
        }

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
        float currentX = elementBox.Left + (float)hsl.GetPadding.Left;

        var arrangedChildren = new List<PdfLayoutInfo>();
        var overflowedChildren = new List<(PdfElementData, PdfRect)>();

        for (int i = 0; i < childMeasures.Count; i++)
        {
            var childMeasure = childMeasures[i];
            var child = (PdfElementData)childMeasure.Element;
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };

            var requiredSpacing = i > 0 ? hsl.GetSpacing : 0;

            float childTotalHeight = childMeasure.Height;
            float childTotalWidth = childMeasure.Width;

            float availableHeightForChild = contentHeight;
            float finalChildHeight = child.GetVerticalOptions is LayoutAlignment.Fill ? availableHeightForChild : childTotalHeight;

            float offsetY = child.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (availableHeightForChild - finalChildHeight) / 2f,
                LayoutAlignment.End => availableHeightForChild - finalChildHeight,
                _ => 0f
            };
            float y = elementBox.Top + (float)hsl.GetPadding.Top + offsetY;

            var spaceAvailableForChild = (elementBox.Right - (float)hsl.GetPadding.Right) - (currentX + requiredSpacing);

            if (childTotalWidth <= spaceAvailableForChild)
            {
                currentX += requiredSpacing;
                var childRect = new PdfRect(currentX, y, childTotalWidth, finalChildHeight);
                var arrangedChild = await renderer.ArrangeAsync(childRect, childContext);
                arrangedChildren.Add(arrangedChild);
                currentX += childTotalWidth;
            }
            else
            {
                currentX += requiredSpacing;
                var overflowBounds = new PdfRect(currentX, y, childTotalWidth, finalChildHeight);
                overflowedChildren.Add((child, overflowBounds));

                context.DiagnosticSink.Submit(new DiagnosticMessage(
                    DiagnosticSeverity.Warning,
                    DiagnosticCodes.LayoutOverflow,
                    $"Element '{child.GetType().Name}' with desired width {childMeasure.Width} overflows the available space of {spaceAvailableForChild} in the HorizontalStackLayout.",
                    new DiagnosticRect(overflowBounds.X, overflowBounds.Y, overflowBounds.Width, overflowBounds.Height)
                ));

                var clippedRect = new PdfRect(currentX, y, spaceAvailableForChild, finalChildHeight);
                if (clippedRect.Width > 0)
                {
                    var arrangedChild = await renderer.ArrangeAsync(clippedRect, childContext);
                    arrangedChildren.Add(arrangedChild);
                }
                currentX += childTotalWidth;
            }
        }

        var finalArrangedRect = new PdfRect(finalRect.X, finalRect.Y, finalRect.Width, measure.Height);
        context.LayoutState[hsl] = new HorizontalLayoutCache(arrangedChildren, overflowedChildren, finalArrangedRect);

        return new PdfLayoutInfo(hsl, finalRect.Width, measure.Height, finalArrangedRect);
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

        bool hasVisualDiagnostics = context.DiagnosticSink.GetType().Name.Contains("Visual");

        foreach (var childInfo in cache.ArrangedChildren)
        {
            bool isOverflowed = cache.OverflowedChildren.Any(oc => oc.Element == childInfo.Element);
            if (isOverflowed && hasVisualDiagnostics)
            {
                continue;
            }

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
