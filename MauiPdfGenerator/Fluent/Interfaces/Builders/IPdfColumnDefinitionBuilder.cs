namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfColumnDefinitionBuilder
{
    IPdfColumnDefinitionBuilder GridLength(GridLength width);
    IPdfColumnDefinitionBuilder GridLength(GridUnitType gridUnitType, double value = 1);
}
