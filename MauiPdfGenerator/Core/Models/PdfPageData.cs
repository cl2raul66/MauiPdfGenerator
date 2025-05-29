using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models; // Asegurar using

namespace MauiPdfGenerator.Core.Models;

internal record PdfPageData(
    PageSizeType Size,
    PageOrientationType Orientation,
    Thickness Margins,
    Color? BackgroundColor,

    IReadOnlyList<PdfElement> Elements,
    float PageDefaultSpacing,
    // PageDefaultFontFamily es ahora PdfFontIdentifier?
    PdfFontIdentifier? PageDefaultFontFamily,
    float PageDefaultFontSize,
    Color PageDefaultTextColor,
    FontAttributes PageDefaultFontAttributes
);
