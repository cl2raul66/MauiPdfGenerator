using MauiPdfGenerator.Core.ObjectModel;

namespace MauiPdfGenerator.Core.FileStructure;

/// <summary>
/// Handles writing the PDF Cross-Reference (XRef) table (classic format).
/// Internal implementation detail.
/// </summary>
internal class PdfXRef
{
    // Store entries keyed by object number for easy sorting/writing
    private readonly SortedDictionary<int, (long Offset, int Generation, bool IsFree)> _entries = [];

    public PdfXRef()
    {
        // Add the mandatory entry for object 0 (free list head)
        _entries[0] = (0, 65535, true);
    }

    /// <summary>
    /// Adds or updates an entry for an indirect object.
    /// </summary>
    public void AddEntry(PdfIndirectObject obj)
    {
        if (obj.ObjectNumber == 0) return; // Object 0 is handled specially
        if (obj.ByteOffset < 0) throw new InvalidOperationException($"Object {obj.ObjectNumber} has not been assigned an offset.");
        _entries[obj.ObjectNumber] = (obj.ByteOffset, obj.GenerationNumber, false);
    }

    /// <summary>
    /// Gets the total number of entries (including object 0), needed for trailer Size.
    /// </summary>
    public int EntryCount => _entries.Count > 0 ? _entries.Keys.Max() + 1 : 1;


    /// <summary>
    /// Writes the classic XRef table to the stream.
    /// Assumes a single contiguous section starting from object 0.
    /// </summary>
    public async Task WriteAsync(Stream stream)
    {
        if (_entries.Count == 0) return; // Should not happen with object 0

        int maxObjectNumber = _entries.Keys.Max();
        int objectCount = maxObjectNumber + 1; // Size of the single section

        // Write xref header line: "xref\n"
        await stream.WriteAsync(Common.PdfConstants.XRefKeyword);
        await stream.WriteAsync(Common.PdfConstants.NewLine);

        // Write section header line: "firstObjNum count\n" (assuming single section 0 N)
        string sectionHeader = $"0 {objectCount}\n";
        await stream.WriteAsync(Common.PdfEncodings.StructureEncoding.GetBytes(sectionHeader));

        // Write entries for objects 0 to maxObjectNumber
        for (int i = 0; i < objectCount; i++)
        {
            if (_entries.TryGetValue(i, out var entry))
            {
                // Format: "oooooooooo ggggg n \n" (10 offset, 5 gen, type, space, newline) - 20 bytes total
                string type = entry.IsFree ? "f" : "n";
                string line = $"{entry.Offset:D10} {entry.Generation:D5} {type} \n";
                await stream.WriteAsync(Common.PdfEncodings.StructureEncoding.GetBytes(line));
            }
            else
            {
                // Object number doesn't exist, write a free entry pointing to object 0
                // This is somewhat arbitrary, but fills the gap. A better approach might
                // involve tracking actual free objects if incremental updates were supported.
                // For simple generation, pointing missing entries to 0 is common.
                string line = $"0000000000 65535 f \n";
                await stream.WriteAsync(Common.PdfEncodings.StructureEncoding.GetBytes(line));
            }
        }
    }
}
