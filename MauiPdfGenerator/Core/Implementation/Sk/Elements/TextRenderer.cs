using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using Microsoft.Extensions.Logging;
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
        TextDecorations TextDecorations,
        float TotalTextHeight
    );

    public async Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfParagraph paragraph)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfParagraph)} or is null.");

        var (font, paint, textToRender, horizontalAlignment, lineBreakMode, textDecorations, textTransform) = await GetTextPropertiesAsync(paragraph, context);

        float availableWidthForText = availableRect.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness;

        var allLines = WrapTextToLines(textToRender, font, availableWidthForText, lineBreakMode);

        float fontLineSpacing = font.Spacing;
        if (fontLineSpacing <= 0) fontLineSpacing = font.Size * 1.2f;

        var linesToDrawThisCall = allLines;

        float totalTextHeight = 0;
        if (linesToDrawThisCall.Any())
        {
            SKFontMetrics fontMetrics = font.Metrics;
            totalTextHeight = (linesToDrawThisCall.Count - 1) * fontLineSpacing + (fontMetrics.Descent - fontMetrics.Ascent);
        }

        float contentWidth = linesToDrawThisCall.Any() ? linesToDrawThisCall.Max(line => font.MeasureText(line)) : 0;

        float boxWidth = paragraph.GetWidthRequest.HasValue
            ? (float)paragraph.GetWidthRequest.Value
            : contentWidth + (float)paragraph.GetPadding.HorizontalThickness;

        float boxHeight = paragraph.GetHeightRequest.HasValue
            ? (float)paragraph.GetHeightRequest.Value
            : totalTextHeight + (float)paragraph.GetPadding.VerticalThickness;

        context.LayoutState[paragraph] = new TextLayoutCache(linesToDrawThisCall, font, paint, fontLineSpacing, horizontalAlignment, textDecorations, totalTextHeight);

        var totalWidth = boxWidth + (float)paragraph.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)paragraph.GetMargin.VerticalThickness;

        return new LayoutInfo(paragraph, totalWidth, totalHeight);
    }

    public Task RenderAsync(SKCanvas canvas, SKRect renderRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfParagraph paragraph)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfParagraph)} or is null.");

        if (!context.LayoutState.TryGetValue(paragraph, out var cachedState) || cachedState is not TextLayoutCache textCache)
        {
            context.Logger.LogError("Text layout cache not found for element. MeasureAsync was likely not called or failed.");
            return Task.CompletedTask;
        }

        var (linesToDraw, font, paint, fontLineSpacing, horizontalAlignment, textDecorations, totalTextHeight) = textCache;

        var elementBox = new SKRect(
            renderRect.Left + (float)paragraph.GetMargin.Left,
            renderRect.Top + (float)paragraph.GetMargin.Top,
            renderRect.Right - (float)paragraph.GetMargin.Right,
            renderRect.Bottom - (float)paragraph.GetMargin.Bottom
        );

        if (paragraph.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(paragraph.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBox, bgPaint);
        }

        if (!linesToDraw.Any())
        {
            font.Dispose();
            paint.Dispose();
            return Task.CompletedTask;
        }

        var contentRect = new SKRect(
            elementBox.Left + (float)paragraph.GetPadding.Left,
            elementBox.Top + (float)paragraph.GetPadding.Top,
            elementBox.Right - (float)paragraph.GetPadding.Right,
            elementBox.Bottom - (float)paragraph.GetPadding.Bottom
        );

        float verticalOffset = paragraph.GetVerticalOptions switch
        {
            LayoutAlignment.Center => (contentRect.Height - totalTextHeight) / 2f,
            LayoutAlignment.End => contentRect.Height - totalTextHeight,
            _ => 0f
        };

        float lineY = contentRect.Top + verticalOffset;

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

    private async Task<(SKFont font, SKPaint paint, string textToRender, TextAlignment horizontalAlignment, LineBreakMode lineBreakMode, TextDecorations textDecorations, TextTransform textTransform)> GetTextPropertiesAsync(PdfParagraph paragraph, PdfGenerationContext context)
    {
        var pageDefinition = context.PageData;
        var fontRegistry = context.FontRegistry;

        PdfFontIdentifier? fontIdentifierToUse = paragraph.CurrentFontFamily
                                               ?? pageDefinition.PageDefaultFontFamily
                                               ?? fontRegistry.GetUserConfiguredDefaultFontIdentifier()
                                               ?? fontRegistry.GetFirstMauiRegisteredFontIdentifier();

        float fontSize = paragraph.CurrentFontSize > 0 ? paragraph.CurrentFontSize : pageDefinition.PageDefaultFontSize;
        Color textColor = paragraph.CurrentTextColor ?? pageDefinition.PageDefaultTextColor;
        FontAttributes fontAttributes = paragraph.CurrentFontAttributes ?? pageDefinition.PageDefaultFontAttributes;

        TextAlignment horizontalAlignment = paragraph.CurrentHorizontalTextAlignment;

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
