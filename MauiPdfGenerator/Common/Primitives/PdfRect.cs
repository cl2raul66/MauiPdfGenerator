namespace MauiPdfGenerator.Common.Primitives;

internal struct PdfRect : IEquatable<PdfRect>
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public PdfRect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public PdfRect(PdfPoint location, PdfSize size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.Width;
        Height = size.Height;
    }

    public PdfPoint Location => new PdfPoint(X, Y);
    public PdfSize Size => new PdfSize(Width, Height);
    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;
    public bool IsEmpty => Width <= 0 || Height <= 0;

    public static readonly PdfRect Empty = new PdfRect(0, 0, 0, 0);

    public bool Equals(PdfRect other) => X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
    public override bool Equals(object obj) => obj is PdfRect other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    public static bool operator ==(PdfRect left, PdfRect right) => left.Equals(right);
    public static bool operator !=(PdfRect left, PdfRect right) => !left.Equals(right);
    public override string ToString() => $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
}
