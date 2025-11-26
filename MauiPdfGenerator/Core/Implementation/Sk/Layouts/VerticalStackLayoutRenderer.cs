using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Core.Implementation.Sk;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class VerticalStackLayoutRenderer : IElementRenderer
{
    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKSize availableSize)
    {
        if (context.Element is not PdfVerticalStackLayoutData vsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfVerticalStackLayoutData)} or is null.");

        float totalHeight = 0;
        float maxWidth = 0;
        var childMeasures = new List<PdfLayoutInfo>();

        var constrainedWidth = vsl.GetWidthRequest.HasValue ? (float)vsl.GetWidthRequest.Value : availableSize.Width;
        var childAvailableWidth = constrainedWidth - (float)vsl.GetPadding.HorizontalThickness - (float)vsl.GetMargin.HorizontalThickness;

        foreach (var child in vsl.GetChildren)
        {
            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };
            var measure = await renderer.MeasureAsync(childContext, new SKSize(childAvailableWidth, float.PositiveInfinity));
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

        if (vsl.GetVerticalOptions is LayoutAlignment.Fill)
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
        if (!context.LayoutState.TryGetValue(vsl, out var state) || state is not List<PdfLayoutInfo> childMeasures)
        {
            throw new InvalidOperationException("ArrangeAsync called without a prior successful MeasureAsync for VerticalStackLayout.");
        }

        var totalMeasuredHeight = childMeasures.Sum(m => m.Height) + (childMeasures.Count > 1 ? vsl.GetSpacing * (childMeasures.Count - 1) : 0);

        if (totalMeasuredHeight > finalRect.Height)
        {
            return new PdfLayoutInfo(vsl, finalRect.Width, 0, PdfRect.Empty, vsl);
        }

        return await ArrangeInternal(finalRect, context, vsl, vsl.GetChildren, childMeasures);
    }

    private async Task<PdfLayoutInfo> ArrangeDivisibleAsync(PdfRect finalRect, PdfGenerationContext context, PdfVerticalStackLayoutData vsl)
    {
        if (!context.LayoutState.TryGetValue(vsl, out var state) || state is not List<PdfLayoutInfo> childMeasures)
        {
            throw new InvalidOperationException("ArrangeAsync called without a prior successful MeasureAsync for VerticalStackLayout.");
        }

        var childrenToArrange = new List<PdfElementData>();
        var remainingChildren = new List<PdfElementData>();
        float availableHeight = finalRect.Height - (float)vsl.GetPadding.VerticalThickness;
        float consumedHeight = 0;
        bool pageIsFull = false;

        for (int i = 0; i < vsl.GetChildren.Count; i++)
        {
            if (pageIsFull)
            {
                remainingChildren.Add(vsl.GetChildren[i]);
                continue;
            }

            var child = vsl.GetChildren[i];
            var measure = childMeasures[i];

            float requiredSpacing = i > 0 ? vsl.GetSpacing : 0;
            float remainingHeightForChild = availableHeight - consumedHeight - requiredSpacing;

            bool isAtomic = IsAtomicElement(child);

            if (isAtomic)
            {
                // Lógica Atómica: Todo o Nada
                if (measure.Height > remainingHeightForChild)
                {
                    remainingChildren.Add(child);
                    pageIsFull = true;
                }
                else
                {
                    childrenToArrange.Add(child);
                    consumedHeight += measure.Height + requiredSpacing;
                }
            }
            else
            {
                // Lógica Divisible: Intentar partir
                if (measure.Height <= remainingHeightForChild)
                {
                    childrenToArrange.Add(child);
                    consumedHeight += measure.Height + requiredSpacing;
                }
                else
                {
                    var renderer = context.RendererFactory.GetRenderer(child);
                    var childContext = context with { Element = child };

                    // Pasamos el espacio restante exacto para forzar el corte
                    var partialRect = new PdfRect(finalRect.X, finalRect.Y + consumedHeight + requiredSpacing, finalRect.Width, remainingHeightForChild);
                    var partialArrange = await renderer.ArrangeAsync(partialRect, childContext);

                    if (partialArrange.Height > 0)
                    {
                        childrenToArrange.Add(child);
                        consumedHeight += partialArrange.Height + requiredSpacing;
                    }

                    if (partialArrange.RemainingElement is not null)
                    {
                        remainingChildren.Add(partialArrange.RemainingElement);
                        pageIsFull = true;
                    }
                    else if (partialArrange.Height <= 0)
                    {
                        remainingChildren.Add(child);
                        pageIsFull = true;
                    }
                }
            }
        }

        var arrangedResult = await ArrangeInternal(finalRect, context, vsl, childrenToArrange, childMeasures);

        PdfVerticalStackLayoutData? continuationLayout = null;
        if (remainingChildren.Count != 0)
        {
            continuationLayout = new PdfVerticalStackLayoutData(remainingChildren, vsl);
        }

        return arrangedResult with { RemainingElement = continuationLayout };
    }

    private async Task<PdfLayoutInfo> ArrangeInternal(PdfRect finalRect, PdfGenerationContext context, PdfVerticalStackLayoutData vsl, IReadOnlyList<PdfElementData> childrenToArrange, IReadOnlyList<PdfLayoutInfo> allMeasures)
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
            var measure = allMeasures.FirstOrDefault(m => m.Element == child);

            var renderer = context.RendererFactory.GetRenderer(child);
            var childContext = context with { Element = child };

            float childTotalWidth = measure.Width;

            float finalChildWidth;
            if (child.GetWidthRequest.HasValue)
            {
                finalChildWidth = (float)child.GetWidthRequest.Value;
            }
            else if (child.GetHorizontalOptions is LayoutAlignment.Fill)
            {
                finalChildWidth = contentWidth;
            }
            else
            {
                finalChildWidth = Math.Min(childTotalWidth, contentWidth);
            }

            float offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentWidth - finalChildWidth) / 2f,
                LayoutAlignment.End => contentWidth - finalChildWidth,
                _ => 0f
            };

            float x = elementBox.Left + (float)vsl.GetPadding.Left + offsetX;

            // --- CORRECCIÓN DEFINITIVA DE LA REGRESIÓN ---
            float heightConstraint;

            if (IsAtomicElement(child))
            {
                // CASO ATÓMICO (Imágenes, Texto Fijo, etc.):
                // Le damos EXACTAMENTE lo que midió. Esto evita que se expanda
                // para llenar el espacio restante de la página, lo que causaba los huecos.
                heightConstraint = measure.Height;
            }
            else
            {
                // CASO FLUJO (Texto Largo):
                // Le damos TODO el espacio restante para que pueda calcular dónde cortarse.
                float remainingSpaceInContainer = (elementBox.Bottom - (float)vsl.GetPadding.Bottom) - currentY;
                heightConstraint = remainingSpaceInContainer;
            }

            var childRect = new PdfRect(x, currentY, finalChildWidth, heightConstraint);

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

    private bool IsAtomicElement(PdfElementData element)
    {
        if (element is PdfParagraphData p)
        {
            // Párrafo es atómico SI tiene altura fija.
            return p.GetHeightRequest.HasValue;
        }
        if (element is PdfVerticalStackLayoutData vsl)
        {
            // VSL es atómico SI NO es Fill.
            return vsl.GetVerticalOptions is not LayoutAlignment.Fill;
        }
        // Imágenes, Líneas, Grids, HSL son atómicos por defecto.
        return true;
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

        canvas.Save();
        canvas.ClipRect(elementBox);

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

        canvas.Restore();
    }

    public Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context)
    {
        return Task.CompletedTask;
    }
}
