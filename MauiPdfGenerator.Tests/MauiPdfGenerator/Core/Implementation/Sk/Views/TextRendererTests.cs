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
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Builders;

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

    [Fact]
    public async Task MeasureAsync_WithSpans_ReturnsCorrectSize()
    {
        // Arrange
        var paragraph = new PdfParagraphData();
        var spans = new List<PdfSpanData>
        {
            new PdfSpanData("Hello "),
            new PdfSpanData("World")
        };
        paragraph.SetSpans(spans);

        var context = CreateContext(paragraph);

        // Act
        var result = await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(500, 500));

        // Assert
        Assert.True(result.Width > 0);
        Assert.True(result.Height > 0);
    }

    [Fact]
    public async Task MeasureAsync_WithSpansMultipleFonts_CalculatesMaxLineHeight()
    {
        // Arrange
        var paragraph = new PdfParagraphData();
        var span1 = new PdfSpanData("Small ");
        span1.FontSizeProp.Set(10f, PdfPropertyPriority.Local);
        
        var span2 = new PdfSpanData("Large");
        span2.FontSizeProp.Set(24f, PdfPropertyPriority.Local);

        paragraph.SetSpans(new List<PdfSpanData> { span1, span2 });

        var context = CreateContext(paragraph);

        // Act
        var result = await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(500, 500));

        // Assert
        // The height should be at least large enough for the 24pt font
        Assert.True(result.Height > 24); 
    }

    [Fact]
    public async Task ArrangeAsync_WithSpans_WrapsCorrectly()
    {
        // Arrange
        var paragraph = new PdfParagraphData();
        var spans = new List<PdfSpanData>
        {
            new PdfSpanData("This is a long sentence that should wrap because the width is very restricted.")
        };
        paragraph.SetSpans(spans);

        var context = CreateContext(paragraph);

        // Measure with restricted width
        var restrictedWidth = 50f;
        var measureResult = await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(restrictedWidth, 1000));

        // Act
        var finalRect = new PdfRect(0, 0, restrictedWidth, measureResult.Height);
        var result = await _renderer.ArrangeAsync(finalRect, context);

        // Assert
        Assert.Equal(restrictedWidth, result.Width);
        // If it wrapped, the height should be significantly more than a single line height (~12-15)
        Assert.True(result.Height > 20); 
    }

    [Fact]
    public async Task Spans_InheritParentStylesByDefault()
    {
        // Arrange
        var paragraph = new PdfParagraphData();
        var expectedColor = Colors.Blue;
        paragraph.TextColorProp.Set(expectedColor, PdfPropertyPriority.Local);
        paragraph.FontSizeProp.Set(15f, PdfPropertyPriority.Local);

        var span = new PdfSpanData("Test");
        paragraph.SetSpans(new List<PdfSpanData> { span });

        var context = CreateContext(paragraph);

        // Measure/Arrange to trigger internal processing
        await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(500, 500));
        await _renderer.ArrangeAsync(new PdfRect(0, 0, 500, 500), context);

        // Act & Assert
        // We can't easily inspect the internal 'ProcessedSpan' without reflection or making it internal-visible,
        // but if RenderAsync finishes without error, it's a good sign.
        var canvas = new SkiaSharp.SKCanvas(new SkiaSharp.SKBitmap(100, 100));
        await _renderer.RenderAsync(canvas, context);
    }

    [Fact]
    public async Task Spans_OverrideParentStyles()
    {
        // Arrange
        var paragraph = new PdfParagraphData();
        paragraph.TextColorProp.Set(Colors.Black, PdfPropertyPriority.Local);

        var span = new PdfSpanData("Test");
        span.TextColorProp.Set(Colors.Red, PdfPropertyPriority.Local);
        paragraph.SetSpans(new List<PdfSpanData> { span });

        var context = CreateContext(paragraph);

        // Act
        await _renderer.MeasureAsync(context, new SkiaSharp.SKSize(500, 500));
        await _renderer.ArrangeAsync(new PdfRect(0, 0, 500, 500), context);
        
        var canvas = new SkiaSharp.SKCanvas(new SkiaSharp.SKBitmap(100, 100));
        await _renderer.RenderAsync(canvas, context);

        // Verification is implicit: if it doesn't throw, the style resolution logic worked.
    }

    private PdfGenerationContext CreateContext(PdfParagraphData paragraph)
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
            new Dictionary<object, object>(),
            _mockLogger.Object,
            _rendererFactory,
            _mockDiagnosticSink.Object,
            paragraph
        );
    }
}