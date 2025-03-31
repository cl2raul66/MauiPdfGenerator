using MauiPdfGenerator.Core.ObjectModel;
using MauiPdfGenerator.Core.FileStructure;
using MauiPdfGenerator.Core.DocumentStructure; 

namespace MauiPdfGenerator.Common;

/// <summary>
/// Orchestrates the writing of all PDF components (Header, Body, XRef, Trailer) to a stream.
/// Internal implementation detail.
/// </summary>
internal class PdfWriter : IDisposable
{
    private readonly List<PdfIndirectObject> _objectsToWrite = [];
    private readonly Dictionary<int, PdfIndirectObject> _objectMap = []; // For quick lookup if needed
    private int _nextObjectNumber = 1;
    private PdfIndirectObject? _catalogIndirectObject; // Reference to the catalog

    public PdfWriter() { }

    /// <summary>
    /// Adds a PDF object to be written as an indirect object, assigning it the next available object number.
    /// </summary>
    /// <param name="obj">The PdfObject to add.</param>
    /// <returns>The created PdfIndirectObject wrapper.</returns>
    public PdfIndirectObject AddObject(PdfObject obj)
    {
        var indirectObj = new PdfIndirectObject(_nextObjectNumber++, obj);
        _objectsToWrite.Add(indirectObj);
        _objectMap[indirectObj.ObjectNumber] = indirectObj;
        return indirectObj;
    }

    /// <summary>
    /// Sets the root Catalog object for the document.
    /// </summary>
    /// <param name="catalog">The PdfCatalog object.</param>
    public void SetCatalog(PdfCatalog catalog)
    {
        // Add the catalog object itself if it wasn't added explicitly
        if (!_objectMap.Any(kvp => kvp.Value.ContainedObject == catalog))
        {
            _catalogIndirectObject = AddObject(catalog);
        }
        else
        {
            _catalogIndirectObject = _objectMap.First(kvp => kvp.Value.ContainedObject == catalog).Value;
        }
    }


    /// <summary>
    /// Writes the entire PDF document structure to the provided stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    public async Task WriteDocumentAsync(Stream stream)
    {
        if (_catalogIndirectObject is null)
            throw new InvalidOperationException("PDF Catalog has not been set before writing the document.");

        long currentOffset = 0;

        // 1. Write Header
        await PdfHeader.WriteAsync(stream);
        currentOffset = stream.Position; // Get position *after* header

        // 2. Write Body (Indirect Objects)
        var xref = new PdfXRef();
        foreach (var indirectObj in _objectsToWrite)
        {
            indirectObj.ByteOffset = currentOffset; // Record offset *before* writing
            xref.AddEntry(indirectObj);             // Add to XRef table data

            await indirectObj.WriteAsync(stream, this); // Write "N M obj ... endobj"
            currentOffset = stream.Position;        // Update offset *after* writing
        }

        // 3. Write XRef Table
        long xrefOffset = currentOffset;
        await xref.WriteAsync(stream);
        _ = stream.Position; 

        // 4. Write Trailer
        int trailerSize = xref.EntryCount; // Get size for trailer
        var trailer = new PdfTrailer(_catalogIndirectObject.CreateReference(), trailerSize, xrefOffset);
        await trailer.WriteAsync(stream);
    }

    public void Dispose()
    {
        // Clean up resources if needed
        _objectsToWrite.Clear();
        _objectMap.Clear();
    }
}
