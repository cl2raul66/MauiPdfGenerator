using MauiPdfGenerator.Fluent.Builders.Elements;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

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

    public IPdfPageChildParagraph Paragraph(string text)
    {
        var builder = new PdfParagraphBuilder(text, _fontRegistry);
        _children.Add(builder);
        return builder;
    }

    public IPdfPageChildHorizontalLine HorizontalLine()
    {
        var builder = new PdfHorizontalLineBuilder();
        _children.Add(builder);
        return builder;
    }

    public IPdfPageChildImage Image(Stream stream)
    {
        var builder = new PdfImageBuilder(stream);
        _children.Add(builder);
        return builder;
    }

    public IPdfVerticalStackLayout VerticalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        // CORRECCIÓN: Se pasa el fontRegistry al constructor.
        var stackBuilder = new PdfVerticalStackLayoutBuilder(_fontRegistry);
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        _children.Add(stackBuilder);
        return stackBuilder;
    }

    public IPdfHorizontalStackLayout HorizontalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        // CORRECCIÓN: Se pasa el fontRegistry al constructor.
        var stackBuilder = new PdfHorizontalStackLayoutBuilder(_fontRegistry);
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        _children.Add(stackBuilder);
        return stackBuilder;
    }

    public IPdfGrid Grid()
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry);
        _children.Add(gridBuilder);
        return gridBuilder;
    }
}
