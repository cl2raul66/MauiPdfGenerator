using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class ImageRenderer : IElementRenderer
{
    public Task<LayoutInfo> MeasureAsync(PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var image = (PdfImage)element;
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(pageDef);

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
            var error = new PdfGenerationException($"Failed to decode image stream: {ex.Message}", ex);
            float phHeight = 50f + (float)image.GetMargin.VerticalThickness + (float)image.GetPadding.VerticalThickness;
            float phWidth = (image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(availableRect.Width, 100f)) + (float)image.GetMargin.HorizontalThickness + (float)image.GetPadding.HorizontalThickness;
            return Task.FromResult(new LayoutInfo(element, phWidth, phHeight, null, error));
        }

        if (skImage is null)
        {
            var error = new PdfGenerationException("Image stream was not readable or resulted in a null image.");
            float phHeight = 50f + (float)image.GetMargin.VerticalThickness + (float)image.GetPadding.VerticalThickness;
            float phWidth = (image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(availableRect.Width, 100f)) + (float)image.GetMargin.HorizontalThickness + (float)image.GetPadding.HorizontalThickness;
            return Task.FromResult(new LayoutInfo(element, phWidth, phHeight, null, error));
        }

        using (skImage)
        {
            float availableContentWidth = availableRect.Width - (float)image.GetMargin.HorizontalThickness - (float)image.GetPadding.HorizontalThickness;
            float availableContentHeight = availableRect.Height - (float)image.GetMargin.VerticalThickness - (float)image.GetPadding.VerticalThickness;

            SKRect targetRect = CalculateTargetRectInternal(skImage, image.CurrentAspect,
                                                        0, 0,
                                                        availableContentWidth, availableContentHeight,
                                                        image.GetWidthRequest, image.GetHeightRequest);

            float totalHeight = targetRect.Height + (float)image.GetPadding.VerticalThickness + (float)image.GetMargin.VerticalThickness;
            float totalWidth = targetRect.Width + (float)image.GetPadding.HorizontalThickness + (float)image.GetMargin.HorizontalThickness;

            return Task.FromResult(new LayoutInfo(element, totalWidth, totalHeight));
        }
    }

    public Task RenderAsync(SKCanvas canvas, PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect renderRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var image = (PdfImage)element;
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(image);

        SKImage? skImage = null;
        try
        {
            if (image.ImageStream.CanRead)
            {
                if (image.ImageStream.CanSeek) image.ImageStream.Position = 0;
                skImage = SKImage.FromEncodedData(image.ImageStream);
            }
        }
        catch
        {
            skImage = null;
        }

        var contentRect = new SKRect(
            renderRect.Left + (float)image.GetMargin.Left + (float)image.GetPadding.Left,
            renderRect.Top + (float)image.GetMargin.Top + (float)image.GetPadding.Top,
            renderRect.Right - (float)image.GetMargin.Right - (float)image.GetPadding.Right,
            renderRect.Bottom - (float)image.GetMargin.Bottom - (float)image.GetPadding.Bottom
        );

        if (skImage is null)
        {
            DrawImageError(canvas, contentRect.Left, contentRect.Top, contentRect.Width, contentRect.Height, "[Image Error]");
            return Task.CompletedTask;
        }

        using (skImage)
        {
            SKRect targetRect = CalculateTargetRectInternal(skImage, image.CurrentAspect, contentRect.Left, contentRect.Top, contentRect.Width, contentRect.Height, image.GetWidthRequest, image.GetHeightRequest);

            if (image.GetBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(image.GetBackgroundColor), Style = SKPaintStyle.Fill };
                canvas.DrawRect(targetRect, bgPaint);
            }

            canvas.DrawImage(skImage, targetRect);
        }
        return Task.CompletedTask;
    }

    private float DrawImageError(SKCanvas? canvas, float x, float y, float availableWidth, float availableHeight, string message = "[Image Error]")
    {
        float phMaxWidth = Math.Max(0, availableWidth);
        float phMaxHeight = Math.Max(0, availableHeight);

        float phWidth = Math.Min(phMaxWidth, 100f);
        float phHeight = Math.Min(phMaxHeight, 50f);

        if (phMaxWidth <= 5 || phMaxHeight <= 5) return 0f;

        SKRect placeholderRect = SKRect.Create(x, y, phWidth, phHeight);
        if (placeholderRect.Height < 5) return 0f;

        if (canvas is null)
        {
            return placeholderRect.Height;
        }

        using var errorBorderPaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
        using var placeholderTypeface = SKTypeface.Default;
        using var skFont = new SKFont(placeholderTypeface, 10f);
        using var placeholderTextPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };

        canvas.DrawRect(placeholderRect, errorBorderPaint);

        SKFontMetrics fontMetrics = skFont.Metrics;
        float textVisualWidth = skFont.MeasureText(message);
        float textX = placeholderRect.Left + (placeholderRect.Width - textVisualWidth) / 2f;
        textX = Math.Max(placeholderRect.Left + 2, Math.Min(textX, placeholderRect.Right - textVisualWidth - 2));
        float textY = placeholderRect.MidY - (fontMetrics.Ascent + fontMetrics.Descent) / 2f;

        canvas.Save();
        canvas.ClipRect(placeholderRect);
        canvas.DrawText(message, textX, textY, skFont, placeholderTextPaint);
        canvas.Restore();

        return placeholderRect.Height;
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
