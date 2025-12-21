using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core.Implementation.Sk;
using MauiPdfGenerator.Core.Implementation.Sk.Layouts;
using MauiPdfGenerator.Core.Implementation.Sk.Views;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Maui;

namespace MauiPdfGenerator.Tests.Core.Implementation.Sk.Layouts;

public class PaginationTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IDiagnosticSink> _mockDiagnosticSink;
    private readonly VerticalStackLayoutRenderer _renderer;

    public PaginationTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockDiagnosticSink = new Mock<IDiagnosticSink>();
        _renderer = new VerticalStackLayoutRenderer();
    }

    [Fact]
    public async Task ArrangeAsync_WithOverflow_CreatesContinuation()
    {
        // Arrange
        var vsl = new PdfVerticalStackLayoutData();
        vsl.SetSpacing(5);

        // Add multiple paragraphs that exceed page height
        for (int i = 0; i < 10; i++)
        {
            var paragraph = new PdfParagraphData($"Line {i}");
            paragraph.FontSizeProp.Set(12f, PdfPropertyPriority.Local);
            vsl.Add(paragraph);
        }

        var context = CreateContext(vsl);

        // Mock child renderer to return fixed height
        var mockChildRenderer = new Mock<IElementRenderer>();
        mockChildRenderer.Setup(r => r.MeasureAsync(It.IsAny<PdfGenerationContext>(), It.IsAny<SkiaSharp.SKSize>()))
            .ReturnsAsync(new PdfLayoutInfo(null, 100, 20));
        mockChildRenderer.Setup(r => r.ArrangeAsync(It.IsAny<PdfRect>(), It.IsAny<PdfGenerationContext>()))
            .ReturnsAsync(new PdfLayoutInfo(null, 100, 20));

        var renderersField = typeof(ElementRendererFactory).GetField("_renderers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var renderersDict = (Dictionary<Type, IElementRenderer>)renderersField.GetValue(context.RendererFactory);
        renderersDict[typeof(PdfParagraphData)] = mockChildRenderer.Object;

        // First measure
        await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 100));

        // Act: Arrange with limited height (only fit ~3 items: 20*3 + 5*2 = 70, page height 100)
        var finalRect = new PdfRect(0, 0, 100, 100);
        var result = await _renderer.ArrangeAsync(finalRect, context);

        // Assert: Should create remaining element since not all fit
        Assert.NotNull(result.RemainingElement);
        Assert.True(result.Height < 100); // Not full height
    }

    [Fact]
    public async Task ArrangeAsync_WithoutOverflow_NoContinuation()
    {
        // Arrange
        var vsl = new PdfVerticalStackLayoutData();
        vsl.SetSpacing(5);

        // Add few paragraphs that fit
        for (int i = 0; i < 3; i++)
        {
            var paragraph = new PdfParagraphData($"Line {i}");
            paragraph.FontSizeProp.Set(12f, PdfPropertyPriority.Local);
            vsl.Add(paragraph);
        }

        var context = CreateContext(vsl);

        // Mock child renderer
        var mockChildRenderer = new Mock<IElementRenderer>();
        mockChildRenderer.Setup(r => r.MeasureAsync(It.IsAny<PdfGenerationContext>(), It.IsAny<SkiaSharp.SKSize>()))
            .ReturnsAsync(new PdfLayoutInfo(null, 100, 20));
        mockChildRenderer.Setup(r => r.ArrangeAsync(It.IsAny<PdfRect>(), It.IsAny<PdfGenerationContext>()))
            .ReturnsAsync(new PdfLayoutInfo(null, 100, 20));

        var renderersField = typeof(ElementRendererFactory).GetField("_renderers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var renderersDict = (Dictionary<Type, IElementRenderer>)renderersField.GetValue(context.RendererFactory);
        renderersDict[typeof(PdfParagraphData)] = mockChildRenderer.Object;

        // First measure
        await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(100, 200));

        // Act: Arrange with enough height
        var finalRect = new PdfRect(0, 0, 100, 200);
        var result = await _renderer.ArrangeAsync(finalRect, context);

        // Assert: No remaining element
        Assert.Null(result.RemainingElement);
        Assert.True(result.Height <= 200);
    }

    private PdfGenerationContext CreateContext(PdfVerticalStackLayoutData vsl)
    {
        var pageData = new PdfPageData(
            Fluent.Enums.PageSizeType.A4,
            Fluent.Enums.PageOrientationType.Portrait,
            new Thickness(0),
            null,
            vsl,
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
            new ElementRendererFactory(),
            _mockDiagnosticSink.Object,
            vsl
        );
    }
}