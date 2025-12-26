using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Builders.Views;
using MauiPdfGenerator.Fluent.Models;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Xunit;

namespace MauiPdfGenerator.Tests.Fluent.Builders.Views;

public class PdfSpanBuilderTests
{
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfSpanBuilderTests()
    {
        _fontRegistry = new PdfFontRegistryBuilder();
    }

    [Fact]
    public void Constructor_SetsText()
    {
        // Arrange
        var text = "Hello World";
        
        // Act
        var builder = new PdfSpanBuilder(text, _fontRegistry);
        var model = builder.GetModel();

        // Assert
        Assert.Equal(text, model.Text);
    }

    [Fact]
    public void FontSize_SetsFontSizeProp()
    {
        // Arrange
        var builder = new PdfSpanBuilder("test", _fontRegistry);
        float size = 15.5f;

        // Act
        builder.FontSize(size);
        var model = builder.GetModel();

        // Assert
        Assert.Equal(size, model.FontSizeProp.Value);
    }

    [Fact]
    public void TextColor_SetsTextColorProp()
    {
        // Arrange
        var builder = new PdfSpanBuilder("test", _fontRegistry);
        var color = Colors.Red;

        // Act
        builder.TextColor(color);
        var model = builder.GetModel();

        // Assert
        Assert.Equal(color, model.TextColorProp.Value);
    }

    [Fact]
    public void FontAttributes_SetsFontAttributesProp()
    {
        // Arrange
        var builder = new PdfSpanBuilder("test", _fontRegistry);
        var attr = FontAttributes.Bold;

        // Act
        builder.FontAttributes(attr);
        var model = builder.GetModel();

        // Assert
        Assert.Equal(attr, model.FontAttributesProp.Value);
    }

    [Fact]
    public void TextDecorations_SetsTextDecorationsProp()
    {
        // Arrange
        var builder = new PdfSpanBuilder("test", _fontRegistry);
        var dec = TextDecorations.Underline;

        // Act
        builder.TextDecorations(dec);
        var model = builder.GetModel();

        // Assert
        Assert.Equal(dec, model.TextDecorationsProp.Value);
    }

    [Fact]
    public void TextTransform_SetsTextTransformProp()
    {
        // Arrange
        var builder = new PdfSpanBuilder("test", _fontRegistry);
        var trans = TextTransform.Uppercase;

        // Act
        builder.TextTransform(trans);
        var model = builder.GetModel();

        // Assert
        Assert.Equal(trans, model.TextTransformProp.Value);
    }

    [Fact]
    public void FontFamily_SetsFontFamilyAndResolvesRegistration()
    {
        // Arrange
        var family = new PdfFontIdentifier("OpenSansSemibold");
        _fontRegistry.Font(family); // Register it
        var builder = new PdfSpanBuilder("test", _fontRegistry);

        // Act
        builder.FontFamily(family);
        var model = builder.GetModel();

        // Assert
        Assert.Equal(family, model.FontFamilyProp.Value);
        Assert.NotNull(model.ResolvedFontRegistration);
        Assert.Equal(family.Alias, model.ResolvedFontRegistration.Identifier.Alias);
    }
}
