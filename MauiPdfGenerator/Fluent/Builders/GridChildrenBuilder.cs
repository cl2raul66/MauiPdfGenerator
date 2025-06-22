using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models.Layouts;

namespace MauiPdfGenerator.Fluent.Builders;

internal class GridChildrenBuilder : IGridChildrenBuilder
{
    private readonly PdfGrid _grid;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public GridChildrenBuilder(PdfGrid grid, PdfFontRegistryBuilder fontRegistry)
    {
        _grid = grid;
        _fontRegistry = fontRegistry;
    }

    private PdfElement AddElement(PdfElement element)
    {
        _grid.Add(element);
        return element;
    }

    public PdfParagraph Paragraph(string text) => (PdfParagraph)AddElement(new PdfParagraph(text, _fontRegistry));
    public PdfImage PdfImage(Stream stream) => (PdfImage)AddElement(new PdfImage(stream));
    public PdfHorizontalLine HorizontalLine() => (PdfHorizontalLine)AddElement(new PdfHorizontalLine());

    public PdfVerticalStackLayout VerticalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfVerticalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        return (PdfVerticalStackLayout)AddElement(stack);
    }

    public PdfHorizontalStackLayout HorizontalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfHorizontalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        return (PdfHorizontalStackLayout)AddElement(stack);
    }

    public PdfGrid Grid(Action<IPageContentBuilder> content)
    {
        var grid = new PdfGrid(_fontRegistry);
        var builder = new PageContentBuilder(_fontRegistry);
        content(builder);
        foreach (var item in builder.GetChildren())
        {
            grid.Add(item);
        }
        return (PdfGrid)AddElement(grid);
    }
}
