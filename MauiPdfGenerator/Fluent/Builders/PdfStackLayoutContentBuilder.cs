using MauiPdfGenerator.Fluent.Builders.Elements;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfStackLayoutContentBuilder : IPdfStackLayoutBuilder
{
    private readonly dynamic _layoutBuilder; // Can be VerticalStackLayoutBuilder or HorizontalStackLayoutBuilder
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfStackLayoutContentBuilder(dynamic layoutBuilder, PdfFontRegistryBuilder fontRegistry)
    {
        _layoutBuilder = layoutBuilder ?? throw new ArgumentNullException(nameof(layoutBuilder));
        _fontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));
    }

    public IPdfParagraph Paragraph(string text)
    {
        var builder = new PdfParagraphBuilder(text, _fontRegistry);
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfHorizontalLine HorizontalLine()
    {
        var builder = new PdfHorizontalLineBuilder();
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfImage Image(Stream stream)
    {
        var builder = new PdfImageBuilder(stream);
        _layoutBuilder.Add(builder);
        return builder;
    }

    public IPdfVerticalStackLayout VerticalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        var stackBuilder = new PdfVerticalStackLayoutBuilder();
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        _layoutBuilder.Add(stackBuilder);
        return stackBuilder;
    }

    public IPdfHorizontalStackLayout HorizontalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder();
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        _layoutBuilder.Add(stackBuilder);
        return stackBuilder;
    }
}
