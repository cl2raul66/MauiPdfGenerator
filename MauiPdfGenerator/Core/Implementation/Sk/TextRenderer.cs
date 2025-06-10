using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;
using System.Diagnostics;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class TextRenderer
{
    private const string Ellipsis = "...";

    public async Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfParagraph paragraph, PdfPageData pageDefinition, SKRect currentPageContentRect, float currentYOnPage, PdfFontRegistryBuilder? fontRegistry)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(paragraph);
        ArgumentNullException.ThrowIfNull(pageDefinition);
        ArgumentNullException.ThrowIfNull(fontRegistry);

        PdfFontIdentifier? fontIdentifierToUse = paragraph.CurrentFontFamily
                                               ?? pageDefinition.PageDefaultFontFamily
                                               ?? fontRegistry.GetUserConfiguredDefaultFontIdentifier()
                                               ?? fontRegistry.GetFirstMauiRegisteredFontIdentifier();


        float fontSize = paragraph.CurrentFontSize > 0 ? paragraph.CurrentFontSize : pageDefinition.PageDefaultFontSize;
        Color textColor = paragraph.CurrentTextColor ?? pageDefinition.PageDefaultTextColor;
        FontAttributes fontAttributes = paragraph.CurrentFontAttributes ?? pageDefinition.PageDefaultFontAttributes;
        TextAlignment horizontalAlignment = paragraph.CurrentHorizontalAlignment;
        LineBreakMode lineBreakMode = paragraph.CurrentLineBreakMode ?? PdfParagraph.DefaultLineBreakMode;
        TextDecorations textDecorations = paragraph.CurrentTextDecorations ?? pageDefinition.PageDefaultTextDecorations;
        TextTransform textTransform = paragraph.CurrentTextTransform ?? pageDefinition.PageDefaultTextTransform;

        FontRegistration? fontRegistration = paragraph.ResolvedFontRegistration;
        if (fontRegistration is null && fontIdentifierToUse.HasValue)
        {
            fontRegistration = fontRegistry.GetFontRegistration(fontIdentifierToUse.Value);
        }

        string? filePathToLoad = null;
        if (fontRegistration is not null && !string.IsNullOrEmpty(fontRegistration.FilePath))
        {
            filePathToLoad = fontRegistration.FilePath;
        }

        string skiaFontAliasToAttempt = fontIdentifierToUse?.Alias ?? string.Empty;

        using var typeface = await SkiaUtils.CreateSkTypefaceAsync(
            skiaFontAliasToAttempt,
            fontAttributes,
            async (fileName) =>
            {
                if (string.IsNullOrEmpty(fileName)) return null;
                try
                {
                    return await FileSystem.OpenAppPackageFileAsync(fileName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TextRenderer] StreamProvider Error for {fileName}: {ex.Message}");
                    return null;
                }
            },
            filePathToLoad
        );

        if (fontSize <= 0) fontSize = PdfParagraph.DefaultFontSize;

        using var font = new SKFont(typeface, fontSize);
        using var paint = new SKPaint
        {
            Color = SkiaUtils.ConvertToSkColor(textColor),
            IsAntialias = true
        };

        string originalText = paragraph.Text ?? string.Empty;
        string textToRender = textTransform switch
        {
            TextTransform.Uppercase => originalText.ToUpperInvariant(),
            TextTransform.Lowercase => originalText.ToLowerInvariant(),
            _ => originalText,
        };

        float availableWidthForTextLayout = currentPageContentRect.Width - (float)paragraph.GetMargin.Left - (float)paragraph.GetMargin.Right;
        availableWidthForTextLayout = Math.Max(0, availableWidthForTextLayout);
        float elementContentLeftX = currentPageContentRect.Left + (float)paragraph.GetMargin.Left;
        float elementContentRightX = currentPageContentRect.Right - (float)paragraph.GetMargin.Right;
        float availableHeightForDrawing = currentPageContentRect.Bottom - currentYOnPage;

        List<string> allLines = BreakTextIntoLines(textToRender, font, availableWidthForTextLayout, lineBreakMode);
        if (allLines.Count == 0) return new RenderOutput(0, 0, null, false);

        float fontLineSpacing = font.Spacing;
        if (fontLineSpacing <= 0) fontLineSpacing = fontSize * 1.2f;

        int linesThatFit = 0;
        if (availableHeightForDrawing > 0 && fontLineSpacing > 0)
        {
            if (lineBreakMode is LineBreakMode.NoWrap or LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation)
            {
                linesThatFit = (availableHeightForDrawing >= fontLineSpacing && allLines.Count != 0) ? 1 : 0;
            }
            else
            {
                linesThatFit = (int)Math.Floor(availableHeightForDrawing / fontLineSpacing);
                linesThatFit = Math.Max(0, Math.Min(linesThatFit, allLines.Count));
            }
        }

        List<string> linesToDrawThisCall = [.. allLines.Take(linesThatFit)];
        float widthDrawnThisCall = 0;
        float lineY = currentYOnPage;
        int linesDrawnCount = 0;

        foreach (string line in linesToDrawThisCall)
        {
            if (lineY + fontLineSpacing > currentPageContentRect.Bottom + 0.01f) break;

            SKRect lineBounds = new();
            float measuredWidth = font.MeasureText(line, out lineBounds);
            widthDrawnThisCall = Math.Max(widthDrawnThisCall, measuredWidth);
            float drawX = elementContentLeftX;

            if (horizontalAlignment is TextAlignment.Center) drawX = elementContentLeftX + (availableWidthForTextLayout - measuredWidth) / 2f;
            else if (horizontalAlignment is TextAlignment.End) drawX = elementContentRightX - measuredWidth;

            drawX = Math.Max(elementContentLeftX, Math.Min(drawX, elementContentRightX - measuredWidth));

            float textDrawY = lineY - lineBounds.Top;

            canvas.Save();

            canvas.ClipRect(SKRect.Create(elementContentLeftX, currentYOnPage, availableWidthForTextLayout, availableHeightForDrawing));
            canvas.DrawText(line, drawX, textDrawY, font, paint);

            if (textDecorations is not TextDecorations.None)
            {
                float decorationThickness = Math.Max(1f, fontSize / 12f);
                using var decorationPaint = new SKPaint
                {
                    Color = paint.Color,
                    StrokeWidth = decorationThickness,
                    IsAntialias = true
                };

                SKFontMetrics fontMetrics = font.Metrics;

                if ((textDecorations & TextDecorations.Underline) != 0)
                {
                    float underlineY = textDrawY + (fontMetrics.UnderlinePosition ?? (decorationThickness * 2));
                    if (fontMetrics.UnderlineThickness.HasValue && fontMetrics.UnderlineThickness.Value > 0)
                    {
                        decorationPaint.StrokeWidth = fontMetrics.UnderlineThickness.Value;
                    }
                    canvas.DrawLine(drawX, underlineY, drawX + measuredWidth, underlineY, decorationPaint);
                }
                if ((textDecorations & TextDecorations.Strikethrough) != 0)
                {
                    float strikeY = textDrawY + (fontMetrics.StrikeoutPosition ?? (-fontMetrics.XHeight / 2f));
                    if (fontMetrics.StrikeoutThickness.HasValue && fontMetrics.StrikeoutThickness.Value > 0)
                    {
                        decorationPaint.StrokeWidth = fontMetrics.StrikeoutThickness.Value;
                    }
                    canvas.DrawLine(drawX, strikeY, drawX + measuredWidth, strikeY, decorationPaint);
                }
            }
            canvas.Restore();

            lineY += fontLineSpacing;
            linesDrawnCount++;
        }

        // --- START OF FINAL CHANGE ---
        // Calculate both layout height and visual height.
        float heightDrawnThisCall = linesDrawnCount * fontLineSpacing;
        float visualHeightDrawn = 0;

        if (linesDrawnCount > 0)
        {
            SKFontMetrics fontMetrics = font.Metrics;
            // The visual height of a single line of text (from the very top to the very bottom of the ink).
            float visualLineHeight = fontMetrics.Descent - fontMetrics.Ascent;

            // The total visual height is the space taken by (n-1) full line spacings,
            // plus the visual height of the font for the final line.
            visualHeightDrawn = (linesDrawnCount - 1) * fontLineSpacing + visualLineHeight;
        }
        // --- END OF FINAL CHANGE ---

        PdfParagraph? remainingParagraph = null;
        bool requiresNewPage = false;
        List<string> remainingLinesList = [.. allLines.Skip(linesThatFit)];
        if (remainingLinesList.Count != 0)
        {
            string remainingOriginalText = string.Join("\n",
                (paragraph.Text ?? string.Empty).Split('\n').Skip(allLines.Count - remainingLinesList.Count)
            );

            remainingParagraph = new PdfParagraph(remainingOriginalText, paragraph);
            requiresNewPage = true;
        }
        else if (linesThatFit == 0 && allLines.Count != 0 && !paragraph.IsContinuation)
        {
            remainingParagraph = paragraph;
            requiresNewPage = true;
        }

        return new RenderOutput(heightDrawnThisCall, widthDrawnThisCall, remainingParagraph, requiresNewPage, visualHeightDrawn);
    }

    // ... (El resto del fichero permanece sin cambios)
    private List<string> BreakTextIntoLines(string text, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text)) return lines;

        string[] textSegments = text.Split(['\n']);
        foreach (string segment in textSegments)
        {
            if (maxWidth <= 0 || lineBreakMode is LineBreakMode.NoWrap)
            {
                lines.Add(segment);
                continue;
            }

            if (lineBreakMode is LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation)
            {
                float segmentWidth = font.MeasureText(segment);
                if (segmentWidth <= maxWidth) lines.Add(segment);
                else lines.Add(TruncateSingleLine(segment, font, maxWidth, lineBreakMode));
            }
            else
            {
                lines.AddRange(WrapSingleLineLogic(segment, font, maxWidth, lineBreakMode));
            }
        }
        return lines;
    }

    private string TruncateSingleLine(string textSegment, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
    {
        float ellipsisWidth = font.MeasureText(Ellipsis);
        if (maxWidth < ellipsisWidth && maxWidth > 0) return Ellipsis.Substring(0, (int)font.BreakText(Ellipsis, maxWidth));
        if (maxWidth <= 0) return string.Empty;

        float availableWidthForText = maxWidth - ellipsisWidth;
        if (availableWidthForText < 0) availableWidthForText = 0;

        if (lineBreakMode is LineBreakMode.TailTruncation)
        {
            long count = font.BreakText(textSegment, availableWidthForText);
            return textSegment[..(int)Math.Max(0, count)] + Ellipsis;
        }
        if (lineBreakMode is LineBreakMode.HeadTruncation)
        {
            int textLength = textSegment.Length;
            int startIndex = textLength;
            for (int i = 1; i <= textLength; i++)
            {
                string sub = textSegment[^i..];
                if (font.MeasureText(sub) <= availableWidthForText)
                {
                    startIndex = textLength - i;
                }
                else
                {
                    break;
                }
            }
            return (startIndex == textLength && textLength > 0 && ellipsisWidth > 0) ? Ellipsis : Ellipsis + textSegment[startIndex..];
        }
        if (lineBreakMode is LineBreakMode.MiddleTruncation)
        {
            if (availableWidthForText <= 0 && ellipsisWidth > 0) return Ellipsis;
            if (availableWidthForText <= 0 && ellipsisWidth <= 0) return string.Empty;


            float startWidth = availableWidthForText / 2f;
            long startCount = font.BreakText(textSegment, startWidth);

            int textLength = textSegment.Length;
            string tempEndString = "";
            for (int i = 1; i <= textLength - (int)startCount; i++)
            {
                string sub = textSegment[^i..];
                if (font.MeasureText(textSegment[..(int)startCount] + Ellipsis + sub) <= maxWidth)
                {
                    tempEndString = sub;
                }
                else
                {
                    break;
                }
            }

            if ((int)startCount == 0 && tempEndString.Length == 0 && textLength > 0 && ellipsisWidth > 0)
            {
                return TruncateSingleLine(textSegment, font, maxWidth, LineBreakMode.TailTruncation);
            }
            if ((int)startCount >= (textLength - tempEndString.Length) && textLength > 0 && tempEndString.Length == 0 && ellipsisWidth > 0)
            {
                long countTail = font.BreakText(textSegment, availableWidthForText);
                return textSegment[..(int)Math.Max(0, countTail)] + Ellipsis;
            }
            return textSegment[..(int)startCount] + Ellipsis + tempEndString;
        }
        return textSegment;
    }

    private IEnumerable<string> WrapSingleLineLogic(string singleLine, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
    {
        var resultingLines = new List<string>();
        if (string.IsNullOrEmpty(singleLine)) { resultingLines.Add(string.Empty); return resultingLines; }

        float singleLineWidth = font.MeasureText(singleLine);
        if (singleLineWidth <= maxWidth) { resultingLines.Add(singleLine); return resultingLines; }

        int currentPosition = 0;
        int textLength = singleLine.Length;
        while (currentPosition < textLength)
        {
            long countMeasured = font.BreakText(singleLine.AsSpan(currentPosition), maxWidth);
            int count = (int)countMeasured;

            if (count == 0 && currentPosition < textLength)
            {
                if (font.MeasureText(singleLine[currentPosition].ToString()) > maxWidth)
                {
                    Debug.WriteLine($"Warning: Single character '{singleLine[currentPosition]}' is wider than maxWidth ({maxWidth}). Outputting empty line for this segment to avoid issues, or consider character wrap if desired.");
                    resultingLines.Add(singleLine.Substring(currentPosition, 1));
                    currentPosition += 1;
                    continue;
                }
                Debug.WriteLine($"Warning: Text break count is 0 for non-empty segment starting at {currentPosition}. Forcing break after 1 character to prevent infinite loop. MaxWidth: {maxWidth}, Character: '{singleLine[currentPosition]}', Width: {font.MeasureText(singleLine[currentPosition].ToString())}");
                count = 1;
            }

            int breakPositionAttempt = currentPosition + count;

            if (breakPositionAttempt >= textLength)
            {
                resultingLines.Add(singleLine[currentPosition..]);
                break;
            }

            int actualBreakPosition = breakPositionAttempt;

            if (lineBreakMode is LineBreakMode.WordWrap)
            {
                string segmentToConsider = singleLine.Substring(currentPosition, count);
                int lastValidBreakInSegment = -1;
                for (int k = segmentToConsider.Length - 1; k >= 0; --k)
                {
                    if (char.IsWhiteSpace(segmentToConsider[k]) || segmentToConsider[k] == '-')
                    {
                        lastValidBreakInSegment = k;
                        break;
                    }
                }

                if (lastValidBreakInSegment > 0 && (currentPosition + lastValidBreakInSegment + 1) > currentPosition)
                {
                    actualBreakPosition = currentPosition + lastValidBreakInSegment + 1;
                }
                else
                {
                    actualBreakPosition = currentPosition + count;
                }
            }

            if (actualBreakPosition <= currentPosition && currentPosition < textLength)
            {
                actualBreakPosition = currentPosition + 1;
            }

            string lineToAdd = singleLine[currentPosition..actualBreakPosition].TrimEnd();
            resultingLines.Add(lineToAdd);
            currentPosition = actualBreakPosition;

            while (currentPosition < textLength && char.IsWhiteSpace(singleLine[currentPosition]))
            {
                currentPosition++;
            }
        }
        return resultingLines;
    }
}
