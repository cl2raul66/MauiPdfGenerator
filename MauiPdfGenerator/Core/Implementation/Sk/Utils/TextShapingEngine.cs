using MauiPdfGenerator.Core.Implementation.Sk.Models;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System.Diagnostics;
using System.Text;

namespace MauiPdfGenerator.Core.Implementation.Sk.Utils;

internal class TextShapingEngine
{
    private readonly SpanRun[] _spanRuns;
    private readonly SKFont _defaultFont;
    private readonly SKPaint _defaultPaint;
    private readonly string _originalText;
    private readonly bool _isRtlParagraph;

    public TextShapingEngine(SpanRun[] spanRuns, SKFont defaultFont, SKPaint defaultPaint, string originalText, bool isRtlParagraph)
    {
        _spanRuns = spanRuns;
        _defaultFont = defaultFont;
        _defaultPaint = defaultPaint;
        _originalText = originalText;
        _isRtlParagraph = isRtlParagraph;
    }

    public float MeasureTextWidth(string text, int lineStartIndex)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var segments = GetSegmentsForLine(text, lineStartIndex);
        float totalWidth = 0;

        foreach (var segment in segments)
        {
            var run = segment.Run;
            var font = run?.Font ?? _defaultFont;

            string textToShape = segment.Text;
            if (run?.Transform is not null)
            {
                textToShape = ApplyTransform(textToShape, run.Transform.Value);
            }

            if (segment.IsRtl)
            {
                using var shaper = new SKShaper(font.Typeface);
                var result = shaper.Shape(textToShape, 0, 0, font);
                totalWidth += result.Width;
            }
            else
            {
                totalWidth += font.MeasureText(textToShape);
            }
        }

