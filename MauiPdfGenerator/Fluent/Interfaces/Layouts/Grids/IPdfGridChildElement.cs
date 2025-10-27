namespace MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

public interface IPdfGridChildElement<TSelf> where TSelf : IPdfGridChildElement<TSelf>
{
    TSelf Row(int row);
    TSelf Column(int column);
    TSelf RowSpan(int span);
    TSelf ColumnSpan(int span);
}
