using MauiPdfGenerator.Fluent.Builders.Views;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PageContentBuilder(PdfFontRegistryBuilder fontRegistry) : IPageContentBuilder
{
    private readonly List<IBuildablePdfElement> _children = [];
    private readonly PdfFontRegistryBuilder _fontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));

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

    public void VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfVerticalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        _children.Add(stackBuilder);
    }

    public void HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        _children.Add(stackBuilder);
    }

    public void Grid(Action<IPdfGrid> layoutSetup)
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry);
        layoutSetup(gridBuilder);
        _children.Add(gridBuilder);
    }
}
