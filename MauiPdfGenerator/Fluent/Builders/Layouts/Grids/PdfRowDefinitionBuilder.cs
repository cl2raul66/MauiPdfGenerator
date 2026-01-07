using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfRowDefinitionBuilder : IPdfRowDefinitionBuilder
{
    private readonly List<PdfRowDefinition> _rows = [];
    public IReadOnlyList<PdfRowDefinition> Rows => _rows.AsReadOnly();

    public IPdfRowDefinitionBuilder GridLength(PdfGridLength height)
    {
        _rows.Add(new PdfRowDefinition(height));
        return this;
    }

    public IPdfRowDefinitionBuilder GridLength(PdfGridUnitType gridUnitType, double value = 1)
    {
        var pdfGridLength = gridUnitType switch
        {
            PdfGridUnitType.Absolute => PdfGridLength.FromAbsolute((float)value),
            PdfGridUnitType.Auto => PdfGridLength.Auto,
            PdfGridUnitType.Star => PdfGridLength.FromStar((float)value),
            _ => throw new ArgumentOutOfRangeException(nameof(gridUnitType))
        };
        _rows.Add(new PdfRowDefinition(pdfGridLength));
        return this;
    }
}
