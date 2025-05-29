namespace MauiPdfGenerator.Fluent.Models;

public readonly struct PdfFontIdentifier : IEquatable<PdfFontIdentifier>
{
    public string Alias { get; }

    public PdfFontIdentifier(string alias)
    {
        ArgumentException.ThrowIfNullOrEmpty(alias, nameof(alias));
        Alias = alias;
    }

    public override bool Equals(object? obj) => obj is PdfFontIdentifier other && Equals(other);

    public bool Equals(PdfFontIdentifier other) => StringComparer.OrdinalIgnoreCase.Equals(Alias, other.Alias);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Alias);

    public static bool operator ==(PdfFontIdentifier left, PdfFontIdentifier right) => left.Equals(right);

    public static bool operator !=(PdfFontIdentifier left, PdfFontIdentifier right) => !left.Equals(right);

    public override string ToString() => Alias;
}
