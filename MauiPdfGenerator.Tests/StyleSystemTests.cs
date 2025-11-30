using Xunit;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Styles;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Fluent.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using MauiPdfGenerator.Diagnostics;
using Moq;
using Microsoft.Extensions.Logging;
using MauiPdfGenerator.Diagnostics.Interfaces;

namespace MauiPdfGenerator.Tests;

public class StyleSystemTests
{
    private readonly PdfDocumentBuilder _docBuilder;
    private readonly PdfResourceBuilder _resourceBuilder;
    private readonly PdfResourceDictionary _resourceDictionary;
    private readonly IDiagnosticSink _diagnosticSink;

    public StyleSystemTests()
    {
        var fontRegistry = new PdfFontRegistryBuilder();
        _resourceDictionary = new PdfResourceDictionary();
        var loggerFactory = new NullLoggerFactory();
        _diagnosticSink = new DiagnosticSink();
        var mockGenerator = new Mock<IPdfCoreGenerator>();

        _docBuilder = new PdfDocumentBuilder(fontRegistry, loggerFactory, _diagnosticSink, mockGenerator.Object);
        _resourceBuilder = new PdfResourceBuilder(_resourceDictionary);
    }


    [Fact]
    public void SimpleStyleApplication_ShouldApplyDefinedProperties()
    {
        // Arrange
        _resourceBuilder.Style<IPdfParagraphStyle>("TestStyle", p =>
        {
            p.TextColor(Colors.Red);
            p.FontSize(20);
        });

        var paragraphBuilder = new PdfParagraphBuilder("Test", new PdfFontRegistryBuilder());
        paragraphBuilder.Style("TestStyle");

        var paragraphData = (PdfParagraphData)paragraphBuilder.GetModel();
        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new StyleResolver(_resourceDictionary, _diagnosticSink);

        // Act
        styleResolver.ApplyStyles(elements);

        // Assert
        Assert.Equal(Colors.Red, paragraphData.CurrentTextColor);
        Assert.Equal(20, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void InheritedStyleApplication_ShouldApplyBaseAndDerivedProperties()
    {
        // Arrange
        _resourceBuilder.Style<IPdfParagraphStyle>("BaseStyle", p =>
        {
            p.TextColor(Colors.Gray);
            p.FontSize(12);
        });
        _resourceBuilder.Style<IPdfParagraphStyle>("DerivedStyle", "BaseStyle", p =>
        {
            p.FontSize(24); // Override base
        });

        var paragraphBuilder = new PdfParagraphBuilder("Test", new PdfFontRegistryBuilder());
        paragraphBuilder.Style("DerivedStyle");

        var paragraphData = (PdfParagraphData)paragraphBuilder.GetModel();
        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new StyleResolver(_resourceDictionary, _diagnosticSink);

        // Act
        styleResolver.ApplyStyles(elements);

        // Assert
        Assert.Equal(Colors.Gray, paragraphData.CurrentTextColor); // Inherited
        Assert.Equal(24, paragraphData.CurrentFontSize); // Overridden
    }

    [Fact]
    public void LocalValuePrecedence_ShouldPrioritizeLocalValueOverStyle()
    {
        // Arrange
        _resourceBuilder.Style<IPdfParagraphStyle>("TestStyle", p =>
        {
            p.TextColor(Colors.Red);
        });

        var paragraphBuilder = new PdfParagraphBuilder("Test", new PdfFontRegistryBuilder());
        paragraphBuilder.Style("TestStyle")
                        .TextColor(Colors.Blue); // Local value set via fluent API

        var paragraphData = (PdfParagraphData)paragraphBuilder.GetModel();
        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new StyleResolver(_resourceDictionary, _diagnosticSink);

        // Act
        styleResolver.ApplyStyles(elements);

        // Assert
        Assert.Equal(Colors.Blue, paragraphData.CurrentTextColor);
    }

    [Fact]
    public void CircularDependency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _resourceBuilder.Style<IPdfParagraphStyle>("StyleA", "StyleB", p => {});
        _resourceBuilder.Style<IPdfParagraphStyle>("StyleB", "StyleA", p => {});

        var paragraphBuilder = new PdfParagraphBuilder("Test", new PdfFontRegistryBuilder());
        paragraphBuilder.Style("StyleA");

        var paragraphData = (PdfParagraphData)paragraphBuilder.GetModel();
        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new StyleResolver(_resourceDictionary, _diagnosticSink);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => styleResolver.ApplyStyles(elements));
    }

    [Fact]
    public void MissingBasedOnStyle_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _resourceBuilder.Style<IPdfParagraphStyle>("MyStyle", "NonExistentBase", p => {});

        var paragraphBuilder = new PdfParagraphBuilder("Test", new PdfFontRegistryBuilder());
        paragraphBuilder.Style("MyStyle");

        var paragraphData = (PdfParagraphData)paragraphBuilder.GetModel();
        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new StyleResolver(_resourceDictionary, _diagnosticSink);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => styleResolver.ApplyStyles(elements));
    }

    [Fact]
    public void ResourcesApi_OnDocumentRoot_ShouldDefineStyleCorrectly()
    {
        // Arrange
        _docBuilder.Resources(rd =>
        {
            rd.Style<IPdfParagraphStyle>("FromDoc", p => p.TextColor(Colors.Green));
        });

        var styleResolver = new StyleResolver(_docBuilder._configurationBuilder.ResourceDictionary, _diagnosticSink);
        var paragraphBuilder = new PdfParagraphBuilder("Test", new PdfFontRegistryBuilder());
        paragraphBuilder.Style("FromDoc");

        var paragraphData = (PdfParagraphData)paragraphBuilder.GetModel();
        var elements = new List<PdfElementData> { paragraphData };

        // Act
        styleResolver.ApplyStyles(elements);

        // Assert
        Assert.Equal(Colors.Green, paragraphData.CurrentTextColor);
    }
}
