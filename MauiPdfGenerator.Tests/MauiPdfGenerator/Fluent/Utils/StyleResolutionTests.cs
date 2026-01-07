using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Utils;
using Moq;
using Xunit;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Fluent.Utils;

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
        var styleId = new PdfStyleIdentifier("RedText");
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);

        styleBuilder.Style<IPdfParagraph>("RedText", s => s.TextColor(Colors.Red));

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.TextColorProp.Set(Colors.Blue, PdfPropertyPriority.Local);
        paragraphData.Style(styleId);

        _resolver.ApplyStyles([paragraphData], null);

        Assert.Equal(Colors.Blue, paragraphData.CurrentTextColor);
    }

    [Fact]
    public void Priority_ExplicitStyle_Should_Win_Over_Default()
    {
        var styleId = new PdfStyleIdentifier("BigText");
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);

        styleBuilder.Style<IPdfParagraph>("BigText", s => s.FontSize(20));

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style(styleId);

        _resolver.ApplyStyles([paragraphData], null);

        Assert.Equal(20f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Priority_ImplicitStyle_Should_Apply_To_Matching_Type()
    {
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);
        styleBuilder.Style<IPdfParagraph>(s => s.FontSize(15));

        var paragraphData = new PdfParagraphData("Test");

        _resolver.ApplyStyles([paragraphData], null);

        Assert.Equal(15f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Priority_Explicit_Should_Win_Over_Implicit()
    {
        var explicitId = new PdfStyleIdentifier("Explicit");
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);

        styleBuilder.Style<IPdfParagraph>(s => s.FontSize(10)); 
        styleBuilder.Style<IPdfParagraph>("Explicit", s => s.FontSize(30)); 

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style(explicitId);

        _resolver.ApplyStyles([paragraphData], null);

        Assert.Equal(30f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Inheritance_BasedOn_Should_Apply_Base_Then_Derived()
    {
        var baseId = new PdfStyleIdentifier("Base");
        var derivedId = new PdfStyleIdentifier("Derived");
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);

        styleBuilder.Style<IPdfParagraph>("Base", s =>
        {
            s.TextColor(Colors.Red);
            s.FontSize(20);
        });

        styleBuilder.Style<IPdfParagraph>("Derived", baseId, s =>
        {
            s.TextColor(Colors.Blue);
        });

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style(derivedId);

        _resolver.ApplyStyles([paragraphData], null);

        Assert.Equal(Colors.Blue, paragraphData.CurrentTextColor);
        Assert.Equal(20f, paragraphData.CurrentFontSize);
    }

    [Fact]
    public void Mixed_Properties_Should_Merge_Correctly()
    {
        var styleId = new PdfStyleIdentifier("MyStyle");
        var styleBuilder = new PdfResourceBuilder(_resourceDictionary);

        styleBuilder.Style<IPdfParagraph>("MyStyle", s =>
        {
            s.FontSize(20);
            s.TextColor(Colors.Red);
        });

        var paragraphData = new PdfParagraphData("Test");
        paragraphData.Style(styleId);

        paragraphData.TextColorProp.Set(Colors.Green, PdfPropertyPriority.Local);

        _resolver.ApplyStyles([paragraphData], null);

        Assert.Equal(Colors.Green, paragraphData.CurrentTextColor);
        Assert.Equal(20f, paragraphData.CurrentFontSize);
    }
}
