using System.Diagnostics;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Common.Utils;
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
        List<string>? LinesToDrawOnThisPage = null,
        PdfRect? FinalArrangedRect = null,
        float LineAdvance = 0,
        float VisualTopOffset = 0,
        float VisualBottomOffset = 0
    );

    // --- Multi-Span Records ---
    private record ProcessedSpan(
        string Text,
        SKFont Font,
        SKPaint Paint,
        TextDecorations TextDecorations,
        float SpaceWidth
    );

    private record VisualSpanFragment(
        string Text,
        ProcessedSpan SourceSpan,
        float Width
    );

    private record VisualLine(
        List<VisualSpanFragment> Fragments,
        float Width,
        float Height,
        float BaselineAscent,
        float BaselineDescent
    );

    private record MultiSpanLayoutCache(
        List<VisualLine>? LinesToDraw = null,
        PdfRect? FinalArrangedRect = null,
        TextAlignment HorizontalAlignment = TextAlignment.Start,
        TextAlignment VerticalTextAlignment = TextAlignment.Start,
        List<ProcessedSpan>? OriginalSpans = null,
        List<VisualLine>? AllMeasuredLines = null
    );

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKSize availableSize)
    {
        if (context.Element is not PdfParagraphData paragraph)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfParagraphData)} or is null.");

        if (paragraph.HasSpans)
        {
            return await MeasureSpansAsync(paragraph, context, availableSize);
        }

        var (font, paint, textToRender, horizontalAlignment, verticalTextAlignment, lineBreakMode, textDecorations, textTransform) = await GetTextPropertiesAsync(paragraph, context);

        float widthForMeasure = paragraph.GetWidthRequest.HasValue
            ? (float)paragraph.GetWidthRequest.Value - (float)paragraph.GetPadding.HorizontalThickness
            : availableSize.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness;

        var allLines = WrapTextToLines(textToRender, font, widthForMeasure, lineBreakMode);

        SKFontMetrics fontMetrics = font.Metrics;
        float lineAdvance = -fontMetrics.Ascent + fontMetrics.Descent;

        float visualTopOffset = 0;
        float visualBottomOffset = 0;

        if (allLines.Count != 0)
        {
            SKRect firstLineBounds = new();
            font.MeasureText(allLines[0], out firstLineBounds);
            visualTopOffset = -firstLineBounds.Top;

            SKRect lastLineBounds = new();
            font.MeasureText(allLines[^1], out lastLineBounds);
            visualBottomOffset = lastLineBounds.Bottom;
        }

        float totalTextHeight = 0;
        if (allLines.Count != 0)
        {
            if (allLines.Count == 1)
            {
                SKRect bounds = new();
                font.MeasureText(allLines[0], out bounds);
                totalTextHeight = bounds.Height;
            }
            else
            {
                totalTextHeight = visualTopOffset + ((allLines.Count - 1) * lineAdvance) + visualBottomOffset;
            }
        }

        float contentWidth = allLines.Count != 0 ? allLines.Max(line => font.MeasureText(line)) : 0;

        float boxWidth = paragraph.GetWidthRequest.HasValue ? (float)paragraph.GetWidthRequest.Value : contentWidth + (float)paragraph.GetPadding.HorizontalThickness;
        float boxHeight = paragraph.GetHeightRequest.HasValue ? (float)paragraph.GetHeightRequest.Value : totalTextHeight + (float)paragraph.GetPadding.VerticalThickness;

        context.LayoutState[paragraph] = new TextLayoutCache(
            font, paint, horizontalAlignment, verticalTextAlignment, textDecorations,
            lineBreakMode, textToRender,
            LinesToDrawOnThisPage: allLines,
            LineAdvance: lineAdvance,
            VisualTopOffset: visualTopOffset, VisualBottomOffset: visualBottomOffset);

        var totalWidth = boxWidth + (float)paragraph.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)paragraph.GetMargin.VerticalThickness;

        return new PdfLayoutInfo(paragraph, totalWidth, totalHeight);
    }

    public Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfParagraphData paragraph)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfParagraphData)} or is null.");

        if (paragraph.HasSpans)
        {
            return ArrangeSpansAsync(paragraph, finalRect, context);
        }

        if (!context.LayoutState.TryGetValue(paragraph, out var stateObj) || stateObj is not TextLayoutCache cache)
        {
            context.Logger.LogError("Text layout cache not found.");
            return Task.FromResult(new PdfLayoutInfo(paragraph, finalRect.Width, finalRect.Height, finalRect));
        }

        // --- LÓGICA CASUÍSTICA ---
        if (paragraph.GetHeightRequest.HasValue)
        {
            // CASO A: Caja Fija (Atomic Box)
            return ArrangeAsFixedBox(finalRect, paragraph, cache, context);
        }
        else
        {
            // CASO B: Flujo Continuo (Flow Text)
            return ArrangeAsFlowText(finalRect, paragraph, cache, context);
        }
    }

    private Task<PdfLayoutInfo> ArrangeAsFixedBox(PdfRect finalRect, PdfParagraphData paragraph, TextLayoutCache baseCache, PdfGenerationContext context)
    {
        // En modo Caja Fija, usamos todas las líneas calculadas.
        // El VSL nos ha pasado finalRect.Height = HeightRequest + Margin, así que confiamos en ello.
        var allLines = WrapTextToLines(baseCache.TransformedText, baseCache.Font,
            finalRect.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness,
            baseCache.LineBreakMode);

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
        var allLines = WrapTextToLines(baseCache.TransformedText, baseCache.Font,
            finalRect.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness,
            baseCache.LineBreakMode);

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
            // Cabe todo
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
            // Split
            var linesForPage = allLines.Take(linesThatFit).ToList();
            var remainingLines = allLines.Skip(linesThatFit).ToList();
            string remainingText = string.Join(PdfStringUtils.NormalizeNewline, remainingLines);
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

        if (paragraph.HasSpans)
        {
            return RenderSpansAsync(canvas, paragraph, context);
        }

        if (!context.LayoutState.TryGetValue(paragraph, out var stateObj) || stateObj is not TextLayoutCache cache)
        {
            return Task.CompletedTask;
        }

        var linesToDraw = cache.LinesToDrawOnThisPage;
        var pdfRenderRect = cache.FinalArrangedRect;

        if (linesToDraw is null || linesToDraw.Count == 0 || pdfRenderRect is null)
        {
            cache.Font.Dispose();
            cache.Paint.Dispose();
            return Task.CompletedTask;
        }

        var (font, paint, lineAdvance, horizontalAlignment, verticalTextAlignment, textDecorations, visualTopOffset) =
            (cache.Font, cache.Paint, cache.LineAdvance, cache.HorizontalAlignment,
             cache.VerticalTextAlignment, cache.TextDecorations, cache.VisualTopOffset);

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
            font.MeasureText(linesToDraw[0], out bounds);
            actualTextHeight = bounds.Height;
        }
        else
        {
            SKRect lastBounds = new();
            font.MeasureText(linesToDraw[^1], out lastBounds);
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

        for (int i = 0; i < linesToDraw.Count; i++)
        {
            string line = linesToDraw[i];
            float measuredWidth = font.MeasureText(line);
            float drawX = contentRect.Left;

            bool isLastLine = (i == linesToDraw.Count - 1);
            bool shouldJustify = horizontalAlignment is TextAlignment.Justify && !isLastLine && line.Contains(' ');

            if (shouldJustify)
            {
                DrawJustifiedLine(canvas, line, contentRect.Left, contentRect.Width, baselineY, font, paint);
                if (textDecorations is not TextDecorations.None)
                {
                    DrawTextDecorations(canvas, line, contentRect.Left, baselineY, font, paint, textDecorations);
                }
            }
            else
            {
                if (horizontalAlignment is TextAlignment.Center) drawX = contentRect.Left + (contentRect.Width - measuredWidth) / 2f;
                else if (horizontalAlignment is TextAlignment.End) drawX = contentRect.Right - measuredWidth;

                canvas.DrawText(line, drawX, baselineY, font, paint);

                if (textDecorations is not TextDecorations.None)
                {
                    DrawTextDecorations(canvas, line, drawX, baselineY, font, paint, textDecorations);
                }
            }

            baselineY += lineAdvance;
        }

        canvas.Restore();

        font.Dispose();
        paint.Dispose();
        return Task.CompletedTask;
    }

    private void DrawJustifiedLine(SKCanvas canvas, string line, float x, float totalWidth, float y, SKFont font, SKPaint paint)
    {
        string[] words = line.Split(' ');
        if (words.Length <= 1)
        {
            canvas.DrawText(line, x, y, font, paint);
            return;
        }

        float totalWordWidth = words.Sum(w => font.MeasureText(w));
        float spaceWidth = font.MeasureText(" ");
        float totalSpaceWidth = (words.Length - 1) * spaceWidth;
        float extraSpace = totalWidth - totalWordWidth - totalSpaceWidth;

        if (extraSpace < 0)
        {
            canvas.DrawText(line, x, y, font, paint);
            return;
        }

        float extraSpacePerGap = extraSpace / (words.Length - 1);
        float currentX = x;

        for (int i = 0; i < words.Length; i++)
        {
            canvas.DrawText(words[i], currentX, y, font, paint);
            currentX += font.MeasureText(words[i]);

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

    private void DrawTextDecorations(SKCanvas canvas, string text, float x, float baselineY, SKFont font, SKPaint paint, TextDecorations decorations)
    {
        float width = font.MeasureText(text);
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

    private List<string> WrapTextToLines(string text, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text)) return lines;

        string normalizedText = PdfStringUtils.NormalizeNewlines(text);
        string[] textSegments = normalizedText.Split(PdfStringUtils.NormalizeNewline);

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
        if (maxWidth < ellipsisWidth && maxWidth > 0) return Ellipsis[..(int)font.BreakText(Ellipsis, maxWidth)];
        if (maxWidth <= 0) return string.Empty;

        float availableWidthForText = maxWidth - ellipsisWidth;
        if (availableWidthForText < 0) availableWidthForText = 0;

        if (lineBreakMode is LineBreakMode.TailTruncation)
        {
            long count = font.BreakText(textSegment, availableWidthForText);
            return string.Concat(textSegment.AsSpan(0, (int)Math.Max(0, count)), Ellipsis);
        }
        if (lineBreakMode is LineBreakMode.HeadTruncation)
        {
            int textLength = textSegment.Length;
            int startIndex = textLength;
            for (int i = 1; i <= textLength; i++)
            {
                string sub = textSegment[(textLength - i)..];
                if (font.MeasureText(sub) <= availableWidthForText)
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
            long startCount = font.BreakText(textSegment, startWidth);

            int textLength = textSegment.Length;
            string tempEndString = "";
            for (int i = 1; i <= textLength - (int)startCount; i++)
            {
                string sub = textSegment[(textLength - i)..];
                if (font.MeasureText(string.Concat(textSegment.AsSpan(0, (int)startCount), Ellipsis, sub)) <= maxWidth)
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
                return string.Concat(textSegment.AsSpan(0, (int)Math.Max(0, countTail)), Ellipsis);
            }
            return string.Concat(textSegment.AsSpan(0, (int)startCount), Ellipsis, tempEndString);
        }
        return textSegment;
    }

    private List<string> ApplyLineBreakModeWrapping(string singleLine, SKFont font, float maxWidth, LineBreakMode lineBreakMode)
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

    #region Multi-Span Implementation

    private async Task<PdfLayoutInfo> MeasureSpansAsync(PdfParagraphData paragraph, PdfGenerationContext context, SKSize availableSize)
    {
        var processedSpans = new List<ProcessedSpan>();
        foreach (var spanData in paragraph.Spans)
        {
            processedSpans.Add(await CreateProcessedSpanAsync(spanData, paragraph, context));
        }

        float maxWidth = paragraph.GetWidthRequest.HasValue
            ? (float)paragraph.GetWidthRequest.Value - (float)paragraph.GetPadding.HorizontalThickness
            : availableSize.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness;

        var allLines = WrapSpansToLines(processedSpans, maxWidth);

        float totalHeight = allLines.Sum(l => l.Height);
        float boxWidth = paragraph.GetWidthRequest.HasValue ? (float)paragraph.GetWidthRequest.Value : (allLines.Count > 0 ? allLines.Max(l => l.Width) : 0) + (float)paragraph.GetPadding.HorizontalThickness;
        float boxHeight = paragraph.GetHeightRequest.HasValue ? (float)paragraph.GetHeightRequest.Value : totalHeight + (float)paragraph.GetPadding.VerticalThickness;

        context.LayoutState[paragraph] = new MultiSpanLayoutCache(
            HorizontalAlignment: paragraph.CurrentHorizontalTextAlignment,
            VerticalTextAlignment: paragraph.CurrentVerticalTextAlignment,
            OriginalSpans: processedSpans,
            AllMeasuredLines: allLines
        );

        return new PdfLayoutInfo(paragraph, boxWidth + (float)paragraph.GetMargin.HorizontalThickness, boxHeight + (float)paragraph.GetMargin.VerticalThickness);
    }

    private Task<PdfLayoutInfo> ArrangeSpansAsync(PdfParagraphData paragraph, PdfRect finalRect, PdfGenerationContext context)
    {
        if (!context.LayoutState.TryGetValue(paragraph, out var stateObj) || stateObj is not MultiSpanLayoutCache cache)
            throw new InvalidOperationException("Layout cache not found for paragraph with spans.");

        var allLines = cache.AllMeasuredLines ?? [];
        float availableHeight = finalRect.Height - (float)paragraph.GetPadding.VerticalThickness;
        float currentY = 0;
        var linesForThisPage = new List<VisualLine>();

        foreach (var line in allLines)
        {
            if (currentY + line.Height <= availableHeight)
            {
                linesForThisPage.Add(line);
                currentY += line.Height;
            }
            else
            {
                break;
            }
        }

        context.LayoutState[paragraph] = cache with
        {
            LinesToDraw = linesForThisPage,
            FinalArrangedRect = finalRect
        };

        return Task.FromResult(new PdfLayoutInfo(paragraph, finalRect.Width, finalRect.Height));
    }

    private Task RenderSpansAsync(SKCanvas canvas, PdfParagraphData paragraph, PdfGenerationContext context)
    {
        if (!context.LayoutState.TryGetValue(paragraph, out var stateObj) || stateObj is not MultiSpanLayoutCache cache)
            return Task.CompletedTask;

        var lines = cache.LinesToDraw ?? [];
        var rect = cache.FinalArrangedRect ?? PdfRect.Empty;

        float currentY = rect.Top + (float)paragraph.GetPadding.Top;

        foreach (var line in lines)
        {
            float currentX = rect.Left + (float)paragraph.GetPadding.Left;

            // Simple horizontal alignment for the whole line
            if (cache.HorizontalAlignment == TextAlignment.Center)
                currentX += (rect.Width - (float)paragraph.GetPadding.HorizontalThickness - line.Width) / 2;
            else if (cache.HorizontalAlignment == TextAlignment.End)
                currentX += rect.Width - (float)paragraph.GetPadding.HorizontalThickness - line.Width;

            foreach (var fragment in line.Fragments)
            {
                canvas.DrawText(fragment.Text, currentX, currentY + line.BaselineAscent, fragment.SourceSpan.Font, fragment.SourceSpan.Paint);
                
                if (fragment.SourceSpan.TextDecorations != TextDecorations.None)
                {
                    DrawTextDecorations(canvas, fragment.Text, currentX, currentY + line.BaselineAscent, fragment.SourceSpan.Font, fragment.SourceSpan.Paint, fragment.SourceSpan.TextDecorations);
                }

                currentX += fragment.Width;
            }

            currentY += line.Height;
        }

        DisposeSpans(cache.OriginalSpans);
        return Task.CompletedTask;
    }

    private async Task<ProcessedSpan> CreateProcessedSpanAsync(PdfSpanData span, PdfParagraphData paragraph, PdfGenerationContext context)
    {
        float fontSize = span.FontSizeProp.Value ?? paragraph.CurrentFontSize;
        Color textColor = span.TextColorProp.Value ?? paragraph.CurrentTextColor ?? PdfParagraphData.DefaultTextColor;
        FontAttributes fontAttrs = span.FontAttributesProp.Value ?? paragraph.CurrentFontAttributes ?? PdfParagraphData.DefaultFontAttributes;
        TextDecorations textDecs = span.TextDecorationsProp.Value ?? paragraph.CurrentTextDecorations ?? PdfParagraphData.DefaultTextDecorations;
        
        PdfFontIdentifier? family = span.FontFamilyProp.Value ?? paragraph.CurrentFontFamily;
        var fontReg = family.HasValue ? context.FontRegistry.GetFontRegistration(family.Value) : paragraph.ResolvedFontRegistration;

        string? filePathToLoad = fontReg?.FilePath;
        string skiaFontAlias = family?.Alias ?? string.Empty;

        var typeface = await SkiaUtils.CreateSkTypefaceAsync(
            skiaFontAlias,
            fontAttrs,
            async (fileName) => 
            {
                if (string.IsNullOrEmpty(fileName)) return null;
                try { return await FileSystem.OpenAppPackageFileAsync(fileName); }
                catch { return null; }
            },
            filePathToLoad
        );

        var font = new SKFont(typeface, fontSize);
        var paint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(textColor), IsAntialias = true };

        return new ProcessedSpan(span.Text, font, paint, textDecs, font.MeasureText(" "));
    }

    private List<VisualLine> WrapSpansToLines(List<ProcessedSpan> spans, float maxWidth)
    {
        var lines = new List<VisualLine>();
        var currentFragments = new List<VisualSpanFragment>();
        float currentLineWidth = 0;

        foreach (var span in spans)
        {
            var words = span.Text.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (i < words.Length - 1) word += " ";
                
                float wordWidth = span.Font.MeasureText(word);

                if (currentLineWidth + wordWidth > maxWidth && currentFragments.Count > 0)
                {
                    lines.Add(CreateVisualLine(currentFragments));
                    currentFragments = new List<VisualSpanFragment>();
                    currentLineWidth = 0;
                }

                currentFragments.Add(new VisualSpanFragment(word, span, wordWidth));
                currentLineWidth += wordWidth;
            }
        }

        if (currentFragments.Count > 0)
        {
            lines.Add(CreateVisualLine(currentFragments));
        }

        return lines;
    }

    private VisualLine CreateVisualLine(List<VisualSpanFragment> fragments)
    {
        float width = fragments.Sum(f => f.Width);
        float maxAscent = 0;
        float maxDescent = 0;

        foreach (var f in fragments)
        {
            var metrics = f.SourceSpan.Font.Metrics;
            maxAscent = Math.Max(maxAscent, -metrics.Ascent);
            maxDescent = Math.Max(maxDescent, metrics.Descent);
        }

        return new VisualLine(fragments, width, maxAscent + maxDescent, maxAscent, maxDescent);
    }

    private void DisposeSpans(List<ProcessedSpan>? spans)
    {
        if (spans == null) return;
        foreach (var span in spans)
        {
            span.Font?.Dispose();
            span.Paint?.Dispose();
        }
    }

    #endregion
}
