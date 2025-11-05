namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfRowDefinitionBuilder
{
    IPdfRowDefinitionBuilder GridLength(GridLength height);
    IPdfRowDefinitionBuilder GridLength(GridUnitType gridUnitType, double value = 1);
}
