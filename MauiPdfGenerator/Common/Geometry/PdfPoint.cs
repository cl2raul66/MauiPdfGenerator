using System.Diagnostics.CodeAnalysis;

namespace MauiPdfGenerator.Common.Geometry;

/// <summary>
/// Representa un punto 2D en coordenadas PDF.
/// </summary>
internal readonly struct PdfPoint : IEquatable<PdfPoint>
{
    /// <summary>
    /// La coordenada X del punto.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// La coordenada Y del punto.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Inicializa una nueva instancia de PdfPoint.
    /// </summary>
    public PdfPoint(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static PdfPoint Zero => new(0, 0);

    public void Deconstruct(out double x, out double y)
    {
        x = X;
        y = Y;
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
