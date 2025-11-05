using MauiPdfGenerator.Fluent.Builders.Elements;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfStackLayoutContentBuilder : IPdfStackLayoutBuilder
{
    private readonly dynamic _layoutBuilder;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfStackLayoutContentBuilder(dynamic layoutBuilder, PdfFontRegistryBuilder fontRegistry)
    {
        _layoutBuilder = layoutBuilder ?? throw new ArgumentNullException(nameof(layoutBuilder));
        _fontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));
    }

    public IPdfLayoutChildParagraph Paragraph(string text)
    {
        var builder = new PdfParagraphBuilder(text, _fontRegistry);
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfLayoutChildHorizontalLine HorizontalLine()
    {
        var builder = new PdfHorizontalLineBuilder();
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfLayoutChildImage Image(System.IO.Stream stream)
    {
        var builder = new PdfImageBuilder(stream);
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfVerticalStackLayout VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfVerticalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        _layoutBuilder.Add(stackBuilder);
        return stackBuilder;
    }

    public IPdfHorizontalStackLayout HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        _layoutBuilder.Add(stackBuilder);
        return stackBuilder;
    }

    public IPdfGrid Grid(Action<IPdfGrid> layoutSetup)
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry);
        layoutSetup(gridBuilder);
        _layoutBuilder.Add(gridBuilder);
        return gridBuilder;
    }
}
