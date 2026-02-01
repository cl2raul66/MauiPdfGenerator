using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Models;

internal record SpanRun(
    int StartIndex,
    int EndIndex,
    SKFont Font,
    SKPaint Paint,
    TextDecorations? Decorations,
    TextTransform? Transform
);

internal record LineRunInfo(
    string Line,
    int LineStartIndex,
    List<SpanRun> IntersectingRuns
)
{
    public bool HasMultipleRuns => IntersectingRuns.Count > 1;
}
