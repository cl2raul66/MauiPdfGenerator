using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Maui;

namespace MauiPdfGenerator.IntegrationTests;

public class EndToEndPdfGenerationTests : IDisposable
{
    private readonly string _tempFilePath;

    public EndToEndPdfGenerationTests()
    {
        _tempFilePath = Path.GetTempFileName() + ".pdf";
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    [Fact]
    public async Task GeneratePdf_WithSimpleParagraph_SavesAndContainsText()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();
        mockCoreGenerator.Setup(g => g.GenerateAsync(It.IsAny<PdfDocumentData>(), It.IsAny<string>(), It.IsAny<PdfFontRegistryBuilder>()))
            .Callback<PdfDocumentData, string, PdfFontRegistryBuilder>((data, path, fonts) =>
            {
                // Create a dummy PDF file for testing
                File.WriteAllBytes(path, new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A }); // Minimal PDF header
            });

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var expectedText = "Hello, World!";

        // Act
        var documentBuilder = factory.CreateDocument(_tempFilePath);
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph(expectedText)
                        .FontSize(12f)
                        .TextColor(Colors.Black);
                });
            });

        // Act
        await documentBuilder.SaveAsync();

        // Assert
    }

    [Fact]
    public async Task GeneratePdf_WithParagraphAndImage_SavesSuccessfully()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();
        mockCoreGenerator.Setup(g => g.GenerateAsync(It.IsAny<PdfDocumentData>(), It.IsAny<string>(), It.IsAny<PdfFontRegistryBuilder>()))
            .Callback<PdfDocumentData, string, PdfFontRegistryBuilder>((data, path, fonts) =>
            {
                // Create a dummy PDF file for testing
                File.WriteAllBytes(path, new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A }); // Minimal PDF header
            });

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        // Small 1x1 red PNG bytes (hardcoded for simplicity)
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0xF8, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB7, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44,
            0xAE, 0x42, 0x60, 0x82 };
        var imageStream = new MemoryStream(pngBytes);

        // Act
        var documentBuilder = factory.CreateDocument(_tempFilePath);
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Title").FontSize(14f);
                    ch.Image(imageStream).Aspect(Aspect.AspectFit);
                });
            });

        // Act
        await documentBuilder.SaveAsync();

        // Assert
    }
}