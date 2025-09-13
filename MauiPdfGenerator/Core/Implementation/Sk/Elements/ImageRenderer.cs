using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models.Elements;
using Microsoft.Extensions.Logging;
using SkiaSharp;

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
            context.Logger.LogWarning(ex, "Failed to decode image stream. A placeholder will be used.");
            skImage = null;
        }

        context.LayoutState[image] = new ImageLayoutCache(skImage, SKRect.Empty, PdfRect.Empty);

        float boxWidth;
        if (image.GetWidthRequest.HasValue)
        {
            boxWidth = (float)image.GetWidthRequest.Value;
        }
        else if (image.GetHorizontalOptions is LayoutAlignment.Fill)
        {
            boxWidth = availableRect.Width - (float)image.GetMargin.HorizontalThickness;
        }
        else
        {
            boxWidth = skImage?.Width ?? 100f;
        }

        float boxHeight;
        if (image.GetHeightRequest.HasValue)
        {
            boxHeight = (float)image.GetHeightRequest.Value;
        }
        else
        {
            float imageContentWidth = boxWidth - (float)image.GetPadding.HorizontalThickness;

            float proportionalImageHeight = 0;
            if (skImage != null && skImage.Width > 0 && imageContentWidth > 0)
            {
                float aspectRatio = (float)skImage.Height / skImage.Width;
                proportionalImageHeight = imageContentWidth * aspectRatio;
            }
            else
            {
                proportionalImageHeight = 50f;
            }

            boxHeight = proportionalImageHeight + (float)image.GetPadding.VerticalThickness;
        }

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

        if (image.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(image.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(renderRect, bgPaint);
        }

        if (cache.SkImage is null)
        {
            DrawImageError(canvas, renderRect, "[Image Load Error]");
            return Task.CompletedTask;
        }

        var skImage = cache.SkImage;

        SKRect finalDrawRect = cache.RelativeTargetRect;
        finalDrawRect.Offset(renderRect.Left + (float)image.GetMargin.Left, renderRect.Top + (float)image.GetMargin.Top);

        canvas.Save();
        canvas.ClipRect(renderRect);
        canvas.DrawImage(skImage, finalDrawRect);
        canvas.Restore();

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
        if (image.Width <= 0 || image.Height <= 0 || container.Width <= 0 || container.Height <= 0 || aspect is Aspect.Fill)
        {
            return container;
        }

        float imageRatio = image.Width / (float)image.Height;
        float containerRatio = container.Width / (float)container.Height;

        float finalWidth = container.Width;
        float finalHeight = container.Height;

        if (aspect is Aspect.AspectFit)
        {
            if (imageRatio > containerRatio)
            {
                finalHeight = container.Width / imageRatio;
            }
            else
            {
                finalWidth = container.Height * imageRatio;
            }
        }
        else if (aspect is Aspect.AspectFill)
        {
            if (imageRatio > containerRatio)
            {
                finalWidth = container.Height * imageRatio;
            }
            else
            {
                finalHeight = container.Width / imageRatio;
            }
        }

        float x = container.Left + (container.Width - finalWidth) / 2f;
        float y = container.Top + (container.Height - finalHeight) / 2f;

        return SKRect.Create(x, y, finalWidth, finalHeight);
    }
}
