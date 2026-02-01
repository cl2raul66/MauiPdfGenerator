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
