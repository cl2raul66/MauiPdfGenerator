using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Common.Utils;

internal class PdfPagePaddingTypeCalculator
{
    private const float NormalPadding = 72f;
    private const float NarrowPadding = 36f;
    private const float ModeratePaddingV = 54f;
    private const float ModeratePaddingH = NormalPadding;
    private const float WidePaddingH = 144f;
    private const float WidePaddingV = NormalPadding;
    internal static Thickness GetThickness(DefaultPagePaddingType defaultMarginType)
    {
        return defaultMarginType switch
        {
            DefaultPagePaddingType.Normal => new Thickness(NormalPadding),
            DefaultPagePaddingType.Narrow => new Thickness(NarrowPadding),
            DefaultPagePaddingType.Moderate => new Thickness(ModeratePaddingH, ModeratePaddingV),
            DefaultPagePaddingType.Wide => new Thickness(WidePaddingH, WidePaddingV),
            _ => new Thickness(NormalPadding)
        };
    }
}
