using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models.Elements;

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

    private IGridCellChild AddElement(PdfElement element)
    {
        var cellChild = new GridCellChild(element);
        _grid.AddChild(cellChild);
        return cellChild;
    }

    public IGridCellChild Paragraph(string text) => AddElement(new PdfParagraph(text, _fontRegistry));
    public IGridCellChild PdfImage(Stream stream) => AddElement(new PdfImage(stream));
    public IGridCellChild HorizontalLine() => AddElement(new PdfHorizontalLine());

    public IGridCellChild VerticalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfVerticalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        return AddElement(stack);
    }

    public IGridCellChild HorizontalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfHorizontalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        return AddElement(stack);
    }

    public IGridCellChild Grid(Action<IGridChildrenBuilder> content)
    {
        var grid = new PdfGrid(_fontRegistry);
        var builder = new GridChildrenBuilder(grid, _fontRegistry);
        content(builder);
        return AddElement(grid);
    }
}
