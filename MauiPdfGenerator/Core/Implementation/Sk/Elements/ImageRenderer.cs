using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models.Elements;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class ImageRenderer : IElementRenderer
{
    private record ImageLayoutCache(SKImage? SkImage, SKRect RelativeTargetRect, PdfRect FinalRect);

    public Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfImageData image)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfImageData)} or is null.");

        SKImage? skImage = null;
        try
        {
            if (image.ImageStream.CanRead)
            {
                if (image.ImageStream.CanSeek) image.ImageStream.Position = 0;
                skImage = SKImage.FromEncodedData(image.ImageStream);
            }
        }
        catch (Exception ex)
        {
            context.DiagnosticSink.Submit(new DiagnosticMessage(
                DiagnosticSeverity.Warning,
                DiagnosticCodes.ImageDecodeError,
                $"Failed to decode image stream. A placeholder will be used. Error: {ex.Message}"
            ));
            skImage = null;
        }

        context.LayoutState[image] = new ImageLayoutCache(skImage, SKRect.Empty, PdfRect.Empty);

        float imageWidth = skImage?.Width ?? 100f;
        float imageHeight = skImage?.Height ?? 50f;
        float aspectRatio = imageWidth > 0 ? imageHeight / imageWidth : 1;

        float boxWidth, boxHeight;

        // --- INICIO DE LA LÓGICA DE MEDIDA DEFINITIVA ---
        if (image.GetWidthRequest.HasValue && image.GetHeightRequest.HasValue)
        {
            boxWidth = (float)image.GetWidthRequest.Value;
            boxHeight = (float)image.GetHeightRequest.Value;
        }
        else if (image.GetWidthRequest.HasValue)
        {
            boxWidth = (float)image.GetWidthRequest.Value;
            float imageContentWidth = boxWidth - (float)image.GetPadding.HorizontalThickness;
            boxHeight = (imageContentWidth * aspectRatio) + (float)image.GetPadding.VerticalThickness;
        }
        else if (image.GetHeightRequest.HasValue)
        {
            boxHeight = (float)image.GetHeightRequest.Value;
            float imageContentHeight = boxHeight - (float)image.GetPadding.VerticalThickness;
            boxWidth = (aspectRatio > 0) ? (imageContentHeight / aspectRatio) + (float)image.GetPadding.HorizontalThickness : imageWidth + (float)image.GetPadding.HorizontalThickness;
        }
        else if (!float.IsInfinity(availableRect.Width) && availableRect.Width > 0)
        {
            // Caso del Grid: No hay Request, pero el padre (Grid) impone un ancho.
            // Respetamos esa restricción.
            boxWidth = availableRect.Width;
            float imageContentWidth = boxWidth - (float)image.GetPadding.HorizontalThickness;
            boxHeight = (imageContentWidth * aspectRatio) + (float)image.GetPadding.VerticalThickness;
        }
        else
        {
            // Fallback final: No hay Request y el padre no impone ancho (ej. HSL).
            // Usamos el tamaño intrínseco de la imagen.
            boxWidth = imageWidth + (float)image.GetPadding.HorizontalThickness;
            boxHeight = imageHeight + (float)image.GetPadding.VerticalThickness;
        }
        // --- FIN DE LA LÓGICA DE MEDIDA DEFINITIVA ---

        var totalWidth = boxWidth + (float)image.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)image.GetMargin.VerticalThickness;

        return Task.FromResult(new PdfLayoutInfo(image, totalWidth, totalHeight));
    }

    public Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfImageData image)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfImageData)} or is null.");

        if (!context.LayoutState.TryGetValue(image, out var state) || state is not ImageLayoutCache initialCache)
        {
            context.Logger.LogError("Image cache was not created during Measure pass.");
            return Task.FromResult(new PdfLayoutInfo(image, finalRect.Width, finalRect.Height, finalRect));
        }

        var elementBoxWidth = finalRect.Width - (float)image.GetMargin.HorizontalThickness;
        var elementBoxHeight = finalRect.Height - (float)image.GetMargin.VerticalThickness;

        var contentRect = SKRect.Create(
            (float)image.GetPadding.Left,
            (float)image.GetPadding.Top,
            elementBoxWidth - (float)image.GetPadding.HorizontalThickness,
            elementBoxHeight - (float)image.GetPadding.VerticalThickness
        );

        SKRect relativeTargetRect = initialCache.SkImage != null
            ? CalculateTargetRect(initialCache.SkImage, contentRect, image.CurrentAspect)
            : SKRect.Empty;

        context.LayoutState[image] = initialCache with { RelativeTargetRect = relativeTargetRect, FinalRect = finalRect };

        return Task.FromResult(new PdfLayoutInfo(image, finalRect.Width, finalRect.Height, finalRect));
    }

    public Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfImageData image)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfImageData)} or is null.");

        if (!context.LayoutState.TryGetValue(image, out var state) || state is not ImageLayoutCache cache)
        {
            DrawImageError(canvas, new SKRect(0, 0, 100, 50), "[Image Layout Error]");
            context.Logger.LogError("Image layout cache not found for element. ArrangeAsync was likely not called or failed.");
            return Task.CompletedTask;
        }

        var renderRect = new SKRect(cache.FinalRect.Left, cache.FinalRect.Top, cache.FinalRect.Right, cache.FinalRect.Bottom);
        var elementBox = new SKRect(
            renderRect.Left + (float)image.GetMargin.Left,
            renderRect.Top + (float)image.GetMargin.Top,
            renderRect.Right - (float)image.GetMargin.Right,
            renderRect.Bottom - (float)image.GetMargin.Bottom
        );

        if (image.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(image.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBox, bgPaint);
        }

        if (cache.SkImage is null)
        {
            DrawImageError(canvas, elementBox, "[Image Load Error]");
            cache.SkImage?.Dispose();
            return Task.CompletedTask;
        }

        var skImage = cache.SkImage;

        SKRect finalDrawRect = cache.RelativeTargetRect;
        finalDrawRect.Offset(elementBox.Left, elementBox.Top);

        canvas.Save();
        canvas.ClipRect(elementBox);
        canvas.DrawImage(skImage, finalDrawRect);
        canvas.Restore();

        skImage.Dispose();

        return Task.CompletedTask;
    }

    public Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context)
    {
        var skBounds = new SKRect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        DrawImageError(canvas, skBounds, "[Imagen fuera de rango]");
        return Task.CompletedTask;
    }

    private void DrawImageError(SKCanvas canvas, SKRect bounds, string message)
    {
        if (bounds.Width <= 5 || bounds.Height <= 5) return;

        using var errorBorderPaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
        using var placeholderTypeface = SKTypeface.Default;
        using var skFont = new SKFont(placeholderTypeface, 10f);
        using var placeholderTextPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };

        canvas.DrawRect(bounds, errorBorderPaint);

        SKFontMetrics fontMetrics = skFont.Metrics;
        float textVisualWidth = skFont.MeasureText(message);
        float textX = bounds.Left + (bounds.Width - textVisualWidth) / 2f;
        textX = Math.Max(bounds.Left + 2, Math.Min(textX, bounds.Right - textVisualWidth - 2));
        float textY = bounds.MidY - (fontMetrics.Ascent + fontMetrics.Descent) / 2f;

        canvas.Save();
        canvas.ClipRect(bounds);
        canvas.DrawText(message, textX, textY, skFont, placeholderTextPaint);
        canvas.Restore();
    }

    private static SKRect CalculateTargetRect(SKImage image, SKRect container, Aspect aspect)
    {
        float imageWidth = image.Width;
        float imageHeight = image.Height;
        float containerWidth = container.Width;
        float containerHeight = container.Height;

        if (imageWidth <= 0 || imageHeight <= 0 || containerWidth <= 0 || containerHeight <= 0)
        {
            return SKRect.Empty;
        }

        float finalWidth = 0;
        float finalHeight = 0;

        switch (aspect)
        {
            case Aspect.Fill:
                return container;

            case Aspect.Center:
                finalWidth = imageWidth;
                finalHeight = imageHeight;
                break;

            case Aspect.AspectFit:
                float imageRatio = imageWidth / imageHeight;
                float containerRatio = containerWidth / containerHeight;
                if (imageRatio > containerRatio)
                {
                    finalWidth = containerWidth;
                    finalHeight = containerWidth / imageRatio;
                }
                else
                {
                    finalHeight = containerHeight;
                    finalWidth = containerHeight * imageRatio;
                }
                break;

            case Aspect.AspectFill:
                imageRatio = imageWidth / imageHeight;
                containerRatio = containerWidth / containerHeight;
                if (imageRatio > containerRatio)
                {
                    finalHeight = containerHeight;
                    finalWidth = containerHeight * imageRatio;
                }
                else
                {
                    finalWidth = containerWidth;
                    finalHeight = containerWidth / imageRatio;
                }
                break;
        }

        float x = container.Left + (containerWidth - finalWidth) / 2f;
        float y = container.Top + (containerHeight - finalHeight) / 2f;

        return SKRect.Create(x, y, finalWidth, finalHeight);
    }
}
