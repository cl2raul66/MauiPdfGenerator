using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfRowDefinitionBuilder : IPdfRowDefinitionBuilder
{
    private readonly List<RowDefinition> _rows = [];
    public IReadOnlyList<RowDefinition> Rows => _rows.AsReadOnly();

    public IPdfRowDefinitionBuilder GridLength(GridLength height)
    {
        _rows.Add(new RowDefinition(height));
        return this;
    }

    public IPdfRowDefinitionBuilder GridLength(GridUnitType gridUnitType, double value = 1)
    {
        _rows.Add(new RowDefinition(new GridLength(value, gridUnitType)));
        return this;
    }
}
