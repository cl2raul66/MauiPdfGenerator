using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class TextRenderer : IElementRenderer
{
    private const string Ellipsis = "...";

    private record TextLayoutCache(
        List<string> LinesToDraw,
        SKFont Font,
        SKPaint Paint,
        float LineSpacing,
        TextAlignment HorizontalAlignment,
        TextDecorations TextDecorations
    );

    public async Task<LayoutInfo> MeasureAsync(PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var paragraph = (PdfParagraph)element;
        var (font, paint, textToRender, horizontalAlignment, lineBreakMode, textDecorations, textTransform) = await GetTextPropertiesAsync(paragraph, pageDef, fontRegistry);

        float availableWidthForElement = paragraph.GetWidthRequest.HasValue ?
            (float)paragraph.GetWidthRequest.Value :
            availableRect.Width - (float)paragraph.GetMargin.HorizontalThickness;

        float availableWidthForTextLayout = availableWidthForElement - (float)paragraph.GetPadding.HorizontalThickness;
        availableWidthForTextLayout = Math.Max(0, availableWidthForTextLayout);

        float availableHeightForElement = paragraph.GetHeightRequest.HasValue ?
            (float)paragraph.GetHeightRequest.Value :
            availableRect.Height - (float)paragraph.GetMargin.VerticalThickness;

        float availableHeightForDrawing = availableHeightForElement - (float)paragraph.GetPadding.VerticalThickness;

        List<string> allLines = WrapTextToLines(textToRender, font, availableWidthForTextLayout, lineBreakMode);

        float fontLineSpacing = font.Spacing;
        if (fontLineSpacing <= 0) fontLineSpacing = font.Size * 1.2f;

        int linesThatFit = 0;
        if (availableHeightForDrawing > 0 && fontLineSpacing > 0)
        {
            if (lineBreakMode is LineBreakMode.NoWrap or LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation)
            {
                linesThatFit = availableHeightForDrawing >= fontLineSpacing && allLines.Count != 0 ? 1 : 0;
            }
            else
            {
                linesThatFit = (int)Math.Floor(availableHeightForDrawing / fontLineSpacing);
                linesThatFit = Math.Max(0, Math.Min(linesThatFit, allLines.Count));
            }
        }

        List<string> linesToDrawThisCall = allLines.Take(linesThatFit).ToList();
        List<string> remainingLinesList = allLines.Skip(linesThatFit).ToList();

        float visualHeightDrawn = 0;
        if (linesToDrawThisCall.Any())
        {
            SKFontMetrics fontMetrics = font.Metrics;
            float visualLineHeight = fontMetrics.Descent - fontMetrics.Ascent;
            visualHeightDrawn = (linesToDrawThisCall.Count - 1) * fontLineSpacing + visualLineHeight;
        }

        float heightDrawn = paragraph.GetHeightRequest.HasValue ?
            (float)paragraph.GetHeightRequest.Value :
            visualHeightDrawn + (float)paragraph.GetPadding.VerticalThickness;

        float totalHeight = heightDrawn + (float)paragraph.GetMargin.VerticalThickness;

        PdfParagraph? remainingParagraph = null;
        if (remainingLinesList.Any())
        {
            string remainingOriginalText = string.Join("\n",
                (paragraph.Text ?? string.Empty).Split('\n').Skip(allLines.Count - remainingLinesList.Count)
            );
            remainingParagraph = new PdfParagraph(remainingOriginalText, paragraph);
        }
        else if (linesThatFit == 0 && allLines.Any() && !paragraph.IsContinuation)
        {
            remainingParagraph = paragraph;
        }

        float measuredContentWidth;
        if (paragraph.GetWidthRequest.HasValue)
        {
            measuredContentWidth = (float)paragraph.GetWidthRequest.Value;
        }
        else if (paragraph.GetHorizontalOptions == LayoutAlignment.Fill)
        {
            measuredContentWidth = availableWidthForElement;
        }
        else
        {
            measuredContentWidth = linesToDrawThisCall.Any() ? linesToDrawThisCall.Max(line => font.MeasureText(line)) : 0;
        }
        float totalWidth = measuredContentWidth + (float)paragraph.GetMargin.HorizontalThickness;

        layoutState[element] = new TextLayoutCache(linesToDrawThisCall, font, paint, fontLineSpacing, horizontalAlignment, textDecorations);

        return new LayoutInfo(element, totalWidth, totalHeight, remainingParagraph);
    }

    public Task RenderAsync(SKCanvas canvas, PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect renderRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        if (!layoutState.TryGetValue(element, out var cachedState) || cachedState is not TextLayoutCache textCache)
        {
            throw new InvalidOperationException("Text layout cache not found. MeasureAsync must be called before RenderAsync for a paragraph.");
        }

        var paragraph = (PdfParagraph)element;
        var (linesToDraw, font, paint, fontLineSpacing, horizontalAlignment, textDecorations) = textCache;

        if (!linesToDraw.Any())
        {
            font.Dispose();
            paint.Dispose();
            return Task.CompletedTask;
        }

        var contentRect = new SKRect(
            renderRect.Left + (float)paragraph.GetMargin.Left + (float)paragraph.GetPadding.Left,
            renderRect.Top + (float)paragraph.GetMargin.Top + (float)paragraph.GetPadding.Top,
            renderRect.Right - (float)paragraph.GetMargin.Right - (float)paragraph.GetPadding.Right,
            renderRect.Bottom - (float)paragraph.GetMargin.Bottom - (float)paragraph.GetPadding.Bottom
        );

        float lineY = contentRect.Top;

        foreach (string line in linesToDraw)
        {
            SKRect lineBounds = new();
            float measuredWidth = font.MeasureText(line, out lineBounds);
            float drawX = contentRect.Left;

            if (horizontalAlignment is TextAlignment.Center) drawX = contentRect.Left + (contentRect.Width - measuredWidth) / 2f;
            else if (horizontalAlignment is TextAlignment.End) drawX = contentRect.Right - measuredWidth;

            drawX = Math.Max(contentRect.Left, Math.Min(drawX, contentRect.Right - measuredWidth));
            float textDrawY = lineY - lineBounds.Top;

            canvas.DrawText(line, drawX, textDrawY, font, paint);

            if (textDecorations is not TextDecorations.None)
            {
                DrawTextDecorations(canvas, font, paint, textDecorations, drawX, textDrawY, measuredWidth);
            }

            lineY += fontLineSpacing;
        }

        font.Dispose();
        paint.Dispose();
        return Task.CompletedTask;
    }

    private void DrawTextDecorations(SKCanvas canvas, SKFont font, SKPaint paint, TextDecorations decorations, float x, float y, float width)
    {
        float decorationThickness = Math.Max(1f, font.Size / 12f);
        using var decorationPaint = new SKPaint
        {
            Color = paint.Color,
            StrokeWidth = decorationThickness,
            IsAntialias = true
        };
        SKFontMetrics fontMetrics = font.Metrics;

        if ((decorations & TextDecorations.Underline) != 0)
        {
            float underlineY = y + (fontMetrics.UnderlinePosition ?? decorationThickness * 2);
            if (fontMetrics.UnderlineThickness.HasValue && fontMetrics.UnderlineThickness.Value > 0)
            {
                decorationPaint.StrokeWidth = fontMetrics.UnderlineThickness.Value;
            }
            canvas.DrawLine(x, underlineY, x + width, underlineY, decorationPaint);
        }

        if ((decorations & TextDecorations.Strikethrough) != 0)
        {
            float strikeY = y + (fontMetrics.StrikeoutPosition ?? -fontMetrics.XHeight / 2f);
            if (fontMetrics.StrikeoutThickness.HasValue && fontMetrics.StrikeoutThickness.Value > 0)
            {
                decorationPaint.StrokeWidth = fontMetrics.StrikeoutThickness.Value;
            }
            canvas.DrawLine(x, strikeY, x + width, strikeY, decorationPaint);
        }
    }

    private async Task<(SKFont font, SKPaint paint, string textToRender, TextAlignment horizontalAlignment, LineBreakMode lineBreakMode, TextDecorations textDecorations, TextTransform textTransform)> GetTextPropertiesAsync(PdfParagraph paragraph, PdfPageData pageDefinition, PdfFontRegistryBuilder fontRegistry)
    {
        PdfFontIdentifier? fontIdentifierToUse = paragraph.CurrentFontFamily
                                               ?? pageDefinition.PageDefaultFontFamily
                                               ?? fontRegistry.GetUserConfiguredDefaultFontIdentifier()
                                               ?? fontRegistry.GetFirstMauiRegisteredFontIdentifier();

        float fontSize = paragraph.CurrentFontSize > 0 ? paragraph.CurrentFontSize : pageDefinition.PageDefaultFontSize;
        Color textColor = paragraph.CurrentTextColor ?? pageDefinition.PageDefaultTextColor;
        FontAttributes fontAttributes = paragraph.CurrentFontAttributes ?? pageDefinition.PageDefaultFontAttributes;
        TextAlignment horizontalAlignment = paragraph.CurrentHorizontalTextAlignment;
        if (horizontalAlignment == PdfParagraph.DefaultHorizontalTextAlignment)
        {
            horizontalAlignment = paragraph.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => TextAlignment.Center,
                LayoutAlignment.End => TextAlignment.End,
                _ => TextAlignment.Start
            };
        }
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
        var typeface = await SkiaUtils.CreateSkTypefaceAsync(
            skiaFontAliasToAttempt,
            fontAttributes,
            async (fileName) =>
            {
                if (string.IsNullOrEmpty(fileName)) return null;
                try
                {
                    return await FileSystem.OpenAppPackageFileAsync(fileName);
                }
                catch
                {
                    return null;
                }
            },
            filePathToLoad
        );
        if (fontSize <= 0) fontSize = PdfParagraph.DefaultFontSize;
        var font = new SKFont(typeface, fontSize);
        var paint = new SKPaint
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

        return (font, paint, textToRender, horizontalAlignment, lineBreakMode, textDecorations, textTransform);
    }

    private List<string> WrapTextToLines(string text, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
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
                else lines.Add(ApplyLineBreakModeTruncation(segment, font, maxWidth, lineBreakMode));
            }
            else
            {
                lines.AddRange(ApplyLineBreakModeWrapping(segment, font, maxWidth, lineBreakMode));
            }
        }
        return lines;
    }

    private string ApplyLineBreakModeTruncation(string textSegment, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
    {
        float ellipsisWidth = font.MeasureText(Ellipsis);
        if (maxWidth < ellipsisWidth && maxWidth > 0) return Ellipsis[..font.BreakText(Ellipsis, maxWidth)];
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
            return startIndex == textLength && textLength > 0 && ellipsisWidth > 0 ? Ellipsis : Ellipsis + textSegment[startIndex..];
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
                return ApplyLineBreakModeTruncation(textSegment, font, maxWidth, LineBreakMode.TailTruncation);
            }
            if ((int)startCount >= textLength - tempEndString.Length && textLength > 0 && tempEndString.Length == 0 && ellipsisWidth > 0)
            {
                long countTail = font.BreakText(textSegment, availableWidthForText);
                return textSegment[..(int)Math.Max(0, countTail)] + Ellipsis;
            }
            return textSegment[..(int)startCount] + Ellipsis + tempEndString;
        }
        return textSegment;
    }

    private IEnumerable<string> ApplyLineBreakModeWrapping(string singleLine, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
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
                    resultingLines.Add(singleLine.Substring(currentPosition, 1));
                    currentPosition += 1;
                    continue;
                }
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

                if (lastValidBreakInSegment > 0 && currentPosition + lastValidBreakInSegment + 1 > currentPosition)
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
