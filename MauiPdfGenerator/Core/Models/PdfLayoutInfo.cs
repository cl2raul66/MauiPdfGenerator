using MauiPdfGenerator.Common.Models;

namespace MauiPdfGenerator.Core.Models;

internal readonly record struct PdfLayoutInfo(
    object Element,
    float Width,
    float Height,
    PdfRect? FinalRect = null,
    PdfElementData? RemainingElement = null
);
