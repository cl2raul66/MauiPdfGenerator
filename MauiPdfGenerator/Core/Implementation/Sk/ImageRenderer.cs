using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class ImageRenderer
{
    public Task<float> RenderAsync(SKCanvas canvas, PdfImage image, PdfPageData pageData, SKRect contentRect, float currentY)
    {
        SKImage? skImage = null;
        float elementHeight = 0;

        Stream imageStream = image.ImageStream;

        try
        {
            if (imageStream.CanSeek)
            {
                imageStream.Position = 0;
            }

            skImage = SKImage.FromEncodedData(imageStream); 
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Exception processing image stream: {ex.Message}. Rendering placeholder.");
            skImage = null; 
        }

        float availableWidth = contentRect.Width - (float)image.GetMargin.Left - (float)image.GetMargin.Right;
        float availableHeightForElement = contentRect.Bottom - currentY - (float)image.GetMargin.Bottom;

        availableWidth = Math.Max(0, availableWidth);
        availableHeightForElement = Math.Max(0, availableHeightForElement);

        float drawX = contentRect.Left + (float)image.GetMargin.Left;
        float drawY = currentY;

        if (availableWidth <= 0 || availableHeightForElement <= 0)
        {
            skImage?.Dispose();
            return Task.FromResult(0f); 
        }

        if (skImage is null)
        {
            elementHeight = RenderPlaceholder(canvas, drawX, drawY, availableWidth, availableHeightForElement);
        }
        else
        {
            using (skImage) 
            {
                SKRect targetRect = CalculateTargetRect(skImage, image.CurrentAspect,
                                                       drawX, drawY,
                                                       availableWidth, availableHeightForElement,
                                                       image.RequestedWidth, image.RequestedHeight);

                if (targetRect.Width > 0 && targetRect.Height > 0 && targetRect.Bottom <= drawY + availableHeightForElement + 0.1f)
                {
                    canvas.DrawImage(skImage, targetRect);
                    canvas.Flush();
                    elementHeight = targetRect.Height;
                }
                else
                {
                    string reason = "";
                    if (targetRect.Width <= 0 || targetRect.Height <= 0) reason = "Zero size.";
                    else reason = $"Exceeds available space (TargetRect.Bottom: {targetRect.Bottom:F1} vs Limit for element content: {drawY + availableHeightForElement:F1}).";

                    elementHeight = RenderPlaceholder(canvas, drawX, drawY, availableWidth, availableHeightForElement, "Image Overflow");
                }
            }
        }
        return Task.FromResult(elementHeight); 
    }

    private float RenderPlaceholder(SKCanvas canvas, float x, float y, float availableWidth, float availableHeight, string message = "[Image Error]")
    {
        using var errorBorderPaint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };
        using var placeholderFont = new SKFont { Size = 10f };
        using var placeholderTextPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };

        float phWidth = Math.Min(availableWidth, 100f);
        float phHeight = Math.Min(availableHeight, 50f);

        if (phWidth <= 0 || phHeight <= 0)
        {
            return 0;
        }

        SKRect placeholderRect = SKRect.Create(x, y, phWidth, phHeight);

        if (placeholderRect.Bottom > y + availableHeight + 0.1f)
        {
            return 0;
        }

        canvas.DrawRect(placeholderRect, errorBorderPaint);

        SKFontMetrics fontMetrics = placeholderFont.Metrics;
        float textVisualWidth = placeholderFont.MeasureText(message);
        float textX = placeholderRect.Left + (placeholderRect.Width - textVisualWidth) / 2f;
        float textY = placeholderRect.MidY - (fontMetrics.Ascent + fontMetrics.Descent) / 2f;

        canvas.DrawText(message, textX, textY, placeholderFont, placeholderTextPaint);

        return phHeight;
    }

    private SKRect CalculateTargetRect(SKImage image, Aspect aspect,
                                     float drawAtX, float drawAtY,
                                     float availableWidthForElement, float availableHeightForElement,
                                     double? requestedWidth, double? requestedHeight)
    {
        float imgWidth = image.Width;
        float imgHeight = image.Height;

        if (imgWidth <= 0 || imgHeight <= 0 || availableWidthForElement <= 0 || availableHeightForElement <= 0)
        {
            return SKRect.Empty;
        }

        float resultWidth, resultHeight;
        float imageAspectRatio = imgWidth / imgHeight;

        float containerWidth = requestedWidth.HasValue ? (float)requestedWidth.Value : availableWidthForElement;
        float containerHeight = requestedHeight.HasValue ? (float)requestedHeight.Value : availableHeightForElement;

        containerWidth = Math.Min(containerWidth, availableWidthForElement);
        containerHeight = Math.Min(containerHeight, availableHeightForElement);

        switch (aspect)
        {
            case Aspect.Fill:
                resultWidth = containerWidth;
                resultHeight = containerHeight;
                break;
            case Aspect.AspectFill:
                float containerAspectFill = containerWidth / containerHeight;
                if (imageAspectRatio > containerAspectFill)
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
                float containerAspectFit = containerWidth / containerHeight;
                if (imageAspectRatio > containerAspectFit)
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

        resultWidth = Math.Min(resultWidth, availableWidthForElement);
        resultHeight = Math.Min(resultHeight, availableHeightForElement);

        float offsetX = (availableWidthForElement - resultWidth) / 2f;
        float offsetY = 0; 

        return SKRect.Create(drawAtX + offsetX, drawAtY + offsetY, resultWidth, resultHeight);
    }
}
