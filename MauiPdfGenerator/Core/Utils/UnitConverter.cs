// MauiPdfGenerator/Core/Utils/UnitConverter.cs
using System;
using MauiPdfGenerator.Common.Primitives; // For UnitOfMeasure

namespace MauiPdfGenerator.Core.Utils;

/// <summary>
/// Utility class to convert values between different units of measure
/// (Points, Millimeters, Inches) and the standard PDF/SkiaSharp unit (Points).
/// </summary>
internal class UnitConverter
{
    private const float MmPerInch = 25.4f;
    private const float PointsPerInch = 72.0f;
    private const float PointsPerMm = PointsPerInch / MmPerInch; // Approx 2.8346

    private readonly UnitOfMeasure _sourceUnits;

    /// <summary>
    /// Initializes a new UnitConverter based on the primary unit used in the document model.
    /// </summary>
    /// <param name="sourceUnits">The UnitOfMeasure used in the PdfDocumentModel settings.</param>
    public UnitConverter(UnitOfMeasure sourceUnits)
    {
        _sourceUnits = sourceUnits;
    }

    /// <summary>
    /// Converts a value from the specified unit to Points.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="unit">The unit of the input value.</param>
    /// <returns>The value converted to Points.</returns>
    public float ToPoints(float value, UnitOfMeasure unit)
    {
        return unit switch
        {
            UnitOfMeasure.Points => value,
            UnitOfMeasure.Inches => value * PointsPerInch,
            UnitOfMeasure.Millimeters => value * PointsPerMm,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), $"Unsupported unit: {unit}"),
        };
    }

    /// <summary>
    /// Converts a value from the document's source units to Points.
    /// </summary>
    /// <param name="value">The value in the document's source units.</param>
    /// <returns>The value converted to Points.</returns>
    public float ToPoints(float value)
    {
        return ToPoints(value, _sourceUnits);
    }

    /// <summary>
    /// Converts a value from Points to the specified unit.
    /// </summary>
    /// <param name="valueInPoints">The value in Points.</param>
    /// <param name="targetUnit">The desired output unit.</param>
    /// <returns>The value converted to the target unit.</returns>
    public float FromPoints(float valueInPoints, UnitOfMeasure targetUnit)
    {
        return targetUnit switch
        {
            UnitOfMeasure.Points => valueInPoints,
            UnitOfMeasure.Inches => valueInPoints / PointsPerInch,
            UnitOfMeasure.Millimeters => valueInPoints / PointsPerMm,
            _ => throw new ArgumentOutOfRangeException(nameof(targetUnit), $"Unsupported unit: {targetUnit}"),
        };
    }

    /// <summary>
    /// Converts a value from Points to the document's source units.
    /// </summary>
    /// <param name="valueInPoints">The value in Points.</param>
    /// <returns>The value converted to the document's source units.</returns>
    public float FromPoints(float valueInPoints)
    {
        return FromPoints(valueInPoints, _sourceUnits);
    }
}
