using MauiPdfGenerator.Common; // For PdfWriter context

namespace MauiPdfGenerator.Core.ObjectModel;

/// <summary>
/// Represents a PDF Stream object, consisting of a dictionary and byte data.
/// Internal as it's an implementation detail.
/// </summary>
internal class PdfStream : PdfObject
{
    public PdfDictionary Dictionary { get; }
    private readonly byte[] _data; // Unencoded/unfiltered data

    public PdfStream(byte[] data)
    {
        Dictionary = new PdfDictionary();
        _data = data ?? []; // Ensure data is never null

        // Set the /Length entry (required)
        Dictionary[PdfConstants.Names.Length] = new PdfInteger(_data.Length);
        // TODO: Add /Filter and encoding/compression logic here later
    }

    public PdfStream(PdfDictionary dictionary, byte[] data)
    {
        Dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _data = data ?? [];

        // Ensure /Length is present and correct
        Dictionary[PdfConstants.Names.Length] = new PdfInteger(_data.Length);
        // TODO: Add /Filter and encoding/compression logic here later
    }

    public byte[] GetData() => _data; // Provide access to raw data if needed

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        // 1. Write the dictionary
        await Dictionary.WriteAsync(stream, writer);
        await WriteBytesAsync(stream, PdfConstants.NewLine);

        // 2. Write the 'stream' keyword
        await WriteBytesAsync(stream, PdfConstants.StreamKeyword);
        await WriteBytesAsync(stream, PdfConstants.NewLine);

        // 3. Write the stream data
        // TODO: Apply filters (compression) to _data before writing if specified in Dictionary[/Filter]
        if (_data.Length > 0)
        {
            await WriteBytesAsync(stream, _data);
        }

        // 4. Write the 'endstream' keyword
        await WriteBytesAsync(stream, PdfConstants.NewLine);
        await WriteBytesAsync(stream, PdfConstants.EndStreamKeyword);
    }
}
