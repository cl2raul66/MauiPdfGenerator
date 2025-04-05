using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF indirect object, wrapping a direct PdfObject
/// and giving it an object number (ID) and generation number.
/// Format: "ID Generation obj ... endobj". Section 7.3.10.
/// </summary>
internal sealed class PdfIndirectObject // Doesn't inherit PdfObject directly, it's a container format
{
    /// <summary>
    /// Gets the object number (ID). Must be unique and positive within the document.
    /// </summary>
    public int Id { get; internal set; } // Settable by the writer/document manager

    /// <summary>
    /// Gets the generation number (usually 0 for new documents). Must be non-negative.
    /// </summary>
    public int Generation { get; internal set; } // Typically fixed at 0

    /// <summary>
    /// Gets the direct PdfObject contained within this indirect object.
    /// </summary>
    public PdfObject Value { get; }

    /// <summary>
    /// Gets the reference that points to this indirect object.
    /// </summary>
    public PdfReference Reference => new PdfReference(Id, Generation);

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfIndirectObject"/> class.
    /// The ID and Generation are typically assigned later by the document writer.
    /// </summary>
    /// <param name="value">The direct PdfObject to wrap. Cannot be null.</param>
    /// <param name="id">Optional initial ID (usually set later).</param>
    /// <param name="generation">Optional initial generation (usually 0).</param>
    public PdfIndirectObject(PdfObject value, int id = 0, int generation = 0)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Id = id; // Might be reassigned
        Generation = generation;
    }

    /// <summary>
    /// Writes the full indirect object syntax ("ID Gen obj ... endobj") to the writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="encoding">The encoding.</param>
    internal void Write(StreamWriter writer, Encoding encoding)
    {
        if (Id <= 0) throw new InvalidOperationException("Indirect object ID has not been assigned.");

        // Write header: "ID Generation obj"
        writer.Write(Id.ToString());
        writer.Write(' ');
        writer.Write(Generation.ToString());
        writer.WriteLine(" obj"); // " obj" followed by newline is conventional

        // Write the contained object's value
        Value.Write(writer, encoding);

        // Write trailer: "endobj"
        writer.WriteLine(); // Newline before endobj is conventional
        writer.WriteLine("endobj");
    }

    /// <summary>
    /// Asynchronously writes the full indirect object syntax ("ID Gen obj ... endobj") to the stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="encoding">The encoding.</param>
    internal async Task WriteAsync(Stream stream, Encoding encoding)
    {
        if (Id <= 0) throw new InvalidOperationException("Indirect object ID has not been assigned.");

        // Write header: "ID Generation obj\n"
        string header = $"{Id} {Generation} obj\n";
        byte[] headerBytes = encoding.GetBytes(header); // Use specified encoding for consistency
        await stream.WriteAsync(headerBytes, 0, headerBytes.Length);

        // Write the contained object's value asynchronously
        await Value.WriteAsync(stream, encoding);

        // Write trailer: "\nendobj\n" (Newlines before and after are conventional)
        byte[] trailerBytes = encoding.GetBytes("\nendobj\n");
        await stream.WriteAsync(trailerBytes, 0, trailerBytes.Length);
    }

    public override string ToString() => $"{Id} {Generation} obj ... endobj"; // Debugging
}
