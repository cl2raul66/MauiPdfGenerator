using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Models;

internal readonly record struct LayoutInfo(
    object Element,
    float Width,
    float Height,
    PdfElement? RemainingElement = null
);
