using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure;
using System.Globalization;
using System.Text;

namespace MauiPdfGenerator.Core.IO;

/// <summary>
/// Writes a PdfDocument object graph to an output stream according to PDF specification.
/// </summary>
internal class PdfWriter
{
    private readonly PdfDocument _document;
    private readonly PdfCrossReferenceTable _xrefTable;
    private readonly Encoding _pdfEncoding = Encoding.ASCII; // PDF structure uses ASCII primarily

    // Optional settings could go here (e.g., PDF version override)

    public PdfWriter(PdfDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _xrefTable = new PdfCrossReferenceTable();
    }

    /// <summary>
    /// Writes the entire PDF document to the specified stream asynchronously.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    public async Task WriteAsync(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite) throw new ArgumentException("Stream must be writable.", nameof(stream));
        if (!stream.CanSeek) throw new ArgumentException("Stream must be seekable for xref table.", nameof(stream));

        // 1. Write Header
        // PDF version (e.g., 2.0). Use the highest version required by features used.
        // % symbol followed by 4 bytes >= 128 for binary signature (recommended)
        string header = "%PDF-2.0\n%\xE2\xE3\xCF\xD3\n"; // Example 2.0 + binary signature
        await stream.WriteAsync(_pdfEncoding.GetBytes(header), 0, header.Length);

        // 2. Write Body (Indirect Objects)
        // Keep track of where each object starts for the xref table.
        var indirectObjects = _document.GetIndirectObjects().OrderBy(o => o.Id).ToList(); // Ensure order for readability?
        foreach (var indirectObject in indirectObjects)
        {
            long offset = stream.Position; // Record offset *before* writing
            _xrefTable.RegisterObjectOffset(indirectObject, offset);
            await indirectObject.WriteAsync(stream, _pdfEncoding);
            // Ensure newline after endobj (WriteAsync in PdfIndirectObject adds one)
        }

        // 3. Write Cross-Reference Table
        long xrefOffset = await _xrefTable.WriteAsync(stream);

        // 4. Write Trailer Dictionary
        var trailerDict = new PdfDictionary();
        // Calculate Size: Highest Object ID used + 1 (or count of objects + 1 if IDs are sequential starting from 1)
        int highestId = _document.GetIndirectObjects().Max(io => io.Id); // Find the actual max ID used
        trailerDict.Add(PdfName.Size, new PdfNumber(highestId + 1));

        //trailerDict.Add(PdfName.Size, new PdfNumber(_document.GetIndirectObjects().Count() + 1)); // Total objects + entry 0
        //trailerDict.Add(PdfName.Root, _document.Catalog.Reference); // Reference to the Catalog obj

        // --- CORREGIDO: Obtener la referencia del Catalog ---
        var catalogIndirect = _document.GetIndirectObjects().FirstOrDefault(io => io.Value == _document.Catalog) ?? throw new InvalidOperationException("PdfCatalog was not found as an indirect object before writing trailer.");
        trailerDict.Add(PdfName.Root, catalogIndirect.Reference); // Usa la referencia del objeto indirecto

        // Add /Info reference if Info dictionary has content
        if (_document.Info.Count > 0)
        {
            // Add Info as an indirect object *now* if it hasn't been added yet.
            var infoIndirect = _document.AddIndirectObject(_document.Info);
            // AddIndirectObject should handle returning existing if already added.
            trailerDict.Add(PdfName.Info, infoIndirect.Reference);
        }

        // TODO: Add /Encrypt reference if document is encrypted
        // TODO: Add /ID array (file identifier) - often useful

        await stream.WriteAsync(_pdfEncoding.GetBytes("trailer\n"), 0, 8);
        await trailerDict.WriteAsync(stream, _pdfEncoding); // Write the trailer dict content
        await stream.WriteAsync(_pdfEncoding.GetBytes("\n"), 0, 1); // Newline after trailer dict

        // 5. Write 'startxref' keyword and offset
        await stream.WriteAsync(_pdfEncoding.GetBytes("startxref\n"), 0, 10);
        string offsetStr = xrefOffset.ToString(CultureInfo.InvariantCulture) + "\n";
        await stream.WriteAsync(_pdfEncoding.GetBytes(offsetStr), 0, offsetStr.Length);

        // 6. Write EOF marker
        await stream.WriteAsync(_pdfEncoding.GetBytes("%%EOF"), 0, 5);

        // Consider flushing the stream if needed by the caller
        await stream.FlushAsync();
    }
}
