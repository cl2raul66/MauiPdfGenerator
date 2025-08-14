using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Models;

internal record PdfPageData(
    PageSizeType Size,
    PageOrientationType Orientation,
    Thickness Padding,
    Color? BackgroundColor,
    IReadOnlyList<PdfElement> Elements,
    float PageDefaultSpacing,
    PdfFontIdentifier? PageDefaultFontFamily,
    float PageDefaultFontSize,
    Color PageDefaultTextColor,
    FontAttributes PageDefaultFontAttributes,
    TextDecorations PageDefaultTextDecorations,
    TextTransform PageDefaultTextTransform 
);