        return totalWidth;
    }

    public float MeasureTextWidth(string text)
    {
        return MeasureTextWidth(text, 0);
    }

    public void DrawShapedLine(SKCanvas canvas, string lineText, float x, float y, int lineStartIndex, TextDecorations paragraphDecorations)
    {
        if (string.IsNullOrEmpty(lineText)) return;

        var segments = GetSegmentsForLine(lineText, lineStartIndex);

        float currentX = x;

        if (_isRtlParagraph)
        {
            float totalLineWidth = 0;

            var segmentWidths = new float[segments.Count];

            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                var run = seg.Run;
                var font = run?.Font ?? _defaultFont;

                string txt = seg.Text;
                if (run?.Transform is not null) txt = ApplyTransform(txt, run.Transform.Value);

                if (seg.IsRtl)
                {
                    using var shaper = new SKShaper(font.Typeface);
                    var result = shaper.Shape(txt, 0, 0, font);
                    segmentWidths[i] = result.Width;
                }
                else
                {
                    segmentWidths[i] = font.MeasureText(txt);
                }

                totalLineWidth += segmentWidths[i];
            }

            currentX = x + totalLineWidth;

            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                float segWidth = segmentWidths[i];

                currentX -= segWidth;

                DrawSegment(canvas, segment, currentX, y, paragraphDecorations);
            }
        }
        else
        {
            foreach (var segment in segments)
            {
                float segWidth = DrawSegment(canvas, segment, currentX, y, paragraphDecorations);
                currentX += segWidth;
            }
        }
    }

    private float DrawSegment(SKCanvas canvas, TextSegment segment, float x, float y, TextDecorations paragraphDecorations)
    {
        var run = segment.Run;
        var font = run?.Font ?? _defaultFont;
        var paint = run?.Paint ?? _defaultPaint;
        var decorations = run?.Decorations ?? paragraphDecorations;

        string textToDraw = segment.Text;
        if (run?.Transform is not null)
        {
            textToDraw = ApplyTransform(textToDraw, run.Transform.Value);
        }

        float width;

        if (segment.IsRtl)
        {
            using var shaper = new SKShaper(font.Typeface);
            canvas.DrawShapedText(shaper, textToDraw, x, y, SKTextAlign.Left, font, paint);
            var result = shaper.Shape(textToDraw, x, y, font);
            width = result.Width;
        }
        else
        {
            canvas.DrawText(textToDraw, x, y, font, paint);
            width = font.MeasureText(textToDraw);
        }

        if (decorations is not TextDecorations.None)
        {
            DrawTextDecorations(canvas, font, paint, decorations, x, y, width);
        }

        return width;
    }

    private List<TextSegment> GetSegmentsForLine(string lineText, int lineStartIndex)
    {
        var segments = new List<TextSegment>();
        int lineEndIndex = lineStartIndex + lineText.Length;
        int currentPos = lineStartIndex;

        while (currentPos < lineEndIndex)
        {
            var run = GetRunAtAbsoluteIndex(currentPos);
            int spanEnd = run is not null ? Math.Min(run.EndIndex, lineEndIndex) : lineEndIndex;

            if (run is null)
            {
                int nextRunStart = lineEndIndex;

                var runsSpan = _spanRuns.AsSpan();
                foreach (var r in runsSpan)
                {
                    if (r.StartIndex > currentPos && r.StartIndex < nextRunStart)
                    {
                        nextRunStart = r.StartIndex;
                    }
                }
                spanEnd = nextRunStart;
            }

            char startChar = lineText[currentPos - lineStartIndex];

            if (IsOpeningBracket(startChar))
            {
                int closingIndex = FindMatchingClosingBracket(lineText, currentPos - lineStartIndex, spanEnd - lineStartIndex);

                if (closingIndex != -1)
                {
                    bool contentIsRtl = AnalyzeContentDirection(lineText, currentPos - lineStartIndex + 1, closingIndex);

                    bool blockIsRtl = contentIsRtl;

                    int length = (closingIndex + 1) - (currentPos - lineStartIndex);

                    string blockText = lineText[(currentPos - lineStartIndex)..(currentPos - lineStartIndex + length)];

                    segments.Add(new TextSegment(blockText, run, blockIsRtl));

                    currentPos += length;
                    continue;
                }
            }

            int segmentEnd = currentPos;
            bool isStartRtl = IsStrongRtlChar(startChar);

            if (IsNeutralChar(startChar) && !IsRtlMirrorable(startChar))
            {
                isStartRtl = _isRtlParagraph;
            }

            for (int i = currentPos; i < spanEnd; i++)
            {
                char c = lineText[i - lineStartIndex];

                if (IsOpeningBracket(c))
                {
                    segmentEnd = i;
                    break;
                }

                if (IsNeutralChar(c))
                {
                    if (isStartRtl)
                    {
                        segmentEnd = i + 1;
                        continue;
                    }

                    if (!isStartRtl && _isRtlParagraph)
                    {
                        bool? nextStrongIsRtl = LookaheadIsNextStrongRtl(lineText, i + 1 - lineStartIndex, spanEnd - lineStartIndex);
                        if (nextStrongIsRtl == true || nextStrongIsRtl is null)
                        {
                            segmentEnd = i;
                            break;
                        }
                    }
                    segmentEnd = i + 1;
                    continue;
                }

                bool isCurrentCharRtl = IsStrongRtlChar(c);

                if (isCurrentCharRtl != isStartRtl)
                {
                    segmentEnd = i;
                    break;
                }

                segmentEnd = i + 1;
            }

            if (segmentEnd <= currentPos) segmentEnd = currentPos + 1;

            int lengthSeg = segmentEnd - currentPos;
            if (lengthSeg > 0)
            {
                int relativeStart = currentPos - lineStartIndex;

                string segmentText = lineText[relativeStart..(relativeStart + lengthSeg)];
                segments.Add(new TextSegment(segmentText, run, isStartRtl));
            }

            currentPos = segmentEnd;
        }

        return segments;
    }

    private int FindMatchingClosingBracket(string text, int openIndex, int limitIndex)
    {
        char openChar = text[openIndex];
        char expectedClose = GetMatchingClose(openChar);
        int depth = 1;

        for (int i = openIndex + 1; i < limitIndex; i++)
        {
            char c = text[i];
            if (c == openChar) depth++;
            else if (c == expectedClose)
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private char GetMatchingClose(char open)
    {
        return open switch
        {
            '(' => ')',
            '[' => ']',
            '{' => '}',
            '<' => '>',
            _ => ' '
        };
    }

    private bool AnalyzeContentDirection(string text, int start, int end)
    {
        bool hasStrongLtr = false;

        for (int i = start; i < end; i++)
        {
            char c = text[i];
            if (IsStrongRtlChar(c)) return true;
            if (IsStrongLtrChar(c)) hasStrongLtr = true;
        }

        if (hasStrongLtr) return false;

        return _isRtlParagraph;
    }

    private static bool? LookaheadIsNextStrongRtl(string text, int startIndex, int endIndex)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            char c = text[i];
            if (IsRtlMirrorable(c)) return true;
            if (IsNeutralChar(c)) continue;
            return IsStrongRtlChar(c);
        }
        return null;
    }

    private static bool IsStrongRtlChar(char c)
    {
        return (c >= 0x0590 && c <= 0x05FF) ||
               (c >= 0x0600 && c <= 0x06FF) ||
               (c >= 0x0750 && c <= 0x077F) ||
               (c >= 0x08A0 && c <= 0x08FF) ||
               (c >= 0xFB50 && c <= 0xFDFF) ||
               (c >= 0xFE70 && c <= 0xFEFF);
    }

    private static bool IsStrongLtrChar(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
    }

    private static bool IsOpeningBracket(char c)
    {
        return c == '(' || c == '[' || c == '{' || c == '<';
    }

    private static bool IsRtlMirrorable(char c)
    {
        return c == '(' || c == ')' || c == '[' || c == ']' || c == '{' || c == '}' || c == '<' || c == '>';
    }

    private static bool IsNeutralChar(char c)
    {
        if (c == '%' || c == '‰' || c == '$' || c == '€' || c == '£' || c == '₪') return false;
        return char.IsWhiteSpace(c) || (char.IsPunctuation(c) && !IsRtlMirrorable(c)) || char.IsSymbol(c);
    }

    private SpanRun? GetRunAtAbsoluteIndex(int absoluteIndex)
    {
        var span = _spanRuns.AsSpan();
        foreach (var run in span)
        {
            if (absoluteIndex >= run.StartIndex && absoluteIndex < run.EndIndex)
            {
                return run;
            }
        }
        return null;
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

    public bool HasMultipleFonts => _spanRuns.Length > 1 || (_spanRuns.Length == 1 && _spanRuns[0].Font.Typeface != _defaultFont.Typeface);

    private record TextSegment(string Text, SpanRun? Run, bool IsRtl);
}
