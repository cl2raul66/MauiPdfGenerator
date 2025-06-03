using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Common.Utils;

internal class MarginCalculator
{
    private const float NormalMargin = 72f;
    private const float NarrowMargin = 36f;
    private const float ModerateMarginV = 54f;
    private const float ModerateMarginH = NormalMargin;
    private const float WideMarginH = 144f;
    private const float WideMarginV = NormalMargin;
    internal static Thickness GetThickness(DefaultMarginType defaultMarginType)
    {
        return defaultMarginType switch
        {
            DefaultMarginType.Normal => new Thickness(NormalMargin),
            DefaultMarginType.Narrow => new Thickness(NarrowMargin),
            DefaultMarginType.Moderate => new Thickness(ModerateMarginH, ModerateMarginV),
            DefaultMarginType.Wide => new Thickness(WideMarginH, WideMarginV),
            _ => new Thickness(NormalMargin)
        };
    }
}
