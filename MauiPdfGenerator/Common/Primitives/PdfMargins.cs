namespace MauiPdfGenerator.Common.Primitives;

internal struct PdfMargins : IEquatable<PdfMargins>
{
    public float Left { get; set; }
    public float Top { get; set; }
    public float Right { get; set; }
    public float Bottom { get; set; }

    public PdfMargins(float all) : this(all, all, all, all) { }

    public PdfMargins(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public static readonly PdfMargins Zero = new PdfMargins(0);

    public bool Equals(PdfMargins other) => Left.Equals(other.Left) && Top.Equals(other.Top) && Right.Equals(other.Right) && Bottom.Equals(other.Bottom);
    public override bool Equals(object obj) => obj is PdfMargins other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);
    public static bool operator ==(PdfMargins left, PdfMargins right) => left.Equals(right);
    public static bool operator !=(PdfMargins left, PdfMargins right) => !left.Equals(right);
    public override string ToString() => $"{{Left={Left}, Top={Top}, Right={Right}, Bottom={Bottom}}}";
}
