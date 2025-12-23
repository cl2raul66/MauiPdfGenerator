using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MauiPdfGenerator.IntegrationTests;

public class ErrorHandlingTests
{
    [Fact]
    public async Task CreateDocument_WithInvalidFontSize_ThrowsArgumentException()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();
        mockCoreGenerator.Setup(g => g.GenerateAsync(It.IsAny<PdfDocumentData>(), It.IsAny<string>(), It.IsAny<PdfFontRegistryBuilder>()))
            .Returns(Task.CompletedTask);

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument("dummy.pdf");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            documentBuilder
                .ContentPage()
                .Content(c =>
                {
                    c.Children(ch =>
                    {
                        ch.Paragraph("Text")
                            .FontSize(-5f); // Invalid negative font size
                    });
                })
                .Build()
                .SaveAsync()
        );
    }

    [Fact]
    public async Task CreateDocument_WithEmptyPath_ThrowsArgumentException()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument(); // No path

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            documentBuilder
                .ContentPage()
                .Content(c =>
                {
                    c.Children(ch =>
                    {
                        ch.Paragraph("Text");
                    });
                })
                .Build()
                .SaveAsync("")
        );
    }

    [Fact]
    public async Task CreateDocument_WithNullImageStream_ThrowsArgumentNullException()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();
        mockCoreGenerator.Setup(g => g.GenerateAsync(It.IsAny<PdfDocumentData>(), It.IsAny<string>(), It.IsAny<PdfFontRegistryBuilder>()))
            .Returns(Task.CompletedTask);

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument("dummy.pdf");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            documentBuilder
                .ContentPage()
                .Content(c =>
                {
                    c.Children(ch =>
                    {
                        ch.Image(null!); // Null stream
                    });
                })
                .Build()
                .SaveAsync()
        );
    }

    [Fact]
    public async Task CreateDocument_WithNoPages_ThrowsInvalidOperationException()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument("dummy.pdf");
        // No pages added

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            documentBuilder.SaveAsync()
        );
        Assert.Contains("No pages have been added", exception.Message);
    }
}