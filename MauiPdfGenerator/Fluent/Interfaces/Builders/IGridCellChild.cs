namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IGridCellChild
{
    IGridCellChild Row(int row);
    IGridCellChild Column(int column);
    IGridCellChild RowSpan(int span);
    IGridCellChild ColumnSpan(int span);
}
