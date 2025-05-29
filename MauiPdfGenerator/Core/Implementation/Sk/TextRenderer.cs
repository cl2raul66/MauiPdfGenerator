using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models; // Asegurar using
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

        // Jerarquía de resolución de fuente:
        // 1. Fuente del párrafo (si se especificó)
        // 2. Fuente predeterminada de la página (si se especificó)
        // 3. Fuente predeterminada del documento configurada por el usuario (si se especificó)
        // 4. Primera fuente registrada en MAUI (si existe)
        // 5. Si nada de lo anterior, será null -> Skia usará su fuente predeterminada.
        PdfFontIdentifier? fontIdentifierToUse = paragraph.CurrentFontFamily
                                               ?? pageDefinition.PageDefaultFontFamily // Este ya considera el default del doc y el 1ro de MAUI
                                               ?? fontRegistry.GetUserConfiguredDefaultFontIdentifier() // Por si la página no tenía uno explícito
                                               ?? fontRegistry.GetFirstMauiRegisteredFontIdentifier();


        float fontSize = paragraph.CurrentFontSize > 0 ? paragraph.CurrentFontSize : pageDefinition.PageDefaultFontSize;
        Color textColor = paragraph.CurrentTextColor ?? pageDefinition.PageDefaultTextColor;
        FontAttributes fontAttributes = paragraph.CurrentFontAttributes ?? pageDefinition.PageDefaultFontAttributes;
        TextAlignment alignment = paragraph.CurrentAlignment;
        LineBreakMode lineBreakMode = paragraph.CurrentLineBreakMode ?? PdfParagraph.DefaultLineBreakMode;

        FontRegistration? fontRegistration = null;
        if (fontIdentifierToUse.HasValue)
        {
            fontRegistration = fontRegistry.GetFontRegistration(fontIdentifierToUse.Value);
        }

        string? filePathIfEmbedding = null;
        if (fontRegistration != null && fontRegistration.ShouldEmbed && !string.IsNullOrEmpty(fontRegistration.FilePath))
        {
            filePathIfEmbedding = fontRegistration.FilePath;
        }

        // Si fontIdentifierToUse es null, skiaFontNameToUse será string.Empty,
        // y SkiaUtils.CreateSkTypefaceAsync usará SKTypeface.Default.
        string skiaFontNameToUse = fontIdentifierToUse?.Alias ?? string.Empty;

        using var typeface = await SkiaUtils.CreateSkTypefaceAsync(
            skiaFontNameToUse,
            fontAttributes,
            async (fileName) =>
            {
                if (string.IsNullOrEmpty(fileName)) return null;
                try { return await FileSystem.OpenAppPackageFileAsync(fileName); }
                catch (Exception ex) { Debug.WriteLine($"[TextRenderer] StreamProvider Error for {fileName}: {ex.Message}"); return null; }
            },
            filePathIfEmbedding
        );

        if (fontSize <= 0) fontSize = PdfParagraph.DefaultFontSize;

        using var font = new SKFont(typeface, fontSize);
        using var paint = new SKPaint
        {
            Color = SkiaUtils.ConvertToSkColor(textColor),
            IsAntialias = true
        };

        var text = paragraph.Text ?? string.Empty;
        float availableWidthForTextLayout = currentPageContentRect.Width - (float)paragraph.GetMargin.Left - (float)paragraph.GetMargin.Right;
        availableWidthForTextLayout = Math.Max(0, availableWidthForTextLayout);
        float elementContentLeftX = currentPageContentRect.Left + (float)paragraph.GetMargin.Left;
        float elementContentRightX = currentPageContentRect.Right - (float)paragraph.GetMargin.Right;
        float availableHeightForDrawing = currentPageContentRect.Bottom - currentYOnPage;

        List<string> allLines = BreakTextIntoLines(text, font, availableWidthForTextLayout, lineBreakMode);
        if (allLines.Count == 0) return new RenderOutput(0, null, false);

        float fontLineSpacing = font.Spacing;
        if (fontLineSpacing <= 0) fontLineSpacing = fontSize;

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
        float heightDrawnThisCall = 0;
        float lineY = currentYOnPage;

        foreach (string line in linesToDrawThisCall)
        {
            if (lineY + fontLineSpacing > currentPageContentRect.Bottom + 0.01f) break;
            SKRect lineBounds = new();
            float measuredWidth = font.MeasureText(line, out lineBounds);
            float drawX = elementContentLeftX;
            if (alignment == TextAlignment.Center) drawX = elementContentLeftX + (availableWidthForTextLayout - measuredWidth) / 2f;
            else if (alignment == TextAlignment.End) drawX = elementContentRightX - measuredWidth;
            drawX = Math.Max(elementContentLeftX, Math.Min(drawX, elementContentRightX - measuredWidth));
            float drawY = lineY - lineBounds.Top;
            canvas.Save();
            canvas.ClipRect(SKRect.Create(elementContentLeftX, currentYOnPage, availableWidthForTextLayout, availableHeightForDrawing));
            canvas.DrawText(line, drawX, drawY, font, paint);
            canvas.Restore();
            heightDrawnThisCall += fontLineSpacing;
            lineY += fontLineSpacing;
        }

        PdfParagraph? remainingParagraph = null;
        bool requiresNewPage = false;
        List<string> remainingLinesList = [.. allLines.Skip(linesThatFit)];
        if (remainingLinesList.Count != 0)
        {
            remainingParagraph = new PdfParagraph(string.Join("\n", remainingLinesList), paragraph);
            requiresNewPage = true;
        }
        else if (linesThatFit == 0 && allLines.Count != 0 && !paragraph.IsContinuation)
        {
            remainingParagraph = paragraph;
            requiresNewPage = true;
        }
        return new RenderOutput(heightDrawnThisCall, remainingParagraph, requiresNewPage);
    }

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
            float segmentWidth = font.MeasureText(segment);
            if (lineBreakMode is LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation)
            {
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
        if (maxWidth < ellipsisWidth) return Ellipsis;
        float availableWidthForText = maxWidth - ellipsisWidth;
        if (availableWidthForText < 0) availableWidthForText = 0;

        if (lineBreakMode is LineBreakMode.TailTruncation)
        {
            long count = font.BreakText(textSegment, availableWidthForText, out _);
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
            return (startIndex == textLength && textLength > 0) ? Ellipsis : Ellipsis + textSegment[startIndex..];
        }
        if (lineBreakMode is LineBreakMode.MiddleTruncation)
        {
            if (availableWidthForText <= 0) return Ellipsis;

            float startWidth = availableWidthForText / 2f;
            long startCount = font.BreakText(textSegment, startWidth, out _);

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
            if ((int)startCount >= (textLength - tempEndString.Length) && textLength > 0 && tempEndString.Length == 0)
            {
                long countTail = font.BreakText(textSegment, availableWidthForText, out _);
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
        if (font.MeasureText(singleLine) <= maxWidth) { resultingLines.Add(singleLine); return resultingLines; }

        int currentPosition = 0;
        int textLength = singleLine.Length;
        while (currentPosition < textLength)
        {
            long countMeasured = font.BreakText(singleLine.AsSpan(currentPosition), maxWidth, out _);
            int count = (int)countMeasured;

            if (count == 0 && currentPosition < textLength)
            {
                Debug.WriteLine($"Advertencia: El conteo de interrupción de texto es 0 para un segmento no vacío que comienza en {currentPosition}. Forzando interrupción después de 1 carácter para evitar bucle. MaxWidth: {maxWidth}, Carácter: '{singleLine[currentPosition]}', Ancho: {font.MeasureText(singleLine[currentPosition].ToString())}");
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
                int lastValidBreak = -1;
                for (int i = breakPositionAttempt; i > currentPosition; --i)
                {
                    if (i >= textLength) continue;
                    char c = singleLine[i];
                    if (char.IsWhiteSpace(c) || c == '-')
                    {
                        lastValidBreak = i;
                        break;
                    }
                }

                if (lastValidBreak != -1)
                {
                    string segmentToConsiderForWordBreak = singleLine.Substring(currentPosition, count);
                    int wordBreakInSegment = segmentToConsiderForWordBreak.LastIndexOfAny([' ', '-']);

                    if (wordBreakInSegment > 0)
                    {
                        actualBreakPosition = currentPosition + wordBreakInSegment + 1;
                    }
                    else
                    {
                        actualBreakPosition = currentPosition + count;
                    }
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

            string lineToAdd = singleLine.Substring(currentPosition, actualBreakPosition - currentPosition).TrimEnd();
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
