using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class VerticalStackLayoutRenderer : IElementRenderer
{
    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfVerticalStackLayoutData vsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfVerticalStackLayoutData)} or is null.");

        float totalHeight = 0;
        float maxWidth = 0;
        var childMeasures = new List<PdfLayoutInfo>();

        var constrainedWidth = (float?)vsl.GetWidthRequest ?? availableRect.Width;
        var childAvailableWidth = constrainedWidth - (float)vsl.GetPadding.HorizontalThickness - (float)vsl.GetMargin.HorizontalThickness;

        foreach (var child in vsl.GetChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };

            var measure = await renderer.MeasureAsync(childContext, SKRect.Create(0, 0, childAvailableWidth, float.PositiveInfinity));
            childMeasures.Add(measure);
            totalHeight += measure.Height;
            maxWidth = Math.Max(maxWidth, measure.Width);
        }

        if (vsl.GetChildren.Count > 1)
        {
            totalHeight += vsl.GetSpacing * (vsl.GetChildren.Count - 1);
        }

        float boxWidth = vsl.GetWidthRequest.HasValue
            ? (float)vsl.GetWidthRequest.Value
            : maxWidth + (float)vsl.GetPadding.HorizontalThickness;

        float boxHeight = totalHeight + (float)vsl.GetPadding.VerticalThickness;

        context.LayoutState[vsl] = childMeasures;

        var totalWidth = boxWidth + (float)vsl.GetMargin.HorizontalThickness;
        var totalHeightWithMargin = boxHeight + (float)vsl.GetMargin.VerticalThickness;

        return new PdfLayoutInfo(vsl, totalWidth, totalHeightWithMargin);
    }

    public async Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfVerticalStackLayoutData vsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfVerticalStackLayoutData)} or is null.");

        if (!context.LayoutState.TryGetValue(vsl, out var state) || state is not List<PdfLayoutInfo> childMeasures)
        {
            context.Logger.LogError("VerticalStackLayout measure state was not found before arranging.");
            return new PdfLayoutInfo(vsl, finalRect.Width, finalRect.Height, finalRect);
        }

        var elementBox = new PdfRect(
            finalRect.Left + (float)vsl.GetMargin.Left,
            finalRect.Top + (float)vsl.GetMargin.Top,
            finalRect.Width - (float)vsl.GetMargin.HorizontalThickness,
            finalRect.Height - (float)vsl.GetMargin.VerticalThickness
        );

        float contentWidth = elementBox.Width - (float)vsl.GetPadding.HorizontalThickness;
        float currentY = elementBox.Top + (float)vsl.GetPadding.Top;

        var arrangedChildren = new List<PdfLayoutInfo>();
        for (int i = 0; i < vsl.GetChildren.Count; i++)
        {
            var child = (PdfElementData)vsl.GetChildren[i];
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
            var childRect = new PdfRect(x, currentY, childWidth, measure.Height);
            var childContext = context with { Element = child };

            var arrangedChild = await renderer.ArrangeAsync(childRect, childContext);
            arrangedChildren.Add(arrangedChild);

            currentY += measure.Height;
            if (i < vsl.GetChildren.Count - 1)
            {
                currentY += vsl.GetSpacing;
            }
        }

        context.LayoutState[vsl] = (arrangedChildren, finalRect);
        return new PdfLayoutInfo(vsl, finalRect.Width, finalRect.Height, finalRect);
    }

    public async Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfVerticalStackLayoutData vsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfVerticalStackLayoutData)} or is null.");

        if (!context.LayoutState.TryGetValue(vsl, out var state) || state is not (List<PdfLayoutInfo> arrangedChildren, PdfRect finalRect))
        {
            context.Logger.LogError("VerticalStackLayout arranged state was not found before rendering.");
            return;
        }

        var elementBox = new SKRect(
            finalRect.Left + (float)vsl.GetMargin.Left,
            finalRect.Top + (float)vsl.GetMargin.Top,
            finalRect.Right - (float)vsl.GetMargin.Right,
            finalRect.Bottom - (float)vsl.GetMargin.Bottom
        );

        if (vsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBox, bgPaint);
        }

        foreach (var childInfo in arrangedChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(childInfo.Element);
            var childContext = context with { Element = (PdfElementData)childInfo.Element };
            await renderer.RenderAsync(canvas, childContext);
        }
    }

    public Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context)
    {
        // Un layout desbordado simplemente no se dibuja.
        return Task.CompletedTask;
    }
}
