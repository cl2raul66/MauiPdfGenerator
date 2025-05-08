using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class TextRenderer
{
    private const string Ellipsis = "...";

    public float Render(SKCanvas canvas, PdfParagraph paragraph, PdfPageData pageData, SKRect contentRect, float currentY)
    {
        // --- Resolve Styles ---
        string fontFamily = paragraph.CurrentFontFamily ?? pageData.PageDefaultFontFamily;
        float fontSize = paragraph.CurrentFontSize > 0 ? paragraph.CurrentFontSize : pageData.PageDefaultFontSize;
        Color textColor = paragraph.CurrentTextColor ?? pageData.PageDefaultTextColor;
        FontAttributes fontAttributes = paragraph.CurrentFontAttributes ?? pageData.PageDefaultFontAttributes;
        TextAlignment alignment = paragraph.CurrentAlignment;
        LineBreakMode lineBreakMode = paragraph.CurrentLineBreakMode ?? PdfParagraph.DefaultLineBreakMode;

        // --- Font and Paint Setup ---
        using var typeface = SkiaUtils.CreateSkTypeface(fontFamily, fontAttributes);
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
            Color = SkiaUtils.ConvertToSkColor(textColor),
            IsAntialias = true
        };

        // --- Measurement and Layout ---
        var text = paragraph.Text ?? string.Empty;
        float availableWidth = contentRect.Width - (float)paragraph.GetMargin.Left - (float)paragraph.GetMargin.Right;
        availableWidth = Math.Max(0, availableWidth); 
        float elementLeftX = contentRect.Left + (float)paragraph.GetMargin.Left;
        float elementRightX = contentRect.Right - (float)paragraph.GetMargin.Right;

        List<string> lines = BreakTextIntoLines(text, font, availableWidth, lineBreakMode);

        float totalHeight = 0;
        float fontSpacing = font.Spacing; 

        // --- Drawing ---
        float lineY = currentY; 

        foreach (string line in lines)
        {
            if (lineY + fontSpacing > contentRect.Bottom + 0.1f) 
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Text overflow before drawing line '{line[..Math.Min(line.Length, 20)]}...' on page {pageData}. Remaining lines skipped.");
                break; 
            }

            SKRect lineBounds = new();
            float measuredWidth = font.MeasureText(line, out lineBounds);

            float drawX = elementLeftX; 

            if (alignment is TextAlignment.Center)
            {
                drawX = elementLeftX + (availableWidth - measuredWidth) / 2f;
            }
            else if (alignment is TextAlignment.End)
            {
                drawX = elementRightX - measuredWidth;
            } 

            drawX = Math.Max(elementLeftX, drawX);

            float drawY = lineY - lineBounds.Top;

            // --- Clipping y Dibujo ---
            canvas.Save();

            SKRect clipRect = new(elementLeftX, lineY, elementRightX, contentRect.Bottom);
            canvas.ClipRect(clipRect);

            canvas.DrawText(line, drawX, drawY, font, paint);

            canvas.Restore();

            // --- Advance Y Position ---
            float lineHeight = fontSpacing; 
            totalHeight += lineHeight;
            lineY += lineHeight;

            if (lineBreakMode is LineBreakMode.NoWrap or LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation)
            {
                break;
            }
        } 

        return totalHeight; 
    }

    private List<string> BreakTextIntoLines(string text, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            return lines; 
        }

        // --- Manejo de NoWrap ---
        if (lineBreakMode is LineBreakMode.NoWrap)
        {
            lines.Add(text); 
            return lines;
        }

        // --- Medición inicial y casos simples ---
        float fullTextWidth = font.MeasureText(text);

        if (fullTextWidth <= maxWidth || maxWidth <= 0)
        {
            lines.Add(text);
            return lines;
        }

        // --- Lógica de Truncamiento ---
        float ellipsisWidth = font.MeasureText(Ellipsis);

        if (maxWidth < ellipsisWidth) 
        {
            lines.Add(Ellipsis); 
            return lines;
        }

        // --- Tail Truncation ---
        if (lineBreakMode is LineBreakMode.TailTruncation)
        {
            float availableWidthForText = maxWidth - ellipsisWidth;
            long count = font.BreakText(text, availableWidthForText, out _); 
            lines.Add(text[..(int)count] + Ellipsis);
            return lines;
        }

        // --- Head Truncation ---
        if (lineBreakMode is LineBreakMode.HeadTruncation)
        {
            float availableWidthForText = maxWidth - ellipsisWidth;
            int textLengthFroHead = text.Length;
            int startIndex = textLengthFroHead; 

            for (int i = 1; i <= textLengthFroHead; i++)
            {
                string sub = text[^i..];
                if (font.MeasureText(sub) <= availableWidthForText)
                {
                    startIndex = textLengthFroHead - i; 
                }
                else
                {
                    break; 
                }
            }
            lines.Add(Ellipsis + text[startIndex..]);
            return lines;
        }

        // --- Middle Truncation ---
        if (lineBreakMode is LineBreakMode.MiddleTruncation)
        {
            float availableWidthForText = maxWidth - ellipsisWidth;
            float startWidth = availableWidthForText / 2f;
            float endWidth = availableWidthForText - startWidth; 

            long startCount = font.BreakText(text, startWidth, out _);

            int textLengthForMiddle = text.Length;
            int endIndex = textLengthForMiddle; 

            for (int i = 1; i <= textLengthForMiddle; i++)
            {
                string sub = text[^i..];
                if (font.MeasureText(sub) <= endWidth)
                {
                    endIndex = textLengthForMiddle - i; 
                }
                else
                {
                    break;
                }
            }

            if ((int)startCount >= endIndex)
            {
                long countTail = font.BreakText(text, maxWidth - ellipsisWidth, out _);
                lines.Add(text[..(int)countTail] + Ellipsis);
            }
            else
            {
                lines.Add(text[..(int)startCount] + Ellipsis + text[endIndex..]);
            }
            return lines;
        }


        // --- Lógica de Wrapping (WordWrap, CharacterWrap, o fallback) ---
        int currentPosition = 0;
        int textLength = text.Length;

        while (currentPosition < textLength)
        {
            long countMeasured = font.BreakText(text.AsSpan(currentPosition), maxWidth, out _);
            int count = (int)Math.Max(0, Math.Min(countMeasured, textLength - currentPosition));

            if (count == 0 && currentPosition < textLength)
            {
                count = 1;
            }

            if (currentPosition + count >= textLength)
            {
                lines.Add(text[currentPosition..]);
                break; 
            }

            int breakPosition = currentPosition + count;

            if (lineBreakMode is LineBreakMode.WordWrap)
            {
                int lastBreakChar = text.LastIndexOfAny([' ', '\t', '-'], breakPosition - 1, count);

                if (lastBreakChar > currentPosition)
                {
                    breakPosition = lastBreakChar + 1; 
                }
            }

            if (breakPosition <= currentPosition && currentPosition < textLength)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Break position calculation stalled at index {currentPosition}. Forcing break after 1 char.");
                breakPosition = currentPosition + 1;
            }

            string line = text[currentPosition..breakPosition].TrimEnd(' ', '\t');
            lines.Add(line);

            currentPosition = breakPosition;
            while (currentPosition < textLength && (text[currentPosition] == ' ' || text[currentPosition] == '\t'))
            {
                currentPosition++;
            }
        }

        return lines;
    }
}
