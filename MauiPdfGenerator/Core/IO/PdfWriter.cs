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

        // 2. Write Body (Indirect Objects) in hierarchical order
        var indirectObjects = _document.GetIndirectObjects().OrderBy(x => x.Id);

        // First write Catalog
        var catalog = indirectObjects.First(io => io.Value == _document.Catalog);
        await WriteIndirectObject(stream, catalog);

        // Then Pages tree
        var pages = indirectObjects.First(io => io.Value == _document.PageTreeRoot);
        await WriteIndirectObject(stream, pages);

        // Then individual pages and their content
        var remainingObjects = indirectObjects.Except([catalog, pages]).OrderBy(io => io.Id);

        foreach (var obj in remainingObjects)
        {
            await WriteIndirectObject(stream, obj);
        }

        // 3. Write Cross-Reference Table
        long xrefOffset = await _xrefTable.WriteAsync(stream);

        // 4. Write Trailer Dictionary
        var trailerDict = new PdfDictionary();
        int highestId = indirectObjects.Max(io => io.Id);
        trailerDict.Add(PdfName.Size, new PdfNumber(highestId + 1));
        trailerDict.Add(PdfName.Root, catalog.Reference);

        await stream.WriteAsync(_pdfEncoding.GetBytes("trailer\n"), 0, 8);
        await trailerDict.WriteAsync(stream, _pdfEncoding);
        await stream.WriteAsync(_pdfEncoding.GetBytes("\nstartxref\n"), 0, 11);

        string offsetStr = xrefOffset.ToString(CultureInfo.InvariantCulture) + "\n";
        await stream.WriteAsync(_pdfEncoding.GetBytes(offsetStr), 0, offsetStr.Length);

        await stream.WriteAsync(_pdfEncoding.GetBytes("%%EOF"), 0, 5);
        await stream.FlushAsync();
    }

    private async Task WriteIndirectObject(Stream stream, PdfIndirectObject obj)
    {
        long offset = stream.Position;
        _xrefTable.RegisterObjectOffset(obj, offset);
        await obj.WriteAsync(stream, _pdfEncoding);
    }
}
