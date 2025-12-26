using MauiPdfGenerator.Fluent.Builders.Views;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfStackLayoutContentBuilder(dynamic layoutBuilder, PdfFontRegistryBuilder fontRegistry) : IPdfStackLayoutBuilder
{
    private readonly dynamic _layoutBuilder = layoutBuilder ?? throw new ArgumentNullException(nameof(layoutBuilder));
    private readonly PdfFontRegistryBuilder _fontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));

    public IPdfLayoutChildParagraph Paragraph(string text)
    {
        var builder = new PdfParagraphBuilder(text, _fontRegistry);
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfLayoutChildParagraph Paragraph(Action<IPdfSpanConfigurator> configure)
    {
        var builder = new PdfParagraphBuilder(configure, _fontRegistry);
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfLayoutChildHorizontalLine HorizontalLine()
    {
        var builder = new PdfHorizontalLineBuilder();
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfLayoutChildImage Image(Stream stream)
    {
        var builder = new PdfImageBuilder(stream);
        _layoutBuilder.Add(builder);
        return builder;
    }

    public void VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfVerticalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        _layoutBuilder.Add(stackBuilder);
    }

    public void HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        _layoutBuilder.Add(stackBuilder);
    }

    public void Grid(Action<IPdfGrid> layoutSetup)
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry);
        layoutSetup(gridBuilder);
        _layoutBuilder.Add(gridBuilder);
    }
}
