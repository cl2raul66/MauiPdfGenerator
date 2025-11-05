using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfColumnDefinitionBuilder : IPdfColumnDefinitionBuilder
{
    private readonly List<ColumnDefinition> _columns = [];
    public IReadOnlyList<ColumnDefinition> Columns => _columns.AsReadOnly();

    public IPdfColumnDefinitionBuilder GridLength(GridLength width)
    {
        _columns.Add(new ColumnDefinition(width));
        return this;
    }

    public IPdfColumnDefinitionBuilder GridLength(GridUnitType gridUnitType, double value = 1)
    {
        _columns.Add(new ColumnDefinition(new GridLength(value, gridUnitType)));
        return this;
    }
}
