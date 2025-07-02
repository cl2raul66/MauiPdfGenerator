using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Models;

internal readonly record struct LayoutInfo(
    PdfElement Element,
    float Width,
    float Height,
    PdfElement? RemainingElement = null,
    PdfGenerationException? Error = null
);
