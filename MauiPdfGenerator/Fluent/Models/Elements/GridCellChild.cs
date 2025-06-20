using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Models.Elements;

internal class GridCellChild : IGridCellChild
{
    internal PdfElement GetChild { get; }
    internal int GetRow { get; private set; } = 0;
    internal int GetColumn { get; private set; } = 0;
    internal int GetRowSpan { get; private set; } = 1;
    internal int GetColumnSpan { get; private set; } = 1;
    internal bool IsPositionExplicit { get; private set; } = false;

    public GridCellChild(PdfElement element)
    {
        GetChild = element;
    }

    public IGridCellChild Row(int row)
    {
        GetRow = row >= 0 ? row : 0;
        IsPositionExplicit = true;
        return this;
    }
    public IGridCellChild Column(int column)
    {
        GetColumn = column >= 0 ? column : 0;
        IsPositionExplicit = true;
        return this;
    }
    public IGridCellChild RowSpan(int span)
    {
        GetRowSpan = span > 1 ? span : 1;
        return this;
    }
    public IGridCellChild ColumnSpan(int span)
    {
        GetColumnSpan = span > 1 ? span : 1;
        return this;
    }
}
