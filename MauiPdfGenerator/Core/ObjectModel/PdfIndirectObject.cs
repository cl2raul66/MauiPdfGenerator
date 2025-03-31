using MauiPdfGenerator.Common; // For PdfWriter context

namespace MauiPdfGenerator.Core.ObjectModel;

/// <summary>
/// Represents an indirect object definition ("objNum genNum obj ... endobj").
/// Internal as it's an implementation detail.
/// </summary>
internal record PdfIndirectObject
{
    public int ObjectNumber { get; }
    public int GenerationNumber { get; }
    public PdfObject ContainedObject { get; }
    public long ByteOffset { get; internal set; } // Set by PdfWriter

    public PdfIndirectObject(int objectNumber, PdfObject containedObject, int generationNumber = 0)
    {
        if (objectNumber <= 0) throw new ArgumentOutOfRangeException(nameof(objectNumber), "Object number must be positive.");
        if (generationNumber < 0) throw new ArgumentOutOfRangeException(nameof(generationNumber), "Generation number must be non-negative.");
        ObjectNumber = objectNumber;
        GenerationNumber = generationNumber;
        ContainedObject = containedObject ?? throw new ArgumentNullException(nameof(containedObject));
        ByteOffset = -1; // Not yet written
    }

    /// <summary>
    /// Writes the full indirect object definition (including obj/endobj) to the stream.
    /// </summary>
    public async Task WriteAsync(Stream stream, PdfWriter writer)
    {
        // Write header: "objNum genNum obj\n"
        string header = $"{ObjectNumber} {GenerationNumber} obj\n";
        await stream.WriteAsync(PdfEncodings.StructureEncoding.GetBytes(header));

        // Write the contained object itself
        await ContainedObject.WriteAsync(stream, writer);

        // Write footer: "\nendobj\n" (ensure newline before endobj)
        await stream.WriteAsync(PdfConstants.NewLine);
        await stream.WriteAsync(PdfConstants.EndObjKeyword);
        await stream.WriteAsync(PdfConstants.NewLine);
    }

    /// <summary>
    /// Creates a reference to this indirect object.
    /// </summary>
    public PdfReference CreateReference() => new(ObjectNumber, GenerationNumber);
}
