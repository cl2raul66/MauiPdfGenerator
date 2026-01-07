using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Builders.Views;
using Xunit;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Fluent.Builders.Views;

public class PdfSpanConfiguratorTests
{
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfSpanConfiguratorTests()
    {
        _fontRegistry = new PdfFontRegistryBuilder();
    }

    [Fact]
    public void SpanConfigurator_MultipleSpans_PreservesOrderAndTextLengths()
    {
        // Arrange
        var configurator = new PdfSpanConfigurator(_fontRegistry);
        var text1 = "First Span";
        var text2 = "Second Span";
        var text3 = "Third Span";

        // Act
        configurator.Text(text1);
        configurator.Text(text2);
        configurator.Text(text3);
        
        var (_, spans) = configurator.Build();

        // Assert
        Assert.Equal(3, spans.Count);
        Assert.Equal(text1.Length, spans[0].TextLength);
        Assert.Equal(text2.Length, spans[1].TextLength);
        Assert.Equal(text3.Length, spans[2].TextLength);
    }
}
