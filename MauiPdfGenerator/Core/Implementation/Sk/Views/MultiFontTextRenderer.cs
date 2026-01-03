using MauiPdfGenerator.Common.Enums;
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
            totalWidth += GetCharWidth(absoluteIndex);
        }

        return totalWidth;
    }

    public float MeasureTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return MeasureTextWidth(text, 0);
    }

    public float MeasureTextWidth(int absoluteIndex)
    {
        return GetCharWidth(absoluteIndex);
    }

    private float GetCharWidth(int absoluteIndex)
    {
        if (absoluteIndex < 0 || absoluteIndex >= _originalText.Length)
            return _defaultFont.MeasureText(" ");

        char c = _originalText[absoluteIndex];

        foreach (var run in _spanRuns)
        {
            if (absoluteIndex >= run.StartIndex && absoluteIndex < run.EndIndex)
            {
                return run.Font.MeasureText(c.ToString());
            }
        }

        return _defaultFont.MeasureText(c.ToString());
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
            float charWidth = font.MeasureText(charText);

            canvas.DrawText(charText, currentX, y, font, paint);

            currentX += charWidth;
        }
    }

    public void DrawTextWithDecorations(SKCanvas canvas, string text, float x, float y, int lineStartIndex, TextDecorations paragraphDecorations)
    {
        if (string.IsNullOrEmpty(text))
            return;

        DrawText(canvas, text, x, y, lineStartIndex);

        float currentX = x;

        for (int i = 0; i < text.Length; i++)
        {
            int absoluteIndex = lineStartIndex + i;
            var run = GetRunAtAbsoluteIndex(absoluteIndex);
            TextDecorations decorations = run?.Decorations ?? paragraphDecorations;

            if (decorations != TextDecorations.None)
            {
                SKFont font = run?.Font ?? _defaultFont;
                SKPaint paint = run?.Paint ?? _defaultPaint;
                string charText = text[i].ToString();
                float charWidth = font.MeasureText(charText);

                DrawTextDecorations(canvas, font, paint, decorations, currentX, y, charWidth);
            }

            var charFont = run?.Font ?? _defaultFont;
            currentX += charFont.MeasureText(text[i].ToString());
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

    public bool HasMultipleFonts => _spanRuns.Count > 1 ||
        (_spanRuns.Count == 1 && _spanRuns[0].Font.Typeface != _defaultFont.Typeface);
}
