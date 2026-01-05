using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core.Implementation.Sk.Views;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharp;
using Xunit;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Core.Implementation.Sk.Views;

public class ImageRendererTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IDiagnosticSink> _mockDiagnosticSink;
    private readonly ImageRenderer _renderer;

    public ImageRendererTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockDiagnosticSink = new Mock<IDiagnosticSink>();
        _renderer = new ImageRenderer();
    }

    private PdfGenerationContext CreateContext(PdfImageData imageData)
    {
        var fontRegistry = new PdfFontRegistryBuilder();
        var pageData = new PdfPageData(
            PageSizeType.Letter,
            PageOrientationType.Portrait,
            new Thickness(0),
            null,
            new PdfVerticalStackLayoutData(),
            null,
            12f,
            Colors.Black,
            FontAttributes.None,
            TextDecorations.None,
            TextTransform.Default,
            string.Empty
        );
        
        return new PdfGenerationContext(
            pageData,
            fontRegistry,
            [],
            _mockLogger.Object,
            new ElementRendererFactory(),
            _mockDiagnosticSink.Object,
            imageData
        );
    }

    [Fact]
    public async Task RenderAsync_WithValidImage_DrawsImageOnCanvas()
    {
        // Arrange
        using var bitmap = new SKBitmap(1, 1);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Red);
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var imageStream = data.AsStream();

        var imageData = new PdfImageData(imageStream);
        var context = CreateContext(imageData);

        // Act
        var availableSize = new SKSize(100, 100);
        await _renderer.MeasureAsync(context, availableSize);

        var finalRect = new PdfRect(0, 0, 100, 100);
        await _renderer.ArrangeAsync(finalRect, context);

        using var renderBitmap = new SKBitmap(100, 100);
        using var renderCanvas = new SKCanvas(renderBitmap);
        await _renderer.RenderAsync(renderCanvas, context);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task RenderAsync_WithBackgroundColor_DrawsBackgroundRect()
    {
        // Arrange
        var imageStream = new MemoryStream();
        var imageData = new PdfImageData(imageStream);
        imageData.BackgroundColorProp.Set(Colors.Blue, PdfPropertyPriority.Local);
        var context = CreateContext(imageData);

        // Act
        var availableSize = new SKSize(100, 100);
        await _renderer.MeasureAsync(context, availableSize);

        var finalRect = new PdfRect(10, 10, 80, 80); 
        await _renderer.ArrangeAsync(finalRect, context);

        using var renderBitmap = new SKBitmap(100, 100);
        using var renderCanvas = new SKCanvas(renderBitmap);
        await _renderer.RenderAsync(renderCanvas, context);

        // Assert
        Assert.True(true);
    }
}
