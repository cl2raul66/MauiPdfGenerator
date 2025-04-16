// --- START OF FILE MauiPdfGenerator/Core/Rendering/TextRenderer.cs ---
using SkiaSharp;
using MauiPdfGenerator.Common.Elements;
using MauiPdfGenerator.Common.Primitives;
using MauiPdfGenerator.Core.Utils;
using System;
using System.Collections.Generic; // For List
using System.Text; // For StringBuilder

namespace MauiPdfGenerator.Core.Rendering;

/// <summary>
/// Handles rendering of TextElementModel onto an SKCanvas.
/// Assumes element coordinates and sizes are already in points.
/// </summary>
internal static class TextRenderer
{
    // No longer needs UnitConverter for basic rendering
    public static void Render(SKCanvas canvas, TextElementModel element, PdfFontManager fontManager)
    {
        if (string.IsNullOrEmpty(element.Text)) return;

        var skColor = SkiaValueConverter.ToSKColor(element.Color);
        var skTypeface = fontManager.GetTypeface(element.Font);
        float fontSizeInPoints = element.Font.Size; // Assumed points

        using var paint = new SKPaint
        {
            Typeface = skTypeface,
            TextSize = fontSizeInPoints,
            Color = skColor,
            IsAntialias = true,
            SubpixelText = true
        };

        // Position and MaxWidth are already in points
        var positionInPoints = SkiaValueConverter.ToSKPoint(element.Position);
        float? maxWidthInPoints = element.MaxWidth; // Already points

        float x = positionInPoints.X;
        float y = positionInPoints.Y;

        // --- Alignment Adjustments ---
        float textWidth = paint.MeasureText(element.Text); // Simple measure for single line alignment

        // Horizontal Alignment (adjust starting X based on reference point)
        switch (element.HorizontalAlignment)
        {
            case PdfHorizontalAlignment.Center:
                x -= textWidth / 2f;
                paint.TextAlign = SKTextAlign.Left; // Draw from calculated left
                break;
            case PdfHorizontalAlignment.Right:
                x -= textWidth;
                paint.TextAlign = SKTextAlign.Left; // Draw from calculated left
                break;
            case PdfHorizontalAlignment.Left:
            default:
                paint.TextAlign = SKTextAlign.Left; // Draw starting at X
                break;
        }

        // Vertical Alignment (adjust Y to baseline based on reference point)
        SKFontMetrics fontMetrics = paint.FontMetrics;
        float baselineOffsetY = 0;
        switch (element.VerticalAlignment)
        {
            case PdfVerticalAlignment.Top:
                baselineOffsetY = -fontMetrics.Ascent;
                break;
            case PdfVerticalAlignment.Center:
                baselineOffsetY = (-fontMetrics.Ascent + fontMetrics.Descent) / 2f - fontMetrics.Descent;
                break;
            case PdfVerticalAlignment.Bottom:
                baselineOffsetY = -fontMetrics.Descent;
                break;
        }
        y += baselineOffsetY;

        // --- Rendering (with wrapping if needed) ---
        if (maxWidthInPoints.HasValue && maxWidthInPoints.Value > 0 && textWidth > maxWidthInPoints.Value)
        {
            var lines = WrapText(element.Text, paint, maxWidthInPoints.Value);
            float lineHeight = paint.FontSpacing;
            float currentY = y; // Start baseline for the first line

            foreach (var line in lines)
            {
                // Recalculate X for each line based on horizontal alignment
                float currentLineWidth = paint.MeasureText(line);
                float lineX = positionInPoints.X; // Start with original reference X
                switch (element.HorizontalAlignment)
                {
                    case PdfHorizontalAlignment.Center: lineX -= currentLineWidth / 2f; break;
                    case PdfHorizontalAlignment.Right: lineX -= currentLineWidth; break;
                    case PdfHorizontalAlignment.Left: default: break;
                }
                canvas.DrawText(line, lineX, currentY, paint);
                currentY += lineHeight; // Move baseline for the next line
            }
        }
        else
        {
            // Draw single line
            canvas.DrawText(element.Text, x, y, paint); // Use adjusted x, y (for alignment)
        }
    }

    private static List<string> WrapText(string text, SKPaint paint, float maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text)) return lines;

        int currentPos = 0;
        while (currentPos < text.Length)
        {
            // Measure how many characters fit
            long measuredCount = paint.BreakText(text.AsSpan(currentPos), maxWidth);

            // Ensure we make progress even if a single character is too wide
            if (measuredCount == 0 && currentPos < text.Length)
            {
                measuredCount = 1;
            }

            // Find a better break point (space) if possible within the measured count
            int breakPos = currentPos + (int)measuredCount;
            if (breakPos < text.Length)
            {
                int lastSpace = text.LastIndexOf(' ', breakPos - 1, (int)measuredCount);
                // Check if LastIndexOf found a space AND it's not the very beginning of the substring
                if (lastSpace != -1 && lastSpace > currentPos)
                {
                    breakPos = lastSpace + 1; // Break after the space
                    measuredCount = breakPos - currentPos; // Adjust measured count
                }
                // Handle case where the whole measured chunk has no spaces (long word)
                else if (paint.MeasureText(text.AsSpan(currentPos, (int)measuredCount)) > maxWidth)
                {
                    // If the chunk itself is too wide, find the last char that DOES fit
                    long fitCount = measuredCount;
                    while (fitCount > 1 && paint.MeasureText(text.AsSpan(currentPos, (int)fitCount)) > maxWidth)
                    {
                        fitCount--;
                    }
                    if (fitCount > 0)
                    {
                        measuredCount = fitCount;
                        breakPos = currentPos + (int)measuredCount;
                    } // else keep measuredCount=1 from above
                }
            }

            lines.Add(text.Substring(currentPos, (int)measuredCount).TrimEnd()); // Add the substring
            currentPos = breakPos; // Move position for next iteration

            // Skip leading spaces for the next line if we broke on a space
            while (currentPos < text.Length && text[currentPos] == ' ')
            {
                currentPos++;
            }
        }
        return lines;
    }
}
// --- END OF FILE MauiPdfGenerator/Core/Rendering/TextRenderer.cs ---
