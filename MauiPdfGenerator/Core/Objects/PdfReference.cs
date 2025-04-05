using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF indirect reference (e.g., "12 0 R"). Section 7.3.10.
/// This object acts as a pointer to a PdfIndirectObject.
/// </summary>
internal sealed class PdfReference : PdfObject, IEquatable<PdfReference>
{
    /// <summary>
    /// Gets the object number (ID).
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the generation number (usually 0 for new objects).
    /// </summary>
    public int Generation { get; }

    // Optional: Could hold a direct reference to the target PdfIndirectObject
    // for easier navigation within the Core logic, but this MUST NOT be serialized.
    // public PdfIndirectObject Target { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfReference"/> class.
    /// </summary>
    /// <param name="id">The object number (must be positive).</param>
    /// <param name="generation">The generation number (must be non-negative).</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public PdfReference(int id, int generation = 0)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Object ID must be positive.");
        if (generation < 0) throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be non-negative.");

        Id = id;
        Generation = generation;
    }

    internal override void Write(StreamWriter writer, Encoding encoding)
    {
        // Format: "ID Generation R"
        writer.Write(Id.ToString()); // Culture invariant not needed for int
        writer.Write(' ');
        writer.Write(Generation.ToString());
        writer.Write(" R");
    }

    // --- Equality (based on ID and Generation) ---
    public bool Equals(PdfReference? other) => other is not null && Id == other.Id && Generation == other.Generation;
    public override bool Equals(object? obj) => obj is PdfReference other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Id, Generation);

    public static bool operator ==(PdfReference left, PdfReference right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(PdfReference left, PdfReference right) => !(left == right);


    public override string ToString() => $"{Id} {Generation} R"; // For debugging
}
