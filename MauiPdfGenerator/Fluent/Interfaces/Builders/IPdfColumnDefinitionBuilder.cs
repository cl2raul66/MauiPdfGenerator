using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfColumnDefinitionBuilder
{
    IPdfColumnDefinitionBuilder GridLength(PdfGridLength width);
    IPdfColumnDefinitionBuilder GridLength(PdfGridUnitType gridUnitType, double value = 1);
}
