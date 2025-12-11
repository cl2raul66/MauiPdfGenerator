namespace MauiPdfGenerator.Fluent.Models;

public readonly struct PdfStyleIdentifier : IEquatable<PdfStyleIdentifier>
{
    public string Key { get; }

    public PdfStyleIdentifier(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
        Key = key;
    }

    public override bool Equals(object? obj) => obj is PdfStyleIdentifier other && Equals(other);

    public bool Equals(PdfStyleIdentifier other) => StringComparer.OrdinalIgnoreCase.Equals(Key, other.Key);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Key);

    public static bool operator ==(PdfStyleIdentifier left, PdfStyleIdentifier right) => left.Equals(right);

    public static bool operator !=(PdfStyleIdentifier left, PdfStyleIdentifier right) => !left.Equals(right);

    public override string ToString() => Key;
}
