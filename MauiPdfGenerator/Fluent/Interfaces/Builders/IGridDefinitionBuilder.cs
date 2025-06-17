namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IGridDefinitionBuilder
{
    void GridLength(double value);
    void GridLength(GridUnitType type, double value = 1.0);
}
