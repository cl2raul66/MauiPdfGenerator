using System.Reflection;
using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Utils;
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

        var paragraph = content.GetChildren[0] as PdfParagraphData;
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

    [Fact]
    public void CreateDocument_WithImage_SetsPropertiesCorrectly()
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
        var expectedAspect = Aspect.AspectFill;
        var expectedWidth = 150.0f;
        using var imageStream = new MemoryStream();

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Image(imageStream)
                        .Aspect(expectedAspect)
                        .WidthRequest(expectedWidth);
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

        var image = content.GetChildren[0] as PdfImageData;
        Assert.NotNull(image);
        Assert.Equal(imageStream, image.ImageStream);
        Assert.Equal(expectedAspect, image.CurrentAspect);
        Assert.Equal(expectedWidth, image.GetWidthRequest);
    }

    [Fact]
    public void CreateDocument_WithStyles_AppliesStylesCorrectly()
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
        var expectedFontSize = 20f;
        var expectedTextColor = Colors.Red;

        // Act
        documentBuilder
            .Resources(r =>
            {
                r.Style<IPdfParagraph>("HeaderStyle", s => s
                    .FontSize(expectedFontSize)
                    .TextColor(expectedTextColor));
            })
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Header Text")
                        .Style(new PdfStyleIdentifier("HeaderStyle"));
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

        var paragraph = content.GetChildren[0] as PdfParagraphData;
        Assert.NotNull(paragraph);
        Assert.Equal("Header Text", paragraph.Text);

        var configBuilder = typeof(PdfDocumentBuilder).GetField("_configurationBuilder", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(concreteBuilder) as PdfConfigurationBuilder;
        Assert.NotNull(configBuilder);

        var styleResolver = new StyleResolver(configBuilder!.ResourceDictionary, mockDiagnosticSink.Object, fontRegistry);
        styleResolver.ApplyStyles([paragraph], null);

        Assert.Equal(expectedFontSize, paragraph.CurrentFontSize);
        Assert.Equal(expectedTextColor, paragraph.CurrentTextColor);
    }

    [Fact]
    public void CreateDocument_WithNestedLayouts_SetsHierarchyCorrectly()
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

        // Act
        documentBuilder
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.VerticalStackLayout(vsl =>
                    {
                        vsl.Children(vslChildren =>
                        {
                            vslChildren.Paragraph("Outer Paragraph");
                            vslChildren.HorizontalStackLayout(hsl =>
                            {
                                hsl.Children(hslChildren =>
                                {
                                    hslChildren.Paragraph("Inner Paragraph 1");
                                    hslChildren.Paragraph("Inner Paragraph 2");
                                });
                            });
                        });
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

        var outerVsl = content.GetChildren[0] as PdfVerticalStackLayoutData;
        Assert.NotNull(outerVsl);
        Assert.Equal(2, outerVsl.GetChildren.Count); 

        var outerParagraph = outerVsl.GetChildren[0] as PdfParagraphData;
        Assert.NotNull(outerParagraph);
        Assert.Equal("Outer Paragraph", outerParagraph.Text);

        var innerHsl = outerVsl.GetChildren[1] as PdfHorizontalStackLayoutData;
        Assert.NotNull(innerHsl);
        Assert.Equal(2, innerHsl.GetChildren.Count); 

        var innerParagraph1 = innerHsl.GetChildren[0] as PdfParagraphData;
        Assert.NotNull(innerParagraph1);
        Assert.Equal("Inner Paragraph 1", innerParagraph1.Text);

        var innerParagraph2 = innerHsl.GetChildren[1] as PdfParagraphData;
        Assert.NotNull(innerParagraph2);
        Assert.Equal("Inner Paragraph 2", innerParagraph2.Text);
    }

    [Fact]
    public void CreateDocument_WithMetadata_SetsMetadataCorrectly()
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
        var expectedTitle = "Test Document";
        var expectedAuthor = "Test Author";

        // Act
        documentBuilder
            .Configuration(config =>
            {
                config.MetaData(meta =>
                {
                    meta.Title(expectedTitle);
                    meta.Author(expectedAuthor);
                });
            })
            .ContentPage()
            .Content(c =>
            {
                c.Children(ch =>
                {
                    ch.Paragraph("Test Content");
                });
            });

        // Assert
        var concreteBuilder = documentBuilder as PdfDocumentBuilder;
        Assert.NotNull(concreteBuilder);

        var metaDataField = typeof(PdfDocumentBuilder).GetField("_configurationBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
        var configBuilder = metaDataField?.GetValue(concreteBuilder) as PdfConfigurationBuilder;
        Assert.NotNull(configBuilder);

        var metaData = configBuilder.MetaDataBuilder;
        Assert.Equal(expectedTitle, metaData.GetTitle);
        Assert.Equal(expectedAuthor, metaData.GetAuthor);
    }
}
