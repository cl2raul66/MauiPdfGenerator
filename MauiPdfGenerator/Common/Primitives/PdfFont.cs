namespace MauiPdfGenerator.Common.Primitives;

internal struct PdfFont : IEquatable<PdfFont>
{
    public string Name { get; set; }
    public float Size { get; set; }
    public PdfFontStyle Style { get; set; }

    public PdfFont(string name, float size, PdfFontStyle style = PdfFontStyle.Normal)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "Helvetica"; // Default PDF font
        if (size <= 0) size = 10; // Default size

        Name = name;
        Size = size;
        Style = style;
    }

    public bool Equals(PdfFont other) => Name == other.Name && Size.Equals(other.Size) && Style == other.Style;
    public override bool Equals(object obj) => obj is PdfFont other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Name, Size, Style);
    public static bool operator ==(PdfFont left, PdfFont right) => left.Equals(right);
    public static bool operator !=(PdfFont left, PdfFont right) => !left.Equals(right);
    public override string ToString() => $"{Name}, {Size}, {Style}";
}
