using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfColumnDefinitionBuilder : IPdfColumnDefinitionBuilder
{
    private readonly List<PdfColumnDefinition> _columns = [];
    public IReadOnlyList<PdfColumnDefinition> Columns => _columns.AsReadOnly();

    public IPdfColumnDefinitionBuilder GridLength(GridLength width)
    {
        var pdfGridLength = ConvertToPdfGridLength(width);
        _columns.Add(new PdfColumnDefinition(pdfGridLength));
        return this;
    }

    public IPdfColumnDefinitionBuilder GridLength(GridUnitType gridUnitType, double value = 1)
    {
        var pdfGridLength = ConvertToPdfGridLength(new GridLength(value, gridUnitType));
        _columns.Add(new PdfColumnDefinition(pdfGridLength));
        return this;
    }

    private static PdfGridLength ConvertToPdfGridLength(GridLength gridLength)
    {
        var pdfUnitType = gridLength.GridUnitType switch
        {
            GridUnitType.Absolute => PdfGridUnitType.Absolute,
            GridUnitType.Auto => PdfGridUnitType.Auto,
            GridUnitType.Star => PdfGridUnitType.Star,
            _ => throw new ArgumentOutOfRangeException(nameof(gridLength.GridUnitType))
        };
        return new PdfGridLength((float)gridLength.Value, pdfUnitType);
    }
}
