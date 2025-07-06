using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Models.Layouts;

internal class GridCellChild<TElement> : IGridCellChild<TElement> where TElement : PdfElement
{
    public TElement Element { get; }

    public GridCellChild(TElement element)
    {
        Element = element;
    }

    public IGridCellChild<TElement> Row(int row) { Element.GridRow = row; return this; }
    public IGridCellChild<TElement> Column(int column) { Element.GridColumn = column; return this; }
    public IGridCellChild<TElement> RowSpan(int span) { Element.GridRowSpan = span; return this; }
    public IGridCellChild<TElement> ColumnSpan(int span) { Element.GridColumnSpan = span; return this; }

    public IGridCellChild<TElement> BackgroundColor(Color? color) { Element.BackgroundColor(color); return this; }
    public IGridCellChild<TElement> HorizontalOptions(LayoutAlignment alignment) { Element.HorizontalOptions(alignment); return this; }
    public IGridCellChild<TElement> VerticalOptions(LayoutAlignment alignment) { Element.VerticalOptions(alignment); return this; }
}
