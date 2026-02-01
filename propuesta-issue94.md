El problema **no está en `PdfSpanBuilder.cs`** (ese código es correcto y está guardando las propiedades en el modelo), sino en cómo el motor de renderizado (`TextRenderer` y `MultiFontTextRenderer`) consume esas propiedades.

Actualmente ocurren dos cosas:
1.  **TextTransform (Uppercase):** El `MultiFontTextRenderer` dibuja el texto original del párrafo carácter por carácter, ignorando la propiedad `Transform` que guardaste en el `SpanRun`.
2.  **FontAttributes (Bold/Italic):** SkiaSharp es estricto. Si solicitas "Comic Sans" + "Bold" y no tienes registrado el archivo de fuente específico (ej. `ComicSansBold.ttf`), Skia devuelve la fuente normal. A diferencia de MAUI (UI), Skia no aplica "falso negrita" (Fake Bold) o "falso itálica" (Skew) automáticamente; hay que configurarlo explícitamente en el `SKPaint`.

Aquí tienes la solución completa implementando la lógica de renderizado faltante.

### 1. Solución para TextTransform (`MultiFontTextRenderer.cs`)

Este archivo necesita aplicar la transformación al texto antes de medirlo o dibujarlo.

```csharp
// Filename: MultiFontTextRenderer.cs
using MauiPdfGenerator.Core.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Views;

internal class MultiFontTextRenderer
{
    private readonly List<SpanRun> _spanRuns;
    private readonly SKFont _defaultFont;
    private readonly SKPaint _defaultPaint;
    private readonly string _originalText;

    public MultiFontTextRenderer(List<SpanRun> spanRuns, SKFont defaultFont, SKPaint defaultPaint, string originalText)
    {
        _spanRuns = spanRuns;
        _defaultFont = defaultFont;
        _defaultPaint = defaultPaint;
        _originalText = originalText;
    }

    public float MeasureTextWidth(string text, int lineStartIndex)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        float totalWidth = 0;

        for (int i = 0; i < text.Length; i++)
        {
            int absoluteIndex = lineStartIndex + i;
            // Pasamos el caracter original para buscar el run, pero medimos el transformado
            totalWidth += GetCharWidth(absoluteIndex, text[i]);
        }

        return totalWidth;
    }

    public float MeasureTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return MeasureTextWidth(text, 0);
    }

    // Modificado para aceptar el char y aplicar transformación
    private float GetCharWidth(int absoluteIndex, char originalChar)
    {
        var run = GetRunAtAbsoluteIndex(absoluteIndex);
        SKFont font = run?.Font ?? _defaultFont;
        
        string textToMeasure = originalChar.ToString();
        
        // Aplicar transformación si existe en el run
        if (run?.Transform.HasValue == true)
        {
            textToMeasure = ApplyTransform(textToMeasure, run.Transform.Value);
        }

        return font.MeasureText(textToMeasure);
    }

    private SpanRun? GetRunAtAbsoluteIndex(int absoluteIndex)
    {
        foreach (var run in _spanRuns)
        {
            if (absoluteIndex >= run.StartIndex && absoluteIndex < run.EndIndex)
            {
                return run;
            }
        }
        return null;
    }

    public void DrawText(SKCanvas canvas, string text, float x, float y, int lineStartIndex)
    {
        if (string.IsNullOrEmpty(text))
            return;

        float currentX = x;

        for (int i = 0; i < text.Length; i++)
        {
            int absoluteIndex = lineStartIndex + i;
            var run = GetRunAtAbsoluteIndex(absoluteIndex);
            SKFont font = run?.Font ?? _defaultFont;
            SKPaint paint = run?.Paint ?? _defaultPaint;

            string charText = text[i].ToString();

            // ✅ FIX: Aplicar transformación antes de dibujar
            if (run?.Transform.HasValue == true)
            {
                charText = ApplyTransform(charText, run.Transform.Value);
            }

            float charWidth = font.MeasureText(charText);

            canvas.DrawText(charText, currentX, y, font, paint);

            currentX += charWidth;
        }
    }

    public void DrawTextWithDecorations(SKCanvas canvas, string text, float x, float y, int lineStartIndex, TextDecorations paragraphDecorations)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // Dibujamos el texto base (que ya aplica transformaciones internamente en DrawText)
        DrawText(canvas, text, x, y, lineStartIndex);

        float currentX = x;

        for (int i = 0; i < text.Length; i++)
        {
            int absoluteIndex = lineStartIndex + i;
            var run = GetRunAtAbsoluteIndex(absoluteIndex);
            TextDecorations decorations = run?.Decorations ?? paragraphDecorations;

            string charText = text[i].ToString();
            
            // ✅ FIX: Necesitamos el texto transformado para medir correctamente el ancho de la decoración
            if (run?.Transform.HasValue == true)
            {
                charText = ApplyTransform(charText, run.Transform.Value);
            }

            SKFont font = run?.Font ?? _defaultFont;
            float charWidth = font.MeasureText(charText);

            if (decorations is not TextDecorations.None)
            {
                SKPaint paint = run?.Paint ?? _defaultPaint;
                DrawTextDecorations(canvas, font, paint, decorations, currentX, y, charWidth);
            }

            currentX += charWidth;
        }
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

    private string ApplyTransform(string text, TextTransform transform)
    {
        return transform switch
        {
            TextTransform.Uppercase => text.ToUpperInvariant(),
            TextTransform.Lowercase => text.ToLowerInvariant(),
            _ => text
        };
    }

    public bool HasMultipleFonts => _spanRuns.Count > 1 ||
        (_spanRuns.Count == 1 && _spanRuns[0].Font.Typeface != _defaultFont.Typeface);
}
```

