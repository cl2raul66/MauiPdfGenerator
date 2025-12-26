using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Builders.Views;
using Xunit;

namespace MauiPdfGenerator.Tests.Fluent.Builders.Views;

public class PdfSpanConfiguratorTests
{
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfSpanConfiguratorTests()
    {
        _fontRegistry = new PdfFontRegistryBuilder();
    }

    [Fact]
    public void SpanConfigurator_MultipleSpans_PreservesOrder()
    {
        // Arrange
        var configurator = new PdfSpanConfigurator(_fontRegistry);
        var text1 = "First Span";
        var text2 = "Second Span";
        var text3 = "Third Span";

        // Act
        configurator.Span(text1);
        configurator.Span(text2);
        configurator.Span(text3);
        
        var spans = configurator.BuildSpans();

        // Assert
        Assert.Equal(3, spans.Count);
        Assert.Equal(text1, spans[0].Text);
        Assert.Equal(text2, spans[1].Text);
        Assert.Equal(text3, spans[2].Text);
    }
}
