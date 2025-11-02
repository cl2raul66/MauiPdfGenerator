using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class VerticalStackLayoutRenderer : IElementRenderer
{
    // ... El método MeasureAsync no necesita cambios ...
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

        float boxWidth = vsl.GetWidthRequest.HasValue ? (float)vsl.GetWidthRequest.Value : maxWidth + (float)vsl.GetPadding.HorizontalThickness;
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

        if (vsl.GetVerticalOptions == LayoutAlignment.Fill)
        {
            return await ArrangeDivisibleAsync(finalRect, context, vsl);
        }
        else
        {
            return await ArrangeAtomicAsync(finalRect, context, vsl);
        }
    }

    private async Task<PdfLayoutInfo> ArrangeAtomicAsync(PdfRect finalRect, PdfGenerationContext context, PdfVerticalStackLayoutData vsl)
    {
        var measure = await MeasureAsync(context, new SKRect(0, 0, finalRect.Width, float.PositiveInfinity));

        if (measure.Height > finalRect.Height)
        {
            return new PdfLayoutInfo(vsl, finalRect.Width, 0, PdfRect.Empty, vsl);
        }

        return await ArrangeInternal(finalRect, context, vsl, vsl.GetChildren);
    }

    private async Task<PdfLayoutInfo> ArrangeDivisibleAsync(PdfRect finalRect, PdfGenerationContext context, PdfVerticalStackLayoutData vsl)
    {
        var childrenToArrange = new List<PdfElementData>();
        var remainingChildren = new List<PdfElementData>();
        float availableHeight = finalRect.Height - (float)vsl.GetPadding.VerticalThickness;
        float consumedHeight = 0;
        bool pageIsFull = false;

        var childMeasures = (List<PdfLayoutInfo>?)context.LayoutState.GetValueOrDefault(vsl) ?? [];

        for (int i = 0; i < vsl.GetChildren.Count; i++)
        {
            if (pageIsFull)
            {
                remainingChildren.Add(vsl.GetChildren[i]);
                continue;
            }

            var child = vsl.GetChildren[i];
            var measure = childMeasures.FirstOrDefault(m => m.Element == child);
            if (measure.Element == null)
            {
                var renderer = context.RendererFactory.GetRenderer(child);
                var childContext = context with { Element = child };
                measure = await renderer.MeasureAsync(childContext, new SKRect(0, 0, finalRect.Width, float.PositiveInfinity));
            }

            float requiredSpacing = i > 0 ? vsl.GetSpacing : 0;
            float remainingHeightForChild = availableHeight - consumedHeight - requiredSpacing;

            if (measure.Height <= remainingHeightForChild)
            {
                childrenToArrange.Add(child);
                consumedHeight += measure.Height + requiredSpacing;
            }
            else
            {
                var renderer = context.RendererFactory.GetRenderer(child);
                var childContext = context with { Element = child };
                var partialRect = new PdfRect(finalRect.X, finalRect.Y + consumedHeight + requiredSpacing, finalRect.Width, remainingHeightForChild);
                var partialArrange = await renderer.ArrangeAsync(partialRect, childContext);

                if (partialArrange.Height > 0)
                {
                    childrenToArrange.Add(child);
                }

                if (partialArrange.RemainingElement != null)
                {
                    remainingChildren.Add(partialArrange.RemainingElement);
                }
                else if (partialArrange.Height <= 0)
                {
                    remainingChildren.Add(child);
                }

                pageIsFull = true;
            }
        }

        var arrangedResult = await ArrangeInternal(finalRect, context, vsl, childrenToArrange);

        PdfVerticalStackLayoutData? continuationLayout = null;
        if (remainingChildren.Any())
        {
            continuationLayout = new PdfVerticalStackLayoutData(remainingChildren, vsl);
        }

        return arrangedResult with { RemainingElement = continuationLayout };
    }

    private async Task<PdfLayoutInfo> ArrangeInternal(PdfRect finalRect, PdfGenerationContext context, PdfVerticalStackLayoutData vsl, IReadOnlyList<PdfElementData> childrenToArrange)
    {
        var elementBox = new PdfRect(
            finalRect.Left + (float)vsl.GetMargin.Left,
            finalRect.Top + (float)vsl.GetMargin.Top,
            finalRect.Width - (float)vsl.GetMargin.HorizontalThickness,
            finalRect.Height - (float)vsl.GetMargin.VerticalThickness
        );

        float contentWidth = elementBox.Width - (float)vsl.GetPadding.HorizontalThickness;
        float currentY = elementBox.Top + (float)vsl.GetPadding.Top;
        float totalArrangedHeight = 0;

        var arrangedChildren = new List<PdfLayoutInfo>();
        for (int i = 0; i < childrenToArrange.Count; i++)
        {
            var child = childrenToArrange[i];
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };

            var measure = await renderer.MeasureAsync(childContext, new SKRect(0, 0, contentWidth, float.PositiveInfinity));

            // --- INICIO DE LA CORRECCIÓN DEL BUG ---
            float childWidth;
            // 1. Si el hijo tiene un WidthRequest, ESE es su ancho.
            if (child.GetWidthRequest.HasValue)
            {
                childWidth = (float)child.GetWidthRequest.Value;
            }
            // 2. Si no, se respeta el HorizontalOptions.
            else
            {
                childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? contentWidth : measure.Width;
            }

            // 3. El offsetX se calcula siempre para posicionar la caja (ahora del ancho correcto) dentro del contentWidth.
            float offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentWidth - childWidth) / 2f,
                LayoutAlignment.End => contentWidth - childWidth,
                _ => 0f // Start o Fill
            };
            // --- FIN DE LA CORRECCIÓN DEL BUG ---

            float x = elementBox.Left + (float)vsl.GetPadding.Left + offsetX;

            var childRect = new PdfRect(x, currentY, childWidth, measure.Height);

            var arrangedChild = await renderer.ArrangeAsync(childRect, childContext);
            arrangedChildren.Add(arrangedChild);

            currentY += arrangedChild.Height;
            totalArrangedHeight += arrangedChild.Height;
            if (i < childrenToArrange.Count - 1)
            {
                currentY += vsl.GetSpacing;
                totalArrangedHeight += vsl.GetSpacing;
            }
        }

        context.LayoutState[vsl] = (arrangedChildren, finalRect);

        float finalHeight = totalArrangedHeight + (float)vsl.GetPadding.VerticalThickness;
        var finalArrangedRect = new PdfRect(finalRect.X, finalRect.Y, finalRect.Width, finalHeight);

        return new PdfLayoutInfo(vsl, finalRect.Width, finalHeight + (float)vsl.GetMargin.VerticalThickness, finalArrangedRect);
    }

    public async Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfVerticalStackLayoutData vsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfVerticalStackLayoutData)} or is null.");

        if (!context.LayoutState.TryGetValue(vsl, out var state) || state is not (List<PdfLayoutInfo> arrangedChildren, PdfRect finalRect))
        {
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
        return Task.CompletedTask;
    }
}
