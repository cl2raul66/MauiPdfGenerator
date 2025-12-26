using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core.Implementation.Sk.Views;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Core.Implementation.Sk.Views;

public class HorizontalLineRendererTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IDiagnosticSink> _mockDiagnosticSink;
    private readonly ElementRendererFactory _rendererFactory;
    private readonly HorizontalLineRenderer _renderer;

    public HorizontalLineRendererTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockDiagnosticSink = new Mock<IDiagnosticSink>();
        _rendererFactory = new ElementRendererFactory();
        _renderer = new HorizontalLineRenderer();
    }

    [Fact]
    public async Task MeasureAsync_WithHorizontalLine_ReturnsCorrectSize()
    {
        // Arrange
        var line = new PdfHorizontalLineData();
        line.HeightRequestProp.Set(2f, PdfPropertyPriority.Local);

        var context = CreateContext(line);

        // Act
        var result = await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Assert
        Assert.Equal(100, result.Width); 
        Assert.Equal(2, result.Height); 
    }

    [Fact]
    public async Task ArrangeAsync_WithMeasuredLine_ReturnsArrangedInfo()
    {
        // Arrange
        var line = new PdfHorizontalLineData();
        var context = CreateContext(line);

        // First measure
        await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Act
        var finalRect = new PdfRect(0, 0, 100, 2);
        var result = await _renderer.ArrangeAsync(finalRect, context);

        // Assert
        Assert.Equal(100, result.Width);
        Assert.Equal(2, result.Height);
    }

    private PdfGenerationContext CreateContext(PdfHorizontalLineData line)
    {
        var pageData = new PdfPageData(
            PageSizeType.A4,
            PageOrientationType.Portrait,
            new Thickness(0),
            null,
            new PdfVerticalStackLayoutData(),
            null,
            12f,
            Colors.Black,
            FontAttributes.None,
            TextDecorations.None,
            TextTransform.Default
        );

        return new PdfGenerationContext(
            pageData,
            new PdfFontRegistryBuilder(),
            [],
            _mockLogger.Object,
            _rendererFactory,
            _mockDiagnosticSink.Object,
            line
        );
    }
}
