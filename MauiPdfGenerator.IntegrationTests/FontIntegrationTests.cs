using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MauiPdfGenerator.IntegrationTests;

public class FontIntegrationTests
{
    [Fact]
    public async Task CreateDocument_WithCustomFont_SetsFontIdentifierCorrectly()
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
        var customFontId = new PdfFontIdentifier("CustomFont");

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Text with custom font")
                        .FontFamily(customFontId);
                });
            });

        await documentBuilder.SaveAsync();

        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        var pages = typeof(PdfDocumentBuilder).GetField("_pages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(concreteBuilder) as List<IPdfPageBuilder>;
        var page = pages![0] as IPdfContentPageBuilder;
        var content = page!.GetContent() as PdfVerticalStackLayoutData;
        var paragraph = content!.GetChildren[0] as PdfParagraphData;

        Assert.Equal(customFontId, paragraph!.FontFamilyProp.Value);
    }

    [Fact]
    public async Task CreateDocument_WithBoldFontAttribute_SetsAttributesCorrectly()
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

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Bold text")
                        .FontAttributes(FontAttributes.Bold);
                });
            });

        await documentBuilder.SaveAsync();

        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        var pages = typeof(PdfDocumentBuilder).GetField("_pages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(concreteBuilder) as List<IPdfPageBuilder>;
        var page = pages![0] as IPdfContentPageBuilder;
        var content = page!.GetContent() as PdfVerticalStackLayoutData;
        var paragraph = content!.GetChildren[0] as PdfParagraphData;

        Assert.Equal(FontAttributes.Bold, paragraph!.FontAttributesProp.Value);
    }

    [Fact]
    public async Task CreateDocument_WithItalicFontAttribute_SetsAttributesCorrectly()
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

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Italic text")
                        .FontAttributes(FontAttributes.Italic);
                });
            });

        await documentBuilder.SaveAsync();

        // Assert
        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        var pages = typeof(PdfDocumentBuilder).GetField("_pages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(concreteBuilder) as List<IPdfPageBuilder>;
        var page = pages![0] as IPdfContentPageBuilder;
        var content = page!.GetContent() as PdfVerticalStackLayoutData;
        var paragraph = content!.GetChildren[0] as PdfParagraphData;

        Assert.Equal(FontAttributes.Italic, paragraph!.FontAttributesProp.Value);
    }
}
