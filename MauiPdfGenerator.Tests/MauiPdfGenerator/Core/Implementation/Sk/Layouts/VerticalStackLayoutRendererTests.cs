using MauiPdfGenerator.Common;
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

namespace MauiPdfGenerator.Tests.Core.Implementation.Sk.Layouts;

public class VerticalStackLayoutRendererTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IDiagnosticSink> _mockDiagnosticSink;
    private readonly ElementRendererFactory _rendererFactory;
    private readonly VerticalStackLayoutRenderer _renderer;

    public VerticalStackLayoutRendererTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockDiagnosticSink = new Mock<IDiagnosticSink>();
        _rendererFactory = new ElementRendererFactory();
        _renderer = new VerticalStackLayoutRenderer();
    }

    [Fact]
    public async Task MeasureAsync_WithSingleChild_ReturnsCorrectSize()
    {
        // Arrange
        var vsl = new PdfVerticalStackLayoutData();
        var paragraph = new PdfParagraphData("Test");
        vsl.Add(paragraph);

        var context = CreateContext(vsl);

        // Mock the child renderer and add to factory via reflection
        var mockChildRenderer = new Mock<IElementRenderer>();
        mockChildRenderer.Setup(r => r.MeasureAsync(It.IsAny<PdfGenerationContext>(), It.IsAny<SkiaSharp.SKSize>()))
            .ReturnsAsync(new PdfLayoutInfo(paragraph, 50, 20));

        var renderersField = typeof(ElementRendererFactory).GetField("_renderers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var renderersDict = (Dictionary<Type, IElementRenderer>)renderersField.GetValue(_rendererFactory);
        renderersDict[typeof(PdfParagraphData)] = mockChildRenderer.Object;

        // Act
        var result = await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Assert
        Assert.Equal(50, result.Width);
        Assert.Equal(20, result.Height);
    }

    [Fact]
    public async Task MeasureAsync_WithMultipleChildren_IncludesSpacing()
    {
        // Arrange
        var vsl = new PdfVerticalStackLayoutData();
        vsl.SetSpacing(10);
        var paragraph1 = new PdfParagraphData("Test1");
        var paragraph2 = new PdfParagraphData("Test2");
        vsl.Add(paragraph1);
        vsl.Add(paragraph2);

        var context = CreateContext(vsl);

        // Mock the child renderers
        var mockChildRenderer = new Mock<IElementRenderer>();
        mockChildRenderer.Setup(r => r.MeasureAsync(It.IsAny<PdfGenerationContext>(), It.IsAny<SkiaSharp.SKSize>()))
            .ReturnsAsync(new PdfLayoutInfo(paragraph1, 50, 20));

        var renderersField = typeof(ElementRendererFactory).GetField("_renderers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var renderersDict = (Dictionary<Type, IElementRenderer>)renderersField.GetValue(_rendererFactory);
        renderersDict[typeof(PdfParagraphData)] = mockChildRenderer.Object;

        // Act
        var result = await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Assert: 20 + 20 + 10 = 50 height
        Assert.Equal(50, result.Height);
    }

    private PdfGenerationContext CreateContext(PdfVerticalStackLayoutData vsl)
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
            Microsoft.Maui.TextTransform.Default
        );

        return new PdfGenerationContext(
            pageData,
            new PdfFontRegistryBuilder(),
            new Dictionary<object, object>(),
            _mockLogger.Object,
            _rendererFactory,
            _mockDiagnosticSink.Object,
            vsl
        );
    }
}