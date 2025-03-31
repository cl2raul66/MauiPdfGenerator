// Ignore Spelling: xref

using MauiPdfGenerator.Core.ObjectModel;

namespace MauiPdfGenerator.Core.FileStructure;

/// <summary>
/// Handles writing the PDF file trailer.
/// Internal implementation detail.
/// </summary>
internal record PdfTrailer
{
    private readonly PdfReference _catalogReference;
    private readonly int _objectCount; // Total number of objects + 1 (for obj 0)
    private readonly long _xrefOffset;

    public PdfTrailer(PdfReference catalogReference, int objectCount, long xrefOffset)
    {
        _catalogReference = catalogReference ?? throw new ArgumentNullException(nameof(catalogReference));
        _objectCount = objectCount;
        _xrefOffset = xrefOffset;
    }

    /// <summary>
    /// Writes the trailer dictionary, startxref offset, and EOF marker.
    /// </summary>
    public async Task WriteAsync(Stream stream)
    {
        // Build trailer dictionary
        var trailerDict = new PdfDictionary();
        trailerDict[Common.PdfConstants.Names.Size] = new PdfInteger(_objectCount);
        trailerDict[Common.PdfConstants.Names.Root] = _catalogReference;
        // TODO: Add /Info dictionary reference (optional, discouraged)
        // TODO: Add /ID array (optional but recommended)
        // TODO: Add /Encrypt dictionary reference (if encrypted)

        await stream.WriteAsync(Common.PdfConstants.TrailerKeyword);
        await stream.WriteAsync(Common.PdfConstants.NewLine);
        await trailerDict.WriteAsync(stream); // Write the dictionary contents
        await stream.WriteAsync(Common.PdfConstants.NewLine);

        // Write startxref
        await stream.WriteAsync(Common.PdfConstants.StartXRefKeyword);
        await stream.WriteAsync(Common.PdfConstants.NewLine);
        await stream.WriteAsync(Common.PdfEncodings.StructureEncoding.GetBytes(_xrefOffset.ToString()));
        await stream.WriteAsync(Common.PdfConstants.NewLine);

        // Write EOF marker
        await stream.WriteAsync(Common.PdfConstants.EofKeyword);
        // Some readers expect a newline after %%EOF, though spec doesn't strictly require it
        await stream.WriteAsync(Common.PdfConstants.NewLine);
    }
}
