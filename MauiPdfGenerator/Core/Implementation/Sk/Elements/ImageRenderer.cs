using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class ImageRenderer : IElementRenderer
{
    private record ImageLayoutCache(SKImage? SkImage);

    public Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfImage image)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfImage)} or is null.");

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

        context.LayoutState[image] = new ImageLayoutCache(skImage);

        float availableContentWidth = availableRect.Width - (float)image.GetMargin.HorizontalThickness;
        float availableContentHeight = availableRect.Height - (float)image.GetMargin.VerticalThickness;

        float boxWidth, boxHeight;

        if (skImage is null)
        {
            boxHeight = 50f + (float)image.GetPadding.VerticalThickness;
            boxWidth = (image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(availableContentWidth, 100f)) + (float)image.GetPadding.HorizontalThickness;
        }
        else
        {
            SKRect targetRect = CalculateTargetRectInternal(skImage, image.CurrentAspect,
                                                        0, 0,
                                                        availableContentWidth - (float)image.GetPadding.HorizontalThickness,
                                                        availableContentHeight - (float)image.GetPadding.VerticalThickness,
                                                        image.GetWidthRequest, image.GetHeightRequest);
            boxHeight = targetRect.Height + (float)image.GetPadding.VerticalThickness;
            boxWidth = targetRect.Width + (float)image.GetPadding.HorizontalThickness;
        }

        float totalWidthWithMargin = boxWidth + (float)image.GetMargin.HorizontalThickness;
        float totalHeightWithMargin = boxHeight + (float)image.GetMargin.VerticalThickness;

        return Task.FromResult(new LayoutInfo(image, totalWidthWithMargin, totalHeightWithMargin));
    }

    public Task RenderAsync(SKCanvas canvas, SKRect renderRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfImage image)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfImage)} or is null.");

        var elementRect = new SKRect(
            renderRect.Left + (float)image.GetMargin.Left,
            renderRect.Top + (float)image.GetMargin.Top,
            renderRect.Right - (float)image.GetMargin.Right,
            renderRect.Bottom - (float)image.GetMargin.Bottom
        );

        if (!context.LayoutState.TryGetValue(image, out var state) || state is not ImageLayoutCache cache)
        {
            DrawImageError(canvas, elementRect, "[Image Layout Error]");
            context.Logger.LogError("Image layout cache not found for element. MeasureAsync was likely not called or failed.");
            return Task.CompletedTask;
        }

        if (image.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(image.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementRect, bgPaint);
        }

        if (cache.SkImage is null)
        {
            DrawImageError(canvas, elementRect, "[Image Load Error]");
            return Task.CompletedTask;
        }

        using var skImage = cache.SkImage;

        var contentRect = new SKRect(
            elementRect.Left + (float)image.GetPadding.Left,
            elementRect.Top + (float)image.GetPadding.Top,
            elementRect.Right - (float)image.GetPadding.Right,
            elementRect.Bottom - (float)image.GetPadding.Bottom
        );

        SKRect targetRect = CalculateTargetRectInternal(skImage, image.CurrentAspect, contentRect.Left, contentRect.Top, contentRect.Width, contentRect.Height, image.GetWidthRequest, image.GetHeightRequest);

        canvas.DrawImage(skImage, targetRect);
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

    private static SKRect CalculateTargetRectInternal(SKImage image, Aspect aspect,
                                     float drawAtX, float drawAtY,
                                     float availableContentWidth, float availableContentHeight,
                                     double? requestedWidth, double? requestedHeight)
    {
        float imgWidth = image.Width;
        float imgHeight = image.Height;

        if (imgWidth <= 0 || imgHeight <= 0 || availableContentWidth <= 0 || availableContentHeight <= 0)
        {
            return SKRect.Empty;
        }

        float resultWidth, resultHeight;
        float imageAspectRatio = imgWidth / imgHeight;

        float containerWidth = requestedWidth.HasValue ? (float)requestedWidth.Value : availableContentWidth;
        containerWidth = Math.Min(containerWidth, availableContentWidth);

        float containerHeight = requestedHeight.HasValue ? (float)requestedHeight.Value : availableContentHeight;
        containerHeight = Math.Min(containerHeight, availableContentHeight);

        if (containerWidth <= 0 || containerHeight <= 0) return SKRect.Empty;

        switch (aspect)
        {
            case Aspect.Fill:
                resultWidth = containerWidth;
                resultHeight = containerHeight;
                break;
            case Aspect.AspectFill:
                float containerAspectFillRatio = containerWidth / containerHeight;
                if (imageAspectRatio > containerAspectFillRatio)
                {
                    resultHeight = containerHeight;
                    resultWidth = resultHeight * imageAspectRatio;
                }
                else
                {
                    resultWidth = containerWidth;
                    resultHeight = resultWidth / imageAspectRatio;
                }
                break;
            case Aspect.AspectFit:
            default:
                float containerAspectFitRatio = containerWidth / containerHeight;
                if (imageAspectRatio > containerAspectFitRatio)
                {
                    resultWidth = containerWidth;
                    resultHeight = resultWidth / imageAspectRatio;
                }
                else
                {
                    resultHeight = containerHeight;
                    resultWidth = resultHeight * imageAspectRatio;
                }
                break;
        }

        resultWidth = Math.Min(resultWidth, availableContentWidth);
        resultHeight = Math.Min(resultHeight, availableContentHeight);

        if (resultWidth <= 0 || resultHeight <= 0) return SKRect.Empty;

        float offsetX = (containerWidth - resultWidth) / 2f;
        float offsetY = 0;

        return SKRect.Create(drawAtX + offsetX, drawAtY + offsetY, resultWidth, resultHeight);
    }
}
