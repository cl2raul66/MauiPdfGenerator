namespace MauiPdfGenerator.Fluent.Primitives;

/// <summary>
/// Specifies the unit of measure for fluent API methods accepting coordinates or sizes.
/// </summary>
public enum Unit
{
    /// <summary>
    /// Device-independent points (1/72nd of an inch).
    /// </summary>
    Points,
    /// <summary>
    /// Millimeters.
    /// </summary>
    Millimeters,
    /// <summary>
    /// Inches.
    /// </summary>
    Inches
}
