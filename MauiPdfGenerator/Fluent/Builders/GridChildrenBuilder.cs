using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
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

    private IGridCellChild<TElement> AddElement<TElement>(TElement element) where TElement : PdfElement
    {
        _grid.Add(element);
        return new GridCellChild<TElement>(element);
    }

    public IGridCellChild<PdfParagraph> Paragraph(string text) => AddElement(new PdfParagraph(text, _fontRegistry));
    public IGridCellChild<PdfImage> PdfImage(Stream stream) => AddElement(new PdfImage(stream));
    public IGridCellChild<PdfHorizontalLine> HorizontalLine() => AddElement(new PdfHorizontalLine());

    public IGridCellChild<PdfVerticalStackLayout> VerticalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfVerticalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        return AddElement(stack);
    }

    public IGridCellChild<PdfHorizontalStackLayout> HorizontalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfHorizontalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        return AddElement(stack);
    }

    public IGridCellChild<PdfGrid> Grid(Action<IPageContentBuilder> content)
    {
        var grid = new PdfGrid(_fontRegistry);
        var builder = new PageContentBuilder(_fontRegistry);
        content(builder);
        foreach (var item in builder.GetChildren())
        {
            grid.Add(item);
        }
        return AddElement(grid);
    }
}
