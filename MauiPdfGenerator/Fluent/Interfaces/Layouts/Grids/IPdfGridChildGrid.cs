using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

public interface IPdfGridChildGrid : IPdfElement<IPdfGridChildGrid>, IPdfGridChildElement<IPdfGridChildGrid>
{
    IPdfGridChildGrid RowSpacing(double value);
    IPdfGridChildGrid ColumnSpacing(double value);
    IPdfGridLayout RowDefinitions(Action<IPdfRowDefinitionBuilder> builder);
    IPdfGridLayout ColumnDefinitions(Action<IPdfColumnDefinitionBuilder> builder);
    void Children(Action<IPdfGridChildrenBuilder> builder);
}
