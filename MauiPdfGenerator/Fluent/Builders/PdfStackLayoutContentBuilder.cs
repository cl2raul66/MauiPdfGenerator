using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Builders.Views;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfStackLayoutContentBuilder(dynamic layoutBuilder, PdfFontRegistryBuilder fontRegistry, PdfResourceDictionary? resourceDictionary = null) : IPdfStackLayoutBuilder
{
    private readonly dynamic _layoutBuilder = layoutBuilder ?? throw new ArgumentNullException(nameof(layoutBuilder));
    private readonly PdfFontRegistryBuilder _fontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));
    private readonly PdfResourceDictionary? _resourceDictionary = resourceDictionary;

    public IPdfLayoutChildParagraph Paragraph(string text)
    {
        var builder = new PdfParagraphBuilder(text, _fontRegistry, _resourceDictionary);
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfLayoutChildParagraph Paragraph(Action<IPdfSpanText> span)
    {
        var builder = new PdfParagraphBuilder(span, _fontRegistry, _resourceDictionary);
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
        var stackBuilder = new PdfVerticalStackLayoutBuilder(_fontRegistry, _resourceDictionary);
        layoutSetup(stackBuilder);
        _layoutBuilder.Add(stackBuilder);
    }

    public void HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder(_fontRegistry, _resourceDictionary);
        layoutSetup(stackBuilder);
        _layoutBuilder.Add(stackBuilder);
    }

    public void Grid(Action<IPdfGrid> layoutSetup)
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry, _resourceDictionary);
        layoutSetup(gridBuilder);
        _layoutBuilder.Add(gridBuilder);
    }
}
