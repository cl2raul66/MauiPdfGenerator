using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfColumnDefinitionBuilder : IPdfColumnDefinitionBuilder
{
    private readonly List<PdfColumnDefinition> _columns = [];
    public IReadOnlyList<PdfColumnDefinition> Columns => _columns.AsReadOnly();

    public IPdfColumnDefinitionBuilder GridLength(PdfGridLength width)
    {
        _columns.Add(new PdfColumnDefinition(width));
        return this;
    }

    public IPdfColumnDefinitionBuilder GridLength(PdfGridUnitType gridUnitType, double value = 1)
    {
        var pdfGridLength = gridUnitType switch
        {
            PdfGridUnitType.Absolute => PdfGridLength.FromAbsolute((float)value),
            PdfGridUnitType.Auto => PdfGridLength.Auto,
            PdfGridUnitType.Star => PdfGridLength.FromStar((float)value),
            _ => throw new ArgumentOutOfRangeException(nameof(gridUnitType))
        };
        _columns.Add(new PdfColumnDefinition(pdfGridLength));
        return this;
    }
}
