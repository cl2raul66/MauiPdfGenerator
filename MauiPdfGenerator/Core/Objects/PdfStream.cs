using System.Diagnostics;
using System.IO.Compression; // For FlateDecode (DeflateStream)
using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF stream object, consisting of a dictionary and a sequence of bytes.
/// Used for large data like page content, images, fonts. Section 7.3.8.
/// </summary>
internal class PdfStream : PdfObject
{
    /// <summary>
    /// Gets the dictionary associated with this stream, containing metadata like Length and Filter.
    /// </summary>
    public PdfDictionary Dictionary { get; }

    /// <summary>
    /// Gets or sets the raw, unfiltered byte data of the stream.
    /// The filters specified in the Dictionary (/Filter key) will be applied
    /// to this data before writing the final PDF stream content.
    /// </summary>
    protected byte[] UnfilteredData { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfStream"/> class with empty data.
    /// </summary>
    public PdfStream() : this([])
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfStream"/> class.
    /// </summary>
    /// <param name="unfilteredData">The raw, unfiltered byte data.</param>
    public PdfStream(byte[] unfilteredData)
    {
        Dictionary = [];
        UnfilteredData = unfilteredData ?? throw new ArgumentNullException(nameof(unfilteredData));
        // Length entry will be set automatically during writing after filtering.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfStream"/> class with an existing dictionary.
    /// Used for cases like Image XObjects where the dictionary is pre-built.
    /// </summary>
    /// <param name="dictionary">The pre-existing dictionary.</param>
    /// <param name="unfilteredData">The raw, unfiltered byte data.</param>
    public PdfStream(PdfDictionary dictionary, byte[] unfilteredData)
    {
        Dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        UnfilteredData = unfilteredData ?? throw new ArgumentNullException(nameof(unfilteredData));
    }


    /// <summary>
    /// Applies the filters specified in the dictionary's /Filter entry to the UnfilteredData.
    /// Currently only supports /FlateDecode.
    /// </summary>
    /// <returns>The filtered (e.g., compressed) byte data.</returns>
    /// <exception cref="NotSupportedException">If an unsupported filter is specified.</exception>
    protected virtual byte[] ApplyFilters()
    {
        Debug.WriteLine("DEBUG: Applying Filters - SKIPPING COMPRESSION"); // Mensaje Debug
                                                           
        return UnfilteredData; 
        //// Check the /Filter entry in the dictionary
        //PdfObject? filterEntry = Dictionary[PdfName.Filter];
        //if (filterEntry is null || filterEntry is PdfNull)
        //{
        //    return UnfilteredData; // No filter specified
        //}

        //// We'll process filters sequentially if it's an array
        //var filters = new List<PdfName>();
        //if (filterEntry is PdfName singleFilter)
        //{
        //    filters.Add(singleFilter);
        //}
        //else if (filterEntry is PdfArray filterArray)
        //{
        //    filters.AddRange(filterArray.OfType<PdfName>());
        //}
        //else
        //{
        //    throw new NotSupportedException($"Unsupported /Filter type: {filterEntry.GetType().Name}");
        //}

        //if (filters.Count == 0)
        //{
        //    return UnfilteredData;
        //}

        //byte[] currentData = UnfilteredData;

        //// TODO: Implement support for /DecodeParms dictionary associated with filters.

        //foreach (var filterName in filters)
        //{
        //    if (filterName == PdfName.FlateDecode)
        //    {
        //        currentData = FlateEncode(currentData);
        //    }
        //    // --- Add support for other filters here ---
        //    // else if (filterName == PdfName.DCTDecode) { /* Handled by image data usually */ }
        //    // else if (filterName == PdfName.ASCIIHexDecode) { ... }
        //    // else if (filterName == PdfName.ASCII85Decode) { ... }
        //    // else if (filterName == PdfName.LZWDecode) { ... } // Requires LZW implementation
        //    // else if (filterName == PdfName.RunLengthDecode) { ... }
        //    else
        //    {
        //        // If filter not supported, should we throw or write unfiltered?
        //        // Throwing is safer to indicate lack of support.
        //        throw new NotSupportedException($"Unsupported stream filter: {filterName.Value}");
        //    }
        //}

        //return currentData;
    }

    /// <summary>
    /// Compresses data using the Flate (Deflate) algorithm.
    /// </summary>
    private byte[] FlateEncode(byte[] data)
    {
        if (data is null || data.Length == 0) return [];

        using var outputStream = new MemoryStream();
        {
            using (var deflateStream = new DeflateStream(outputStream, CompressionLevel.SmallestSize))
            {
                deflateStream.Write(data, 0, data.Length);
            } // El Dispose del DeflateStream hace un flush implícito

            // Obtener los datos comprimidos después de que DeflateStream se haya cerrado
            // pero antes de que outputStream se cierre
            return outputStream.ToArray();
        } // outputStream se cierra aquí
    }


    internal override void Write(StreamWriter writer, Encoding encoding)
    {
        // Streams must be written asynchronously or directly to the underlying stream
        // because StreamWriter might interfere with raw byte writing.
        throw new NotSupportedException("PdfStream must be written asynchronously or directly to a Stream.");
    }

    internal override async Task WriteAsync(Stream stream, Encoding encoding)
    {
        byte[] filteredData = ApplyFilters();

        // Update the /Length entry in the dictionary *before* writing it
        Dictionary.Add(PdfName.Length, new PdfNumber(filteredData.Length));

        // 1. Write the dictionary
        await Dictionary.WriteAsync(stream, encoding);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("\nstream\n"), 0, 8); // "\nstream\n" convention

        // 2. Write the (potentially filtered) byte data
        if (filteredData.Length > 0)
        {
            await stream.WriteAsync(filteredData, 0, filteredData.Length);
        }

        // 3. Write the endstream keyword
        // Spec: "The sequence of bytes that makes up a stream occurs after the stream keyword and before the endstream keyword."
        // "There should be an end-of-line marker before the endstream keyword"
        // "The sequence EOL endstream EOL is the preferred form" (but just \nendstream\n is common)
        await stream.WriteAsync(Encoding.ASCII.GetBytes("\nendstream"), 0, 10); // "\nendstream"
    }

    public override string ToString() => $"<<{Dictionary.Count} entries>> stream ... endstream"; // Debugging
}
