using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class TextRenderer
{
    private const string Ellipsis = "...";

    // Corrected Signature: Removed bool isContinuation
    public RenderOutput Render(SKCanvas canvas, PdfParagraph paragraph, PdfPageData pageDefinition, SKRect currentPageContentRect, float currentYOnPage)
    {
        // --- Resolve Styles ---
        string fontFamily = paragraph.CurrentFontFamily ?? pageDefinition.PageDefaultFontFamily;
        float fontSize = paragraph.CurrentFontSize > 0 ? paragraph.CurrentFontSize : pageDefinition.PageDefaultFontSize;
        Color textColor = paragraph.CurrentTextColor ?? pageDefinition.PageDefaultTextColor;
        FontAttributes fontAttributes = paragraph.CurrentFontAttributes ?? pageDefinition.PageDefaultFontAttributes;
        TextAlignment alignment = paragraph.CurrentAlignment;
        LineBreakMode lineBreakMode = paragraph.CurrentLineBreakMode ?? PdfParagraph.DefaultLineBreakMode;

        // --- Font and Paint Setup ---
        using var typeface = SkiaUtils.CreateSkTypeface(fontFamily, fontAttributes);
        if (typeface is null)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Could not create typeface for family '{fontFamily}'. Using default. Paragraph skipped.");
            return new RenderOutput(0, null, false);
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
        float availableWidthForTextLayout = currentPageContentRect.Width - (float)paragraph.GetMargin.Left - (float)paragraph.GetMargin.Right;
        availableWidthForTextLayout = Math.Max(0, availableWidthForTextLayout);

        float elementContentLeftX = currentPageContentRect.Left + (float)paragraph.GetMargin.Left;
        float elementContentRightX = currentPageContentRect.Right - (float)paragraph.GetMargin.Right;

        float availableHeightForDrawing = currentPageContentRect.Bottom - currentYOnPage;

        List<string> allLines = BreakTextIntoLines(text, font, availableWidthForTextLayout, lineBreakMode);
        if (!allLines.Any())
        {
            return new RenderOutput(0, null, false);
        }

        float fontLineSpacing = font.Spacing;
        if (fontLineSpacing <= 0) fontLineSpacing = fontSize; // Basic fallback for spacing

        int linesThatFit = 0;
        if (availableHeightForDrawing > 0 && fontLineSpacing > 0)
        {
            // For truncation modes, only 1 line is ever considered for fitting.
            if (lineBreakMode == LineBreakMode.NoWrap ||
                lineBreakMode == LineBreakMode.HeadTruncation ||
                lineBreakMode == LineBreakMode.MiddleTruncation ||
                lineBreakMode == LineBreakMode.TailTruncation)
            {
                linesThatFit = (availableHeightForDrawing >= fontLineSpacing && allLines.Any()) ? 1 : 0;
            }
            else // WordWrap, CharacterWrap
            {
                linesThatFit = (int)Math.Floor(availableHeightForDrawing / fontLineSpacing);
                linesThatFit = Math.Max(0, Math.Min(linesThatFit, allLines.Count));
            }
        }


        List<string> linesToDrawThisCall = allLines.Take(linesThatFit).ToList();
        float heightDrawnThisCall = 0;
        float lineY = currentYOnPage;

        // --- Drawing ---
        foreach (string line in linesToDrawThisCall)
        {
            // Double check, though linesThatFit should prevent this
            if (lineY + fontLineSpacing > currentPageContentRect.Bottom + 0.1f)
            {
                break;
            }

            SKRect lineBounds = new(); // For vertical alignment (ascent/descent)
            float measuredWidth = font.MeasureText(line, out lineBounds);

            float drawX = elementContentLeftX;
            if (alignment == TextAlignment.Center)
            {
                drawX = elementContentLeftX + (availableWidthForTextLayout - measuredWidth) / 2f;
            }
            else if (alignment == TextAlignment.End)
            {
                drawX = elementContentRightX - measuredWidth;
            }
            drawX = Math.Max(elementContentLeftX, Math.Min(drawX, elementContentRightX - measuredWidth)); // Clamp to content area

            // Adjust Y for text baseline
            float drawY = lineY - lineBounds.Top;

            canvas.Save();
            // Clip to the element's designated content area horizontally, and page bottom vertically
            SKRect clipRect = SKRect.Create(elementContentLeftX, currentYOnPage, availableWidthForTextLayout, availableHeightForDrawing);
            canvas.ClipRect(clipRect);

            canvas.DrawText(line, drawX, drawY, font, paint);
            canvas.Restore();

            heightDrawnThisCall += fontLineSpacing;
            lineY += fontLineSpacing;
        }

        PdfParagraph? remainingParagraph = null;
        bool requiresNewPage = false;

        List<string> remainingLinesList = allLines.Skip(linesThatFit).ToList();
        if (remainingLinesList.Any())
        {
            string remainingText = string.Join("\n", remainingLinesList);
            remainingParagraph = new PdfParagraph(remainingText, paragraph); // Creates a continuation paragraph
            requiresNewPage = true;
        }
        // This condition checks if NO lines fit, but there WAS text, and the paragraph is NOT ALREADY a continuation.
        // This means an original paragraph didn't fit at all and needs to be moved entirely.
        else if (linesThatFit == 0 && allLines.Any() && !paragraph.IsContinuation)
        {
            // The original paragraph itself is the "remainder".
            // We create a "continuation" version of it to ensure its top margin isn't re-applied if it's just being moved.
            // If it was truly split, the new PdfParagraph(remainingText, paragraph) handles this.
            // This case is for: "Paragraph P1, doesn't fit current page at all. Move P1 to new page."
            // On the new page, it should behave like a fresh element regarding its top margin *unless* it was a prior continuation.
            // The current logic in SkPdfGenerationService for isElementIntrinsicallyAContinuation handles margins.
            // So, we just need to ensure the original paragraph is returned.
            remainingParagraph = paragraph; // Pass the original paragraph to be retried.
                                            // SkPdfGenerationService will check its `IsContinuation` property.
                                            // If it's already a continuation that didn't fit, its IsContinuation is true.
                                            // If it's an original that didn't fit, IsContinuation is false.
            requiresNewPage = true;
        }


        return new RenderOutput(heightDrawnThisCall, remainingParagraph, requiresNewPage);
    }

    private List<string> BreakTextIntoLines(string text, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text)) // No text, no lines.
        {
            return lines;
        }
        // If no width, or NoWrap, or non-wrapping truncation, effectively one line for breaking logic,
        // but still respect explicit \n
        if (maxWidth <= 0 || lineBreakMode == LineBreakMode.NoWrap)
        {
            return text.Split('\n').ToList(); // Respect explicit newlines even for NoWrap
        }

        float fullTextWidth = font.MeasureText(text); // Measure the longest segment if text has \n

        // For truncation modes, they always result in a single output line (potentially with ellipsis)
        if (lineBreakMode is LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation)
        {
            if (fullTextWidth <= maxWidth)
            {
                lines.Add(text); // Fits, no truncation needed
                return lines;
            }

            float ellipsisWidth = font.MeasureText(Ellipsis);
            if (maxWidth < ellipsisWidth)
            {
                lines.Add(Ellipsis);
                return lines;
            }

            if (lineBreakMode is LineBreakMode.TailTruncation)
            {
                float availableWidthForText = maxWidth - ellipsisWidth;
                long count = font.BreakText(text, availableWidthForText, out _);
                lines.Add(text[..(int)Math.Max(0, count)] + Ellipsis);
                return lines;
            }

            if (lineBreakMode is LineBreakMode.HeadTruncation)
            {
                float availableWidthForText = maxWidth - ellipsisWidth;
                int textLength = text.Length;
                int startIndex = textLength; // Start by assuming no part of the end fits

                for (int i = 1; i <= textLength; i++)
                {
                    string sub = text[^i..]; // Text from end
                    if (font.MeasureText(sub) <= availableWidthForText)
                    {
                        startIndex = textLength - i; // This part fits
                    }
                    else
                    {
                        break; // Previous length was the max that fit
                    }
                }
                // If startIndex is still textLength, it means not even one char from end fits with ellipsis
                if (startIndex == textLength && textLength > 0)
                { // take nothing from end
                    lines.Add(Ellipsis); // Or some other minimal representation
                }
                else
                {
                    lines.Add(Ellipsis + text[startIndex..]);
                }
                return lines;
            }

            if (lineBreakMode is LineBreakMode.MiddleTruncation)
            {
                float availableWidthForText = maxWidth - ellipsisWidth;
                if (availableWidthForText <= 0)
                {
                    lines.Add(Ellipsis);
                    return lines;
                }
                float startWidth = availableWidthForText / 2f;
                // float endWidth = availableWidthForText - startWidth; // Not strictly needed with this logic

                long startCount = font.BreakText(text, startWidth, out _);

                int textLength = text.Length;
                int endIndex = (int)startCount;

                // Iterate from the end of the string to find how much fits in the second half
                string tempEndString = "";
                for (int i = 1; i <= textLength - (int)startCount; i++) // only consider chars after startCount
                {
                    string sub = text[^(i)..]; // Chars from the very end
                    if (font.MeasureText(text[..(int)startCount] + Ellipsis + sub) <= maxWidth)
                    {
                        tempEndString = sub; // This combination fits
                    }
                    else
                    {
                        break; // Adding this char made it too long
                    }
                }
                endIndex = textLength - tempEndString.Length;


                if ((int)startCount >= endIndex && textLength > 0) // Overlap or no space for end part meaningfully
                {
                    // Fallback to tail truncation logic
                    long countTail = font.BreakText(text, maxWidth - ellipsisWidth, out _);
                    lines.Add(text[..(int)Math.Max(0, countTail)] + Ellipsis);
                }
                else if (textLength == 0)
                {
                    lines.Add(string.Empty);
                }
                else
                {
                    lines.Add(text[..(int)startCount] + Ellipsis + text[endIndex..]);
                }
                return lines;
            }
        }

        // --- Lógica de Wrapping (WordWrap, CharacterWrap) para texto que excede maxWidth ---
        // Handles explicit \n characters first by splitting, then breaking each segment.
        var preSplitLines = text.Split('\n');
        foreach (var singleOrLongerLine in preSplitLines)
        {
            lines.AddRange(BreakSingleLineLogic(singleOrLongerLine, font, maxWidth, lineBreakMode));
        }

        return lines;
    }

    // Helper for WordWrap and CharacterWrap on a single line of text (no \n within)
    private IEnumerable<string> BreakSingleLineLogic(string singleLine, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
    {
        var resultingLines = new List<string>();
        if (string.IsNullOrEmpty(singleLine)) // Preserve empty lines if they came from original \n\n
        {
            resultingLines.Add(string.Empty);
            return resultingLines;
        }
        if (font.MeasureText(singleLine) <= maxWidth) // Fits entirely
        {
            resultingLines.Add(singleLine);
            return resultingLines;
        }

        // At this point, lineBreakMode is WordWrap or CharacterWrap, and singleLine is too long.
        int currentPosition = 0;
        int textLength = singleLine.Length;

        while (currentPosition < textLength)
        {
            long countMeasured = font.BreakText(singleLine.AsSpan(currentPosition), maxWidth, out float measuredWidth);
            int count = (int)countMeasured;

            if (count == 0 && currentPosition < textLength) // BreakText couldn't fit even one char (e.g. very narrow maxWidth)
            {
                if (lineBreakMode == LineBreakMode.CharacterWrap || singleLine.Length - currentPosition == 1)
                {
                    count = 1; // Force one char for CharacterWrap, or if it's the last char
                }
                else // WordWrap and count is 0, means first word is too long
                {
                    // Find first space/hyphen in the remainder to break the long word, or take whole word if no break
                    int tempBreak = currentPosition;
                    bool breakFound = false;
                    for (int i = currentPosition; i < textLength; ++i)
                    {
                        if (char.IsWhiteSpace(singleLine[i]) || singleLine[i] == '-')
                        {
                            tempBreak = i;
                            breakFound = true;
                            break;
                        }
                    }
                    count = (breakFound ? tempBreak : textLength) - currentPosition;
                    if (count == 0 && currentPosition < textLength) count = textLength - currentPosition; // take rest if no break
                }
            }

            int breakPosition = currentPosition + count;

            if (currentPosition + count >= textLength) // Remaining text fits or is the last chunk
            {
                resultingLines.Add(singleLine[currentPosition..]);
                break;
            }

            // Adjust breakPosition for WordWrap
            if (lineBreakMode == LineBreakMode.WordWrap)
            {
                int lastBreakChar = -1;
                // Search backwards from the proposed breakPosition (exclusive) to currentPosition (inclusive)
                for (int i = breakPosition - 1; i >= currentPosition; i--)
                {
                    char c = singleLine[i];
                    if (char.IsWhiteSpace(c) || c == '-')
                    {
                        lastBreakChar = i;
                        break;
                    }
                }

                if (lastBreakChar > currentPosition) // Found a suitable word break
                {
                    breakPosition = lastBreakChar + 1; // Break *after* the space/hyphen
                }
                // else: No word break found in the fitting chunk (long word).
                // breakPosition remains as determined by font.BreakText (character break).
            }
            // For CharacterWrap, breakPosition determined by font.BreakText is fine.

            if (breakPosition <= currentPosition && currentPosition < textLength) // Safety: ensure progress
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Text break position calculation stalled at index {currentPosition}. Forcing break after 1 char.");
                breakPosition = currentPosition + 1;
            }

            string lineToAdd = singleLine[currentPosition..breakPosition].TrimEnd();
            resultingLines.Add(lineToAdd);
            currentPosition = breakPosition;

            // Skip leading spaces for the next line's start
            while (currentPosition < textLength && char.IsWhiteSpace(singleLine[currentPosition]))
            {
                currentPosition++;
            }
        }
        return resultingLines;
    }
}
