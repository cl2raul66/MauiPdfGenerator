namespace MauiPdfGenerator.Common.Geometry;

/// <summary>
/// Representa un rectángulo 2D con posición y tamaño.
/// </summary>
internal readonly struct PdfRectangle : IEquatable<PdfRectangle>
{
    /// <summary>
    /// La coordenada X del rectángulo.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// La coordenada Y del rectángulo.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// El ancho del rectángulo.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// El alto del rectángulo.
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// Inicializa una nueva instancia de PdfRectangle.
    /// </summary>
    public PdfRectangle(double x, double y, double width, double height)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width cannot be negative.");
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height cannot be negative.");

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    // --- Propiedades Derivadas ---

    public double Left => X;
    public double Top => Y;
    public double Right => X + Width;
    public double Bottom => Y + Height;

    public PdfPoint Location => new(X, Y);
    public PdfSize Size => new(Width, Height);

    public static PdfRectangle Empty => new(0, 0, 0, 0);

    // --- Métodos de Utilidad ---

    public bool Contains(PdfPoint point)
    {
        return point.X >= Left && point.X <= Right &&
               point.Y >= Top && point.Y <= Bottom;
    }

    public bool Contains(PdfRectangle rect)
    {
        return rect.Left >= Left && rect.Right <= Right &&
               rect.Top >= Top && rect.Bottom <= Bottom;
    }

    public bool IntersectsWith(PdfRectangle rect)
    {
        return rect.Left <= Right && rect.Right >= Left &&
               rect.Top <= Bottom && rect.Bottom >= Top;
    }

    // --- Métodos de Igualdad ---

    public bool Equals(PdfRectangle other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) &&
               Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public override bool Equals(object? obj)
    {
        return obj is PdfRectangle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    // --- Operadores ---

    public static bool operator ==(PdfRectangle left, PdfRectangle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PdfRectangle left, PdfRectangle right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }
}
