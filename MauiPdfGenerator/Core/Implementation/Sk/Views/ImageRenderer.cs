using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models.Views;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;
using System.Diagnostics;
using MauiPdfGenerator.Core.Implementation.Sk.Utils;

namespace MauiPdfGenerator.Core.Implementation.Sk.Views;

internal class ImageRenderer : IElementRenderer
{
    private record ImageMeasureCache(
        SKImage? SkImage,
        float IntrinsicWidth,
        float IntrinsicHeight,
        float AspectRatio);

    private record ImageArrangeCache(
        ImageMeasureCache MeasureCache,
        SKRect RelativeContentRect,
        PdfRect ElementBounds);

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKSize availableSize)
    {
        if (context.Element is not PdfImageData image)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfImageData)} or is null.");

        var (skImage, imageWidth, imageHeight, aspectRatio) = await LoadImageMetadataAsync(image, context);
        context.LayoutState[image] = new ImageMeasureCache(skImage, imageWidth, imageHeight, aspectRatio);

        float boxWidth, boxHeight;

        bool hasWidthRequest = image.GetWidthRequest.HasValue;
        bool hasHeightRequest = image.GetHeightRequest.HasValue;
        bool isWidthConstrained = !float.IsInfinity(availableSize.Width);
        bool isHeightConstrained = !float.IsInfinity(availableSize.Height);

        if (hasWidthRequest && hasHeightRequest)
        {
            boxWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : 0;
            boxHeight = image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : 0;
        }
        else if (hasWidthRequest)
        {
            boxWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : 0;
            float contentWidth = boxWidth - (float)image.GetPadding.HorizontalThickness;
            boxHeight = (aspectRatio > 0) ? (contentWidth * aspectRatio) + (float)image.GetPadding.VerticalThickness : (float)image.GetPadding.VerticalThickness;
        }
        else if (hasHeightRequest)
        {
            boxHeight = image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : 0;
            float contentHeight = boxHeight - (float)image.GetPadding.VerticalThickness;
            boxWidth = (aspectRatio > 0) ? (contentHeight / aspectRatio) + (float)image.GetPadding.HorizontalThickness : (float)image.GetPadding.HorizontalThickness;
        }
        else if (isWidthConstrained && isHeightConstrained)
        {
            float availableContentWidth = availableSize.Width - (float)image.GetMargin.HorizontalThickness - (float)image.GetPadding.HorizontalThickness;
            float availableContentHeight = availableSize.Height - (float)image.GetMargin.VerticalThickness - (float)image.GetPadding.VerticalThickness;

            float containerRatio = availableContentHeight / availableContentWidth;

            if (aspectRatio > containerRatio)
            {
                boxHeight = availableContentHeight + (float)image.GetPadding.VerticalThickness;
                boxWidth = (availableContentHeight / aspectRatio) + (float)image.GetPadding.HorizontalThickness;
            }
            else 
            {
                boxWidth = availableContentWidth + (float)image.GetPadding.HorizontalThickness;
                boxHeight = (availableContentWidth * aspectRatio) + (float)image.GetPadding.VerticalThickness;
            }
        }
        else if (isWidthConstrained)
        {
            boxWidth = availableSize.Width - (float)image.GetMargin.HorizontalThickness;
            float contentWidth = boxWidth - (float)image.GetPadding.HorizontalThickness;
            boxHeight = (contentWidth * aspectRatio) + (float)image.GetPadding.VerticalThickness;
        }
        else if (isHeightConstrained)
        {
            boxHeight = availableSize.Height - (float)image.GetMargin.VerticalThickness;
            float contentHeight = boxHeight - (float)image.GetPadding.VerticalThickness;
            boxWidth = (aspectRatio > 0) ? (contentHeight / aspectRatio) + (float)image.GetPadding.HorizontalThickness : (float)image.GetPadding.HorizontalThickness;
        }
        else
        {
            boxWidth = imageWidth + (float)image.GetPadding.HorizontalThickness;
            boxHeight = imageHeight + (float)image.GetPadding.VerticalThickness;
        }

        var totalWidth = boxWidth + (float)image.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)image.GetMargin.VerticalThickness;

        return new PdfLayoutInfo(image, totalWidth, totalHeight);
    }

    public Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfImageData image)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfImageData)} or is null.");

        if (!context.LayoutState.TryGetValue(image, out var state) || state is not ImageMeasureCache measureCache)
        {
            throw new InvalidOperationException("ArrangeAsync called without a prior successful MeasureAsync. This indicates a bug in the layout orchestrator.");
        }

        var elementBounds = new PdfRect(
            finalRect.X + (float)image.GetMargin.Left,
            finalRect.Y + (float)image.GetMargin.Top,
            finalRect.Width - (float)image.GetMargin.HorizontalThickness,
            finalRect.Height - (float)image.GetMargin.VerticalThickness
        );

        var contentRect = SKRect.Create(
            (float)image.GetPadding.Left,
            (float)image.GetPadding.Top,
            elementBounds.Width - (float)image.GetPadding.HorizontalThickness,
            elementBounds.Height - (float)image.GetPadding.VerticalThickness
        );

        var targetRect = measureCache.SkImage is not null
            ? CalculateTargetRect(measureCache.SkImage, contentRect, image.CurrentAspect)
            : SKRect.Empty;

        context.LayoutState[image] = new ImageArrangeCache(measureCache, targetRect, elementBounds);

        return Task.FromResult(new PdfLayoutInfo(image, finalRect.Width, finalRect.Height, finalRect));
    }

    public Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfImageData image)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfImageData)} or is null.");

        if (!context.LayoutState.TryGetValue(image, out var state) || state is not ImageArrangeCache arrangeCache)
        {
            context.Logger.LogError("RenderAsync called without a prior successful ArrangeAsync. Element will not be rendered.");
            return Task.CompletedTask;
        }

        var elementBounds = new SKRect(arrangeCache.ElementBounds.Left, arrangeCache.ElementBounds.Top, arrangeCache.ElementBounds.Right, arrangeCache.ElementBounds.Bottom);

        if (image.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(image.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBounds, bgPaint);
        }

        var skImage = arrangeCache.MeasureCache.SkImage;
        if (skImage is null)
        {
            DrawImageError(canvas, elementBounds, "[Image Load Error]");
            return Task.CompletedTask;
        }

        SKRect finalDrawRect = arrangeCache.RelativeContentRect;
        finalDrawRect.Offset(elementBounds.Left, elementBounds.Top);

        canvas.Save();
        canvas.ClipRect(elementBounds);
        canvas.DrawImage(skImage, finalDrawRect);
        canvas.Restore();

        skImage.Dispose();

        return Task.CompletedTask;
    }

    private async Task<(SKImage? SkImage, float IntrinsicWidth, float IntrinsicHeight, float AspectRatio)> LoadImageMetadataAsync(PdfImageData image, PdfGenerationContext context)
    {
        try
        {
            if (image.ImageStream.CanRead)
            {
                if (image.ImageStream.CanSeek) image.ImageStream.Position = 0;
                using var memoryStream = new MemoryStream();
                await image.ImageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var skImage = SKImage.FromEncodedData(memoryStream);
                if (skImage is not null)
                {
                    float width = skImage.Width;
                    float height = skImage.Height;
                    float ratio = width > 0 ? height / width : 1;
                    return (skImage, width, height, ratio);
                }
            }
        }
        catch (Exception ex)
        {
            context.DiagnosticSink.Submit(new DiagnosticMessage(
                DiagnosticSeverity.Warning,
                DiagnosticCodes.ImageDecodeError,
                $"Failed to decode image stream. A placeholder will be used. Error: {ex.Message}"
            ));
        }
        return (null, 100f, 50f, 0.5f);
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
