using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class GridDefinitionBuilder : IGridDefinitionBuilder
{
    private readonly List<PdfGridLength> _definitions = [];
    public IReadOnlyList<PdfGridLength> GetDefinitions() => _definitions.AsReadOnly();

    public void GridLength(double value)
    {
        _definitions.Add(new PdfGridLength(value, GridUnitType.Absolute));
    }

    public void GridLength(GridUnitType type, double value = 1.0)
    {
        _definitions.Add(new PdfGridLength(value, type));
    }
}
