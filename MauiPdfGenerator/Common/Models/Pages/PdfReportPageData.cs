using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Pages;

internal record PdfReportPageData(
    PageSizeType Size,
    PageOrientationType Orientation,
    Thickness Padding,
    Color? BackgroundColor,
    PdfLayoutElementData? Header,
    PdfPageOccurrence HeaderOccurrence,
    PdfLayoutElementData? Footer,
    PdfPageOccurrence FooterOccurrence,
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
