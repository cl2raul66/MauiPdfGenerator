using MauiPdfGenerator.Fluent.Models.Elements; 
using MauiPdfGenerator.Core.Models;
using SkiaSharp;
using MauiPdfGenerator.Common.Enums;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class ImageRenderer
{
    private readonly IImageResolverService _imageResolverService;

    public ImageRenderer(IImageResolverService imageResolverService)
    {
        _imageResolverService = imageResolverService ?? throw new ArgumentNullException(nameof(imageResolverService));
    }

    public async Task<float> RenderAsync(SKCanvas canvas, PdfImage image, PdfPageData pageData, SKRect contentRect, float currentY)
    {
        SKImage? skImage = null;
        Stream? imageStream = null;

        try
        {
            // Usar el enum interno correcto de PdfImage
            imageStream = await _imageResolverService.GetStreamAsync(image.SourceData, image.DeterminedSourceKind);

            if (imageStream is not null && imageStream.Length > 0)
            {
                if (imageStream.CanSeek) imageStream.Position = 0;
                skImage = SKImage.FromEncodedData(imageStream);
                if (skImage == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Error: Failed to decode image data for source: {image.SourceData}");
                }
            }
            else if (imageStream is null) { /* Error logged by resolver */ }
            else { System.Diagnostics.Debug.WriteLine($"Warning: Image stream is empty for source: {image.SourceData}"); }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing image source {image.SourceData}: {ex.Message}");
            skImage = null;
        }
        finally
        {
            // Usar el enum interno correcto de PdfImage
            if (image.DeterminedSourceKind != PdfImageSourceKind.Stream && imageStream != null)
            {
                await imageStream.DisposeAsync();
            }
        }

        // --- Calculate Available Space ---
        float availableWidth = contentRect.Width - (float)image.GetMargin.Left - (float)image.GetMargin.Right;
        float availableHeight = contentRect.Bottom - currentY - (float)image.GetMargin.Bottom;
        availableWidth = Math.Max(0, availableWidth);
        availableHeight = Math.Max(0, availableHeight);

        float drawX = contentRect.Left + (float)image.GetMargin.Left;
        float drawY = currentY;

        if (availableWidth <= 0 || availableHeight <= 0)
        {
            System.Diagnostics.Debug.WriteLine("Warning: No available space to render image or placeholder.");
            if (skImage != null) skImage.Dispose(); // Dispose image if not drawn
            return 0;
        }

        // --- Draw Placeholder or Image ---
        if (skImage == null)
        {
            float placeholderHeight = RenderPlaceholder(canvas, drawX, drawY, availableWidth, availableHeight);
            return placeholderHeight;
        }
        else
        {
            using (skImage) // Dispose SKImage when done
            {
                SKRect targetRect = CalculateTargetRect(skImage, image.CurrentAspect,
                                                       drawX, drawY, availableWidth, availableHeight,
                                                       image.RequestedWidth, image.RequestedHeight);

                if (targetRect.Width > 0 && targetRect.Height > 0 && targetRect.Bottom <= contentRect.Bottom + 0.1f)
                {
                    canvas.DrawImage(skImage, targetRect);
                    return targetRect.Height;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Image '{image.SourceData}' could not be drawn. Calculated target rect ({targetRect}) exceeds available space (Bottom: {contentRect.Bottom}).");
                    // skImage se dispone automáticamente por el using
                    float placeholderHeight = RenderPlaceholder(canvas, drawX, drawY, availableWidth, availableHeight, "Image Overflow");
                    return placeholderHeight;
                }
            } // skImage is disposed here
        }
    }

    /// <summary>
    /// Renders a placeholder rectangle with error text. (Updated to avoid obsolete members)
    /// </summary>
    private float RenderPlaceholder(SKCanvas canvas, float x, float y, float width, float height, string message = "[Image Error]")
    {
        // --- Placeholder Style Setup ---
        // Paint for the border
        using var borderPaint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true // AntiAlias puede mejorar la apariencia del borde
        };

        // Font for the text (replaces SKPaint.TextSize)
        using var textFont = new SKFont
        {
            Size = 10f // Set font size here
            // Typeface = SKTypeface.Default // Opcional: especificar un typeface si se desea
        };

        // Paint for the text (only color and antialias needed now)
        using var textPaint = new SKPaint
        {
            Color = SKColors.Red,
            IsAntialias = true
        };

        // --- Placeholder Size ---
        float phWidth = Math.Min(width, 100);
        float phHeight = Math.Min(height, 50);

        if (phWidth <= 0 || phHeight <= 0) return 0;

        SKRect placeholderRect = new(x, y, x + phWidth, y + phHeight);

        // --- Draw Bounding Box ---
        canvas.DrawRect(placeholderRect, borderPaint);

        // --- Draw Centered Text ---
        // 1. Measure text width using the font (replaces obsolete paint measurement)
        float textWidth = textFont.MeasureText(message);

        // 2. Calculate X for centering (replaces SKPaint.TextAlign)
        float textX = placeholderRect.Left + (placeholderRect.Width - textWidth) / 2f;

        // 3. Get Font Metrics using the font (replaces SKPaint.FontMetrics)
        SKFontMetrics fontMetrics = textFont.Metrics;

        // 4. Calculate Y for vertical centering (baseline position)
        float textY = placeholderRect.MidY - (fontMetrics.Ascent + fontMetrics.Descent) / 2f;

        // 5. Draw text using the overload with SKFont (replaces obsolete DrawText overload)
        canvas.DrawText(message, textX, textY, textFont, textPaint);

        return placeholderRect.Height;
    }


    /// <summary>
    /// Calculates the destination rectangle for drawing the image based on aspect ratio and requested dimensions.
    /// (No changes needed in this method regarding obsolescence warnings)
    /// </summary>
    private SKRect CalculateTargetRect(SKImage image, Aspect aspect, float targetX, float targetY, float availableWidth, float availableHeight, double? requestedWidth, double? requestedHeight)
    {
        float imgWidth = image.Width;
        float imgHeight = image.Height;

        if (imgWidth <= 0 || imgHeight <= 0 || availableWidth <= 0 || availableHeight <= 0)
        {
            return SKRect.Empty;
        }

        // Determine the base size (prefer requested, fallback to available)
        // Important: requested dimensions might be larger than available space initially
        float reqW = requestedWidth.HasValue ? (float)requestedWidth.Value : availableWidth;
        float reqH = requestedHeight.HasValue ? (float)requestedHeight.Value : availableHeight;

        // Clamp requested dimensions initially ONLY if Aspect.Fill is used,
        // otherwise, let aspect calculation determine final size before clamping.
        float resultWidth = reqW;
        float resultHeight = reqH;


        float imageAspect = imgWidth / imgHeight;
        // Use the actual available space aspect ratio for calculations involving fitting/filling
        float containerAspect = availableWidth / availableHeight;

        switch (aspect)
        {
            case Aspect.Fill:
                // Ignore image aspect. Use requested size, but clamp to available space.
                resultWidth = Math.Min(reqW, availableWidth);
                resultHeight = Math.Min(reqH, availableHeight);
                break;

            case Aspect.AspectFill:
                // Fill available space, preserving aspect, potentially cropping.
                // Scale based on which dimension needs to be *larger* to fill the space.
                if (imageAspect > containerAspect) // Image wider than container -> scale to fit height, width will overextend
                {
                    resultHeight = availableHeight;
                    resultWidth = resultHeight * imageAspect;
                }
                else // Image taller than container -> scale to fit width, height will overextend
                {
                    resultWidth = availableWidth;
                    resultHeight = resultWidth / imageAspect;
                }

                // Now apply requested dimensions IF they are smaller than the calculated fill size
                if (requestedWidth.HasValue) resultWidth = Math.Min(resultWidth, (float)requestedWidth.Value);
                if (requestedHeight.HasValue) resultHeight = Math.Min(resultHeight, (float)requestedHeight.Value);

                // Finally ensure it doesn't exceed available space (this might cause cropping visually)
                resultWidth = Math.Min(resultWidth, availableWidth);
                resultHeight = Math.Min(resultHeight, availableHeight);

                break;

            case Aspect.AspectFit:
            default:
                // Fit within available space, preserving aspect, potentially leaving letterbox/pillarbox.
                // Scale based on which dimension needs to be *smaller* to fit.
                if (imageAspect > containerAspect) // Image wider than container -> scale to fit width
                {
                    resultWidth = availableWidth;
                    resultHeight = resultWidth / imageAspect;
                }
                else // Image taller than container -> scale to fit height
                {
                    resultHeight = availableHeight;
                    resultWidth = resultHeight * imageAspect;
                }

                // Apply requested dimensions IF they are smaller than the calculated fit size
                if (requestedWidth.HasValue) resultWidth = Math.Min(resultWidth, (float)requestedWidth.Value);
                if (requestedHeight.HasValue) resultHeight = Math.Min(resultHeight, (float)requestedHeight.Value);

                // Final clamp to ensure it doesn't exceed available (though AspectFit logic should already handle this)
                resultWidth = Math.Min(resultWidth, availableWidth);
                resultHeight = Math.Min(resultHeight, availableHeight);
                break;
        }

        // --- Alignment within available space ---
        // Default: Align top-left of the element's margin box
        float finalX = targetX;
        float finalY = targetY;

        // Optional: Centering (Uncomment if needed)
        // finalX = targetX + (availableWidth - resultWidth) / 2f;
        // finalY = targetY + (availableHeight - resultHeight) / 2f;

        // Ensure coordinates are valid before creating rect
        if (resultWidth < 0) resultWidth = 0;
        if (resultHeight < 0) resultHeight = 0;


        SKRect finalRect = SKRect.Create(finalX, finalY, resultWidth, resultHeight);

        return finalRect;
    }
}
