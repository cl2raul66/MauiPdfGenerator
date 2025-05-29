using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models; // Asegurar using

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

internal interface IPdfContentPageBuilder : IPdfPageBuilder
{
    PageSizeType GetEffectivePageSize();
    Thickness GetEffectiveMargin();
    PageOrientationType GetEffectivePageOrientation();
    Color? GetEffectiveBackgroundColor();
    IReadOnlyList<PdfElement> GetElements();
    float GetPageSpacing();
    // GetPageDefaultFontFamily ahora devuelve PdfFontIdentifier?
    PdfFontIdentifier? GetPageDefaultFontFamily();
    float GetPageDefaultFontSize();
    Color GetPageDefaultTextColor();
    FontAttributes GetPageDefaultFontAttributes();
}
