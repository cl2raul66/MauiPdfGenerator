using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class ImageRenderer
{
    public Task<MeasureOutput> MeasureAsync(PdfImage image, PdfPageData pageDefinition, SKRect contentRect, float currentY)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(pageDefinition);

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
            float phHeight = DrawImageError(null, 0, 0, contentRect.Width, contentRect.Height);
            float phWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(contentRect.Width, 100f);
            float totalHeight = (image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : phHeight) + (float)image.GetPadding.VerticalThickness;
            float totalWidth = phWidth + (float)image.GetPadding.HorizontalThickness;
            return Task.FromResult(new MeasureOutput(totalHeight, phHeight, totalWidth, [], null, false, 0, 0, 0, 0, error));
        }

        if (skImage is null)
        {
            float phHeight = DrawImageError(null, 0, 0, contentRect.Width, contentRect.Height);
            float phWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(contentRect.Width, 100f);
            float totalHeight = (image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : phHeight) + (float)image.GetPadding.VerticalThickness;
            float totalWidth = phWidth + (float)image.GetPadding.HorizontalThickness;
            var error = new PdfGenerationException("Image stream was not readable or resulted in a null image.");
            return Task.FromResult(new MeasureOutput(totalHeight, phHeight, totalWidth, [], null, false, 0, 0, 0, 0, error));
        }

        using (skImage)
        {
            float availableWidth = contentRect.Width;
            float availableHeight = contentRect.Height;

            SKRect targetRect = CalculateTargetRectInternal(skImage, image.CurrentAspect,
                                                        0, 0,
                                                        availableWidth, availableHeight,
                                                        image.GetWidthRequest, image.GetHeightRequest);

            if (targetRect.Height > 0 && targetRect.Width > 0 && targetRect.Height <= availableHeight + 0.01f)
            {
                float totalHeight = (image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : targetRect.Height) + (float)image.GetPadding.VerticalThickness;
                float totalWidth = (image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : targetRect.Width) + (float)image.GetPadding.HorizontalThickness;
                return Task.FromResult(new MeasureOutput(totalHeight, targetRect.Height, totalWidth, [], null, false, 0, 0, 0, 0, null));
            }

            SKSize newPagePhysicalSize = SkiaUtils.GetSkPageSize(pageDefinition.Size, pageDefinition.Orientation);
            float newPageAvailHeight = newPagePhysicalSize.Height - (float)pageDefinition.Margins.VerticalThickness - (float)image.GetMargin.VerticalThickness - (float)image.GetPadding.VerticalThickness;

            if (targetRect.Height > newPageAvailHeight)
            {
                float phHeight = DrawImageError(null, 0, 0, availableWidth, availableHeight, "[Imagen Demasiado Grande]");
                float phWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(availableWidth, 100f);
                float totalHeight = (image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : phHeight) + (float)image.GetPadding.VerticalThickness;
                float totalWidth = phWidth + (float)image.GetPadding.HorizontalThickness;
                var error = new PdfGenerationException("Image is too large to fit on a new page.");
                return Task.FromResult(new MeasureOutput(totalHeight, phHeight, totalWidth, [], null, false, 0, 0, 0, 0, error));
            }

            return Task.FromResult(new MeasureOutput(0, 0, 0, [], image, true, 0, 0, 0, 0, null));
        }
    }

    public Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfImage image, PdfPageData pageDefinition, SKRect contentRect, float currentY)
    {
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

        if (skImage is null)
        {
            float phHeight = DrawImageError(canvas, contentRect.Left, contentRect.Top, contentRect.Width, contentRect.Height, "[Image Error]");
            float phWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(contentRect.Width, 100f);
            return Task.FromResult(new RenderOutput(phHeight, phWidth, null, false, phHeight));
        }

        using (skImage)
        {
            SKRect targetRect = CalculateTargetRectInternal(skImage, image.CurrentAspect, contentRect.Left, contentRect.Top, contentRect.Width, contentRect.Height,image.GetWidthRequest, image.GetHeightRequest);

            float extraX = (contentRect.Width - targetRect.Width) / 2f;
            float extraY = (contentRect.Height - targetRect.Height) / 2f;
            SKRect centeredRect = SKRect.Create(
                contentRect.Left + Math.Max(0, extraX),
                contentRect.Top + Math.Max(0, extraY),
                targetRect.Width,
                targetRect.Height
            );

            if (image.GetBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(image.GetBackgroundColor), Style = SKPaintStyle.Fill };
                canvas.DrawRect(centeredRect, bgPaint);
            }

            canvas.DrawImage(skImage, centeredRect);

            float visualHeight = centeredRect.Height;
            float consumedWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : centeredRect.Width;
            return Task.FromResult(new RenderOutput(visualHeight, consumedWidth, null, false, visualHeight));
        }
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
