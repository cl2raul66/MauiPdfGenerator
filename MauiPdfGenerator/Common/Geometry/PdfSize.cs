using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPdfGenerator.Common.Geometry;

/// <summary>
/// Represents an immutable size with Width and Height.
/// Dimensions must be non-negative.
/// Marked internal as it's primarily for internal library use.
/// </summary>
internal readonly struct PdfSize : IEquatable<PdfSize>
{
    /// <summary>
    /// Gets a size with zero width and height.
    /// </summary>
    public static readonly PdfSize Zero = new PdfSize(0, 0);

    /// <summary>
    /// Gets the width component of the size.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Gets the height component of the size.
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// Gets a value indicating whether this size has zero width and height.
    /// </summary>
    public bool IsZero => Width == 0 && Height == 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfSize"/> struct.
    /// </summary>
    /// <param name="width">The width (must be non-negative).</param>
    /// <param name="height">The height (must be non-negative).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if width or height is negative.</exception>
    public PdfSize(double width, double height)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width cannot be negative.");
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height cannot be negative.");

        Width = width;
        Height = height;
    }

    // --- Equality Methods ---

    public bool Equals(PdfSize other)
    {
        // See comment in PdfPoint regarding tolerance if needed
        return Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public override bool Equals(object? obj)
    {
        return obj is PdfSize other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    // --- Operators ---

    public static bool operator ==(PdfSize left, PdfSize right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PdfSize left, PdfSize right)
    {
        return !(left == right);
    }

    // --- ToString ---

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"{{Width={Width}, Height={Height}}}";
    }
}
