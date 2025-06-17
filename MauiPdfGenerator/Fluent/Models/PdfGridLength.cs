namespace MauiPdfGenerator.Fluent.Models;

public readonly struct PdfGridLength
{
    public double Value { get; }
    public GridUnitType GridUnitType { get; }

    public PdfGridLength(double value, GridUnitType gridUnitType)
    {
        Value = value;
        GridUnitType = gridUnitType;
    }

    public static implicit operator PdfGridLength(double absoluteValue) => new(absoluteValue, GridUnitType.Absolute);
}
