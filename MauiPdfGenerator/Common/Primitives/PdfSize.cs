namespace MauiPdfGenerator.Common.Primitives;

internal struct PdfSize : IEquatable<PdfSize>
{
    public float Width { get; set; }
    public float Height { get; set; }

    public PdfSize(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public static readonly PdfSize Empty = new PdfSize(0, 0);
    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Equals(PdfSize other) => Width.Equals(other.Width) && Height.Equals(other.Height);
    public override bool Equals(object obj) => obj is PdfSize other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Width, Height);
    public static bool operator ==(PdfSize left, PdfSize right) => left.Equals(right);
    public static bool operator !=(PdfSize left, PdfSize right) => !left.Equals(right);
    public override string ToString() => $"{{Width={Width}, Height={Height}}}";
}
