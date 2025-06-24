namespace MauiPdfGenerator.Common;

internal interface IGridCellInfo
{
    int Row { get; }
    int Column { get; }
    int RowSpan { get; }
    int ColumnSpan { get; }
}
