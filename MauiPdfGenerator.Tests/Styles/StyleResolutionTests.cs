using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Utils;
using Moq; 
using Xunit;

namespace MauiPdfGenerator.Tests.Styles;

public class StyleResolutionTests
{
    private readonly PdfResourceDictionary _resourceDictionary;
    private readonly Mock<IDiagnosticSink> _mockSink;
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly StyleResolver _resolver;

    public StyleResolutionTests()
    {
        _resourceDictionary = new PdfResourceDictionary();
        _mockSink = new Mock<IDiagnosticSink>();
        _fontRegistry = new PdfFontRegistryBuilder();

        _resolver = new StyleResolver(_resourceDictionary, _mockSink.Object, _fontRegistry);
    }

    [Fact]
    public void Priority_Local_Should_Win_Over_ExplicitStyle()
    {
        // Arrange
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        // CORRECCIÓN: Usamos IPdfParagraph
        styleBuilder.Style<IPdfParagraph>("RedText", s => s.TextColor(Colors.Red));

        var paragraphData = new PdfParagraphData("Test");
        // Simulamos valor local (Prioridad 3)
        paragraphData.TextColorProp.Set(Colors.Blue, PdfPropertyPriority.Local);
        paragraphData.Style("RedText");

        // Act
        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        // Assert: Gana Local (Blue)
        Assert.Equal(Colors.Blue, paragraphData.CurrentTextColor);
    }

    [Fact]
    public void Priority_ExplicitStyle_Should_Win_Over_Default()
    {
        // Arrange
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>("BigText", s => s.FontSize(20));

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("BigText");

        // Act
        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        // Assert: Gana Estilo (20)
        Assert.Equal(20f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Priority_ImplicitStyle_Should_Apply_To_Matching_Type()
    {
        // Arrange
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        // Estilo implícito (sin clave)
        styleBuilder.Style<IPdfParagraph>(s => s.FontSize(15));

        var paragraphData = new PdfParagraphData("Test");

        // Act
        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        // Assert: Gana Implícito (15)
        Assert.Equal(15f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Priority_Explicit_Should_Win_Over_Implicit()
    {
        // Arrange
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>(s => s.FontSize(10)); // Implícito (Prio 1)
        styleBuilder.Style<IPdfParagraph>("Explicit", s => s.FontSize(30)); // Explícito (Prio 2)

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("Explicit");

        // Act
        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        // Assert: Gana Explícito (30)
        Assert.Equal(30f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Inheritance_BasedOn_Should_Apply_Base_Then_Derived()
    {
        // Arrange
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);

        styleBuilder.Style<IPdfParagraph>("Base", s =>
        {
            s.TextColor(Colors.Red);
            s.FontSize(20);
        });

        styleBuilder.Style<IPdfParagraph>("Derived", "Base", s =>
        {
            s.TextColor(Colors.Blue);
        });

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("Derived");

        // Act
        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        // Assert
        Assert.Equal(Colors.Blue, paragraphData.CurrentTextColor); // Sobrescrito
        Assert.Equal(20f, paragraphData.CurrentFontSize);          // Heredado
    }

    [Fact]
    public void Mixed_Properties_Should_Merge_Correctly()
    {
        // Arrange
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>("MyStyle", s =>
        {
            s.FontSize(20);
            s.TextColor(Colors.Red);
        });

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("MyStyle");

        // Local gana en Color, pero FontSize se queda default (gana estilo)
        paragraphData.TextColorProp.Set(Colors.Green, PdfPropertyPriority.Local);

        // Act
        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        // Assert
        Assert.Equal(Colors.Green, paragraphData.CurrentTextColor);
        Assert.Equal(20f, paragraphData.CurrentFontSize);
    }
}
