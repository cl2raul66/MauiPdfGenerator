using MauiPdfGenerator.Common.Models;

namespace MauiPdfGenerator.Core.Models;

internal readonly record struct LayoutInfo(
    object Element,
    float Width,
    float Height,
    PdfElementData? RemainingElement = null
);
