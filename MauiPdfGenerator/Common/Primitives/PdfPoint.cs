namespace MauiPdfGenerator.Common.Primitives;

internal struct PdfPoint : IEquatable<PdfPoint>
{
    public float X { get; set; }
    public float Y { get; set; }

    public PdfPoint(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static readonly PdfPoint Zero = new PdfPoint(0, 0);

    public bool Equals(PdfPoint other) => X.Equals(other.X) && Y.Equals(other.Y);
    public override bool Equals(object obj) => obj is PdfPoint other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static bool operator ==(PdfPoint left, PdfPoint right) => left.Equals(right);
    public static bool operator !=(PdfPoint left, PdfPoint right) => !left.Equals(right);
    public override string ToString() => $"{{X={X}, Y={Y}}}";
}
