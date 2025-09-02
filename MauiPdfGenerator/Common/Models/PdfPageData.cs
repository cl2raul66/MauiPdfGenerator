using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models;

internal record PdfPageData(
    PageSizeType Size,
    PageOrientationType Orientation,
    Thickness Padding,
    Color? BackgroundColor,
    IReadOnlyList<PdfElementData> Elements,
    PdfFontIdentifier? PageDefaultFontFamily,
    float PageDefaultFontSize,
    Color PageDefaultTextColor,
    FontAttributes PageDefaultFontAttributes,
    TextDecorations PageDefaultTextDecorations,
    TextTransform PageDefaultTextTransform
);