### 2. Solución para FontAttributes (`TextRenderer.cs`)

Este archivo necesita activar `IsFakeBoldText` y `TextSkewX` (para itálica) cuando la fuente cargada no soporta nativamente el estilo solicitado.

```csharp
// Filename: TextRenderer.cs
using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;
using MauiPdfGenerator.Fluent.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.HarfBuzz;

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
        List<SpanRun>? SpanRuns = null
    );

    public async Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKSize availableSize)
    {
        if (context.Element is not PdfParagraphData paragraph)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfParagraphData)} or is null.");

        var (font, paint, textToRender, horizontalAlignment, verticalTextAlignment, lineBreakMode, textDecorations, textTransform) = await GetTextPropertiesAsync(paragraph, context);

        float widthForMeasure = paragraph.GetWidthRequest.HasValue
            ? (float)paragraph.GetWidthRequest.Value - (float)paragraph.GetPadding.HorizontalThickness
            : availableSize.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness;

        var spanRuns = new List<SpanRun>();
        MultiFontTextRenderer? multiFontRenderer = null;

        if (paragraph.HasSpans)
        {
            spanRuns = await ResolveSpanRunsAsync(paragraph, context, font, paint);
            if (spanRuns.Count > 0)
            {
                multiFontRenderer = new MultiFontTextRenderer(spanRuns, font, paint, textToRender);
            }
        }

        var allLines = WrapTextToLinesWithIndex(textToRender, font, widthForMeasure, lineBreakMode, multiFontRenderer);

        SKFontMetrics fontMetrics = font.Metrics;
        float lineAdvance = -fontMetrics.Ascent + fontMetrics.Descent;

        float visualTopOffset = 0;
        float visualBottomOffset = 0;

        if (allLines.Count != 0)
        {
            SKRect firstLineBounds = new();
            font.MeasureText(allLines[0].Line, out firstLineBounds);
            visualTopOffset = -firstLineBounds.Top;

            SKRect lastLineBounds = new();
            font.MeasureText(allLines[^1].Line, out lastLineBounds);
            visualBottomOffset = lastLineBounds.Bottom;
        }

        float totalTextHeight = 0;
        if (allLines.Count != 0)
        {
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

        float contentWidth = allLines.Count != 0 ? allLines.Max(line => font.MeasureText(line.Line)) : 0;

        float boxWidth = paragraph.GetWidthRequest.HasValue ? (float)paragraph.GetWidthRequest.Value : contentWidth + (float)paragraph.GetPadding.HorizontalThickness;
        float boxHeight = paragraph.GetHeightRequest.HasValue ? (float)paragraph.GetHeightRequest.Value : totalTextHeight + (float)paragraph.GetPadding.VerticalThickness;

        context.LayoutState[paragraph] = new TextLayoutCache(
            font, paint, horizontalAlignment, verticalTextAlignment, textDecorations,
            lineBreakMode, textToRender,
            LinesToDrawOnThisPage: allLines,
            LineAdvance: lineAdvance,
            VisualTopOffset: visualTopOffset, VisualBottomOffset: visualBottomOffset,
            SpanRuns: spanRuns);

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
        var multiFontRenderer = baseCache.SpanRuns?.Count > 0
            ? new MultiFontTextRenderer(baseCache.SpanRuns, baseCache.Font, baseCache.Paint, baseCache.TransformedText)
            : null;

        var allLines = WrapTextToLinesWithIndex(baseCache.TransformedText, baseCache.Font,
            finalRect.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness,
            baseCache.LineBreakMode, multiFontRenderer);

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
        var multiFontRenderer = baseCache.SpanRuns?.Count > 0
            ? new MultiFontTextRenderer(baseCache.SpanRuns, baseCache.Font, baseCache.Paint, baseCache.TransformedText)
            : null;

        var allLines = WrapTextToLinesWithIndex(baseCache.TransformedText, baseCache.Font,
            finalRect.Width - (float)paragraph.GetMargin.HorizontalThickness - (float)paragraph.GetPadding.HorizontalThickness,
            baseCache.LineBreakMode, multiFontRenderer);

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

        var multiFontRenderer = spanRuns?.Count > 0 ? new MultiFontTextRenderer(spanRuns, font, paint, textCache.TransformedText) : null;

        bool isRTL = IsRTLCulture(context.PageData.Culture);
        SKShaper? shaper = isRTL ? new SKShaper(font.Typeface) : null;

        for (int i = 0; i < linesToDraw.Count; i++)
        {
            var (line, startIndex) = linesToDraw[i];
            float measuredWidth = multiFontRenderer?.MeasureTextWidth(line, startIndex) ?? font.MeasureText(line);
            float drawX = contentRect.Left;

            bool isLastLine = (i == linesToDraw.Count - 1);
            bool shouldJustify = horizontalAlignment is TextAlignment.Justify && !isLastLine && line.Contains(' ');

            if (shouldJustify)
            {
                DrawJustifiedLine(canvas, line, contentRect.Left, contentRect.Width, baselineY, font, paint, multiFontRenderer, startIndex);
                if (textDecorations is not TextDecorations.None)
                {
                    DrawTextDecorations(canvas, font, paint, textDecorations, contentRect.Left, baselineY, contentRect.Width);
                }
            }
            else
            {
                if (isRTL && shaper != null)
                {
                    float x = contentRect.Right;
                    canvas.DrawShapedText(shaper, line, x, baselineY, SKTextAlign.Right, font, paint);
                }
                else
                {
                    if (horizontalAlignment is TextAlignment.Center) drawX = contentRect.Left + (contentRect.Width - measuredWidth) / 2f;
                    else if (horizontalAlignment is TextAlignment.End) drawX = contentRect.Right - measuredWidth;

                    if (multiFontRenderer != null)
                    {
                        multiFontRenderer.DrawTextWithDecorations(canvas, line, drawX, baselineY, startIndex, textDecorations);
                    }
                    else
                    {
                        canvas.DrawText(line, drawX, baselineY, font, paint);

                        if (textDecorations is not TextDecorations.None)
                        {
                            DrawTextDecorations(canvas, font, paint, textDecorations, drawX, baselineY, measuredWidth);
                        }
                    }
                }
            }

            baselineY += lineAdvance;
        }

        canvas.Restore();

        shaper?.Dispose();
        font.Dispose();
        paint.Dispose();
        return Task.CompletedTask;
    }

    private static bool IsRTLCulture(string culture)
    {
        if (string.IsNullOrEmpty(culture)) return false;

        try
        {
            var cultureInfo = new System.Globalization.CultureInfo(culture);
            return cultureInfo.TextInfo.IsRightToLeft;
        }
        catch
        {
            return false;
        }
    }

    private void DrawJustifiedLine(SKCanvas canvas, string line, float x, float totalWidth, float y, SKFont font, SKPaint paint, MultiFontTextRenderer? multiFontRenderer = null, int lineStartIndex = 0)
    {
        string[] words = line.Split(' ');
        if (words.Length <= 1)
        {
            if (multiFontRenderer != null)
            {
                multiFontRenderer.DrawText(canvas, line, x, y, lineStartIndex);
            }
            else
            {
                canvas.DrawText(line, x, y, font, paint);
            }
            return;
        }

        float totalWordWidth = words.Sum(w => multiFontRenderer?.MeasureTextWidth(w, lineStartIndex + line.IndexOf(w, StringComparison.Ordinal)) ?? font.MeasureText(w));
        float spaceWidth = multiFontRenderer?.MeasureTextWidth(" ", lineStartIndex + line.IndexOf(" ", StringComparison.Ordinal)) ?? font.MeasureText(" ");
        float totalSpaceWidth = (words.Length - 1) * spaceWidth;
        float extraSpace = totalWidth - totalWordWidth - totalSpaceWidth;

        if (extraSpace < 0)
        {
            if (multiFontRenderer != null)
            {
                multiFontRenderer.DrawText(canvas, line, x, y, lineStartIndex);
            }
            else
            {
                canvas.DrawText(line, x, y, font, paint);
            }
            return;
        }

        float extraSpacePerGap = extraSpace / (words.Length - 1);
        float currentX = x;

        for (int i = 0; i < words.Length; i++)
        {
            int wordStartIndex = line.IndexOf(words[i], StringComparison.Ordinal);
            if (multiFontRenderer != null)
            {
                multiFontRenderer.DrawText(canvas, words[i], currentX, y, lineStartIndex + wordStartIndex);
            }
            else
            {
                canvas.DrawText(words[i], currentX, y, font, paint);
            }
            currentX += multiFontRenderer?.MeasureTextWidth(words[i], lineStartIndex + wordStartIndex) ?? font.MeasureText(words[i]);

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

        // ✅ FIX: Aplicar Fake Bold / Fake Italic si la fuente cargada no lo soporta nativamente
        if ((fontAttributes & FontAttributes.Bold) != 0 && !typeface.IsBold)
        {
            paint.IsFakeBoldText = true;
        }
        if ((fontAttributes & FontAttributes.Italic) != 0 && !typeface.IsItalic)
        {
            paint.TextSkewX = -0.25f; // Valor estándar para simular itálica
        }

        string originalText = paragraph.Text ?? string.Empty;
        string textToRender = textTransform switch
        {
            TextTransform.Uppercase => originalText.ToUpperInvariant(),
            TextTransform.Lowercase => originalText.ToLowerInvariant(),
            _ => originalText,
        };

        return (font, paint, textToRender, horizontalAlignment, verticalTextAlignment, lineBreakMode, textDecorations, textTransform);
    }

    private List<(string Line, int StartIndex)> WrapTextToLinesWithIndex(string text, SKFont font, float maxWidth, LineBreakMode lineBreakMode, MultiFontTextRenderer? multiFontRenderer = null)
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
                currentIndex += segment.Length + 1; // +1 for newline
                continue;
            }

            if (lineBreakMode is LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation)
            {
                float segmentWidth = multiFontRenderer?.MeasureTextWidth(segment) ?? font.MeasureText(segment);
                if (segmentWidth <= maxWidth)
                {
                    lines.Add((segment, currentIndex));
                }
                else
                {
                    lines.Add((ApplyLineBreakModeTruncation(segment, font, maxWidth, lineBreakMode, multiFontRenderer), currentIndex));
                }
            }
            else
            {
                var wrappedLines = ApplyLineBreakModeWrappingWithIndex(segment, font, maxWidth, lineBreakMode, multiFontRenderer, currentIndex);
                lines.AddRange(wrappedLines);
                currentIndex += segment.Length + 1; // +1 for newline
            }
        }

        return lines;
    }

    private List<(string Line, int StartIndex)> ApplyLineBreakModeWrappingWithIndex(string singleLine, SKFont font, float maxWidth, LineBreakMode lineBreakMode, MultiFontTextRenderer? multiFontRenderer, int startIndex)
    {
        var resultingLines = new List<(string, int)>();
        if (string.IsNullOrEmpty(singleLine)) { resultingLines.Add((string.Empty, startIndex)); return resultingLines; }

        float singleLineWidth = multiFontRenderer?.MeasureTextWidth(singleLine) ?? font.MeasureText(singleLine);
        if (singleLineWidth <= maxWidth) { resultingLines.Add((singleLine, startIndex)); return resultingLines; }

        int currentPosition = 0;
        int textLength = singleLine.Length;

        while (currentPosition < textLength)
        {
            long countMeasured = multiFontRenderer != null
                ? CountCharactersInWidth(singleLine.Substring(currentPosition), maxWidth, multiFontRenderer)
                : font.BreakText(singleLine.AsSpan(currentPosition), maxWidth);
            int count = (int)countMeasured;

            if (count == 0 && currentPosition < textLength)
            {
                float charWidth = multiFontRenderer?.MeasureTextWidth(currentPosition) ?? font.MeasureText(singleLine[currentPosition].ToString());
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

    private List<string> WrapTextToLines(string text, SKFont font, float maxWidth, LineBreakMode lineBreakMode, MultiFontTextRenderer? multiFontRenderer = null)
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
                float segmentWidth = multiFontRenderer?.MeasureTextWidth(segment) ?? font.MeasureText(segment);
                if (segmentWidth <= maxWidth) lines.Add(segment);
                else lines.Add(ApplyLineBreakModeTruncation(segment, font, maxWidth, lineBreakMode, multiFontRenderer));
            }
            else
            {
                lines.AddRange(ApplyLineBreakModeWrapping(segment, font, maxWidth, lineBreakMode, multiFontRenderer));
            }
        }
        return lines;
    }

    private string ApplyLineBreakModeTruncation(string textSegment, SKFont font, float maxWidth, LineBreakMode lineBreakMode, MultiFontTextRenderer? multiFontRenderer = null)
    {
        float ellipsisWidth = multiFontRenderer?.MeasureTextWidth(Ellipsis) ?? font.MeasureText(Ellipsis);
        if (maxWidth < ellipsisWidth && maxWidth > 0) return Ellipsis[..(int)font.BreakText(Ellipsis, maxWidth)];
        if (maxWidth <= 0) return string.Empty;

        float availableWidthForText = maxWidth - ellipsisWidth;
        if (availableWidthForText < 0) availableWidthForText = 0;

        if (lineBreakMode is LineBreakMode.TailTruncation)
        {
            long count = multiFontRenderer != null ? CountCharactersInWidth(textSegment, availableWidthForText, multiFontRenderer) : font.BreakText(textSegment, availableWidthForText);
            return string.Concat(textSegment.AsSpan(0, (int)Math.Max(0, count)), Ellipsis);
        }
        if (lineBreakMode is LineBreakMode.HeadTruncation)
        {
            int textLength = textSegment.Length;
            int startIndex = textLength;
            for (int i = 1; i <= textLength; i++)
            {
                string sub = textSegment[(textLength - i)..];
                float width = multiFontRenderer?.MeasureTextWidth(sub) ?? font.MeasureText(sub);
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
            long startCount = multiFontRenderer != null ? CountCharactersInWidth(textSegment, startWidth, multiFontRenderer) : font.BreakText(textSegment, startWidth);

            int textLength = textSegment.Length;
            string tempEndString = "";
            for (int i = 1; i <= textLength - (int)startCount; i++)
            {
                string sub = textSegment[(textLength - i)..];
                string fullString = string.Concat(textSegment.AsSpan(0, (int)startCount), Ellipsis, sub);
                float width = multiFontRenderer?.MeasureTextWidth(fullString) ?? font.MeasureText(fullString);
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
                return ApplyLineBreakModeTruncation(textSegment, font, maxWidth, LineBreakMode.TailTruncation, multiFontRenderer);
            }
            if ((int)startCount >= textLength - tempEndString.Length && textLength > 0 && tempEndString.Length == 0 && ellipsisWidth > 0)
            {
                long countTail = multiFontRenderer != null ? CountCharactersInWidth(textSegment, availableWidthForText, multiFontRenderer) : font.BreakText(textSegment, availableWidthForText);
                return string.Concat(textSegment.AsSpan(0, (int)Math.Max(0, countTail)), Ellipsis);
            }
            return string.Concat(textSegment.AsSpan(0, (int)startCount), Ellipsis, tempEndString);
        }
        return textSegment;
    }

    private long CountCharactersInWidth(string text, float maxWidth, MultiFontTextRenderer multiFontRenderer)
    {
        float currentWidth = 0;
        for (int i = 0; i < text.Length; i++)
        {
            float charWidth = multiFontRenderer.MeasureTextWidth(i); // Usamos el índice relativo al string pasado
            if (currentWidth + charWidth > maxWidth)
            {
                return i;
            }
            currentWidth += charWidth;
        }
        return text.Length;
    }

    private List<string> ApplyLineBreakModeWrapping(string singleLine, SKFont font, float maxWidth, LineBreakMode lineBreakMode, MultiFontTextRenderer? multiFontRenderer = null)
    {
        var resultingLines = new List<string>();
        if (string.IsNullOrEmpty(singleLine)) { resultingLines.Add(string.Empty); return resultingLines; }

        float singleLineWidth = multiFontRenderer?.MeasureTextWidth(singleLine) ?? font.MeasureText(singleLine);
        if (singleLineWidth <= maxWidth) { resultingLines.Add(singleLine); return resultingLines; }

        int currentPosition = 0;
        int textLength = singleLine.Length;
        while (currentPosition < textLength)
        {
            long countMeasured;
            if (multiFontRenderer != null)
            {
                countMeasured = CountCharactersInWidth(singleLine.Substring(currentPosition), maxWidth, multiFontRenderer);
            }
            else
            {
                countMeasured = font.BreakText(singleLine.AsSpan(currentPosition), maxWidth);
            }
            int count = (int)countMeasured;

            if (count == 0 && currentPosition < textLength)
            {
                float charWidth = multiFontRenderer?.MeasureTextWidth(currentPosition) ?? font.MeasureText(singleLine[currentPosition].ToString());
                if (charWidth > maxWidth)
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

    #region Span Support

    private async Task<List<SpanRun>> ResolveSpanRunsAsync(
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

                // ✅ FIX: Aplicar Fake Bold / Fake Italic para Spans
                if ((fontAttributes & FontAttributes.Bold) != 0 && !typeface.IsBold)
                {
                    paint.IsFakeBoldText = true;
                }
                if ((fontAttributes & FontAttributes.Italic) != 0 && !typeface.IsItalic)
                {
                    paint.TextSkewX = -0.25f;
                }

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

        return runs;
    }

    private string ApplyTextTransform(string text, TextTransform? transform)
    {
        return transform switch
        {
            TextTransform.Uppercase => text.ToUpperInvariant(),
            TextTransform.Lowercase => text.ToLowerInvariant(),
            _ => text
        };
    }

    #endregion
}
```