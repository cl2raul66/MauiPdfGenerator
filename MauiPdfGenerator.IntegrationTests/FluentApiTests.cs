using System.Reflection;
using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MauiPdfGenerator.IntegrationTests;

public class FluentApiTests
{
    [Fact]
    public void CreateDocument_WithParagraph_SetsPropertiesCorrectly()
    {
        // Arrange
        var fontRegistry = new PdfFontRegistryBuilder();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        // LoggerFactory needs to return a logger
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockDiagnosticSink = new Mock<IDiagnosticSink>();
        var mockCoreGenerator = new Mock<IPdfCoreGenerator>();

        var factory = new PdfDocumentFactory(
            fontRegistry,
            mockLoggerFactory.Object,
            mockDiagnosticSink.Object,
            mockCoreGenerator.Object);

        var documentBuilder = factory.CreateDocument();
        var expectedText = "Hello, World!";
        var expectedFontSize = 18f;
        var expectedTextColor = Colors.Blue;
        var expectedAlignment = TextAlignment.Center;

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph(expectedText)
                        .FontSize(expectedFontSize)
                        .TextColor(expectedTextColor)
                        .HorizontalTextAlignment(expectedAlignment);
                });
            });

        // Assert
        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        Assert.NotNull(concreteBuilder);
        var pages = GetInternalPages(concreteBuilder);
        Assert.NotNull(pages);
        Assert.Single(pages);

        var pageBuilder = pages.First() as IPdfContentPageBuilder;
        Assert.NotNull(pageBuilder);

        var content = pageBuilder.GetContent() as PdfVerticalStackLayoutData;
        Assert.NotNull(content);
        Assert.Single(content.GetChildren);

        var paragraph = content.GetChildren.First() as PdfParagraphData;
        Assert.NotNull(paragraph);
        Assert.Equal(expectedText, paragraph.Text);
        Assert.Equal(expectedFontSize, paragraph.CurrentFontSize);
        Assert.Equal(expectedTextColor, paragraph.CurrentTextColor);
        Assert.Equal(expectedAlignment, paragraph.CurrentHorizontalTextAlignment);
    }

    private List<IPdfPageBuilder>? GetInternalPages(PdfDocumentBuilder builder)
    {
        var pagesField = typeof(PdfDocumentBuilder).GetField("_pages", BindingFlags.NonPublic | BindingFlags.Instance);
        return pagesField?.GetValue(builder) as List<IPdfPageBuilder>;
    }
}
