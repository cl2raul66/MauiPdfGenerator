using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MauiPdfGenerator.IntegrationTests;

public class DiagnosticIntegrationTests
{
    [Fact]
    public async Task CreateDocument_WithOversizedContent_TriggersLayoutOverflowDiagnostic()
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
                // Simulate layout overflow diagnostic
                mockDiagnosticSink.Object.Submit(new DiagnosticMessage(
                    DiagnosticSeverity.Warning,
                    "LAYOUT-001",
                    "Element overflows its cell."
                ));
            });

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument("dummy.pdf");

        // Create content that might overflow
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Very long text that might overflow the page width and height constraints in a real scenario.")
                        .WidthRequest(50f); // Force small width to simulate overflow
                });
            });

        // Act
        await documentBuilder.SaveAsync();

        // Assert
        mockDiagnosticSink.Verify(d => d.Submit(It.Is<DiagnosticMessage>(m =>
            m.Code == "LAYOUT-001" && m.Severity == DiagnosticSeverity.Warning)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateDocument_WithMissingFont_TriggersFontNotFoundDiagnostic()
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
                // Simulate font not found diagnostic
                mockDiagnosticSink.Object.Submit(new DiagnosticMessage(
                    DiagnosticSeverity.Error,
                    "RESOURCE-002",
                    "Font 'NonExistentFont' not found."
                ));
            });

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument("dummy.pdf");

        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Text with missing font");
                    // In real scenario, specifying a font not in registry would trigger this
                });
            });

        // Act
        await documentBuilder.SaveAsync();

        // Assert
        mockDiagnosticSink.Verify(d => d.Submit(It.Is<DiagnosticMessage>(m =>
            m.Code == "RESOURCE-002" && m.Severity == DiagnosticSeverity.Error)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateDocument_WithInvalidImage_TriggersImageDecodeErrorDiagnostic()
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
                // Simulate image decode error diagnostic
                mockDiagnosticSink.Object.Submit(new DiagnosticMessage(
                    DiagnosticSeverity.Error,
                    "RESOURCE-001",
                    "Image decode error."
                ));
            });

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument("dummy.pdf");

        using var invalidImageStream = new MemoryStream(new byte[] { 1, 2, 3 }); // Invalid image data

        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Image(invalidImageStream);
                });
            });

        // Act
        await documentBuilder.SaveAsync();

        // Assert
        mockDiagnosticSink.Verify(d => d.Submit(It.Is<DiagnosticMessage>(m =>
            m.Code == "RESOURCE-001" && m.Severity == DiagnosticSeverity.Error)), Times.AtLeastOnce);
    }
}