using MauiPdfGenerator.Core.Objects; // For PdfName

namespace MauiPdfGenerator.Core.Images;

/// <summary>
/// Defines the contract for processing image data to extract information
/// and bytes needed for PDF embedding. This abstracts the underlying image library (e.g., SkiaSharp).
/// </summary>
internal interface IPdfImageProcessor : IDisposable // Make it disposable in case implementations use native resources
{
    /// <summary>
    /// Processes the image data from the stream.
    /// </summary>
    /// <param name="imageStream">The input stream containing the image data. Should be readable and seekable.</param>
    void Load(Stream imageStream);

    /// <summary>
    /// Gets the width of the loaded image in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the loaded image in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets the appropriate PDF Name for the image's color space (e.g., /DeviceRGB, /DeviceGray, /Indexed).
     /// </summary>
    PdfName PdfColorSpace { get; }

    /// <summary>
    /// Gets the number of bits used per color component.
    /// </summary>
    int BitsPerComponent { get; }

    /// <summary>
    /// Gets the appropriate PDF Name for the filter required to decode the image data within the PDF,
    /// or null if the data is already in its final PDF format (e.g., raw DCT for JPEG).
    /// Examples: /DCTDecode, /FlateDecode.
    /// </summary>
    PdfName? PdfFilter { get; }

    /// <summary>
    /// Gets the raw image data bytes to be embedded in the PDF stream.
    /// This will be the original DCT data for JPEGs, or the raw pixel data
    /// (potentially needing Flate compression later) for other formats.
    /// </summary>
    byte[] GetImageData();


    // --- Optional / Advanced ---

    ///// <summary>
    ///// Gets the image data for the alpha channel mask (/SMask), if applicable.
    ///// Returns null if the image has no transparency or if transparency is handled differently.
    ///// </summary>
    ///// <returns>Byte array for the alpha mask stream, or null.</returns>
    //// byte[]? GetAlphaMaskData();

    ///// <summary>
    ///// Gets the PDF dictionary for decode parameters (/DecodeParms), if needed for the filter.
    ///// </summary>
    //// PdfDictionary? GetDecodeParms();

    ///// <summary>
    ///// Gets the PDF object representing the color palette for indexed color spaces.
    ///// </summary>
    //// PdfObject? GetIndexedColorPalette();
}
