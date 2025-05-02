using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Builders;

internal interface IPdfContentPageBuilder
{
    PageSizeType GetEffectivePageSize();

    Thickness GetEffectiveMargin();

    PageOrientationType GetEffectivePageOrientation();

    Color? GetEffectiveBackgroundColor();

    string? GetEffectiveDefaultFontAlias();

    Action<IPdfContentPage>? GetContentPage();
}
