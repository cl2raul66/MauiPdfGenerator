using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfRowDefinitionBuilder
{
    IPdfRowDefinitionBuilder GridLength(PdfGridLength height);
    IPdfRowDefinitionBuilder GridLength(PdfGridUnitType gridUnitType, double value = 1);
}
