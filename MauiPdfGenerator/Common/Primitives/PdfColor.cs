namespace MauiPdfGenerator.Common.Primitives;

internal struct PdfColor : IEquatable<PdfColor>
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; } // Alpha channel (0=transparent, 255=opaque)

    public PdfColor(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    // --- Predefined Colors ---
    public static PdfColor Transparent => new PdfColor(0, 0, 0, 0);
    public static PdfColor Black => new PdfColor(0, 0, 0);
    public static PdfColor White => new PdfColor(255, 255, 255);
    public static PdfColor Red => new PdfColor(255, 0, 0);
    public static PdfColor Lime => new PdfColor(0, 255, 0);
    public static PdfColor Blue => new PdfColor(0, 0, 255);
    public static PdfColor Yellow => new PdfColor(255, 255, 0);
    public static PdfColor Cyan => new PdfColor(0, 255, 255);
    public static PdfColor Magenta => new PdfColor(255, 0, 255);
    public static PdfColor Silver => new PdfColor(192, 192, 192);
    public static PdfColor Gray => new PdfColor(128, 128, 128);
    public static PdfColor Maroon => new PdfColor(128, 0, 0);
    public static PdfColor Olive => new PdfColor(128, 128, 0);
    public static PdfColor Green => new PdfColor(0, 128, 0);
    public static PdfColor Purple => new PdfColor(128, 0, 128);
    public static PdfColor Teal => new PdfColor(0, 128, 128);
    public static PdfColor Navy => new PdfColor(0, 0, 128);
    public static PdfColor Orange => new PdfColor(255, 165, 0);

    // --- Equality ---
    public bool Equals(PdfColor other) => R == other.R && G == other.G && B == other.B && A == other.A;
    public override bool Equals(object obj) => obj is PdfColor other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public static bool operator ==(PdfColor left, PdfColor right) => left.Equals(right);
    public static bool operator !=(PdfColor left, PdfColor right) => !left.Equals(right);
    public override string ToString() => $"{{R={R}, G={G}, B={B}, A={A}}}";
}
