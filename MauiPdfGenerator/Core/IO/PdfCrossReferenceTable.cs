using MauiPdfGenerator.Core.Objects;
using System.Text;

namespace MauiPdfGenerator.Core.IO;

/// <summary>
/// Represents and writes the PDF Cross-Reference Table (xref). Section 7.5.4.
/// Uses the traditional table format.
/// </summary>
internal class PdfCrossReferenceTable
{
    // Stores the byte offset for each object ID. Key: ObjectID, Value: Byte Offset
    private readonly Dictionary<int, long> _objectOffsets =[];
    private int _highestObjectId = 0; // Track the highest ID used for table size

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfCrossReferenceTable"/> class.
    /// </summary>
    public PdfCrossReferenceTable()
    {
        // Entry 0 is mandatory and has generation 65535, marks the head of the free list.
        // It typically points nowhere meaningful in simple new documents. Offset 0.
        _objectOffsets[0] = 0;
    }

    /// <summary>
    /// Records the byte offset where an indirect object starts in the output stream.
    /// </summary>
    /// <param name="indirectObject">The indirect object whose offset is being recorded.</param>
    /// <param name="offset">The byte offset in the output stream.</param>
    public void RegisterObjectOffset(PdfIndirectObject indirectObject, long offset)
    {
        if (indirectObject.Id <= 0)
            throw new ArgumentException("Cannot register offset for object with invalid ID.", nameof(indirectObject));
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");

        _objectOffsets[indirectObject.Id] = offset;
        if (indirectObject.Id > _highestObjectId)
        {
            _highestObjectId = indirectObject.Id;
        }
    }

    /// <summary>
    /// Writes the cross-reference table to the specified stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <returns>The byte offset where the xref table started.</returns>
    public async Task<long> WriteAsync(Stream stream)
    {
        long startOffset = stream.Position;
        var ascii = Encoding.ASCII;

        await stream.WriteAsync(ascii.GetBytes("xref\n"), 0, 5);

        // PDF spec requires entries to be grouped into contiguous subsections.
        // For simplicity in new documents where IDs are sequential, we write one subsection.
        // A more robust implementation would handle fragmented IDs and free objects.

        // Subsection header: "startIndex count"
        // We start at 0 and go up to _highestObjectId + 1 (to include the highest ID)
        int objectCount = _highestObjectId + 1;
        string subsectionHeader = $"0 {objectCount}\n";
        await stream.WriteAsync(ascii.GetBytes(subsectionHeader), 0, subsectionHeader.Length);

        // Write entries for each object ID from 0 to highestObjectId
        for (int id = 0; id < objectCount; id++)
        {
            string entry;
            if (id == 0)
            {
                // Special entry 0: 10-digit offset (0), 5-digit generation (65535), ' f ' (free)
                entry = "0000000000 65535 f \n"; // EOL is CR LF or LF, we use LF
            }
            else if (_objectOffsets.TryGetValue(id, out long offset))
            {
                // Object exists: 10-digit offset, 5-digit generation (0), ' n ' (in use)
                // Format offset padded with leading zeros to 10 digits.
                // Format generation (0) padded with leading zeros to 5 digits.
                entry = $"{offset:D10} 00000 n \n";
            }
            else
            {
                // Object ID doesn't exist (e.g., if IDs weren't sequential).
                // Mark as free, pointing to the next free object (or 0).
                // For simple generation, we assume sequential and don't track free list.
                // If an ID is missing, this indicates an issue or a need for free list handling.
                // For now, treat missing IDs as if they were free (points to 0, gen 65535).
                // This is a simplification!
                System.Diagnostics.Debug.WriteLine($"Warning: Object ID {id} not found in offsets. Writing as free entry.");
                entry = "0000000000 65535 f \n";
            }

            // Each entry is exactly 20 bytes long (including EOL: LF or CR LF).
            // We use LF (\n) which is 1 byte, so padding needs to match 20 bytes total.
            if (entry.Length != 20)
            {
                // This should not happen with the D10 and D5 formats, but check anyway.
                throw new InvalidOperationException($"XRef entry for ID {id} has incorrect length ({entry.Length}). Entry: '{entry.Replace("\n", "\\n")}'");
            }

            await stream.WriteAsync(ascii.GetBytes(entry), 0, entry.Length);
        }

        return startOffset;
    }
}
