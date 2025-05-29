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
                // skImage remains null
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
            skImage = null; // Ensure skImage is null on error
        }

        float elementContentDrawX = contentRect.Left + (float)image.GetMargin.Left;
        float elementContentDrawY = currentY;

        float availableWidthForImageContent = contentRect.Width - (float)image.GetMargin.Left - (float)image.GetMargin.Right;
        float availableHeightForImageContent = contentRect.Bottom - currentY - (float)image.GetMargin.Bottom;

        availableWidthForImageContent = Math.Max(0, availableWidthForImageContent);
        availableHeightForImageContent = Math.Max(0, availableHeightForImageContent);

        if (skImage is null)
        {
            float phHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY, availableWidthForImageContent, availableHeightForImageContent);
            return Task.FromResult(new RenderOutput(phHeight, null, false));
        }

        using (skImage)
        {
            SKRect targetRectInCurrentSpace = CalculateTargetRect(skImage, image.CurrentAspect,
                                                       elementContentDrawX, elementContentDrawY,
                                                       availableWidthForImageContent, availableHeightForImageContent,
                                                       image.RequestedWidth, image.RequestedHeight);

            if (targetRectInCurrentSpace.Height > 0 && targetRectInCurrentSpace.Width > 0 && targetRectInCurrentSpace.Height <= availableHeightForImageContent)
            {
                canvas.DrawImage(skImage, targetRectInCurrentSpace);
                // canvas.Flush(); // Flush is often not needed per draw call, document flush is more typical.
                return Task.FromResult(new RenderOutput(targetRectInCurrentSpace.Height, null, false));
            }

            SKSize newPagePhysicalSize = SkiaUtils.GetSkPageSize(pageDefinition.Size, pageDefinition.Orientation);
            float newPageAvailWidthForImageContent = newPagePhysicalSize.Width
                                                 - (float)pageDefinition.Margins.Left - (float)pageDefinition.Margins.Right
                                                 - (float)image.GetMargin.Left - (float)image.GetMargin.Right;
            float newPageAvailHeightForImageContent = newPagePhysicalSize.Height
                                                  - (float)pageDefinition.Margins.Top - (float)pageDefinition.Margins.Bottom
                                                  - (float)image.GetMargin.Top - (float)image.GetMargin.Bottom;

            newPageAvailWidthForImageContent = Math.Max(0, newPageAvailWidthForImageContent);
            newPageAvailHeightForImageContent = Math.Max(0, newPageAvailHeightForImageContent);

            if (newPageAvailWidthForImageContent <= 0 || newPageAvailHeightForImageContent <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Image too large. No content space on new page. PageSize: {pageDefinition.Size}, PageMargins: {pageDefinition.Margins}, ImageMargins: {image.GetMargin}");
                float phHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY, availableWidthForImageContent, availableHeightForImageContent, "[Imagen Demasiado Grande]");
                return Task.FromResult(new RenderOutput(phHeight, null, false));
            }

            SKRect targetRectOnNewPage = CalculateTargetRect(skImage, image.CurrentAspect,
                                                           0, 0,
                                                           newPageAvailWidthForImageContent, newPageAvailHeightForImageContent,
                                                           image.RequestedWidth, image.RequestedHeight);

            if (targetRectOnNewPage.Height > 0 && targetRectOnNewPage.Width > 0 &&
                targetRectOnNewPage.Height <= newPageAvailHeightForImageContent &&
                targetRectOnNewPage.Width <= newPageAvailWidthForImageContent)
            {
                return Task.FromResult(new RenderOutput(0, image, true));
            }

            System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Image too large for a new page. Calculated new page rect: {targetRectOnNewPage}, Available: {newPageAvailWidthForImageContent}x{newPageAvailHeightForImageContent}");
            float placeholderHeight = RenderPlaceholder(canvas, elementContentDrawX, elementContentDrawY,
                                                      availableWidthForImageContent, availableHeightForImageContent,
                                                      "[Imagen Demasiado Grande]");
            return Task.FromResult(new RenderOutput(placeholderHeight, null, false));
        }
    }

    // Made RenderPlaceholder sychronous by using SKTypeface.Default for simplicity.
    // If a specific font loaded async is needed here, this method would need to become async.
    private float RenderPlaceholder(SKCanvas canvas, float x, float y, float availableWidth, float availableHeight, string message = "[Image Error]")
    {
        float phMaxWidth = Math.Max(0, availableWidth);
        float phMaxHeight = Math.Max(0, availableHeight);

        if (phMaxWidth <= 5 || phMaxHeight <= 5) return 0f;

        using var errorBorderPaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };

        // For placeholder, SKTypeface.Default is usually sufficient and avoids async complexities here.
        // If "Helvetica" or specific attributes were critical, this would need to be async and use CreateSkTypefaceAsync.
        using var placeholderTypeface = SKTypeface.Default; // Simpler than SkiaUtils.CreateSkTypefaceAsync for placeholder
        using var skFont = new SKFont(placeholderTypeface, 10f);
        // SKPaint no longer takes SKFont in constructor
        using var placeholderTextPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
        // placeholderTextPaint.Typeface = placeholderTypeface; // Set properties on SKPaint if not using SKFont in DrawText
        // placeholderTextPaint.TextSize = 10f;

        float phWidth = Math.Min(phMaxWidth, 100f);
        float phHeight = Math.Min(phMaxHeight, 50f);

        SKRect placeholderRect = SKRect.Create(x, y, phWidth, phHeight);

        if (placeholderRect.Bottom > y + phMaxHeight + 0.01f) // Adjusted tolerance
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG ImageRenderer: Placeholder for '{message}' ({placeholderRect.Height}h) would exceed availableHeight ({phMaxHeight}). Clamping or skipping.");
            placeholderRect.Bottom = y + phMaxHeight;
            if (placeholderRect.Height < 5) return 0f;
        }

        canvas.DrawRect(placeholderRect, errorBorderPaint);

        SKFontMetrics fontMetrics = skFont.Metrics;
        // Use SKFont.MeasureText
        float textVisualWidth = skFont.MeasureText(message); // Pass only string to SKFont.MeasureText for width

        float textX = placeholderRect.Left + (placeholderRect.Width - textVisualWidth) / 2f;
        textX = Math.Max(placeholderRect.Left + 2, Math.Min(textX, placeholderRect.Right - textVisualWidth - 2));

        float textY = placeholderRect.MidY - (fontMetrics.Ascent + fontMetrics.Descent) / 2f;

        canvas.Save();
        canvas.ClipRect(placeholderRect);
        // Use SKCanvas.DrawText overload that takes SKFont
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
            default: // AspectFit is the default
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

        // Ensure the calculated dimensions do not exceed the actual available content space
        resultWidth = Math.Min(resultWidth, availableContentWidth);
        resultHeight = Math.Min(resultHeight, availableContentHeight);


        if (resultWidth <= 0 || resultHeight <= 0) return SKRect.Empty;

        // Centering logic (optional, based on desired alignment within availableContentWidth)
        // Current logic centers horizontally, aligns top vertically.
        float offsetX = (availableContentWidth - resultWidth) / 2f;
        float offsetY = 0; // Align to top within its drawing slot (currentY)

        return SKRect.Create(drawAtX + offsetX, drawAtY + offsetY, resultWidth, resultHeight);
    }
}
