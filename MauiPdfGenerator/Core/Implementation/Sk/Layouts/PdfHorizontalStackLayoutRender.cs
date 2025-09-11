using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfHorizontalStackLayoutRender : IElementRenderer
{
    public async Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfHorizontalStackLayoutData hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayoutData)} or is null.");

        var childMeasures = new List<LayoutInfo>();

        // El ancho disponible para los hijos está limitado por el WidthRequest del propio HSL.
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

        // El ancho final del HSL es su WidthRequest o el ancho de su contenido.
        float boxWidth = hsl.GetWidthRequest.HasValue
            ? (float)hsl.GetWidthRequest.Value
            : contentWidth + (float)hsl.GetPadding.HorizontalThickness;

        float boxHeight = contentHeight + (float)hsl.GetPadding.VerticalThickness;

        context.LayoutState[hsl] = childMeasures;

        var totalWidth = boxWidth + (float)hsl.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)hsl.GetMargin.VerticalThickness;

        return new LayoutInfo(hsl, totalWidth, totalHeight);
    }

    // ... El resto del fichero (ArrangeAsync, RenderAsync) no necesita cambios ...
    public async Task<LayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalStackLayoutData hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayoutData)} or is null.");

        if (!context.LayoutState.TryGetValue(hsl, out var state) || state is not List<LayoutInfo> childMeasures)
        {
            context.Logger.LogError("HorizontalStackLayout measure state was not found before arranging.");
            return new LayoutInfo(hsl, finalRect.Width, finalRect.Height, finalRect);
        }

        var elementBox = new PdfRect(
            finalRect.Left + (float)hsl.GetMargin.Left,
            finalRect.Top + (float)hsl.GetMargin.Top,
            finalRect.Width - (float)hsl.GetMargin.HorizontalThickness,
            finalRect.Height - (float)hsl.GetMargin.VerticalThickness
        );

        float contentHeight = elementBox.Height - (float)hsl.GetPadding.VerticalThickness;
        float currentX = elementBox.Left + (float)hsl.GetPadding.Left;

        var arrangedChildren = new List<LayoutInfo>();
        for (int i = 0; i < childMeasures.Count; i++)
        {
            var measure = childMeasures[i];
            var child = (PdfElementData)measure.Element;
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
            if (i < childMeasures.Count - 1)
            {
                currentX += hsl.GetSpacing;
            }
        }

        context.LayoutState[hsl] = (arrangedChildren, finalRect);
        return new LayoutInfo(hsl, finalRect.Width, finalRect.Height, finalRect);
    }

    public async Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalStackLayoutData hsl)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalStackLayoutData)} or is null.");

        if (!context.LayoutState.TryGetValue(hsl, out var state) || state is not (List<LayoutInfo> arrangedChildren, PdfRect finalRect))
        {
            context.Logger.LogError("HorizontalStackLayout arranged state was not found before rendering.");
            return;
        }

        var elementBox = new SKRect(
            finalRect.Left + (float)hsl.GetMargin.Left,
            finalRect.Top + (float)hsl.GetMargin.Top,
            finalRect.Right - (float)hsl.GetMargin.Right,
            finalRect.Bottom - (float)hsl.GetMargin.Bottom
        );

        canvas.Save();
        canvas.ClipRect(elementBox);

        if (hsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
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
}
