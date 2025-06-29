using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Models;

internal readonly record struct MeasureOutput(
    float HeightRequired,
    float VisualHeight,
    float WidthRequired,
    List<string> Lines,
    PdfElement? RemainingElement,
    bool RequiresNewPage,
    float LineSpacing,
    float ContentTopY,
    float AvailableWidth,
    float AvailableHeight,
    PdfGenerationException? Error
);
