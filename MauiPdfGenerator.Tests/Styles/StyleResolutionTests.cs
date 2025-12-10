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
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>("RedText", s => s.TextColor(Colors.Red));

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.TextColorProp.Set(Colors.Blue, PdfPropertyPriority.Local);
        paragraphData.Style("RedText");

        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        Assert.Equal(Colors.Blue, paragraphData.CurrentTextColor);
    }

    [Fact]
    public void Priority_ExplicitStyle_Should_Win_Over_Default()
    {
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>("BigText", s => s.FontSize(20));

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("BigText");

        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        Assert.Equal(20f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Priority_ImplicitStyle_Should_Apply_To_Matching_Type()
    {
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>(s => s.FontSize(15));

        var paragraphData = new PdfParagraphData("Test");

        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        Assert.Equal(15f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Priority_Explicit_Should_Win_Over_Implicit()
    {
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>(s => s.FontSize(10)); 
        styleBuilder.Style<IPdfParagraph>("Explicit", s => s.FontSize(30)); 

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("Explicit");

        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        Assert.Equal(30f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Inheritance_BasedOn_Should_Apply_Base_Then_Derived()
    {
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

        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        Assert.Equal(Colors.Blue, paragraphData.CurrentTextColor); 
        Assert.Equal(20f, paragraphData.CurrentFontSize);       
    }

    [Fact]
    public void Mixed_Properties_Should_Merge_Correctly()
    {
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>("MyStyle", s =>
        {
            s.FontSize(20);
            s.TextColor(Colors.Red);
        });

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style("MyStyle");

        paragraphData.TextColorProp.Set(Colors.Green, PdfPropertyPriority.Local);

        _resolver.ApplyStyles(new List<PdfElementData> { paragraphData });

        Assert.Equal(Colors.Green, paragraphData.CurrentTextColor);
        Assert.Equal(20f, paragraphData.CurrentFontSize);
    }
}
