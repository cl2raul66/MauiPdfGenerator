namespace MauiPdfGenerator.Common;

internal interface IPdfGridCellInfo
{
    int Row { get; }
    int Column { get; }
    int RowSpan { get; }
    int ColumnSpan { get; }
}
