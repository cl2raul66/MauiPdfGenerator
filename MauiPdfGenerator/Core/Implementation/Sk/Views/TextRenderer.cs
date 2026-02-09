using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Core.Implementation.Sk.Models;
using MauiPdfGenerator.Core.Implementation.Sk.Utils;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;
using MauiPdfGenerator.Fluent.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Views;

internal class TextRenderer : IElementRenderer
{
    private const string Ellipsis = "...";

    private record TextLayoutCache(
        SKFont Font,
        SKPaint Paint,
        TextAlignment HorizontalAlignment,
        TextAlignment VerticalTextAlignment,
        TextDecorations TextDecorations,
        LineBreakMode LineBreakMode,
        string TransformedText,
        List<(string Line, int StartIndex)>? LinesToDrawOnThisPage = null,
        PdfRect? FinalArrangedRect = null,
        float LineAdvance = 0,
        float VisualTopOffset = 0,
        float VisualBottomOffset = 0,
        SpanRun[]? SpanRuns = null,
        bool IsRTL = false
    );

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKSize availableSize)
    {
        if (context.Element is not PdfParagraphData paragraph)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfParagraphData)} or is null.");

        var (font, paint, textToRender, horizontalAlignment, verticalTextAlignment, lineBreakMode, textDecorations, textTransform) = await GetTextPropertiesAsync(paragraph, context);

        string effectiveCulture = paragraph.Culture ?? context.PageData.Culture;
        bool isRTL = SkiaUtils.IsRtlCulture(effectiveCulture);

        float widthForMeasure = paragraph.GetWidthRequest.HasValue
            ? (float)paragraph.GetWidthRequest.Value - (float)paragraph.GetPadding.HorizontalThickness
            : availableSize.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness;

        SpanRun[] spanRuns;

        if (paragraph.HasSpans)
        {
            spanRuns = await ResolveSpanRunsAsync(paragraph, context, font, paint);
        }
        else
        {
            spanRuns = [new SpanRun(0, textToRender.Length, font, paint, null, null)];
        }

        var textEngine = new TextShapingEngine(spanRuns, font, paint, textToRender, isRTL);

        var allLines = WrapTextToLinesWithIndex(textToRender, font, widthForMeasure, lineBreakMode, textEngine);

        SKFontMetrics fontMetrics = font.Metrics;
        float lineAdvance = -fontMetrics.Ascent + fontMetrics.Descent;

        float visualTopOffset = 0;
        float visualBottomOffset = 0;
        float totalTextHeight = 0;

        if (allLines.Count != 0)
        {
            SKRect firstLineBounds = new();
            font.MeasureText(allLines[0].Line, out firstLineBounds);
            visualTopOffset = -firstLineBounds.Top;

            SKRect lastLineBounds = new();
            font.MeasureText(allLines[^1].Line, out lastLineBounds);
            visualBottomOffset = lastLineBounds.Bottom;

            if (allLines.Count == 1)
            {
                SKRect bounds = new();
                font.MeasureText(allLines[0].Line, out bounds);
                totalTextHeight = bounds.Height;
            }
            else
            {
                totalTextHeight = visualTopOffset + ((allLines.Count - 1) * lineAdvance) + visualBottomOffset;
            }
        }

        float contentWidth = 0;
        if (allLines.Count != 0)
        {
            contentWidth = allLines.Max(line => textEngine.MeasureTextWidth(line.Line, line.StartIndex));
        }

        float boxWidth = paragraph.GetWidthRequest.HasValue ? (float)paragraph.GetWidthRequest.Value : contentWidth + (float)paragraph.GetPadding.HorizontalThickness;
        float boxHeight = paragraph.GetHeightRequest.HasValue ? (float)paragraph.GetHeightRequest.Value : totalTextHeight + (float)paragraph.GetPadding.VerticalThickness;

        context.LayoutState[paragraph] = new TextLayoutCache(
            font, paint, horizontalAlignment, verticalTextAlignment, textDecorations,
            lineBreakMode, textToRender,
            LinesToDrawOnThisPage: allLines,
            LineAdvance: lineAdvance,
            VisualTopOffset: visualTopOffset, VisualBottomOffset: visualBottomOffset,
            SpanRuns: spanRuns,
            IsRTL: isRTL);

        var totalWidth = boxWidth + (float)paragraph.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)paragraph.GetMargin.VerticalThickness;

        return new PdfLayoutInfo(paragraph, totalWidth, totalHeight);
    }

    public Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfParagraphData paragraph)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfParagraphData)} or is null.");

        if (!context.LayoutState.TryGetValue(paragraph, out var state) || state is not TextLayoutCache baseCache)
        {
            context.Logger.LogError("Text layout cache not found.");
            return Task.FromResult(new PdfLayoutInfo(paragraph, finalRect.Width, finalRect.Height, finalRect));
        }

        if (paragraph.GetHeightRequest.HasValue)
        {
            return ArrangeAsFixedBox(finalRect, paragraph, baseCache, context);
        }
        else
        {
            return ArrangeAsFlowText(finalRect, paragraph, baseCache, context);
        }
    }

    private Task<PdfLayoutInfo> ArrangeAsFixedBox(PdfRect finalRect, PdfParagraphData paragraph, TextLayoutCache baseCache, PdfGenerationContext context)
    {
        var textEngine = new TextShapingEngine(baseCache.SpanRuns!, baseCache.Font, baseCache.Paint, baseCache.TransformedText, baseCache.IsRTL);

        var allLines = WrapTextToLinesWithIndex(baseCache.TransformedText, baseCache.Font,
            finalRect.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness,
            baseCache.LineBreakMode, textEngine);

        var finalCache = baseCache with
        {
            LinesToDrawOnThisPage = allLines,
            FinalArrangedRect = finalRect
        };
        context.LayoutState[paragraph] = finalCache;

        return Task.FromResult(new PdfLayoutInfo(paragraph, finalRect.Width, finalRect.Height, finalRect));
    }

    private Task<PdfLayoutInfo> ArrangeAsFlowText(PdfRect finalRect, PdfParagraphData paragraph, TextLayoutCache baseCache, PdfGenerationContext context)
    {
        var textEngine = new TextShapingEngine(baseCache.SpanRuns!, baseCache.Font, baseCache.Paint, baseCache.TransformedText, baseCache.IsRTL);

        var allLines = WrapTextToLinesWithIndex(baseCache.TransformedText, baseCache.Font,
            finalRect.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness,
            baseCache.LineBreakMode, textEngine);

        float availableHeightForText = finalRect.Height - (float)paragraph.GetMargin.VerticalThickness - (float)paragraph.GetPadding.VerticalThickness;
        float lineAdvance = baseCache.LineAdvance > 0 ? baseCache.LineAdvance : 1;
        float visualTop = baseCache.VisualTopOffset;
        float visualBottom = baseCache.VisualBottomOffset;

        int linesThatFit = 0;
        if (availableHeightForText >= visualTop)
        {
            float remaining = availableHeightForText - visualTop;
            int additionalLines = (int)Math.Floor(remaining / lineAdvance);
            linesThatFit = 1 + additionalLines;
        }

        if (linesThatFit >= allLines.Count) linesThatFit = allLines.Count;
        else if (linesThatFit > 0)
        {
            float heightWithLastLine = visualTop + ((linesThatFit - 1) * lineAdvance) + visualBottom;
            if (heightWithLastLine > availableHeightForText) linesThatFit--;
        }

        if (linesThatFit <= 0)
        {
            return Task.FromResult(new PdfLayoutInfo(paragraph, finalRect.Width, 0, PdfRect.Empty, paragraph));
        }

        if (linesThatFit >= allLines.Count)
        {
            float totalTextHeight = (allLines.Count == 1)
                ? (visualTop + visualBottom)
                : visualTop + ((allLines.Count - 1) * lineAdvance) + visualBottom;

            float consumedHeight = totalTextHeight + (float)paragraph.GetPadding.VerticalThickness + (float)paragraph.GetMargin.VerticalThickness;
            var finalArrangedRect = new PdfRect(finalRect.X, finalRect.Y, finalRect.Width, consumedHeight);

            var finalCache = baseCache with { LinesToDrawOnThisPage = allLines, FinalArrangedRect = finalArrangedRect };
            context.LayoutState[paragraph] = finalCache;

            return Task.FromResult(new PdfLayoutInfo(paragraph, finalRect.Width, consumedHeight, finalArrangedRect));
        }
        else
        {
            var linesForPage = allLines.Take(linesThatFit).ToList();
            var remainingLines = allLines.Skip(linesThatFit).ToList();
            string remainingText = string.Join(PdfStringUtils.NormalizeNewline, remainingLines.Select(l => l.Line));
            var remainingParagraph = new PdfParagraphData(remainingText, paragraph);

            float heightOnPage = visualTop + ((linesForPage.Count - 1) * lineAdvance) + visualBottom;
            float consumedHeight = heightOnPage + (float)paragraph.GetPadding.VerticalThickness + (float)paragraph.GetMargin.VerticalThickness;

            var finalArrangedRect = new PdfRect(finalRect.X, finalRect.Y, finalRect.Width, consumedHeight);
            var pageCache = baseCache with { LinesToDrawOnThisPage = linesForPage, FinalArrangedRect = finalArrangedRect };
            context.LayoutState[paragraph] = pageCache;

            return Task.FromResult(new PdfLayoutInfo(paragraph, finalRect.Width, consumedHeight, finalArrangedRect, remainingParagraph));
        }
    }

    public Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfParagraphData paragraph)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfParagraphData)} or is null.");

        if (!context.LayoutState.TryGetValue(paragraph, out var cachedState) || cachedState is not TextLayoutCache textCache)
        {
            return Task.CompletedTask;
        }

        var linesToDraw = textCache.LinesToDrawOnThisPage;
        var pdfRenderRect = textCache.FinalArrangedRect;

        if (linesToDraw is null || linesToDraw.Count == 0 || pdfRenderRect is null)
        {
            textCache.Font.Dispose();
            textCache.Paint.Dispose();
            return Task.CompletedTask;
        }

        var (font, paint, lineAdvance, horizontalAlignment, verticalTextAlignment, textDecorations, visualTopOffset, spanRuns) =
            (textCache.Font, textCache.Paint, textCache.LineAdvance, textCache.HorizontalAlignment,
             textCache.VerticalTextAlignment, textCache.TextDecorations, textCache.VisualTopOffset, textCache.SpanRuns);

        var renderRect = new SKRect(pdfRenderRect.Value.Left, pdfRenderRect.Value.Top, pdfRenderRect.Value.Right, pdfRenderRect.Value.Bottom);

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

        var contentRect = new SKRect(
            elementBox.Left + (float)paragraph.GetPadding.Left,
            elementBox.Top + (float)paragraph.GetPadding.Top,
            elementBox.Right - (float)paragraph.GetPadding.Right,
            elementBox.Bottom - (float)paragraph.GetPadding.Bottom
        );

        float actualTextHeight;
        if (linesToDraw.Count == 1)
        {
            SKRect bounds = new();
            font.MeasureText(linesToDraw[0].Line, out bounds);
            actualTextHeight = bounds.Height;
        }
        else
        {
            SKRect lastBounds = new();
            font.MeasureText(linesToDraw[^1].Line, out lastBounds);
            float visualBottom = lastBounds.Bottom;
            actualTextHeight = visualTopOffset + ((linesToDraw.Count - 1) * lineAdvance) + visualBottom;
        }

        float verticalOffset = verticalTextAlignment switch
        {
            TextAlignment.Center => (contentRect.Height - actualTextHeight) / 2f,
            TextAlignment.End => contentRect.Height - actualTextHeight,
            _ => 0f
        };

        float baselineY = contentRect.Top + verticalOffset + visualTopOffset;

        canvas.Save();
        canvas.ClipRect(contentRect);

        var textEngine = new TextShapingEngine(spanRuns!, font, paint, textCache.TransformedText, textCache.IsRTL);

        bool isRTL = textCache.IsRTL;

        for (int i = 0; i < linesToDraw.Count; i++)
        {
            var (line, startIndex) = linesToDraw[i];

            float measuredWidth = textEngine.MeasureTextWidth(line, startIndex);

            float drawX = contentRect.Left;

            bool isLastLine = (i == linesToDraw.Count - 1);
            bool shouldJustify = horizontalAlignment is TextAlignment.Justify && !isLastLine && line.Contains(' ') && !isRTL;

            if (shouldJustify)
            {
                DrawJustifiedLine(canvas, line, contentRect.Left, contentRect.Width, baselineY, font, paint, textEngine, startIndex);
                if (textDecorations is not TextDecorations.None)
                {
                    DrawTextDecorations(canvas, font, paint, textDecorations, contentRect.Left, baselineY, contentRect.Width);
                }
            }
            else
            {
                if (horizontalAlignment is TextAlignment.Center)
                    drawX = contentRect.Left + (contentRect.Width - measuredWidth) / 2f;
                else if (horizontalAlignment is TextAlignment.End)
                    drawX = contentRect.Right - measuredWidth;

                if (isRTL)
                {
                    if (horizontalAlignment == TextAlignment.Start) drawX = contentRect.Right - measuredWidth;
                    else if (horizontalAlignment == TextAlignment.End) drawX = contentRect.Left;
                }

                textEngine.DrawShapedLine(canvas, line, drawX, baselineY, startIndex, textDecorations);
            }

            baselineY += lineAdvance;
        }

        canvas.Restore();

        font.Dispose();
        paint.Dispose();
        return Task.CompletedTask;
    }

    private void DrawJustifiedLine(SKCanvas canvas, string line, float x, float totalWidth, float y, SKFont font, SKPaint paint, TextShapingEngine textEngine, int lineStartIndex = 0)
    {
        string[] words = line.Split(' ');
        if (words.Length <= 1)
        {
            textEngine.DrawShapedLine(canvas, line, x, y, lineStartIndex, TextDecorations.None);
            return;
        }

        float totalWordWidth = words.Sum(w => textEngine.MeasureTextWidth(w, lineStartIndex + line.IndexOf(w, StringComparison.Ordinal)));
        float spaceWidth = textEngine.MeasureTextWidth(" ", lineStartIndex + line.IndexOf(' '));
        float totalSpaceWidth = (words.Length - 1) * spaceWidth;
        float extraSpace = totalWidth - totalWordWidth - totalSpaceWidth;

        if (extraSpace < 0)
        {
            textEngine.DrawShapedLine(canvas, line, x, y, lineStartIndex, TextDecorations.None);
            return;
        }

        float extraSpacePerGap = extraSpace / (words.Length - 1);
        float currentX = x;

        for (int i = 0; i < words.Length; i++)
        {
            int wordStartIndex = line.IndexOf(words[i], StringComparison.Ordinal);
            textEngine.DrawShapedLine(canvas, words[i], currentX, y, lineStartIndex + wordStartIndex, TextDecorations.None);

            currentX += textEngine.MeasureTextWidth(words[i], lineStartIndex + wordStartIndex);

            if (i < words.Length - 1)
            {
                currentX += spaceWidth + extraSpacePerGap;
            }
        }
    }

    public Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context)
    {
        return Task.CompletedTask;
    }

    private void DrawTextDecorations(SKCanvas canvas, SKFont font, SKPaint paint, TextDecorations decorations, float x, float baselineY, float width)
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
            float underlineY = baselineY + (fontMetrics.UnderlinePosition ?? decorationThickness * 2);
            if (fontMetrics.UnderlineThickness.HasValue && fontMetrics.UnderlineThickness.Value > 0)
            {
                decorationPaint.StrokeWidth = fontMetrics.UnderlineThickness.Value;
            }
            canvas.DrawLine(x, underlineY, x + width, underlineY, decorationPaint);
        }

        if ((decorations & TextDecorations.Strikethrough) != 0)
        {
            float strikeY = baselineY + (fontMetrics.StrikeoutPosition ?? -fontMetrics.XHeight / 2f);
            if (fontMetrics.StrikeoutThickness.HasValue && fontMetrics.StrikeoutThickness.Value > 0)
            {
                decorationPaint.StrokeWidth = fontMetrics.StrikeoutThickness.Value;
            }
            canvas.DrawLine(x, strikeY, x + width, strikeY, decorationPaint);
        }
    }

    private async Task<(SKFont font, SKPaint paint, string textToRender, TextAlignment horizontalAlignment, TextAlignment verticalTextAlignment, LineBreakMode lineBreakMode, TextDecorations textDecorations, TextTransform textTransform)> GetTextPropertiesAsync(PdfParagraphData paragraph, PdfGenerationContext context)
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
        TextAlignment verticalTextAlignment = paragraph.CurrentVerticalTextAlignment;

        LineBreakMode lineBreakMode = paragraph.CurrentLineBreakMode ?? PdfParagraphData.DefaultLineBreakMode;
        TextDecorations textDecorations = paragraph.CurrentTextDecorations ?? pageDefinition.PageDefaultTextDecorations;
        TextTransform textTransform = paragraph.CurrentTextTransform ?? pageDefinition.PageDefaultTextTransform;
        PdfFontRegistration? fontRegistration = paragraph.ResolvedFontRegistration;
        if (fontRegistration is null && fontIdentifierToUse.HasValue)
        {
            fontRegistration = fontRegistry.GetFontRegistration(fontIdentifierToUse.Value);
            if (fontRegistration is null)
            {
                context.DiagnosticSink.Submit(new DiagnosticMessage(
                    DiagnosticSeverity.Warning,
                    DiagnosticCodes.FontNotFound,
                    $"The font with alias '{fontIdentifierToUse.Value.Alias}' was not found in the document's font registry. A system default font will be used as a fallback."
                ));
            }
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
        if (fontSize <= 0) fontSize = PdfParagraphData.DefaultFontSize;
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

        return (font, paint, textToRender, horizontalAlignment, verticalTextAlignment, lineBreakMode, textDecorations, textTransform);
    }

    private List<(string Line, int StartIndex)> WrapTextToLinesWithIndex(string text, SKFont font, float maxWidth, LineBreakMode lineBreakMode, TextShapingEngine textEngine)
    {
        var lines = new List<(string, int)>();
        if (string.IsNullOrEmpty(text)) return lines;

        string normalizedText = PdfStringUtils.NormalizeNewlines(text);
        string[] textSegments = normalizedText.Split(PdfStringUtils.NormalizeNewline);

        int currentIndex = 0;

        foreach (string segment in textSegments)
        {
            if (maxWidth <= 0 || lineBreakMode is LineBreakMode.NoWrap)
            {
                lines.Add((segment, currentIndex));
                currentIndex += segment.Length + 1;
                continue;
            }

            if (lineBreakMode is LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation)
            {
                float segmentWidth = textEngine.MeasureTextWidth(segment);
                if (segmentWidth <= maxWidth)
                {
                    lines.Add((segment, currentIndex));
                }
                else
                {
                    lines.Add((ApplyLineBreakModeTruncation(segment, font, maxWidth, lineBreakMode, textEngine), currentIndex));
                }
            }
            else
            {
                var wrappedLines = ApplyLineBreakModeWrappingWithIndex(segment, font, maxWidth, lineBreakMode, textEngine, currentIndex);
                lines.AddRange(wrappedLines);
                currentIndex += segment.Length + 1;
            }
        }

        return lines;
    }

    private List<(string Line, int StartIndex)> ApplyLineBreakModeWrappingWithIndex(string singleLine, SKFont font, float maxWidth, LineBreakMode lineBreakMode, TextShapingEngine textEngine, int startIndex)
    {
        var resultingLines = new List<(string, int)>();
        if (string.IsNullOrEmpty(singleLine)) { resultingLines.Add((string.Empty, startIndex)); return resultingLines; }

        float singleLineWidth = textEngine.MeasureTextWidth(singleLine);
        if (singleLineWidth <= maxWidth) { resultingLines.Add((singleLine, startIndex)); return resultingLines; }

        int currentPosition = 0;
        int textLength = singleLine.Length;

        while (currentPosition < textLength)
        {
            long countMeasured = CountCharactersInWidth(singleLine.Substring(currentPosition), maxWidth, textEngine);
            int count = (int)countMeasured;

            if (count == 0 && currentPosition < textLength)
            {
                float charWidth = textEngine.MeasureTextWidth(singleLine[currentPosition].ToString());
                if (charWidth > maxWidth)
                {
                    resultingLines.Add((singleLine.Substring(currentPosition, 1), startIndex + currentPosition));
                    currentPosition += 1;
                    continue;
                }
                count = 1;
            }

            int breakPositionAttempt = currentPosition + count;

            if (breakPositionAttempt >= textLength)
            {
                resultingLines.Add((singleLine[currentPosition..], startIndex + currentPosition));
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
            resultingLines.Add((lineToAdd, startIndex + currentPosition));
            currentPosition = actualBreakPosition;

            while (currentPosition < textLength && char.IsWhiteSpace(singleLine[currentPosition]))
            {
                currentPosition++;
            }
        }

        return resultingLines;
    }

    private string ApplyLineBreakModeTruncation(string textSegment, SKFont font, float maxWidth, LineBreakMode lineBreakMode, TextShapingEngine textEngine)
    {
        float ellipsisWidth = textEngine.MeasureTextWidth(Ellipsis);
        if (maxWidth < ellipsisWidth && maxWidth > 0) return Ellipsis[..(int)font.BreakText(Ellipsis, maxWidth)];
        if (maxWidth <= 0) return string.Empty;

        float availableWidthForText = maxWidth - ellipsisWidth;
        if (availableWidthForText < 0) availableWidthForText = 0;

        if (lineBreakMode is LineBreakMode.TailTruncation)
        {
            long count = CountCharactersInWidth(textSegment, availableWidthForText, textEngine);
            return string.Concat(textSegment.AsSpan(0, (int)Math.Max(0, count)), Ellipsis);
        }
        if (lineBreakMode is LineBreakMode.HeadTruncation)
        {
            int textLength = textSegment.Length;
            int startIndex = textLength;
            for (int i = 1; i <= textLength; i++)
            {
                string sub = textSegment[(textLength - i)..];
                float width = textEngine.MeasureTextWidth(sub);
                if (width <= availableWidthForText)
                {
                    startIndex = textLength - i;
                }
                else
                {
                    break;
                }
            }
            return startIndex == textLength && textLength > 0 && ellipsisWidth > 0 ? Ellipsis : string.Concat(Ellipsis, textSegment.AsSpan(startIndex));
        }
        if (lineBreakMode is LineBreakMode.MiddleTruncation)
        {
            if (availableWidthForText <= 0 && ellipsisWidth > 0) return Ellipsis;
            if (availableWidthForText <= 0 && ellipsisWidth <= 0) return string.Empty;

            float startWidth = availableWidthForText / 2f;
            long startCount = CountCharactersInWidth(textSegment, startWidth, textEngine);

            int textLength = textSegment.Length;
            string tempEndString = "";
            for (int i = 1; i <= textLength - (int)startCount; i++)
            {
                string sub = textSegment[(textLength - i)..];
                string fullString = string.Concat(textSegment.AsSpan(0, (int)startCount), Ellipsis, sub);
                float width = textEngine.MeasureTextWidth(fullString);
                if (width <= maxWidth)
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
                return ApplyLineBreakModeTruncation(textSegment, font, maxWidth, LineBreakMode.TailTruncation, textEngine);
            }
            if ((int)startCount >= textLength - tempEndString.Length && textLength > 0 && tempEndString.Length == 0 && ellipsisWidth > 0)
            {
                long countTail = CountCharactersInWidth(textSegment, availableWidthForText, textEngine);
                return string.Concat(textSegment.AsSpan(0, (int)Math.Max(0, countTail)), Ellipsis);
            }
            return string.Concat(textSegment.AsSpan(0, (int)startCount), Ellipsis, tempEndString);
        }
        return textSegment;
    }

    private long CountCharactersInWidth(string text, float maxWidth, TextShapingEngine textEngine)
    {
        for (int i = 0; i < text.Length; i++)
        {
            string sub = text[..(i + 1)];
            float width = textEngine.MeasureTextWidth(sub);

            if (width > maxWidth)
            {
                return i;
            }
        }
        return text.Length;
    }

    #region Span Support
    private async Task<SpanRun[]> ResolveSpanRunsAsync(
        PdfParagraphData paragraph,
        PdfGenerationContext context,
        SKFont defaultFont,
        SKPaint defaultPaint)
    {
        if (!paragraph.HasSpans || paragraph.Spans.Count == 0)
            return [];

        var pageDefinition = context.PageData;
        var fontRegistry = context.FontRegistry;

        var runs = new List<SpanRun>();

        foreach (var span in paragraph.Spans)
        {
            SKFont font;
            SKPaint paint;
            TextDecorations? decorations = null;
            TextTransform? transform = null;

            bool hasLocalFontFamily = span.FontFamilyProp.Priority > PdfPropertyPriority.Default;
            bool hasLocalFontSize = span.FontSizeProp.Priority > PdfPropertyPriority.Default;
            bool hasLocalTextColor = span.TextColorProp.Priority > PdfPropertyPriority.Default;
            bool hasLocalFontAttributes = span.FontAttributesProp.Priority > PdfPropertyPriority.Default;
            bool hasLocalDecorations = span.TextDecorationsProp.Priority > PdfPropertyPriority.Default;
            bool hasLocalTransform = span.TextTransformProp.Priority > PdfPropertyPriority.Default;

            bool hasLocalProperties = hasLocalFontFamily || hasLocalFontSize || hasLocalTextColor ||
                                      hasLocalFontAttributes || hasLocalDecorations || hasLocalTransform;

            if (!hasLocalProperties)
            {
                font = defaultFont;
                paint = defaultPaint;
            }
            else
            {
                float fontSize = hasLocalFontSize ? span.FontSizeProp.Value ?? defaultFont.Size : defaultFont.Size;
                FontAttributes fontAttributes = hasLocalFontAttributes
                    ? (span.FontAttributesProp.Value ?? FontAttributes.None)
                    : (paragraph.CurrentFontAttributes ?? pageDefinition.PageDefaultFontAttributes);

                PdfFontIdentifier? fontIdentifierToUse = hasLocalFontFamily
                    ? span.CurrentFontFamily
                    : paragraph.CurrentFontFamily ?? pageDefinition.PageDefaultFontFamily ?? fontRegistry.GetUserConfiguredDefaultFontIdentifier() ?? fontRegistry.GetFirstMauiRegisteredFontIdentifier();

                PdfFontRegistration? fontRegistration = span.ResolvedFontRegistration;
                if (fontRegistration is null && fontIdentifierToUse.HasValue)
                {
                    fontRegistration = fontRegistry.GetFontRegistration(fontIdentifierToUse.Value);
                }

                string? filePathToLoad = fontRegistration?.FilePath;
                string skiaFontAliasToAttempt = fontIdentifierToUse?.Alias ?? string.Empty;

                var typeface = await SkiaUtils.CreateSkTypefaceAsync(
                    skiaFontAliasToAttempt,
                    fontAttributes,
                    async (fileName) =>
                    {
                        if (string.IsNullOrEmpty(fileName)) return null;
                        try { return await FileSystem.OpenAppPackageFileAsync(fileName); }
                        catch { return null; }
                    },
                    filePathToLoad
                );

                font = new SKFont(typeface, fontSize);

                Color textColor = hasLocalTextColor
                    ? (span.CurrentTextColor ?? pageDefinition.PageDefaultTextColor)
                    : (paragraph.CurrentTextColor ?? pageDefinition.PageDefaultTextColor);

                paint = new SKPaint
                {
                    Color = SkiaUtils.ConvertToSkColor(textColor),
                    IsAntialias = true
                };

                if (hasLocalDecorations)
                    decorations = span.CurrentTextDecorations;

                if (hasLocalTransform)
                    transform = span.CurrentTextTransform;
            }

            runs.Add(new SpanRun(
                StartIndex: span.StartIndex,
                EndIndex: span.EndIndex,
                Font: font,
                Paint: paint,
                Decorations: decorations,
                Transform: transform
            ));
        }

        return [.. runs];
    }
    #endregion
}
