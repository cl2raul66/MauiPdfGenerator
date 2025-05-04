using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

internal interface IPdfContentPageBuilder : IPdfPageBuilder
{
    PageSizeType GetEffectivePageSize();

    Thickness GetEffectiveMargin();

    PageOrientationType GetEffectivePageOrientation();

    Color? GetEffectiveBackgroundColor();

    IReadOnlyList<PdfElement> GetElements();

    float GetPageSpacing();

    string GetPageDefaultFontFamily();

    float GetPageDefaultFontSize();

    Color GetPageDefaultTextColor();

    FontAttributes GetPageDefaultFontAttributes();
}
