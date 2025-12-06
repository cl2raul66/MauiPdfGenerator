using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfGrid<TSelf> : IPdfElement<TSelf>, IPdfLayoutChild<TSelf>
    where TSelf : IPdfElement<TSelf>
{
    TSelf RowSpacing(double value);
    TSelf ColumnSpacing(double value);

    IPdfGridLayout RowDefinitions(Action<IPdfRowDefinitionBuilder> builder);
    IPdfGridLayout ColumnDefinitions(Action<IPdfColumnDefinitionBuilder> builder);
    void Children(Action<IPdfGridChildrenBuilder> builder);
}

public interface IPdfGrid : IPdfGrid<IPdfGrid>
{
}

public interface IPdfGridLayout
{
    IPdfGridLayout RowDefinitions(Action<IPdfRowDefinitionBuilder> builder);
    IPdfGridLayout ColumnDefinitions(Action<IPdfColumnDefinitionBuilder> builder);
    void Children(Action<IPdfGridChildrenBuilder> builder);
}
