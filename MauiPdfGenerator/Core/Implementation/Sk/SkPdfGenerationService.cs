using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkPdfGenerationService : IPdfGenerationService
{
    public Task GenerateAsync(PdfDocumentData documentData, string filePath)
    {
        try
        {
            var metadata = new SKDocumentPdfMetadata
            {
                Title = documentData.Title ?? string.Empty,
                Author = documentData.Author ?? string.Empty,
                Subject = documentData.Subject ?? string.Empty,
                Keywords = documentData.Keywords ?? string.Empty,
                Creator = documentData.Creator ?? string.Empty,
                Producer = documentData.Producer ?? "MauiPdfGenerator (SkiaSharp)",
                Creation = documentData.CreationDate ?? DateTime.Now,
                Modified = DateTime.Now,
                RasterDpi = 300,
                EncodingQuality = 100,
                PdfA = false
            };

            using var stream = new SKFileWStream(filePath);
            using var pdfDoc = SKDocument.CreatePdf(stream, metadata) ?? throw new PdfGenerationException("SkiaSharp failed to create the PDF document stream.");


            for (int pageIndex = 0; pageIndex < documentData.Pages.Count; pageIndex++)
            {
                var pageData = documentData.Pages[pageIndex];

                SKSize pageSize = GetSkPageSize(pageData.Size, pageData.Orientation);
                using SKCanvas canvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);


                if (pageData.BackgroundColor is not null)
                {
                    canvas.Clear(ConvertToSkColor(pageData.BackgroundColor));
                }
                else
                {
                    canvas.Clear(SKColors.White);
                }


                var margins = pageData.Margins;
                var contentRect = new SKRect(
                    (float)margins.Left,
                    (float)margins.Top,
                    pageSize.Width - (float)margins.Right,
                    pageSize.Height - (float)margins.Bottom
                );

                if (contentRect.Width <= 0 || contentRect.Height <= 0)
                {

                    System.Diagnostics.Debug.WriteLine($"Warning: Page margins result in zero or negative content area for page {pageIndex}. Skipping content rendering.");
                    pdfDoc.EndPage();
                    continue;
                }

                float currentY = contentRect.Top;

                foreach (var element in pageData.Elements)
                {
                    if (currentY >= contentRect.Bottom)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Content overflow detected before drawing element on page {pageIndex}. Subsequent elements skipped.");
                        break;
                    }

                    currentY += (float)element.GetMargin.Top;

                    if (currentY >= contentRect.Bottom)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Content overflow detected after applying top margin on page {pageIndex}. Element skipped.");
                        break; 
                    }


                    float elementHeight = 0;

                    switch (element)
                    {
                        case PdfParagraph p:
                            elementHeight = RenderParagraph(canvas, p, pageData, contentRect, currentY);
                            break;

                        case PdfHorizontalLine l:
                            elementHeight = RenderHorizontalLine(canvas, l, pageData, contentRect, currentY);
                            break;

                        default:
                            System.Diagnostics.Debug.WriteLine($"Warning: Unsupported element type '{element.GetType().Name}' encountered.");
                            break;
                    }

                    if (currentY + elementHeight > contentRect.Bottom)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Element {element.GetType().Name} caused content overflow on page {pageIndex}.");
                        currentY = contentRect.Bottom + 1;
                    }
                    else
                    {
                        currentY += elementHeight;
                        currentY += (float)element.GetMargin.Bottom;
                        currentY += pageData.PageDefaultSpacing;
                    }
                }

                pdfDoc.EndPage();
            }

            pdfDoc.Close();
            return Task.CompletedTask;
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            return Task.FromException(new PdfGenerationException($"Error generating PDF with SkiaSharp: {ex.Message}", ex));
        }
    }

    private float RenderParagraph(SKCanvas canvas, PdfParagraph paragraph, PdfPageData pageData, SKRect contentRect, float currentY)
    {
        string fontFamily = paragraph.CurrentFontFamily ?? pageData.PageDefaultFontFamily;
        float fontSize = paragraph.CurrentFontSize > 0 ? paragraph.CurrentFontSize : pageData.PageDefaultFontSize;
        Color textColor = paragraph.CurrentTextColor ?? pageData.PageDefaultTextColor;
        FontAttributes fontAttributes = paragraph.CurrentFontAttributes ?? pageData.PageDefaultFontAttributes;
        TextAlignment alignment = paragraph.CurrentAlignment;

        // --- Font and Paint Setup ---
        using var typeface = CreateSkTypeface(fontFamily, fontAttributes);
        if (typeface is null)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Could not create typeface for family '{fontFamily}'. Using default. Paragraph skipped.");
            return 0; 
        }

        if (fontSize <= 0)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Invalid font size {fontSize} for paragraph. Using default {PdfParagraph.DefaultFontSize}f.");
            fontSize = PdfParagraph.DefaultFontSize;
        }
        using var font = new SKFont(typeface, fontSize);

        using var paint = new SKPaint
        {
            Color = ConvertToSkColor(textColor),
            IsAntialias = true
        };

        // --- Text Measurement ---
        var text = paragraph.Text ?? string.Empty;
        SKRect textBounds = new(); 

        float measuredWidth = font.MeasureText(text, out textBounds);

        float availableWidth = contentRect.Width - (float)paragraph.GetMargin.Left - (float)paragraph.GetMargin.Right;
        availableWidth = Math.Max(0, availableWidth);

        float drawX = contentRect.Left + (float)paragraph.GetMargin.Left;

        if (alignment == TextAlignment.Center)
        {
            drawX = contentRect.Left + (float)paragraph.GetMargin.Left + (availableWidth - measuredWidth) / 2f;
        }
        else if (alignment == TextAlignment.End)
        {
            drawX = contentRect.Right - (float)paragraph.GetMargin.Right - measuredWidth;
        }

        drawX = Math.Max(contentRect.Left + (float)paragraph.GetMargin.Left, drawX);

        float drawY = currentY - textBounds.Top;


        // --- Text Wrapping (Basic Placeholder - Phase 2) ---
        if (measuredWidth > availableWidth && availableWidth > 0)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Text '{text[..Math.Min(text.Length, 20)]}...' exceeds available width ({availableWidth:F1}pt) and will be clipped or overflow. Measured: {measuredWidth:F1}pt.");
        }

        // --- Draw Text ---
        if (currentY < contentRect.Bottom)
        {
            canvas.DrawText(text, drawX, drawY, font, paint);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Skipping drawing paragraph '{text[..Math.Min(text.Length, 20)]}...' as currentY ({currentY:F1}) is already past contentRect.Bottom ({contentRect.Bottom:F1}).");
            return 0;
        }

        return textBounds.Height;
    }

    private float RenderHorizontalLine(SKCanvas canvas, PdfHorizontalLine line, PdfPageData pageData, SKRect contentRect, float currentY)
    {
        // --- Style Determination ---
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor;

        if (thickness <= 0)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Invalid horizontal line thickness {thickness}. Using default {PdfHorizontalLine.DefaultThickness}f.");
            thickness = PdfHorizontalLine.DefaultThickness;
        }


        // --- Paint Setup ---
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = ConvertToSkColor(color),
            StrokeWidth = thickness
        };

        // --- Calculate Coordinates ---
        float startX = contentRect.Left + (float)line.GetMargin.Left;
        float endX = contentRect.Right - (float)line.GetMargin.Right;
        float lineY = currentY + thickness / 2f;

        if (startX > endX)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Horizontal line margins result in negative length (Start: {startX:F1}, End: {endX:F1}). Line not drawn.");
            return thickness;
        }

        // --- Draw Line ---
        if (lineY < contentRect.Bottom)
        {
            canvas.DrawLine(startX, lineY, endX, lineY, paint);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Skipping drawing horizontal line as its position ({lineY:F1}) is past contentRect.Bottom ({contentRect.Bottom:F1}).");
        }

        return thickness;
    }

    // --- SkiaSharp Helper Methods ---

    private SKTypeface? CreateSkTypeface(string fontFamily, FontAttributes fontAttributes)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(fontFamily))
        {
            System.Diagnostics.Debug.WriteLine("Warning: CreateSkTypeface called with null or empty font family. Using default typeface.");
            return SKTypeface.Default;
        }


        SKFontStyleWeight weight = (fontAttributes & FontAttributes.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slant = (fontAttributes & FontAttributes.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        var style = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);

        // Attempt 1: Get specific style
        SKTypeface? typeface = SKTypeface.FromFamilyName(fontFamily, style);

        // Attempt 2: Fallback to family name default style if specific style failed
        // (e.g., requested Italic for a font that doesn't have it)
        if (typeface is null)
        {
            System.Diagnostics.Debug.WriteLine($"Debug: Specific style ({style}) not found for font '{fontFamily}'. Trying family default.");
            typeface = SKTypeface.FromFamilyName(fontFamily);
        }


        // Attempt 3: Fallback to system default if family name itself wasn't found
        if (typeface is null)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Font family '{fontFamily}' not found. Using system default typeface.");
            typeface = SKTypeface.Default;
        }

        // If SKTypeface.Default itself fails (very unlikely), typeface could still be null here.
        if (typeface is null)
        {
            System.Diagnostics.Debug.WriteLine($"Critical Warning: Failed to obtain any valid typeface, including system default.");
        }


        return typeface;
    }

    private SKColor ConvertToSkColor(Color? mauiColor)
    {
        // Default to black if null is passed
        if (mauiColor is null) return SKColors.Black;

        // Clamp values just in case they are outside the 0-1 range
        float red = Math.Clamp(mauiColor.Red, 0f, 1f);
        float green = Math.Clamp(mauiColor.Green, 0f, 1f);
        float blue = Math.Clamp(mauiColor.Blue, 0f, 1f);
        float alpha = Math.Clamp(mauiColor.Alpha, 0f, 1f);


        return new SKColor(
            (byte)(red * 255),
            (byte)(green * 255),
            (byte)(blue * 255),
            (byte)(alpha * 255)
        );
    }

    private SKSize GetSkPageSize(PageSizeType size, PageOrientationType orientation)
    {
        float width, height;
        switch (size)
        {
            // ISO Sizes
            case PageSizeType.A3: width = 841.89f; height = 1190.55f; break;
            case PageSizeType.A4: width = 595.28f; height = 841.89f; break;
            case PageSizeType.A5: width = 419.53f; height = 595.28f; break;
            case PageSizeType.B5: width = 498.90f; height = 708.66f; break; // ISO B5

            // North American Sizes
            case PageSizeType.Letter: width = 612f; height = 792f; break; // 8.5 x 11 in
            case PageSizeType.Legal: width = 612f; height = 1008f; break; // 8.5 x 14 in
            case PageSizeType.Tabloid: width = 792f; height = 1224f; break; // 11 x 17 in
            case PageSizeType.Executive: width = 522f; height = 756f; break; // 7.25 x 10.5 in

            // Envelopes (Width/Height based on Portrait orientation)
            case PageSizeType.Envelope_10: width = 297f; height = 684f; break;  // 4.125 x 9.5 in
            case PageSizeType.Envelope_DL: width = 311.81f; height = 623.62f; break; // 110 x 220 mm

            default:
                System.Diagnostics.Debug.WriteLine($"Warning: Unsupported PageSizeType '{size}'. Defaulting to A4.");
                width = 595.28f; height = 841.89f; // Default to A4
                break;
        }

        return orientation == PageOrientationType.Landscape ? new SKSize(height, width) : new SKSize(width, height);
    }
}