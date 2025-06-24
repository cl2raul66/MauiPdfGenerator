using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using Microsoft.Maui.Graphics;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Models.Layouts;

internal class GridCellChild<TElement> : IGridCellChild<TElement> where TElement : PdfElement
{
    public TElement Element { get; }

    public GridCellChild(TElement element)
    {
        Element = element;
    }

    public IGridCellChild<TElement> Row(int row) { Element.Row(row); return this; }
    public IGridCellChild<TElement> Column(int column) { Element.Column(column); return this; }
    public IGridCellChild<TElement> RowSpan(int span) { Element.RowSpan(span); return this; }
    public IGridCellChild<TElement> ColumnSpan(int span) { Element.ColumnSpan(span); return this; }

    public IGridCellChild<TElement> BackgroundColor(Color? color) { Element.BackgroundColor(color); return this; }
    public IGridCellChild<TElement> HorizontalOptions(LayoutAlignment alignment) { Element.HorizontalOptions(alignment); return this; }
    public IGridCellChild<TElement> VerticalOptions(LayoutAlignment alignment) { Element.VerticalOptions(alignment); return this; }

    // Métodos de estilo específicos para tipos
    public IGridCellChild<TElement> TextColor(Color color)
    {
        if (Element is PdfParagraph p) p.TextColor(color);
        return this;
    }
}
