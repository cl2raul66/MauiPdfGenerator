using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfGrid : IPdfElement<IPdfGrid>
{
    IPdfGrid RowSpacing(double value);
    IPdfGrid ColumnSpacing(double value);
    IPdfGridLayout RowDefinitions(Action<IPdfRowDefinitionBuilder> builder);
    IPdfGridLayout ColumnDefinitions(Action<IPdfColumnDefinitionBuilder> builder);
    void Children(Action<IPdfGridChildrenBuilder> builder);
}

public interface IPdfGridLayout
{
    IPdfGridLayout RowDefinitions(Action<IPdfRowDefinitionBuilder> builder);
    IPdfGridLayout ColumnDefinitions(Action<IPdfColumnDefinitionBuilder> builder);
    void Children(Action<IPdfGridChildrenBuilder> builder);
}
