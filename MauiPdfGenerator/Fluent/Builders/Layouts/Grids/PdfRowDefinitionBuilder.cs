using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfRowDefinitionBuilder : IPdfRowDefinitionBuilder
{
    private readonly List<PdfRowDefinition> _rows = [];
    public IReadOnlyList<PdfRowDefinition> Rows => _rows.AsReadOnly();

    public IPdfRowDefinitionBuilder GridLength(GridLength height)
    {
        var pdfGridLength = ConvertToPdfGridLength(height);
        _rows.Add(new PdfRowDefinition(pdfGridLength));
        return this;
    }

    public IPdfRowDefinitionBuilder GridLength(GridUnitType gridUnitType, double value = 1)
    {
        var pdfGridLength = ConvertToPdfGridLength(new GridLength(value, gridUnitType));
        _rows.Add(new PdfRowDefinition(pdfGridLength));
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
