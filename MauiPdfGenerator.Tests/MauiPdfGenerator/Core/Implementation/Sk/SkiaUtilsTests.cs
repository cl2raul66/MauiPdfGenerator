using Xunit;
using MauiPdfGenerator.Core.Implementation.Sk;
using SkiaSharp;
using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Core.Implementation.Sk;

public class SkiaUtilsTests
{
    [Fact]
    public void ConvertToSkColor_WithNullColor_ReturnsBlack()
    {
        // Arrange
        Color? mauiColor = null;

        // Act
        var skColor = SkiaUtils.ConvertToSkColor(mauiColor);

        // Assert
        Assert.Equal(SKColors.Black, skColor);
    }

    [Theory]
    [InlineData(0, 0, 0, 1)] // Black
    [InlineData(1, 1, 1, 1)] // White
    [InlineData(1, 0, 0, 1)] // Red
    [InlineData(0, 1, 0, 0.5)] // Green with alpha
    public void ConvertToSkColor_WithValidColor_ReturnsCorrectSkColor(float r, float g, float b, float a)
    {
        // Arrange
        var mauiColor = new Color(r, g, b, a);
        var expectedSkColor = new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));

        // Act
        var skColor = SkiaUtils.ConvertToSkColor(mauiColor);

        // Assert
        Assert.Equal(expectedSkColor, skColor);
    }

    [Theory]
    [InlineData(PageSizeType.A4, PageOrientationType.Portrait, 595.28f, 841.89f)]
    [InlineData(PageSizeType.A4, PageOrientationType.Landscape, 841.89f, 595.28f)]
    [InlineData(PageSizeType.Letter, PageOrientationType.Portrait, 612f, 792f)]
    [InlineData(PageSizeType.Letter, PageOrientationType.Landscape, 792f, 612f)]
    [InlineData(PageSizeType.Legal, PageOrientationType.Portrait, 612f, 1008f)]
    public void GetSkPageSize_WithVariousSizesAndOrientations_ReturnsCorrectSkSize(PageSizeType size, PageOrientationType orientation, float expectedWidth, float expectedHeight)
    {
        // Arrange & Act
        var skSize = SkiaUtils.GetSkPageSize(size, orientation);

        // Assert
        Assert.Equal(expectedWidth, skSize.Width);
        Assert.Equal(expectedHeight, skSize.Height);
    }
}
