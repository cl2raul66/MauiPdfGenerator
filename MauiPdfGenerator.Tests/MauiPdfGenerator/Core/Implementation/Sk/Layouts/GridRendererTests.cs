using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core.Implementation.Sk;
using MauiPdfGenerator.Core.Implementation.Sk.Layouts;
using MauiPdfGenerator.Core.Implementation.Sk.Views;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Core.Implementation.Sk.Layouts;

public class GridRendererTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IDiagnosticSink> _mockDiagnosticSink;
    private readonly Mock<IElementRendererFactory> _mockRendererFactory;
    private readonly GridRenderer _renderer;

    public GridRendererTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockDiagnosticSink = new Mock<IDiagnosticSink>();
        _mockRendererFactory = new Mock<IElementRendererFactory>();
        _renderer = new GridRenderer();
    }

    [Fact]
    public async Task MeasureAsync_WithSimpleAutoGrid_ReturnsCorrectSize()
    {
        // Arrange
        var grid = new PdfGridData();
        var paragraph = new PdfParagraphData("Test");
        grid.Add(paragraph);
        grid.SetColumnDefinitions([new PdfColumnDefinition(PdfGridLength.Auto)]);
        grid.SetRowDefinitions([new PdfRowDefinition(PdfGridLength.Auto)]);

        var context = CreateContext(grid);

        var mockChildRenderer = new Mock<IElementRenderer>();
        mockChildRenderer.Setup(r => r.MeasureAsync(It.IsAny<PdfGenerationContext>(), It.IsAny<SkiaSharp.SKSize>()))
            .ReturnsAsync(new PdfLayoutInfo(paragraph, 50, 20));

        _mockRendererFactory.Setup(f => f.GetRenderer(It.IsAny<PdfElementData>())).Returns(mockChildRenderer.Object);

        // Act
        var result = await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Assert
        Assert.Equal(50, result.Width); 
        Assert.Equal(20, result.Height); 
    }

    [Fact]
    public async Task ArrangeAsync_WithMeasuredGrid_ReturnsArrangedInfo()
    {
        // Arrange
        var grid = new PdfGridData();
        var paragraph = new PdfParagraphData("Test");
        grid.Add(paragraph);
        grid.SetColumnDefinitions([new PdfColumnDefinition(PdfGridLength.Auto)]);
        grid.SetRowDefinitions([new PdfRowDefinition(PdfGridLength.Auto)]);

        var context = CreateContext(grid);

        // First measure
        var mockChildRenderer = new Mock<IElementRenderer>();
        mockChildRenderer.Setup(r => r.MeasureAsync(It.IsAny<PdfGenerationContext>(), It.IsAny<SkiaSharp.SKSize>()))
            .ReturnsAsync(new PdfLayoutInfo(paragraph, 50, 20));

        _mockRendererFactory.Setup(f => f.GetRenderer(It.IsAny<PdfElementData>())).Returns(mockChildRenderer.Object);

        await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Mock arrange
        mockChildRenderer.Setup(r => r.ArrangeAsync(It.IsAny<PdfRect>(), It.IsAny<PdfGenerationContext>()))
            .ReturnsAsync(new PdfLayoutInfo(paragraph, 50, 20));

        // Act
        var finalRect = new PdfRect(0, 0, 100, 100);
        var result = await _renderer.ArrangeAsync(finalRect, context);

        // Assert
        Assert.Equal(100, result.Width);
        Assert.True(result.Height > 0);
    }

    private PdfGenerationContext CreateContext(PdfGridData grid)
    {
        var pageData = new PdfPageData(
            PageSizeType.A4,
            PageOrientationType.Portrait,
            new Thickness(0),
            null,
            new PdfVerticalStackLayoutData(),
            null,
            12f,
            Microsoft.Maui.Graphics.Colors.Black,
            Microsoft.Maui.Controls.FontAttributes.None,
            Microsoft.Maui.TextDecorations.None,
            Microsoft.Maui.TextTransform.Default,
            string.Empty
        );

        return new PdfGenerationContext(
            pageData,
            new PdfFontRegistryBuilder(),
            [],
            _mockLogger.Object,
            _mockRendererFactory.Object,
            _mockDiagnosticSink.Object,
            grid
        );
    }
}
