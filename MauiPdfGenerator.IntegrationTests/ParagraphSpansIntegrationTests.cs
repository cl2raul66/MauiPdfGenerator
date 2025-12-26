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
using Microsoft.Maui;

namespace MauiPdfGenerator.IntegrationTests;

public class ParagraphSpansIntegrationTests
{
    [Fact]
    public void CreateDocument_WithSpans_SetsPropertiesCorrectly()
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

        var documentBuilder = factory.CreateDocument();
        
        var span1Text = "Span 1";
        var span2Text = "Span 2";
        var span2FontSize = 24f;
        var span2TextColor = Colors.Red;

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph(p =>
                    {
                        p.Span(span1Text);
                        p.Span(span2Text)
                            .FontSize(span2FontSize)
                            .TextColor(span2TextColor);
                    });
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
        Assert.True(paragraph.HasSpans);
        Assert.Equal(2, paragraph.Spans.Count);

        Assert.Equal(span1Text, paragraph.Spans[0].Text);
        Assert.Equal(span2Text, paragraph.Spans[1].Text);
        Assert.Equal(span2FontSize, paragraph.Spans[1].FontSizeProp.Value);
        Assert.Equal(span2TextColor, paragraph.Spans[1].TextColorProp.Value);
    }

    [Fact]
    public void CreateDocument_SpansInheritAndOverrideStyles_SetsPropertiesCorrectly()
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

        var documentBuilder = factory.CreateDocument();
        
        var paragraphFontSize = 18f;
        var paragraphTextColor = Colors.Blue;

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph(p =>
                    {
                        p.Span("Inheriting span");
                        p.Span("Overriding span")
                            .FontSize(30f);
                    })
                    .FontSize(paragraphFontSize)
                    .TextColor(paragraphTextColor);
                });
            });

        // Assert
        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        Assert.NotNull(concreteBuilder);
        var pages = GetInternalPages(concreteBuilder);
        var pageBuilder = pages!.First() as IPdfContentPageBuilder;
        var content = pageBuilder!.GetContent() as PdfVerticalStackLayoutData;
        var paragraph = content!.GetChildren[0] as PdfParagraphData;

        Assert.Equal(paragraphFontSize, paragraph!.FontSizeProp.Value);
        Assert.Equal(paragraphTextColor, paragraph.TextColorProp.Value);

        Assert.Null(paragraph.Spans[0].FontSizeProp.Value); 
        Assert.Null(paragraph.Spans[0].TextColorProp.Value);

        Assert.Equal(30f, paragraph.Spans[1].FontSizeProp.Value);
        Assert.Null(paragraph.Spans[1].TextColorProp.Value);
    }

    private List<IPdfPageBuilder>? GetInternalPages(PdfDocumentBuilder builder)
    {
        var pagesField = typeof(PdfDocumentBuilder).GetField("_pages", BindingFlags.NonPublic | BindingFlags.Instance);
        return pagesField?.GetValue(builder) as List<IPdfPageBuilder>;
    }
}
