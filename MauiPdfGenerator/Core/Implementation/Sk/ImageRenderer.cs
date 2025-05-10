using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class ImageRenderer
{
    // Corrected Signature: Removed bool isContinuation
    public Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfImage image, PdfPageData pageDefinition, SKRect contentRect, float currentY)
    {
        SKImage? skImage = null;
        Stream imageStream = image.ImageStream; // This is the MemoryStream

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

        // currentY is the Y position on the page *after* the element's top margin has been applied by SkPdfGenerationService.
        // So, image content starts at currentY.
        float elementContentDrawX = contentRect.Left + (float)image.GetMargin.Left;
        float elementContentDrawY = currentY;

        // Available width/height for the image's *content* (excluding its own margins)
        float availableWidthForImageContent = contentRect.Width - (float)image.GetMargin.Left - (float)image.GetMargin.Right;
        float availableHeightForImageContent = contentRect.Bottom - currentY - (float)image.GetMargin.Bottom;

        availableWidthForImageContent = Math.Max(0, availableWidthForImageContent);
        availableHeightForImageContent = Math.Max(0, availableHeightForImageContent);

        // Scenario 1: Image decoding failed
        if (skImage is null)
        {
            float phHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY, availableWidthForImageContent, availableHeightForImageContent);
            return Task.FromResult(new RenderOutput(phHeight, null, false));
        }

        // Using skImage from here
        using (skImage)
        {
            // Calculate how the image would render in the current available space
            SKRect targetRectInCurrentSpace = CalculateTargetRect(skImage, image.CurrentAspect,
                                                       elementContentDrawX, elementContentDrawY,
                                                       availableWidthForImageContent, availableHeightForImageContent,
                                                       image.RequestedWidth, image.RequestedHeight);

            // Scenario 2: Image (content part) fits in the currently available height on this page
            if (targetRectInCurrentSpace.Height > 0 && targetRectInCurrentSpace.Width > 0 && targetRectInCurrentSpace.Height <= availableHeightForImageContent)
            {
                canvas.DrawImage(skImage, targetRectInCurrentSpace);
                canvas.Flush();
                return Task.FromResult(new RenderOutput(targetRectInCurrentSpace.Height, null, false));
            }

            // Scenario 3: Image doesn't fit the remaining space on this page. Check if it fits on a new page.
            // Get full new page dimensions
            SKSize newPagePhysicalSize = SkiaUtils.GetSkPageSize(pageDefinition.Size, pageDefinition.Orientation);

            // Calculate available content area on a fresh new page for this image (page margins + image margins)
            float newPageAvailWidthForImageContent = newPagePhysicalSize.Width
                                                 - (float)pageDefinition.Margins.Left - (float)pageDefinition.Margins.Right
                                                 - (float)image.GetMargin.Left - (float)image.GetMargin.Right;
            float newPageAvailHeightForImageContent = newPagePhysicalSize.Height
                                                  - (float)pageDefinition.Margins.Top - (float)pageDefinition.Margins.Bottom
                                                  - (float)image.GetMargin.Top - (float)image.GetMargin.Bottom;

            newPageAvailWidthForImageContent = Math.Max(0, newPageAvailWidthForImageContent);
            newPageAvailHeightForImageContent = Math.Max(0, newPageAvailHeightForImageContent);

            if (newPageAvailWidthForImageContent <= 0 || newPageAvailHeightForImageContent <= 0) // No space even on a new page after margins
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Image too large. No content space on new page. PageSize: {pageDefinition.Size}, PageMargins: {pageDefinition.Margins}, ImageMargins: {image.GetMargin}");
                float phHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY, availableWidthForImageContent, availableHeightForImageContent, "[Imagen Demasiado Grande]");
                return Task.FromResult(new RenderOutput(phHeight, null, false));
            }

            SKRect targetRectOnNewPage = CalculateTargetRect(skImage, image.CurrentAspect,
                                                           0, 0, // Dummy X,Y for calculation
                                                           newPageAvailWidthForImageContent, newPageAvailHeightForImageContent,
                                                           image.RequestedWidth, image.RequestedHeight);

            // Scenario 3a: Image fits on a new page
            if (targetRectOnNewPage.Height > 0 && targetRectOnNewPage.Width > 0 &&
                targetRectOnNewPage.Height <= newPageAvailHeightForImageContent &&
                targetRectOnNewPage.Width <= newPageAvailWidthForImageContent)
            {
                // Don't draw. Signal to move the original image to a new page.
                return Task.FromResult(new RenderOutput(0, image, true));
            }

            // Scenario 3b: Image is too large even for a new empty page. Render placeholder in current available space.
            System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Image too large for a new page. Calculated new page rect: {targetRectOnNewPage}, Available: {newPageAvailWidthForImageContent}x{newPageAvailHeightForImageContent}");
            float placeholderHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY,
                                                      availableWidthForImageContent, availableHeightForImageContent,
                                                      "[Imagen Demasiado Grande]");
            return Task.FromResult(new RenderOutput(placeholderHeight, null, false));
        }
    }

    private float RenderPlaceholder(SKCanvas canvas, float x, float y, float availableWidth, float availableHeight, string message = "[Image Error]")
    {
        float phMaxWidth = Math.Max(0, availableWidth);
        float phMaxHeight = Math.Max(0, availableHeight);

        if (phMaxWidth <= 5 || phMaxHeight <= 5) return 0f; // Not enough space for a meaningful placeholder

        using var errorBorderPaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
        using var placeholderTypeface = SkiaUtils.CreateSkTypeface("Helvetica", FontAttributes.None) ?? SKTypeface.Default;
        using var skFont = new SKFont(placeholderTypeface, 10f);
        using var placeholderTextPaint = new SKPaint(skFont) { Color = SKColors.Red, IsAntialias = true };

        float phWidth = Math.Min(phMaxWidth, 100f);
        float phHeight = Math.Min(phMaxHeight, 50f);

        SKRect placeholderRect = SKRect.Create(x, y, phWidth, phHeight);

        // Ensure placeholder itself doesn't overflow the given availableHeight for drawing.
        if (placeholderRect.Bottom > y + phMaxHeight + 0.1f)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Placeholder for '{message}' ({placeholderRect.Height}h) would exceed availableHeight ({phMaxHeight}). Clamping or skipping.");
            // Adjust placeholder height if it overflows its own drawing budget
            placeholderRect.Bottom = y + phMaxHeight;
            if (placeholderRect.Height < 5) return 0f; // Too small after clamping
        }

        canvas.DrawRect(placeholderRect, errorBorderPaint);

        SKFontMetrics fontMetrics = skFont.Metrics;
        float textVisualWidth = placeholderTextPaint.MeasureText(message);

        float textX = placeholderRect.Left + (placeholderRect.Width - textVisualWidth) / 2f;
        textX = Math.Max(placeholderRect.Left + 2, Math.Min(textX, placeholderRect.Right - textVisualWidth - 2)); // Clamp with padding

        float textY = placeholderRect.MidY - (fontMetrics.Ascent + fontMetrics.Descent) / 2f;

        canvas.Save();
        canvas.ClipRect(placeholderRect); // Clip text to placeholder box
        canvas.DrawText(message, textX, textY, placeholderTextPaint);
        canvas.Restore();

        return placeholderRect.Height;
    }

    private SKRect CalculateTargetRect(SKImage image, Aspect aspect,
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

        resultWidth = Math.Min(resultWidth, availableContentWidth);
        resultHeight = Math.Min(resultHeight, availableContentHeight);

        if (resultWidth <= 0 || resultHeight <= 0) return SKRect.Empty;

        float offsetX = (availableContentWidth - resultWidth) / 2f;
        float offsetY = 0;

        return SKRect.Create(drawAtX + offsetX, drawAtY + offsetY, resultWidth, resultHeight);
    }
}
