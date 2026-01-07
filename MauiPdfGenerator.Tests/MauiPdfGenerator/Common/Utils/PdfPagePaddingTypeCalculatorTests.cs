using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Fluent.Enums;
using Xunit;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Common.Utils;

public class PdfPagePaddingTypeCalculatorTests
{
    [Fact]
    public void GetThickness_Normal_ReturnsNormalPadding()
    {
        var result = PdfPagePaddingTypeCalculator.GetThickness(DefaultPagePaddingType.Normal);
        Assert.Equal(new Thickness(72f), result);
    }

    [Fact]
    public void GetThickness_Narrow_ReturnsNarrowPadding()
    {
        var result = PdfPagePaddingTypeCalculator.GetThickness(DefaultPagePaddingType.Narrow);
        Assert.Equal(new Thickness(36f), result);
    }

    [Fact]
    public void GetThickness_Moderate_ReturnsModeratePadding()
    {
        var result = PdfPagePaddingTypeCalculator.GetThickness(DefaultPagePaddingType.Moderate);
        Assert.Equal(new Thickness(72f, 54f), result);
    }

    [Fact]
    public void GetThickness_Wide_ReturnsWidePadding()
    {
        var result = PdfPagePaddingTypeCalculator.GetThickness(DefaultPagePaddingType.Wide);
        Assert.Equal(new Thickness(144f, 72f), result);
    }

    [Fact]
    public void GetThickness_InvalidType_ReturnsNormalPadding()
    {
        var result = PdfPagePaddingTypeCalculator.GetThickness((DefaultPagePaddingType)999);
        Assert.Equal(new Thickness(72f), result);
    }
}
