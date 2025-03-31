using MauiPdfGenerator.Common; // For PdfWriter context

namespace MauiPdfGenerator.Core.ObjectModel;

/// <summary>
/// Represents an indirect object reference (e.g., "12 0 R").
/// Internal as it's an implementation detail.
/// </summary>
internal class PdfReference : PdfObject
{
    public int ObjectNumber { get; }
    public int GenerationNumber { get; }

    public PdfReference(int objectNumber, int generationNumber = 0)
    {
        if (objectNumber <= 0) throw new ArgumentOutOfRangeException(nameof(objectNumber), "Object number must be positive.");
        if (generationNumber < 0) throw new ArgumentOutOfRangeException(nameof(generationNumber), "Generation number must be non-negative.");
        ObjectNumber = objectNumber;
        GenerationNumber = generationNumber;
    }

    // Constructor helper from an indirect object
    public PdfReference(PdfIndirectObject indirectObject)
        : this(indirectObject.ObjectNumber, indirectObject.GenerationNumber) { }


    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        // Format: "objNum genNum R"
        string reference = $"{ObjectNumber} {GenerationNumber} R";
        await WriteBytesAsync(stream, PdfEncodings.StructureEncoding.GetBytes(reference));
    }

    public override bool Equals(object? obj) => obj is PdfReference other && Equals(other);
    public bool Equals(PdfReference other) => ObjectNumber == other.ObjectNumber && GenerationNumber == other.GenerationNumber;
    public override int GetHashCode() => HashCode.Combine(ObjectNumber, GenerationNumber);
    public override string ToString() => $"{ObjectNumber} {GenerationNumber} R";

    public static bool operator ==(PdfReference left, PdfReference right) => left.Equals(right);
    public static bool operator !=(PdfReference left, PdfReference right) => !left.Equals(right);
}
