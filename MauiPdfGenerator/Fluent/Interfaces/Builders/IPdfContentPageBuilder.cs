using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

internal interface IPdfContentPageBuilder : IPdfPageBuilder
{
    PageSizeType GetEffectivePageSize();
    Thickness GetEffectivePadding();
    PageOrientationType GetEffectivePageOrientation();
    Color? GetEffectiveBackgroundColor();
    PdfLayoutElementData GetContent();
    PdfFontIdentifier? GetPageDefaultFontFamily();
    float GetPageDefaultFontSize();
    Color GetPageDefaultTextColor();
    FontAttributes GetPageDefaultFontAttributes();
    TextDecorations GetPageDefaultTextDecorations();
    TextTransform GetPageDefaultTextTransform();
    string GetEffectiveCulture();
    PdfResourceDictionary PageResources { get; }
}
