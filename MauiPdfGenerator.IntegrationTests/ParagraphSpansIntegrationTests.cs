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
                        p.Text(span1Text);
                        p.Text(span2Text)
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

        // Verify concatenated text
        Assert.Equal(span1Text + span2Text, paragraph.Text);
        
        // Verify span lengths
        Assert.Equal(span1Text.Length, paragraph.Spans[0].TextLength);
        Assert.Equal(span2Text.Length, paragraph.Spans[1].TextLength);
        
        // Verify span properties
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
        var span1Text = "Inheriting span";
        var span2Text = "Overriding span";

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph(p =>
                    {
                        p.Text(span1Text);
                        p.Text(span2Text)
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

        // Verify concatenated text
        Assert.Equal(span1Text + span2Text, paragraph.Text);
        
        // Verify span properties
        Assert.Equal(span1Text.Length, paragraph.Spans[0].TextLength);
        Assert.Equal(span2Text.Length, paragraph.Spans[1].TextLength);
        
        Assert.Null(paragraph.Spans[0].FontSizeProp.Value); 
        Assert.Null(paragraph.Spans[0].TextColorProp.Value);

        Assert.Equal(30f, paragraph.Spans[1].FontSizeProp.Value);
        Assert.Null(paragraph.Spans[1].TextColorProp.Value);
    }

    [Fact]
    public void CreateDocument_WithSpans_CalculatesPositionsCorrectly()
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
        
        var span1Text = "Hola ";
        var span2Text = "Mundo!";

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph(p =>
                    {
                        p.Text(span1Text);
                        p.Text(span2Text);
                    });
                });
            });

        // Assert
        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        Assert.NotNull(concreteBuilder);
        var paragraph = GetParagraph(concreteBuilder);
        Assert.NotNull(paragraph);
        
        // Verify concatenated text
        Assert.Equal(span1Text + span2Text, paragraph.Text);
        
        // Verify span positions
        Assert.Equal(0, paragraph.Spans[0].StartIndex);
        Assert.Equal(span1Text.Length, paragraph.Spans[0].EndIndex);
        
        Assert.Equal(span1Text.Length, paragraph.Spans[1].StartIndex);
        Assert.Equal(span1Text.Length + span2Text.Length, paragraph.Spans[1].EndIndex);
    }

    [Fact]
    public void CreateDocument_WithSpans_EmptySpanHandledCorrectly()
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
        
        var span1Text = "Start";
        var span2Text = ""; // Empty
        var span3Text = "End";

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph(p =>
                    {
                        p.Text(span1Text);
                        p.Text(span2Text);
                        p.Text(span3Text);
                    });
                });
            });

        // Assert
        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        Assert.NotNull(concreteBuilder);
        var paragraph = GetParagraph(concreteBuilder);
        Assert.NotNull(paragraph);
        
        // Verify concatenated text (empty spans should not add characters)
        Assert.Equal(span1Text + span3Text, paragraph.Text);
        
        // Verify empty span is handled (length 0)
        Assert.Equal(0, paragraph.Spans[1].TextLength);
    }

    [Fact]
    public void CreateDocument_WithSpans_TextAlignmentAppliedToAll()
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
        
        var span1Text = "First";
        var span2Text = "Second";

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph(p =>
                    {
                        p.Text(span1Text);
                        p.Text(span2Text);
                    })
                    .HorizontalTextAlignment(TextAlignment.Center);
                });
            });

        // Assert
        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        Assert.NotNull(concreteBuilder);
        var paragraph = GetParagraph(concreteBuilder);
        Assert.NotNull(paragraph);
        
        // Verify text alignment is on paragraph, not individual spans
        Assert.Equal(TextAlignment.Center, paragraph.CurrentHorizontalTextAlignment);
        Assert.Equal(span1Text + span2Text, paragraph.Text);
    }

    private List<IPdfPageBuilder>? GetInternalPages(PdfDocumentBuilder builder)
    {
        var pagesField = typeof(PdfDocumentBuilder).GetField("_pages", BindingFlags.NonPublic | BindingFlags.Instance);
        return pagesField?.GetValue(builder) as List<IPdfPageBuilder>;
    }

    private PdfParagraphData? GetParagraph(PdfDocumentBuilder builder)
    {
        var pages = GetInternalPages(builder);
        if (pages is null || pages.Count == 0) return null;

        var pageBuilder = pages.First() as IPdfContentPageBuilder;
        if (pageBuilder is null) return null;

        var content = pageBuilder.GetContent() as PdfVerticalStackLayoutData;
        if (content is null || content.GetChildren.Count == 0) return null;

        return content.GetChildren.First() as PdfParagraphData;
    }
}
