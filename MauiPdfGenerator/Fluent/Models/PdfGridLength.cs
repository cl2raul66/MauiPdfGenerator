namespace MauiPdfGenerator.Fluent.Models;

/// <summary>
/// Represents the length of a Grid row or column dimension.
/// Mirrors Microsoft.Maui.GridLength functionality but uses PdfGridUnitType.
/// </summary>
public readonly struct PdfGridLength : IEquatable<PdfGridLength>
{
    // --- Static Predefined Values ---
    /// <summary>
    /// Represents automatic sizing based on content.
    /// Use with 'using static MauiPdfGenerator.Fluent.Models.PdfGridLength;'.
    /// </summary>
    public static readonly PdfGridLength Auto = new PdfGridLength(-1, PdfGridUnitType.Auto);

    /// <summary>
    /// Represents the default star value (1*).
    /// Use with 'using static MauiPdfGenerator.Fluent.Models.PdfGridLength;'.
    /// </summary>
    public static readonly PdfGridLength DefaultStar = new PdfGridLength(1, PdfGridUnitType.Star);

    // --- Instance Properties ---
    public double Value { get; }
    public PdfGridUnitType GridUnitType { get; }

    public bool IsAbsolute => GridUnitType == PdfGridUnitType.Absolute;
    public bool IsAuto => GridUnitType == PdfGridUnitType.Auto;
    public bool IsStar => GridUnitType == PdfGridUnitType.Star;

    // --- Constructors ---
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfGridLength"/> struct with an absolute value.
    /// </summary>
    /// <param name="value">The absolute width or height in device-independent units.</param>
    public PdfGridLength(double value) : this(value, PdfGridUnitType.Absolute) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfGridLength"/> struct with a specified value and unit type.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="type">The unit type (Absolute, Auto, Star).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if value is negative for Absolute or Star types.</exception>
    /// <exception cref="ArgumentException">Thrown if type is Auto but value is not the internal marker (-1).</exception>
    public PdfGridLength(double value, PdfGridUnitType type)
    {
        if (value < 0 && type != PdfGridUnitType.Auto) // Allow negative only for Auto marker
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative, except for internal Auto marker.");
        if (type == PdfGridUnitType.Auto && value != -1)
            throw new ArgumentException("Value must be -1 for GridUnitType.Auto", nameof(value));


        Value = value;
        GridUnitType = type;
    }

    // --- Static Factory Methods ---

    /// <summary>
    /// Creates an Absolute (Fixed) PdfGridLength.
    /// Use with 'using static MauiPdfGenerator.Fluent.Models.PdfGridLength;'.
    /// </summary>
    /// <param name="value">The absolute length.</param>
    public static PdfGridLength Fixed(double value) => new PdfGridLength(value, PdfGridUnitType.Absolute);

    /// <summary>
    /// Creates a Star (Proportional) PdfGridLength.
    /// Use with 'using static MauiPdfGenerator.Fluent.Models.PdfGridLength;'.
    /// </summary>
    /// <param name="value">The proportional weight (e.g., 1 for 1*, 2 for 2*).</param>
    public static PdfGridLength Star(double value) => new PdfGridLength(value, PdfGridUnitType.Star);

    // --- Equality Members ---
    public bool Equals(PdfGridLength other) => Value == other.Value && GridUnitType == other.GridUnitType;
    public override bool Equals(object? obj) => obj is PdfGridLength other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value, GridUnitType);
    public static bool operator ==(PdfGridLength left, PdfGridLength right) => left.Equals(right);
    public static bool operator !=(PdfGridLength left, PdfGridLength right) => !left.Equals(right);

    public override string ToString()
    {
        return GridUnitType switch
        {
            PdfGridUnitType.Absolute => Value.ToString(),
            PdfGridUnitType.Auto => "Auto",
            PdfGridUnitType.Star => (Value == 1 ? "*" : $"{Value}*"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

/// <summary>
/// Defines the type of unit used for PdfGridLength.
/// </summary>
public enum PdfGridUnitType
{
    /// <summary>
    /// Size is determined by the content of the cell (pixels).
    /// </summary>
    Absolute,
    /// <summary>
    /// Size is determined by the content unless explicit size is set (internal marker).
    /// </summary>
    Auto,
    /// <summary>
    /// Size is proportional to the available space.
    /// </summary>
    Star
}
