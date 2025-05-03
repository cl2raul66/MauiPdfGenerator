using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Builders;

internal interface IPdfContentPageBuilder : IPdfPageBuilder
{
    PageSizeType GetEffectivePageSize();

    Thickness GetEffectiveMargin();

    PageOrientationType GetEffectivePageOrientation();

    Color? GetEffectiveBackgroundColor();

    string? GetEffectiveDefaultFontAlias();


    IReadOnlyList<PdfElement> GetElements();

    float GetPageSpacing();

    string GetPageDefaultFontFamily();

    float GetPageDefaultFontSize();

    Color GetPageDefaultTextColor();
}
