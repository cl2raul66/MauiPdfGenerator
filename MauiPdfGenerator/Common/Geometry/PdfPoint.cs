using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPdfGenerator.Common.Geometry;

/// <summary>
/// Represents an immutable point with X and Y coordinates (typically in PDF units).
/// Marked internal as it's primarily for internal library use.
/// </summary>
internal readonly struct PdfPoint : IEquatable<PdfPoint>
{
    /// <summary>
    /// Gets the point at the origin (0, 0).
    /// </summary>
    public static readonly PdfPoint Zero = new PdfPoint(0, 0);

    /// <summary>
    /// Gets the X-coordinate of the point.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets the Y-coordinate of the point.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfPoint"/> struct.
    /// </summary>
    /// <param name="x">The X-coordinate.</param>
    /// <param name="y">The Y-coordinate.</param>
    public PdfPoint(double x, double y)
    {
        X = x;
        Y = y;
    }

    // --- Equality Methods ---

    public bool Equals(PdfPoint other)
    {
        // Using tolerance might be needed if calculations involve floating point inaccuracies,
        // but for simple storage and retrieval, direct comparison is often sufficient.
        // const double tolerance = 1e-9;
        // return Math.Abs(X - other.X) < tolerance && Math.Abs(Y - other.Y) < tolerance;
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is PdfPoint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    // --- Operators ---

    public static bool operator ==(PdfPoint left, PdfPoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PdfPoint left, PdfPoint right)
    {
        return !(left == right);
    }

    // Optional: Vector-like operations if needed later
    public static PdfPoint operator +(PdfPoint pt, PdfSize sz) => new PdfPoint(pt.X + sz.Width, pt.Y + sz.Height);
    public static PdfPoint operator -(PdfPoint pt, PdfSize sz) => new PdfPoint(pt.X - sz.Width, pt.Y - sz.Height);


    // --- ToString ---

    [ExcludeFromCodeCoverage] // Standard simple ToString
    public override string ToString()
    {
        return $"{{X={X}, Y={Y}}}";
    }
}
