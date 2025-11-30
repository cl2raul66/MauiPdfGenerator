using Xunit;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;

namespace MauiPdfGenerator.Tests;

public class StyleSystemTests
{
    [Fact]
    public void SimpleStyleApplication_ShouldApplyDefinedProperties()
    {
        // Arrange
        var resourceDictionary = new PdfResourceDictionary();
        var resourceBuilder = new PdfResourceBuilder(resourceDictionary);
        resourceBuilder.Style<IPdfParagraph>("TestStyle", p =>
        {
            p.TextColor(Colors.Red);
            p.FontSize(20);
        });

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("TestStyle");

        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new Core.StyleResolver(resourceDictionary);

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
        var resourceDictionary = new PdfResourceDictionary();
        var resourceBuilder = new PdfResourceBuilder(resourceDictionary);
        resourceBuilder.Style<IPdfParagraph>("BaseStyle", p =>
        {
            p.TextColor(Colors.Gray);
            p.FontSize(12);
        });
        resourceBuilder.Style<IPdfParagraph>("DerivedStyle", "BaseStyle", p =>
        {
            p.FontSize(24); // Override base
        });

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("DerivedStyle");

        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new Core.StyleResolver(resourceDictionary);

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
        var resourceDictionary = new PdfResourceDictionary();
        var resourceBuilder = new PdfResourceBuilder(resourceDictionary);
        resourceBuilder.Style<IPdfParagraph>("TestStyle", p =>
        {
            p.TextColor(Colors.Red);
        });

        // Use the builder to set the local value
        var fontRegistry = new PdfFontRegistryBuilder();
        var paragraphBuilder = new PdfParagraphBuilder("Test", fontRegistry);
        paragraphBuilder.Style("TestStyle");
        paragraphBuilder.TextColor(Colors.Blue); // Local value set via fluent API

        var paragraphData = (PdfParagraphData)paragraphBuilder.GetModel();
        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new Core.StyleResolver(resourceDictionary);

        // Act
        styleResolver.ApplyStyles(elements);

        // Assert
        Assert.Equal(Colors.Blue, paragraphData.CurrentTextColor);
    }

    [Fact]
    public void CircularDependency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var resourceDictionary = new PdfResourceDictionary();
        var resourceBuilder = new PdfResourceBuilder(resourceDictionary);
        resourceBuilder.Style<IPdfParagraph>("StyleA", "StyleB", p => {});
        resourceBuilder.Style<IPdfParagraph>("StyleB", "StyleA", p => {});

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("StyleA");

        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new Core.StyleResolver(resourceDictionary);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => styleResolver.ApplyStyles(elements));
    }

    [Fact]
    public void MissingBasedOnStyle_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var resourceDictionary = new PdfResourceDictionary();
        var resourceBuilder = new PdfResourceBuilder(resourceDictionary);
        resourceBuilder.Style<IPdfParagraph>("MyStyle", "NonExistentBase", p => {});

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("MyStyle");

        var elements = new List<PdfElementData> { paragraphData };
        var styleResolver = new Core.StyleResolver(resourceDictionary);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => styleResolver.ApplyStyles(elements));
    }
}
