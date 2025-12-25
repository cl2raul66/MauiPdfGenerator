using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core.Implementation.Sk;
using MauiPdfGenerator.Core.Implementation.Sk.Views;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Maui;

namespace MauiPdfGenerator.Tests.Core.Implementation.Sk.Views;

public class TextRendererTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IDiagnosticSink> _mockDiagnosticSink;
    private readonly ElementRendererFactory _rendererFactory;
    private readonly TextRenderer _renderer;

    public TextRendererTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockDiagnosticSink = new Mock<IDiagnosticSink>();
        _rendererFactory = new ElementRendererFactory();
        _renderer = new TextRenderer();
    }

    [Fact]
    public async Task MeasureAsync_WithSimpleParagraph_ReturnsCorrectSize()
    {
        // Arrange
        var paragraph = new PdfParagraphData("Hello World");
        paragraph.FontSizeProp.Set(12f, PdfPropertyPriority.Local);

        var context = CreateContext(paragraph);

        // Act
        var result = await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Assert
        Assert.True(result.Width > 0);
        Assert.True(result.Height > 0);
    }

    [Fact]
    public async Task ArrangeAsync_WithMeasuredParagraph_ReturnsArrangedInfo()
    {
        // Arrange
        var paragraph = new PdfParagraphData("Test");
        var context = CreateContext(paragraph);

        // First measure
        await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Act
        var finalRect = new PdfRect(0, 0, 100, 50);
        var result = await _renderer.ArrangeAsync(finalRect, context);

        // Assert
        Assert.Equal(100, result.Width);
        Assert.True(result.Height > 0);
    }

    private PdfGenerationContext CreateContext(PdfParagraphData paragraph)
    {
        var pageData = new PdfPageData(
            Fluent.Enums.PageSizeType.A4,
            Fluent.Enums.PageOrientationType.Portrait,
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
            new Fluent.Builders.PdfFontRegistryBuilder(),
            new Dictionary<object, object>(),
            _mockLogger.Object,
            _rendererFactory,
            _mockDiagnosticSink.Object,
            paragraph
        );
    }
}