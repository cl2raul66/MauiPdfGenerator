using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Pages;

internal record PdfContentPageData(
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
    TextTransform PageDefaultTextTransform,
    string Culture
) : PdfPageData(
    Size, Orientation, Padding, BackgroundColor,
    PageDefaultFontFamily, PageDefaultFontSize, PageDefaultTextColor,
    PageDefaultFontAttributes, PageDefaultTextDecorations, PageDefaultTextTransform, Culture
);
