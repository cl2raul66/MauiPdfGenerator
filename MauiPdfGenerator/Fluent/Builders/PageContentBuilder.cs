using MauiPdfGenerator.Fluent.Builders.Elements;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PageContentBuilder : IPageContentBuilder
{
    private readonly List<IBuildablePdfElement> _children = [];
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PageContentBuilder(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));
    }

    internal IReadOnlyList<IBuildablePdfElement> GetBuildableChildren() => _children.AsReadOnly();

    public IPdfParagraph Paragraph(string text)
    {
        var builder = new PdfParagraphBuilder(text, _fontRegistry);
        _children.Add(builder);
        return builder;
    }

    public IPdfHorizontalLine HorizontalLine()
    {
        var builder = new PdfHorizontalLineBuilder();
        _children.Add(builder);
        return builder;
    }

    public IPdfImage Image(Stream stream)
    {
        var builder = new PdfImageBuilder(stream);
        _children.Add(builder);
        return builder;
    }

    public IPdfVerticalStackLayout VerticalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        var stackBuilder = new PdfVerticalStackLayoutBuilder();
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        _children.Add(stackBuilder);
        return stackBuilder;
    }

    public IPdfHorizontalStackLayout HorizontalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder();
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        _children.Add(stackBuilder);
        return stackBuilder;
    }
}
