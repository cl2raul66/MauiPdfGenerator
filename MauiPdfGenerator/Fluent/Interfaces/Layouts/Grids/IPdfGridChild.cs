namespace MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

public interface IPdfGridChild<TSelf> : IPdfLayoutChild<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Row(int row);
    TSelf Column(int column);
    TSelf RowSpan(int span);
    TSelf ColumnSpan(int span);
}
