using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models;

internal record PdfPageData(
    PageSizeType Size,
    PageOrientationType Orientation,
    Thickness Padding,
    Color? BackgroundColor,
    PdfLayoutElementData Content, 
    PdfFontIdentifier? PageDefaultFontFamily,
    float PageDefaultFontSize,
    Color PageDefaultTextColor,
    FontAttributes PageDefaultFontAttributes,
    TextDecorations PageDefaultTextDecorations,
    TextTransform PageDefaultTextTransform
);
