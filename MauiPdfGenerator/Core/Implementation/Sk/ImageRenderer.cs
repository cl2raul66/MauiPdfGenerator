using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class ImageRenderer
{
    public Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfImage image, PdfPageData pageDefinition, SKRect contentRect, float currentY)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(pageDefinition);
        SKImage? skImage = null;
        Stream imageStream = image.ImageStream;

        try
        {
            if (!imageStream.CanRead)
            {
                System.Diagnostics.Debug.WriteLine("DEBUG ImageRenderer: Image stream is not readable. Rendering placeholder.");
            }
            else
            {
                if (imageStream.CanSeek)
                {
                    imageStream.Position = 0;
                }
                skImage = SKImage.FromEncodedData(imageStream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Exception processing image stream: {ex.Message}. Rendering placeholder.");
            skImage = null;
        }

        float elementContentDrawX = contentRect.Left + (float)image.GetMargin.Left + (float)image.GetPadding.Left;
        float elementContentDrawY = currentY + (float)image.GetPadding.Top;

        float availableWidthForImageContent = contentRect.Width - (float)image.GetMargin.HorizontalThickness - (float)image.GetPadding.HorizontalThickness;
        float availableHeightForImageContent = contentRect.Bottom - currentY - (float)image.GetMargin.VerticalThickness - (float)image.GetPadding.VerticalThickness;

        availableWidthForImageContent = Math.Max(0, availableWidthForImageContent);
        availableHeightForImageContent = Math.Max(0, availableHeightForImageContent);

        if (skImage is null)
        {
            float phHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY, availableWidthForImageContent, availableHeightForImageContent);
            float phWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(availableWidthForImageContent, 100f);
            float totalHeight = (image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : phHeight) + (float)image.GetPadding.VerticalThickness;
            float totalWidth = phWidth + (float)image.GetPadding.HorizontalThickness;
            return Task.FromResult(new RenderOutput(totalHeight, totalWidth, null, false, phHeight));
        }

        using (skImage)
        {
            SKRect targetRectInCurrentSpace = CalculateTargetRect(skImage, image.CurrentAspect,
                                                       elementContentDrawX, elementContentDrawY,
                                                       availableWidthForImageContent, availableHeightForImageContent,
                                                       image.GetWidthRequest, image.GetHeightRequest);

            if (targetRectInCurrentSpace.Height > 0 && targetRectInCurrentSpace.Width > 0 && targetRectInCurrentSpace.Height <= availableHeightForImageContent)
            {
                canvas.DrawImage(skImage, targetRectInCurrentSpace);
                float totalHeight = (image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : targetRectInCurrentSpace.Height) + (float)image.GetPadding.VerticalThickness;
                float totalWidth = (image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : targetRectInCurrentSpace.Width) + (float)image.GetPadding.HorizontalThickness;
                return Task.FromResult(new RenderOutput(totalHeight, totalWidth, null, false, targetRectInCurrentSpace.Height));
            }

            SKSize newPagePhysicalSize = SkiaUtils.GetSkPageSize(pageDefinition.Size, pageDefinition.Orientation);
            float newPageAvailWidthForImageContent = newPagePhysicalSize.Width
                                                 - (float)pageDefinition.Margins.Left - (float)pageDefinition.Margins.Right
                                                 - (float)image.GetMargin.HorizontalThickness - (float)image.GetPadding.HorizontalThickness;
            float newPageAvailHeightForImageContent = newPagePhysicalSize.Height
                                                  - (float)pageDefinition.Margins.Top - (float)pageDefinition.Margins.Bottom
                                                  - (float)image.GetMargin.VerticalThickness - (float)image.GetPadding.VerticalThickness;

            newPageAvailWidthForImageContent = Math.Max(0, newPageAvailWidthForImageContent);
            newPageAvailHeightForImageContent = Math.Max(0, newPageAvailHeightForImageContent);

            if (newPageAvailWidthForImageContent <= 0 || newPageAvailHeightForImageContent <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Image too large. No content space on new page. PageSize: {pageDefinition.Size}, PageMargins: {pageDefinition.Margins}, ImageMargins: {image.GetMargin}, ImagePadding: {image.GetPadding}");
                float phHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY, availableWidthForImageContent, availableHeightForImageContent, "[Imagen Demasiado Grande]");
                float phWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(availableWidthForImageContent, 100f);
                float totalHeight = (image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : phHeight) + (float)image.GetPadding.VerticalThickness;
                float totalWidth = phWidth + (float)image.GetPadding.HorizontalThickness;
                return Task.FromResult(new RenderOutput(totalHeight, totalWidth, null, false, phHeight));
            }

            SKRect targetRectOnNewPage = CalculateTargetRect(skImage, image.CurrentAspect,
                                                           0, 0,
                                                           newPageAvailWidthForImageContent, newPageAvailHeightForImageContent,
                                                           image.GetWidthRequest, image.GetHeightRequest);

            if (targetRectOnNewPage.Height > 0 && targetRectOnNewPage.Width > 0 &&
                targetRectOnNewPage.Height <= newPageAvailHeightForImageContent &&
                targetRectOnNewPage.Width <= newPageAvailWidthForImageContent)
            {
                return Task.FromResult(new RenderOutput(0, 0, image, true));
            }

            System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Image too large for a new page. Calculated new page rect: {targetRectOnNewPage}, Available: {newPageAvailWidthForImageContent}x{newPageAvailHeightForImageContent}");
            float placeholderHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY,
                                                      availableWidthForImageContent, availableHeightForImageContent,
                                                      "[Imagen Demasiado Grande]");
            float placeholderWidth = image.GetWidthRequest.HasValue ? (float)image.GetWidthRequest.Value : Math.Min(availableWidthForImageContent, 100f);
            float totalFinalHeight = (image.GetHeightRequest.HasValue ? (float)image.GetHeightRequest.Value : placeholderHeight) + (float)image.GetPadding.VerticalThickness;
            float totalFinalWidth = placeholderWidth + (float)image.GetPadding.HorizontalThickness;
            return Task.FromResult(new RenderOutput(totalFinalHeight, totalFinalWidth, null, false, placeholderHeight));
        }
    }

    private float RenderPlaceholder(SKCanvas canvas, float x, float y, float availableWidth, float availableHeight, string message = "[Image Error]")
    {
        float phMaxWidth = Math.Max(0, availableWidth);
        float phMaxHeight = Math.Max(0, availableHeight);

        if (phMaxWidth <= 5 || phMaxHeight <= 5) return 0f;

        using var errorBorderPaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
        using var placeholderTypeface = SKTypeface.Default;
        using var skFont = new SKFont(placeholderTypeface, 10f);
        using var placeholderTextPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };

        float phWidth = Math.Min(phMaxWidth, 100f);
        float phHeight = Math.Min(phMaxHeight, 50f);

        SKRect placeholderRect = SKRect.Create(x, y, phWidth, phHeight);

        if (placeholderRect.Bottom > y + phMaxHeight + 0.01f)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Placeholder for '{message}' ({placeholderRect.Height}h) would exceed availableHeight ({phMaxHeight}). Clamping or skipping.");
            placeholderRect.Bottom = y + phMaxHeight;
            if (placeholderRect.Height < 5) return 0f;
        }

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

    private SKRect CalculateTargetRect(SKImage image, Aspect aspect,
                                     float drawAtX, float drawAtY,
                                     float availableContentWidth, float availableContentHeight,
                                     double? requestedWidth, double? requestedHeight)
    {
        ArgumentNullException.ThrowIfNull(image);

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
