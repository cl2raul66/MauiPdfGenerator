using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
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

        // Prioridad para la fuente: Párrafo -> Página -> Documento (usuario) -> Documento (primera MAUI)
        PdfFontIdentifier? fontIdentifierToUse = paragraph.CurrentFontFamily
                                               ?? pageDefinition.PageDefaultFontFamily
                                               ?? fontRegistry.GetUserConfiguredDefaultFontIdentifier()
                                               ?? fontRegistry.GetFirstMauiRegisteredFontIdentifier();


        float fontSize = paragraph.CurrentFontSize > 0 ? paragraph.CurrentFontSize : pageDefinition.PageDefaultFontSize;
        Color textColor = paragraph.CurrentTextColor ?? pageDefinition.PageDefaultTextColor;
        FontAttributes fontAttributes = paragraph.CurrentFontAttributes ?? pageDefinition.PageDefaultFontAttributes;
        TextAlignment alignment = paragraph.CurrentAlignment;
        LineBreakMode lineBreakMode = paragraph.CurrentLineBreakMode ?? PdfParagraph.DefaultLineBreakMode;

        FontRegistration? fontRegistration = paragraph.ResolvedFontRegistration; // Usar la pre-resuelta si existe
        if (fontRegistration == null && fontIdentifierToUse.HasValue) // Fallback si no se resolvió en el párrafo
        {
            fontRegistration = fontRegistry.GetFontRegistration(fontIdentifierToUse.Value);
        }

        string? filePathForEmbeddingOrLookup = null;
        // filePathForEmbeddingOrLookup se usa para:
        // 1. Incrustar la fuente si fontRegistration.ShouldEmbed es true.
        // 2. O, si no se incrusta pero el archivo existe, para leer el FamilyName real del archivo.
        if (fontRegistration is not null && !string.IsNullOrEmpty(fontRegistration.FilePath))
        {
            // Se pasa el FilePath independientemente de ShouldEmbed.
            // CreateSkTypefaceAsync decidirá si incrusta basado en si el SKTypeface se puede crear
            // desde este path. El "ShouldEmbed" de FontRegistration es más una intención.
            // La lógica de si realmente se usa para incrustar o solo para lookup del nombre real
            // está dentro de CreateSkTypefaceAsync.
            filePathForEmbeddingOrLookup = fontRegistration.FilePath;
        }

        // El primer argumento para CreateSkTypefaceAsync es el ALIAS o nombre de referencia deseado.
        // El último argumento es el filePath que SkiaUtils intentará usar para cargar/incrustar
        // y/o para obtener el nombre real de la familia.
        string skiaFontAliasToAttempt = fontIdentifierToUse?.Alias ?? string.Empty;

        using var typeface = await SkiaUtils.CreateSkTypefaceAsync(
            skiaFontAliasToAttempt, // El alias que el usuario especificó
            fontAttributes,
            async (fileName) => // Stream provider
            {
                if (string.IsNullOrEmpty(fileName)) return null;
                try
                {
                    // Asumimos que fileName es relativo al paquete de la app
                    return await FileSystem.OpenAppPackageFileAsync(fileName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TextRenderer] StreamProvider Error for {fileName}: {ex.Message}");
                    return null;
                }
            },
            filePathForEmbeddingOrLookup // El path al archivo .ttf/.otf si está disponible
        );

        if (fontSize <= 0) fontSize = PdfParagraph.DefaultFontSize;

        using var font = new SKFont(typeface, fontSize);
        // Es importante que el 'typeface' aquí sea el que SkiaSharp ha resuelto.
        // Si typeface.FamilyName difiere del 'skiaFontAliasToAttempt', es porque Skia
        // resolvió a una fuente de sistema con un nombre diferente o usó la de defecto.
        // El documento PDF, al referenciar fuentes, usualmente usa el nombre de la familia postscript
        // o el nombre de la familia tal como la conoce el sistema.
        // SKDocument se encarga de esto cuando se usa SKPaint con un SKTypeface.

        using var paint = new SKPaint
        {
            Color = SkiaUtils.ConvertToSkColor(textColor),
            IsAntialias = true
            // No es necesario establecer paint.Typeface = typeface si se va a usar SKFont
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
            if (alignment is TextAlignment.Center) drawX = elementContentLeftX + (availableWidthForTextLayout - measuredWidth) / 2f;
            else if (alignment is TextAlignment.End) drawX = elementContentRightX - measuredWidth;
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
        if (maxWidth < ellipsisWidth && maxWidth > 0) return Ellipsis.Substring(0, (int)font.BreakText(Ellipsis, maxWidth)); // Truncate ellipsis if needed
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
                // Ensure that we are not trying to make the string longer than original with ellipsis in the middle
                if (font.MeasureText(textSegment[..(int)startCount] + Ellipsis + sub) <= maxWidth)
                {
                    tempEndString = sub;
                }
                else
                {
                    break;
                }
            }

            if ((int)startCount == 0 && tempEndString.Length == 0 && textLength > 0 && ellipsisWidth > 0) // Cannot fit anything but ellipsis
            {
                return TruncateSingleLine(textSegment, font, maxWidth, LineBreakMode.TailTruncation); // Fallback to tail
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
                    // Depending on desired behavior, could add the char and let it overflow, or skip, or add empty.
                    // For now, let's assume we skip if a single char is too wide after trying to break.
                    // Or, if we must output something, we might take the first char.
                    // This scenario should be rare with typical fonts/text.
                    resultingLines.Add(singleLine.Substring(currentPosition, 1)); // Add the single char that's too wide
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
                // Look backwards from breakPositionAttempt for a whitespace or hyphen
                int wordBreakSearchStart = currentPosition + count;
                bool foundWordBreak = false;
                for (int i = wordBreakSearchStart; i > currentPosition; --i)
                {
                    if (i >= textLength) continue;
                    char c = singleLine[i - 1]; // Check char before potential break
                    if (char.IsWhiteSpace(c) || c == '-')
                    {
                        // Break after this character (i.e., i is the start of the next line)
                        // or if it's whitespace, the break effectively happens at i-1
                        actualBreakPosition = (char.IsWhiteSpace(c) || c == '-') ? i : i + 1;
                        // If 'i' itself is the break (e.g. space), then actualBreakPosition includes it for trimming later
                        // More robust: find the last space/hyphen within the substring singleLine.Substring(currentPosition, count)
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

                        if (lastValidBreakInSegment > 0) // Found a word break within the fitting part
                        {
                            actualBreakPosition = currentPosition + lastValidBreakInSegment + 1;
                        }
                        else // No word break, so break at char limit
                        {
                            actualBreakPosition = currentPosition + count;
                        }
                        foundWordBreak = true; // To avoid re-setting actualBreakPosition if this logic runs
                        break;
                    }
                }
                if (!foundWordBreak) // No suitable word break found, so break at character limit
                {
                    actualBreakPosition = currentPosition + count;
                }
            }
            // Ensure actualBreakPosition is not stuck
            if (actualBreakPosition <= currentPosition && currentPosition < textLength)
            {
                actualBreakPosition = currentPosition + 1;
            }

            string lineToAdd = singleLine.Substring(currentPosition, actualBreakPosition - currentPosition).TrimEnd();
            resultingLines.Add(lineToAdd);
            currentPosition = actualBreakPosition;

            // Skip leading whitespace for the next line
            while (currentPosition < textLength && char.IsWhiteSpace(singleLine[currentPosition]))
            {
                currentPosition++;
            }
        }
        return resultingLines;
    }
}
