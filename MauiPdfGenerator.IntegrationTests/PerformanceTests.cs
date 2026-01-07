using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MauiPdfGenerator.IntegrationTests;

public class PerformanceTests
{
    [Fact]
    public async Task GenerateSimpleDocument_PerformanceWithinLimits()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();
        mockCoreGenerator.Setup(g => g.GenerateAsync(It.IsAny<PdfDocumentData>(), It.IsAny<string>(), It.IsAny<PdfFontRegistryBuilder>()))
            .Callback(async () => await Task.Delay(10)) 
            .Returns(Task.CompletedTask);

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument("dummy.pdf");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Performance test content");
                });
            })
            .Build()
            .SaveAsync();
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Generation took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    [Fact]
    public async Task GenerateDocumentWithMultipleElements_PerformanceScales()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();
        mockCoreGenerator.Setup(g => g.GenerateAsync(It.IsAny<PdfDocumentData>(), It.IsAny<string>(), It.IsAny<PdfFontRegistryBuilder>()))
            .Callback(async () => await Task.Delay(20)) 
            .Returns(Task.CompletedTask);

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument("dummy.pdf");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        ch.Paragraph($"Paragraph {i}");
                    }
                });
            })
            .Build()
            .SaveAsync();
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Generation took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
    }
}
